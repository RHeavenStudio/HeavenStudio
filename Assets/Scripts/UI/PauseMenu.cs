using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Common
{
    public class PauseMenu : MonoBehaviour
    {
        public enum Options
        {
            Continue,
            StartOver,
            Settings,
            Quit
        }

        // TODO
        // MAKE OPTIONS ACCEPT MOUSE INPUT

        [SerializeField] float patternSpeed = 1f;
        [SerializeField] SettingsDialog settingsDialog;
        [SerializeField] Animator animator;
        [SerializeField] TMP_Text chartTitleText;
        [SerializeField] TMP_Text chartArtistText;
        [SerializeField] GameObject optionArrow;
        [SerializeField] GameObject optionHolder;

        [SerializeField] RectTransform patternL;
        [SerializeField] RectTransform patternR;
        
        public static bool IsPaused { get { return isPaused; } }

        private static bool isPaused = false;
        private double pauseBeat;
        private bool canPick = false;
        private bool isQuitting = false;
        private int optionSelected = 0;

        void Pause()
        {
            if (GlobalGameManager.IsShowingDialog) return;
            if (!Conductor.instance.isPlaying) return;
            Conductor.instance.Pause();
            pauseBeat = Conductor.instance.songPositionInBeatsAsDouble;
            chartTitleText.text = GameManager.instance.Beatmap["remixtitle"].ToString();
            chartArtistText.text = GameManager.instance.Beatmap["remixauthor"].ToString();
            animator.Play("PauseShow");
            SoundByte.PlayOneShot("ui/PauseIn");

            isPaused = true;
            canPick = false;
            optionSelected = 0;
        }

        void UnPause(bool instant = false)
        {
            if ((!instant) && (!Conductor.instance.isPaused)) return;
            Conductor.instance.Play(pauseBeat);
            if (instant)
            {
                animator.Play("NoPose");
            }
            else
            {
                animator.Play("PauseHide");
                SoundByte.PlayOneShot("ui/PauseOut");
            }
            
            isPaused = false;
            canPick = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            isPaused = false;
            isQuitting = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isQuitting) return;
            if (PlayerInput.GetInputController(1).GetActionDown((int) InputController.ActionsPad.Pause, out _))
            {
                if (isPaused)
                {
                    UnPause();
                }
                else
                {
                    Pause();
                }
            }
            else if (isPaused && canPick && !settingsDialog.IsOpen)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || PlayerInput.GetInputController(1).GetActionDown((int)InputController.ActionsPad.Up, out _))
                {
                    optionSelected--;
                    if (optionSelected < 0)
                    {
                        optionSelected = optionHolder.transform.childCount - 1;
                    }
                    ChooseOption((Options) optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || PlayerInput.GetInputController(1).GetActionDown((int)InputController.ActionsPad.Down, out _))
                {
                    optionSelected++;
                    if (optionSelected > optionHolder.transform.childCount - 1)
                    {
                        optionSelected = 0;
                    }
                    ChooseOption((Options) optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || PlayerInput.GetInputController(1).GetActionDown((int)InputController.ActionsPad.East, out _))
                {
                    UseOption((Options) optionSelected);
                }
            }

            if (isPaused)
            {
                patternL.anchoredPosition = new Vector2((Time.realtimeSinceStartup * patternSpeed) % 13, patternL.anchoredPosition.y);
                patternR.anchoredPosition = new Vector2(-(Time.realtimeSinceStartup * patternSpeed) % 13, patternR.anchoredPosition.y);
            }
        }

        public void ChooseCurrentOption()
        {
            ChooseOption((Options) optionSelected, false);
            canPick = true;
        }

        public void ChooseOption(Options option, bool sound = true)
        {
            optionArrow.transform.position = new Vector3(optionArrow.transform.position.x, optionHolder.transform.GetChild((int) option).position.y, optionArrow.transform.position.z);
            foreach (Transform child in optionHolder.transform)
            {
                child.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            optionHolder.transform.GetChild((int) option).transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            if (sound)
                SoundByte.PlayOneShot("ui/UIOption");
        }

        void UseOption(Options option)
        {
            switch (option)
            {
                case Options.Continue:
                    OnContinue();
                    break;
                case Options.StartOver:
                    OnRestart();
                    break;
                case Options.Settings:
                    OnSettings();
                    SoundByte.PlayOneShot("ui/UISelect");
                    break;
                case Options.Quit:
                    OnQuit();
                    break;
            }
        }

        void OnContinue()
        {
            UnPause();
        }

        void OnRestart()
        {
            UnPause(true);
            GlobalGameManager.ForceFade(0, 1f, 0.5f);
            GameManager.instance.Stop(0, true, 1.5f);
            SoundByte.PlayOneShot("ui/UIEnter");
        }

        void OnQuit()
        {
            isQuitting = true;
            SoundByte.PlayOneShot("ui/PauseQuit");
            GlobalGameManager.LoadScene("Editor", 0, 0.1f);
        }

        void OnSettings()
        {
            settingsDialog.SwitchSettingsDialog();
        }
    }
}