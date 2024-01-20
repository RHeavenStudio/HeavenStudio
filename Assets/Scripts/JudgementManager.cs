using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TMPro;
using Jukebox;
using UnityEngine.Playables;
using UnityEngine.Networking;
using HeavenStudio.Games;
using HeavenStudio.InputSystem;

namespace HeavenStudio
{
    [RequireComponent(typeof(PlayableDirector), typeof(AudioSource))]
    public class JudgementManager : MonoBehaviour
    {
        enum Rank
        {
            Ng = 0,
            Ok = 1,
            Hi = 2
        }

        public enum InputCategory : int
        {
            Normal = 0,
            Keep = 1,
            Aim = 2,
            Repeat = 3
            // higher values are (will be) custom categories
        }

        [Serializable]
        public struct MedalInfo
        {
            public double beat;
            public string name;
            public double score;
            public bool cleared;
        }

        [Serializable]
        public struct InputInfo
        {
            public double beat;
            public double accuracyState;
            public double timeOffset;
            public float weight;
            public int category;
        }

        [Serializable]
        public struct JudgementInfo
        {
            public List<InputInfo> inputs;
            public List<MedalInfo> medals;

            public double finalScore;
            public bool star, perfect;
            public DateTime time;
        }

        const string MessageAdd = "Also... ";

        static JudgementInfo judgementInfo;
        static RiqBeatmap playedBeatmap;

        public static void SetPlayInfo(JudgementInfo info, RiqBeatmap beatmap)
        {
            judgementInfo = info;
            playedBeatmap = beatmap;
        }

        [Header("Bar parameters")]
        [SerializeField] float barDuration;
        [SerializeField] float barRankWait;
        [SerializeField] float rankMusWait;
        [SerializeField] Color barColourNg, barColourOk, barColourHi;
        [SerializeField] Color numColourNg, numColourOk, numColourHi;

        [Header("Audio clips")]
        [SerializeField] AudioClip messageMid;
        [SerializeField] AudioClip messageLast;
        [SerializeField] AudioClip barLoop, barStop;
        [SerializeField] AudioClip rankNg, rankOk, rankHi;
        [SerializeField] AudioClip musNgStart, musOkStart, musHiStart;
        [SerializeField] AudioClip musNg, musOk, musHi;
        [SerializeField] AudioClip jglNg, jglOk, jglHi;

        [Header("References")]
        [SerializeField] TMP_Text header;
        [SerializeField] TMP_Text message0;
        [SerializeField] TMP_Text message1;
        [SerializeField] TMP_Text message2;
        [SerializeField] TMP_Text barText;
        [SerializeField] Slider barSlider;

        [SerializeField] TMP_Text epilogueMessage;
        [SerializeField] Image epilogueImage;
        [SerializeField] Image epilogueFrame;
        [SerializeField] AspectRatioFitter epilogueFitter;
        [SerializeField] Sprite epilogueNg, epilogueOk, epilogueHi;
        [SerializeField] Sprite epilogueFrmNg, epilogueFrmOk, epilogueFrmHi;

        [SerializeField] GameObject bg;
        [SerializeField] GameObject rankLogo;
        [SerializeField] TMP_Text justOk;
        [SerializeField] Animator rankAnim;
        [SerializeField] ParticleSystem okParticles1, okParticles2;
        [SerializeField] CanvasScaler scaler;
        [SerializeField] Animator canvasAnim;

        AudioSource audioSource;
        List<int> usedCategories;
        float[] categoryInputs;
        double[] categoryScores;
        string msg0, msg1, msg2;
        float barTime = 0, barStartTime = float.MaxValue;
        Rank rank;
        bool twoMessage = false, barStarted = false, didRank = false, didEpilogue = false, subRank = false;

