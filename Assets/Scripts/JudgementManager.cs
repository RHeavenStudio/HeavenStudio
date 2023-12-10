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
        [SerializeField] AspectRatioFitter epilogueFitter;

        [SerializeField] GameObject bg;
        [SerializeField] GameObject rankLogo;
        [SerializeField] Animator rankAnim;
        [SerializeField] CanvasScaler scaler;
        [SerializeField] Animator canvasAnim;

        AudioSource audioSource;
        bool twoMessage = false, barStarted = false, didRank = false, didEpilogue = false;
        float barTime = 0, barStartTime = float.MaxValue;

        public void PrepareJudgement()
        {
            bg.SetActive(false);
            rankLogo.SetActive(false);

            barText.text = "0";
            barSlider.value = 0;
            barText.color = numColourNg;
            barSlider.fillRect.GetComponent<Image>().color = barColourNg;

            // temp
            twoMessage = false;
            // judgementInfo = new()
            // {
            //     finalScore = 0.79,
            // };
            header.text = playedBeatmap != null ? playedBeatmap["resultcaption"] : "Rhythm League Notes";
            // end temp

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

            string imagePath = "";
            string imageName = "";
            EntityTypes.Resource? imageResource;
            if (judgementInfo.finalScore < Minigame.rankOkThreshold)
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_ng_res"] : null;
            }
            else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_ok_res"] : null;
            }
            else
            {
                imageResource = playedBeatmap != null ? playedBeatmap["epilogue_hi_res"] : null;
            }

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
                    epilogueImage.sprite = null;
                    epilogueFitter.aspectRatio = 16f / 9f;
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
                epilogueImage.sprite = null;
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
            if (judgementInfo.finalScore < Minigame.rankOkThreshold)
            {
                message0.text = playedBeatmap != null ? playedBeatmap["resultcommon_ng"] : "Try harder next time.";
            }
            else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
            {
                message0.text = playedBeatmap != null ? playedBeatmap["resultcommon_ok"] : "Eh. Passable.";
            }
            else
            {
                message0.text = playedBeatmap != null ? playedBeatmap["resultcommon_hi"] : "Good rhythm.";
            }
        }

        public void ShowMessage1()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageMid);
            // message1.text = "message line 1";
            message1.text = "skill issue";
        }

        public void ShowMessage2()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageLast);
            // message2.text = "message line 2";
            message2.text = "lmao lmao";
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
            bg.SetActive(true);
            if (judgementInfo.finalScore < Minigame.rankOkThreshold)
            {
                rankAnim.Play("Ng");
                audioSource.PlayOneShot(rankNg);
            }
            else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
            {
                rankAnim.Play("Ok");
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
            if (judgementInfo.finalScore < Minigame.rankOkThreshold)
            {
                audioSource.PlayOneShot(musNgStart);
                audioSource.clip = musNg;
                audioSource.loop = true;
                audioSource.PlayScheduled(AudioSettings.dspTime + musNgStart.length);
            }
            else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
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
                    canvasAnim.Play("EpilogueOpen");
                    audioSource.Stop();
                    if (judgementInfo.finalScore < Minigame.rankOkThreshold)
                    {
                        epilogueMessage.text = playedBeatmap != null ? playedBeatmap["epilogue_ng"] : "Try Again picture";
                        audioSource.PlayOneShot(jglNg);
                    }
                    else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
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

                    if (judgementInfo.finalScore < Minigame.rankOkThreshold)
                    {
                        barText.color = numColourNg;
                        barSlider.fillRect.GetComponent<Image>().color = barColourNg;
                    }
                    else if (judgementInfo.finalScore < Minigame.rankHiThreshold)
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