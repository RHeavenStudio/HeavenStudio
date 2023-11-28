using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.Common;
using Starpelly;
using System.Linq;

using SFB;
using Jukebox;
using TMPro;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private Button createButton;

        [SerializeField] private Animator logoAnim;

        [SerializeField] private List<Animator> starAnims;

        [SerializeField] private Animator pressAnyKeyAnim;

        [SerializeField] private float bpm = 114f;
        [SerializeField] private float offset = 0f;

        [SerializeField] private RawImage bg;

        [SerializeField] private float bgXSpeed;
        [SerializeField] private float bgYSpeed;

        [SerializeField] private Collider2D logoHoverCollider;

        [SerializeField] private SettingsDialog settingsPanel;

        [SerializeField] private GameObject snsPanel;
        [SerializeField] private TMP_Text snsVersionText;

        [SerializeField] private GameObject playPanel;
        [SerializeField] private TMP_Text chartTitleText;
        [SerializeField] private TMP_Text chartMapperText;
        [SerializeField] private TMP_Text chartDescText;

        private AudioSource musicSource;

        private double songPosBeat;
        private double songPos;
        private double time;
        private double targetBopBeat;

        private int loops;

        private double lastAbsTime;
        private double startTime;

        private bool altBop;

        private bool logoRevealed;

        private bool menuMode, snsRevealed;

        private Animator menuAnim;

        private void Start()
        {
            menuAnim = GetComponent<Animator>();
            musicSource = GetComponent<AudioSource>();
            musicSource.PlayScheduled(AudioSettings.dspTime);
            startTime = Time.realtimeSinceStartupAsDouble;
            var _rand = new System.Random();
            starAnims = starAnims.OrderBy(_ => _rand.Next()).ToList();

#if UNITY_EDITOR
            versionText.text = "EDITOR";
#else
            versionText.text = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
#endif
        }

        private void Update()
        {
            bg.uvRect = new Rect(bg.uvRect.position + (new Vector2(bgXSpeed, bgYSpeed) * Time.deltaTime), bg.uvRect.size);
            if (songPos >= musicSource.clip.length)
            {
                time = 0;
                targetBopBeat = 0;
                loops++;
            }
            double absTime = Time.realtimeSinceStartupAsDouble - startTime;
            double dt = absTime - lastAbsTime;
            lastAbsTime = absTime;

            time += dt;

            songPos = time + offset;

            songPosBeat = SecsToBeats(songPos);
            if (Input.anyKeyDown)
            {
                if (logoRevealed && !menuMode)
                {
                    menuMode = true;
                    menuAnim.Play("Revealed", 0, 0);
                    pressAnyKeyAnim.Play("PressKeyFadeOut", 0, 0);
                }
                // else if (snsRevealed)
                // {
                //     snsRevealed = false;
                //     snsPanel.SetActive(false);
                // }
            }
            if (loops == 0 && !logoRevealed)
            {
                float normalizedBeat = GetPositionFromBeat(4, 1);
                if (normalizedBeat > 0 && normalizedBeat <= 1f)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", normalizedBeat);
                    pressAnyKeyAnim.DoNormalizedAnimation("PressKeyFadeIn", normalizedBeat);
                }
                else if (normalizedBeat < 0)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", 0);
                }
                else if (normalizedBeat > 1f)
                {
                    logoRevealed = true;
                }
            }
            if (songPosBeat - 1 >= targetBopBeat)
            {
                if (targetBopBeat <= 3 && loops == 0)
                {
                    starAnims[(int)targetBopBeat].Play("StarAppearBop", 0, 0);
                    if (targetBopBeat == 3) starAnims[4].Play("StarAppearBop", 0, 0);
                    for (int i = 0; i < (int)targetBopBeat; i++)
                    {
                        starAnims[i].Play("StarBopNoRot", 0, 0);
                    }
                }
                else
                {
                    foreach (var star in starAnims)
                    {
                        star.Play("StarBop", 0, 0);
                    }
                }
                if (targetBopBeat > 3 || loops > 0)
                {
                    logoAnim.Play(altBop ? "LogoBop2" : "LogoBop", 0, 0);
                    altBop = !altBop;
                }
                targetBopBeat += 1;
                // if ((!settingsPanel.IsOpen) && logoRevealed && logoHoverCollider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                // {
                //     SoundByte.PlayOneShot("metronome");
                // }
            }
        }

        public double SecsToBeats(double s)
        {
            return s / 60f * bpm;
        }

        public double BeatsToSecs(double beats, float bpm)
        {
            return beats / bpm * 60f;
        }

        public float GetPositionFromBeat(float startBeat, float length)
        {
            float a = Mathp.Normalize((float)songPosBeat, startBeat, startBeat + length);
            return a;
        }

        public void CreatePressed()
        {
            GlobalGameManager.PlayOpenFile = null;
            GlobalGameManager.LoadScene("Editor");
            SoundByte.PlayOneShot("ui/UIEnter");
        }

        public void PlayPressed()
        {
            SoundByte.PlayOneShot("ui/UISelect");
            // temp: open file browser then go into quickplay
            OpenQuickplayFileDialog();
            // go into the play mode menu
        }

        void OpenQuickplayFileDialog()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Remix File ", new string[] { "riq" }),
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Open Remix", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty)
                {
                    SoundByte.PlayOneShot("ui/UICancel");
                    return;
                }

                try
                {
                    RiqFileHandler.UnlockCache();
                    string tmpDir = RiqFileHandler.ExtractRiq(path);
                    Debug.Log("Imported RIQ successfully!");
                    RiqBeatmap beatmap = RiqFileHandler.ReadRiq();
                    GlobalGameManager.PlayOpenFile = path;
                    chartTitleText.text = beatmap["remixtitle"];
                    chartMapperText.text = beatmap["remixauthor"];
                    chartDescText.text = beatmap["remixdesc"];

                    playPanel.SetActive(true);
                    SoundByte.PlayOneShot("ui/UISelect");
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error importing RIQ: {e.Message}");
                    Debug.LogException(e);
                    GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                    return;
                }
            });
        }

        public void PlayPanelAccept()
        {
            SoundByte.PlayOneShot("ui/UIEnter");
            GlobalGameManager.LoadScene("Game", 0.35f, -1);
        }

        public void PlayPanelBack()
        {
            SoundByte.PlayOneShot("ui/UICancel");
            playPanel.SetActive(false);
        }

        public void SocialsPressed()
        {
            snsRevealed = true;
            snsVersionText.text = GlobalGameManager.buildTime;
            snsPanel.SetActive(true);
            SoundByte.PlayOneShot("ui/UISelect");
            // show a panel with our SNS links
        }

        public void SocialsClose()
        {
            snsRevealed = false;
            snsPanel.SetActive(false);
            SoundByte.PlayOneShot("ui/UICancel");
        }

        public void SettingsPressed()
        {
            settingsPanel.SwitchSettingsDialog();
            SoundByte.PlayOneShot("ui/UISelect");
            // notes:
            //  gameplay settings currently don't work due to the overlay pereview requiring the screen composition setup from a gameplay prefab
            //  adding the attract screen will fix this since we'd need to add that prefab for it anyways
        }

        public void QuitPressed()
        {
            SoundByte.PlayOneShot("ui/PauseQuit");
            Application.Quit();
        }
    }
}

