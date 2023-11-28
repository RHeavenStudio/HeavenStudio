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

        int btPause, btUp, btDown, btConfirm;

        void Pause()
        {
            if (GlobalGameManager.IsShowingDialog) return;
            if (!Conductor.instance.isPlaying) return;
            GameManager.instance.CircleCursor.LockCursor(true);
            Conductor.instance.Pause();
            pauseBeat = Conductor.instance.songPositionInBeatsAsDouble;
            chartTitleText.text = GameManager.instance.Beatmap["remixtitle"].ToString();
            chartArtistText.text = GameManager.instance.Beatmap["remixauthor"].ToString();
            animator.Play("PauseShow");
            SoundByte.PlayOneShot("ui/PauseIn");

            isPaused = true;
            canPick = false;
            optionSelected = 0;
            ChooseOption((Options)optionSelected, false);
        }

        void UnPause(bool instant = false)
        {
            if ((!instant) && (!Conductor.instance.isPaused)) return;
            // GameManager.instance.CircleCursor.LockCursor(true);
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
            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    btPause = (int)InputController.ActionsTouch.Pause;
                    btConfirm = (int)InputController.ActionsTouch.Tap;
                    btUp = -1;
                    btDown = -1;
                    break;
                case InputController.ControlStyles.Baton:
                    btPause = (int)InputController.ActionsBaton.Pause;
                    btUp = (int)InputController.ActionsBaton.Up;
                    btDown = (int)InputController.ActionsBaton.Down;
                    btConfirm = (int)InputController.ActionsBaton.Face;
                    break;
                default:
                    btPause = (int)InputController.ActionsPad.Pause;
                    btUp = (int)InputController.ActionsPad.Up;
                    btDown = (int)InputController.ActionsPad.Down;
                    btConfirm = (int)InputController.ActionsPad.East;
                    break;
            }

            if (isQuitting) return;

            if (PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btPause, out _) && !settingsDialog.IsOpen)
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
                if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                {
                    foreach (Transform t in optionHolder.transform)
                    {
                        if (t.GetComponent<Collider2D>().OverlapPoint(PlayerInput.GetInputController(1).GetPointer()))
                        {
                            int idx = t.GetSiblingIndex();
                            ChooseOption((Options)idx, idx != optionSelected);
                            optionSelected = idx;
                            break;
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btUp, out _))
                {
                    optionSelected--;
                    if (optionSelected < 0)
                    {
                        optionSelected = optionHolder.transform.childCount - 1;
                    }
                    ChooseOption((Options)optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btDown, out _))
                {
                    optionSelected++;
                    if (optionSelected > optionHolder.transform.childCount - 1)
                    {
                        optionSelected = 0;
                    }
                    ChooseOption((Options)optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btConfirm, out _))
                {
                    if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                    {
                        if (optionHolder.transform.GetChild(optionSelected).GetComponent<Collider2D>().OverlapPoint(PlayerInput.GetInputController(1).GetPointer()))
                        {
                            UseOption((Options)optionSelected);
                        }
                    }
                    else
                    {
                        UseOption((Options)optionSelected);
                    }
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
            ChooseOption((Options)optionSelected, false);
            canPick = true;
        }

        public void ChooseOption(Options option, bool sound = true)
        {
            optionArrow.transform.position = new Vector3(optionArrow.transform.position.x, optionHolder.transform.GetChild((int)option).position.y, optionArrow.transform.position.z);
            foreach (Transform child in optionHolder.transform)
            {
                child.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            optionHolder.transform.GetChild((int)option).transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
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
            GlobalGameManager.ForceFade(0, 0f, -1f);
            GameManager.instance.Stop(0, true, 1f);
            SoundByte.PlayOneShot("ui/UIEnter");
        }

        void OnQuit()
        {
            isQuitting = true;
            SoundByte.PlayOneShot("ui/PauseQuit");
            GameManager.instance.CircleCursor.LockCursor(false);
            GlobalGameManager.LoadScene("Title", 0, 0.35f);
        }

        void OnSettings()
        {
            GameManager.instance.CircleCursor.LockCursor(false);
            settingsDialog.SwitchSettingsDialog();
        }
    }
}