        public void PrepareJudgement()
        {
            bg.SetActive(false);
            rankLogo.SetActive(false);
            justOk.gameObject.SetActive(false);
            subRank = false;

            barText.text = "0";
            barSlider.value = 0;
            barText.color = numColourNg;
            barSlider.fillRect.GetComponent<Image>().color = barColourNg;

            string propSuffix = "ng";
            double inputs = 0, score = 0;
            foreach (var input in judgementInfo.inputs)
            {
                inputs += input.weight;
                score += Math.Clamp(input.accuracyState, 0, 1) * input.weight;
            }
            if (inputs > 0)
            {
                score /= inputs;
            }
            else
            {
                score = 0;
            }
            judgementInfo.finalScore = score;
            if (judgementInfo.finalScore < Minigame.rankOkThreshold)
            {
                rank = Rank.Ng;
                propSuffix = "ng";
            }
            else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
            {
                rank = Rank.Ok;
                propSuffix = "ok";
            }
            else
            {
                rank = Rank.Hi;
                propSuffix = "hi";
            }

            GetCategoryInfo();

            int firstCat = 0, secondCat = 0;
            double lastScore = 0;
            if (usedCategories.Count == 1)
            {
                twoMessage = false;
                if (playedBeatmap != null)
                {
                    msg0 = playedBeatmap[$"resultcommon_{propSuffix}"];
                }
                else
                {
                    msg0 = rank switch
                    {
                        Rank.Ng => "Try harder next time.",
                        Rank.Ok => "Eh. Passable.",
                        _ => "Good rhythm.",
                    };
                }
            }
            else
            {
                switch (rank)
                {
                    case Rank.Ok:
                        // check if any category has a hi score
                        foreach (int cat in usedCategories)
                        {
                            if (categoryScores[cat] > lastScore)
                            {
                                lastScore = categoryScores[cat];
                                firstCat = cat;
                            }
                        }
                        SetOkMessages(firstCat, lastScore);
                        break;
                    case Rank.Ng:
                        // find the first and second worst categories
                        firstCat = -1;
                        secondCat = -1;
                        lastScore = double.MaxValue;
                        foreach (int cat in usedCategories)
                        {
                            if (categoryScores[cat] < lastScore)
                            {
                                lastScore = categoryScores[cat];
                                firstCat = cat;
                            }
                        }
                        lastScore = double.MaxValue;
                        foreach (int cat in usedCategories)
                        {
                            if (cat == firstCat) continue;
                            if (categoryScores[cat] < lastScore)
                            {
                                lastScore = categoryScores[cat];
                                secondCat = cat;
                            }
                        }
                        // only show one message if only one category fails
                        twoMessage = categoryScores[secondCat] < Minigame.rankOkThreshold;
                        if (playedBeatmap != null)
                        {
                            msg0 = msg1 = playedBeatmap[$"resultcat{firstCat}_ng"];
                            msg2 = playedBeatmap[$"resultcat{secondCat}_ng"];
                        }
                        else
                        {
                            msg0 = msg1 = "Try harder next time.";
                            msg2 = "Try harder next time.";
                        }
                        break;
                    case Rank.Hi:
                        // find the first and second best categories
                        firstCat = -1;
                        secondCat = -1;
                        lastScore = 0;
                        foreach (int cat in usedCategories)
                        {
                            if (categoryScores[cat] > lastScore)
                            {
                                lastScore = categoryScores[cat];
                                firstCat = cat;
                            }
                        }
                        lastScore = 0;
                        foreach (int cat in usedCategories)
                        {
                            if (cat == firstCat) continue;
                            if (categoryScores[cat] > lastScore)
                            {
                                lastScore = categoryScores[cat];
                                secondCat = cat;
                            }
                        }
                        // only show one message if only one category passes
                        twoMessage = categoryScores[secondCat] >= Minigame.rankHiThreshold;
                        if (playedBeatmap != null)
                        {
                            msg0 = msg1 = playedBeatmap[$"resultcat{firstCat}_hi"];
                            msg2 = playedBeatmap[$"resultcat{secondCat}_hi"];
                        }
                        else
                        {
                            msg0 = msg1 = "Good rhythm.";
                            msg2 = "Good rhythm.";
                        }
                        break;
                }
            }

            header.text = playedBeatmap != null ? playedBeatmap["resultcaption"] : "Rhythm League Notes";

            if (twoMessage)
            {
                message0.gameObject.SetActive(false);
                message1.gameObject.SetActive(true);
                message2.gameObject.SetActive(true);
                message1.text = " ";
                message2.text = " ";
            }
            else
            {
                message0.gameObject.SetActive(true);
                message1.gameObject.SetActive(false);
                message2.gameObject.SetActive(false);
                message0.text = " ";
            }

            string imagePath;
            string imageName;
            EntityTypes.Resource? imageResource;
            if (rank == Rank.Ng)
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_ng_res"] : null;
            }
            else if (rank == Rank.Ok)
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_ok_res"] : null;
            }
            else
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_hi_res"] : null;
            }

            epilogueFrame.sprite = rank switch
            {
                Rank.Ok => epilogueFrmOk,
                Rank.Hi => epilogueFrmHi,
                _ => epilogueFrmNg
            };

            if (imageResource != null)
            {
                imagePath = imageResource.Value.path;
                imageName = imageResource.Value.name;
                try
                {
                    string fsPath = RiqFileHandler.GetResourcePath(imageName, imagePath);
                    // fetch the image using UnityWebRequest
                    StartCoroutine(LoadImage(fsPath));
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Debug.Log("image resource doesn't exist, using blank placeholder");
                    epilogueImage.sprite = rank switch
                    {
                        Rank.Ok => epilogueOk,
                        Rank.Hi => epilogueHi,
                        _ => epilogueNg
                    };
                    epilogueFitter.aspectRatio = 16f / 9f;
                }
            }
        }

        void SetOkMessages(int cat, double score)
        {
            twoMessage = false;
            if (score >= Minigame.rankHiThreshold)
            {
                // just OK
                subRank = true;
                if (playedBeatmap != null)
                {
                    msg0 = playedBeatmap[$"resultcat{cat}_hi"];
                }
                else
                {
                    msg0 = "Good rhythm.";
                }
            }
            else
            {
                if (playedBeatmap != null)
                {
                    msg0 = playedBeatmap[$"resultcommon_ok"];
                }
                else
                {
                    msg0 = "Eh. Passable.";
                }
            }
        }

        void GetCategoryInfo()
        {
            int maxCat = 0;
            usedCategories = new();
            if (playedBeatmap == null || playedBeatmap.data.beatmapSections.Count == 0)
            {
                usedCategories.Add(0);
                return;
            }
            foreach (var section in playedBeatmap.data.beatmapSections)
            {
                int cat = section["category"];
                if (!usedCategories.Contains(cat))
                {
                    usedCategories.Add(cat);
                    maxCat = Mathf.Max(maxCat, cat);
                }
            }
            usedCategories.Sort();

            categoryInputs = new float[maxCat + 1];
            categoryScores = new double[maxCat + 1];
            foreach (var input in judgementInfo.inputs)
            {
                categoryInputs[input.category] += input.weight;
                categoryScores[input.category] += input.accuracyState * input.weight;
            }
            for (int i = 0; i < categoryScores.Length; i++)
            {
                if (categoryInputs[i] > 0)
                {
                    categoryScores[i] /= categoryInputs[i];
                }
            }
        }

        IEnumerator LoadImage(string path)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                epilogueImage.sprite = rank switch
                {
                    Rank.Ok => epilogueOk,
                    Rank.Hi => epilogueHi,
                    _ => epilogueNg
                };
                epilogueFitter.aspectRatio = 16f / 9f;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                epilogueImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                epilogueImage.preserveAspect = true;
                epilogueFitter.aspectRatio = (float)texture.width / (float)texture.height;
            }
        }

        public void ShowMessage0()
        {
            if (twoMessage) return;
            audioSource.PlayOneShot(messageLast);
            message0.text = msg0;
        }

        public void ShowMessage1()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageMid);
            // message1.text = "message line 1";
            message1.text = msg1;
        }

        public void ShowMessage2()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageLast);
            // message2.text = "message line 2";
            message2.text = MessageAdd + msg2;
        }

        public void StartBar()
        {
            audioSource.clip = barLoop;
            audioSource.Play();

            barStartTime = Time.time;
            barTime = (float)judgementInfo.finalScore * barDuration;

            barStarted = true;
        }

        public void ShowRank()
        {
            rankLogo.SetActive(true);
            // bg.SetActive(true);
            if (rank == Rank.Ng)
            {
                rankAnim.Play("Ng");
                audioSource.PlayOneShot(rankNg);
            }
            else if (rank == Rank.Ok)
            {
                rankAnim.Play("Ok");
                if (subRank)
                {
                    justOk.gameObject.SetActive(true);
                    justOk.text = "...but, just";
                }
                audioSource.PlayOneShot(rankOk);
            }
            else
            {
                rankAnim.Play("Hi");
                audioSource.PlayOneShot(rankHi);
            }
        }

        public void StartRankMusic()
        {
            if (rank == Rank.Ng)
            {
                audioSource.PlayOneShot(musNgStart);
                audioSource.clip = musNg;
                audioSource.loop = true;
                audioSource.PlayScheduled(AudioSettings.dspTime + musNgStart.length);
            }
            else if (rank == Rank.Ok)
            {
                audioSource.PlayOneShot(musOkStart);
                audioSource.clip = musOk;
                audioSource.loop = true;
                audioSource.PlayScheduled(AudioSettings.dspTime + musOkStart.length);
            }
            else
            {
                audioSource.PlayOneShot(musHiStart);
                audioSource.clip = musHi;
                audioSource.loop = true;
                audioSource.PlayScheduled(AudioSettings.dspTime + musHiStart.length);
            }
            didRank = true;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private IEnumerator WaitAndRank()
        {
            yield return new WaitForSeconds(barRankWait);
            ShowRank();
            yield return new WaitForSeconds(rankMusWait);
            StartRankMusic();
        }

        private void Update()
        {
            float w = Screen.width / 1920f;
            float h = Screen.height / 1080f;
            scaler.scaleFactor = Mathf.Min(w, h);

            InputController currentController = PlayerInput.GetInputController(1);
            if (currentController.GetLastButtonDown() > 0)
            {
                if (didRank && !didEpilogue)
                {
                    // start the sequence for epilogue
                    okParticles1.Stop();
                    okParticles2.Stop();
                    canvasAnim.Play("EpilogueOpen");
                    audioSource.Stop();
                    if (rank == Rank.Ng)
                    {
                        epilogueMessage.text = playedBeatmap != null ? playedBeatmap["epilogue_ng"] : "Try Again picture";
                        audioSource.PlayOneShot(jglNg);
                    }
                    else if (rank == Rank.Ok)
                    {
                        epilogueMessage.text = playedBeatmap != null ? playedBeatmap["epilogue_ok"] : "OK picture";
                        audioSource.PlayOneShot(jglOk);
                    }
                    else
                    {
                        epilogueMessage.text = playedBeatmap != null ? playedBeatmap["epilogue_hi"] : "Superb picture";
                        audioSource.PlayOneShot(jglHi);
                    }
                    didEpilogue = true;
                }
                else if (didEpilogue)
                {
                    audioSource.Stop();
                    RiqFileHandler.ClearCache();
                    GlobalGameManager.LoadScene("Title", 0.35f, 0.5f);
                }
                else if (barStarted)
                {
                    barTime = Time.time - barStartTime;
                }
            }
            if (barStarted)
            {
                float t = Time.time - barStartTime;
                if (t >= barTime)
                {
                    barStarted = false;
                    audioSource.Stop();
                    audioSource.PlayOneShot(barStop);
                    barText.text = ((int)(judgementInfo.finalScore * 100)).ToString();
                    barSlider.value = (float)judgementInfo.finalScore;

                    if (rank == Rank.Ng)
                    {
                        barText.color = numColourNg;
                        barSlider.fillRect.GetComponent<Image>().color = barColourNg;
                    }
                    else if (rank == Rank.Ok)
                    {
                        barText.color = numColourOk;
                        barSlider.fillRect.GetComponent<Image>().color = barColourOk;
                    }
                    else
                    {
                        barText.color = numColourHi;
                        barSlider.fillRect.GetComponent<Image>().color = barColourHi;
                    }

                    StartCoroutine(WaitAndRank());
                }
                else
                {
                    float v = t / barTime * (float)judgementInfo.finalScore;
                    barText.text = ((int)(v * 100)).ToString();
                    barSlider.value = v;

                    if (v < Minigame.rankOkThreshold)
                    {
                        barText.color = numColourNg;
                        barSlider.fillRect.GetComponent<Image>().color = barColourNg;
                    }
                    else if (v < Minigame.rankHiThreshold)
                    {
                        barText.color = numColourOk;
                        barSlider.fillRect.GetComponent<Image>().color = barColourOk;
                    }
                    else
                    {
                        barText.color = numColourHi;
                        barSlider.fillRect.GetComponent<Image>().color = barColourHi;
                    }
                }
            }

        }
    }
}