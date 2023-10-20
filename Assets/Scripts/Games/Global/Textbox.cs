using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using HeavenStudio.TextboxUtilities;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Games.Global
{
    public class Textbox : MonoBehaviour
    {
        public enum TextboxAnchor {
            TopLeft,
            TopMiddle,
            TopRight,
            Left,
            Middle,
            Right,
            BottomLeft,
            BottomMiddle,
            BottomRight
        }

        public enum ClosedCaptionsAnchor {
            Top,
            Bottom,
        }

        private List<RiqEntity> textboxEvents = new List<RiqEntity>();
        private List<RiqEntity> openCaptionsEvents = new List<RiqEntity>();
        private List<RiqEntity> idolEvents = new List<RiqEntity>();
        private List<RiqEntity> closedCaptionsEvents = new List<RiqEntity>();

        public static Textbox instance { get; private set; }

        [Header("Objects")]
        public GameObject TextboxEnabler;
        public TextboxObject TextboxObject;

        public GameObject OpenCaptionsEnabler;
        public TMP_Text OpenCaptionsLabel;
        public RectTransform OpenCaptionsLabelRect;

        public GameObject IdolEnabler;
        public Animator IdolAnimator;
        public TMP_Text IdolSongLabel;
        public TMP_Text IdolArtistLabel;

        public GameObject ClosedCaptionsEnabler;
        public TMP_Text ClosedCaptionsLabel;
        public RectTransform ClosedCaptionsLabelRect;
        public RectTransform ClosedCaptionsBgRect;

        float XAnchor = 1.5f;
        float YAnchor = 1.75f;

        Vector2 textboxSize = new Vector2(3f, 0.75f);

        bool idolShown = false;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
            TextboxEnabler.SetActive(false);
            OpenCaptionsEnabler.SetActive(false);
            ClosedCaptionsEnabler.SetActive(false);
            UpdateTextboxDisplay();
            UpdateOpenCaptionsDisplay();
            UpdateClosedCaptionsDisplay();
        }

        public void Update()
        {
            UpdateTextboxDisplay();
            UpdateOpenCaptionsDisplay();
            UpdateIdolDisplay();
            UpdateClosedCaptionsDisplay();
        }

        public void OnBeatChanged(double beat)
        {
            TextboxEnabler.SetActive(false);
            OpenCaptionsEnabler.SetActive(false);
            ClosedCaptionsEnabler.SetActive(false);

            textboxEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "display textbox" });
            openCaptionsEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "display open captions" });
            idolEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "display song artist" });
            closedCaptionsEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "display closed captions" });

            UpdateTextboxDisplay();
            UpdateOpenCaptionsDisplay();
            UpdateClosedCaptionsDisplay();

            UpdateIdolDisplay();
            if (!idolShown)
            {
                IdolAnimator.Play("NoPose", -1, 0);
                IdolAnimator.speed = 1;
            }
        }

        private void UpdateTextboxDisplay()
        {
            foreach (var e in textboxEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f && prog <= 1f)
                {
                    TextboxEnabler.SetActive(true);
                    TextboxObject.SetText(e["text1"]);
                    TextboxObject.Resize(e["valA"], e["valB"]);

                    // ouch
                    switch (e["type"])
                    {
                        case (int) TextboxAnchor.TopLeft:
                            TextboxEnabler.transform.localPosition = new Vector3(-XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopMiddle:
                            TextboxEnabler.transform.localPosition = new Vector3(0, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopRight:
                            TextboxEnabler.transform.localPosition = new Vector3(XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.Left:
                            TextboxEnabler.transform.localPosition = new Vector3(-XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.Middle:
                            TextboxEnabler.transform.localPosition = new Vector3(0, 0);
                            break;
                        case (int) TextboxAnchor.Right:
                            TextboxEnabler.transform.localPosition = new Vector3(XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.BottomLeft:
                            TextboxEnabler.transform.localPosition = new Vector3(-XAnchor, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomMiddle:
                            TextboxEnabler.transform.localPosition = new Vector3(0, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomRight:
                            TextboxEnabler.transform.localPosition = new Vector3(XAnchor, -YAnchor);
                            break;
                        default:
                            TextboxEnabler.transform.localPosition = new Vector3(0, 0);
                            break;
                    }

                    return;
                }
                if (prog > 1f || prog < 0f)
                {
                    TextboxEnabler.transform.localPosition = new Vector3(0, 0);
                    TextboxEnabler.SetActive(false);
                }
            }
        }

        private void UpdateOpenCaptionsDisplay()
        {
            foreach (var e in openCaptionsEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f && prog <= 1f)
                {
                    OpenCaptionsEnabler.SetActive(true);
                    OpenCaptionsLabel.text = e["text1"];

                    OpenCaptionsLabelRect.sizeDelta = new Vector2(18f * e["valA"], 2.5f * e["valB"]);

                    // ouch
                    switch (e["type"])
                    {
                        case (int) TextboxAnchor.TopLeft:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(-XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopMiddle:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(0, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopRight:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.Left:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(-XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.Middle:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(0, 0);
                            break;
                        case (int) TextboxAnchor.Right:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.BottomLeft:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(-XAnchor, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomMiddle:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(0, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomRight:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(XAnchor, -YAnchor);
                            break;
                        default:
                            OpenCaptionsEnabler.transform.localPosition = new Vector3(0, 0);
                            break;
                    }

                    return;
                }
                if (prog > 1f || prog < 0f)
                {
                    OpenCaptionsEnabler.transform.localPosition = new Vector3(0, 0);
                    OpenCaptionsEnabler.SetActive(false);
                }
            }
        }

        private void UpdateIdolDisplay()
        {
            var cond = Conductor.instance;
            foreach (var e in idolEvents)
            {
                float prog = cond.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f && prog <= 1f)
                {
                    float inp;
                    if (e["instantOn"])
                    {
                        inp = 1;
                    }
                    else
                    {
                        inp = cond.GetPositionFromBeat(e.beat, 1);
                    }
                    IdolSongLabel.text = e["text1"];
                    IdolArtistLabel.text = e["text2"];

                    IdolAnimator.Play("IdolShow", -1, Mathf.Min(inp, 1));
                    IdolAnimator.speed = 0;

                    idolShown = true;

                    return;
                }
                else if (idolShown)
                {
                    if (prog < 1f || e["instantOff"])
                    {
                        IdolAnimator.Play("NoPose", -1, 0);
                        IdolAnimator.speed = 1;
                        idolShown = false;
                    }
                    else
                    {
                        IdolAnimator.Play("IdolHide", -1, 0);
                        IdolAnimator.speed = (1f / cond.pitchedSecPerBeat) * 0.5f;
                        idolShown = false;
                    }
                }
            }
        }

        private void UpdateClosedCaptionsDisplay()
        {
            foreach (var e in closedCaptionsEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f && prog <= 1f)
                {
                    ClosedCaptionsEnabler.SetActive(true);
                    ClosedCaptionsLabel.text = e["text1"];

                    ClosedCaptionsLabelRect.sizeDelta = new Vector2(9f, e["valA"]);
                    ClosedCaptionsBgRect.sizeDelta = new Vector2(9f, e["valA"]);

                    switch (e["type"])
                    {
                        case (int) ClosedCaptionsAnchor.Bottom:
                            ClosedCaptionsEnabler.transform.localPosition = new Vector3(0, -2.5f + e["valA"]/2);
                            break;
                        default:
                            ClosedCaptionsEnabler.transform.localPosition = new Vector3(0, 2.5f - e["valA"]/2);
                            break;
                    }

                    return;
                }
                if (prog > 1f || prog < 0f)
                {
                    ClosedCaptionsEnabler.SetActive(false);
                }
            }
        }
    }
}
