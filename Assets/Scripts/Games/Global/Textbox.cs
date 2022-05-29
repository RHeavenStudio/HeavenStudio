using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

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

        private List<Beatmap.Entity> showEvents = new List<Beatmap.Entity>();
        Textbox instance;

        [Header("Objects")]
        public GameObject Enabler;
        public TMP_Text Label;
        public SpriteRenderer UL;
        public SpriteRenderer UR;
        public SpriteRenderer DL;
        public SpriteRenderer DR;

        float TextboxWidth = 1f;
        float TextboxHeight = 1f;

        float XAnchor = 1.5f;
        float YAnchor = 1.75f;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
            Enabler.SetActive(false);
            UpdateTextboxDisplay();
        }

        public void Update()
        {
            UpdateTextboxDisplay();
        }

        public void OnBeatChanged(float beat)
        {
            Enabler.SetActive(false);

            showEvents = EventCaller.GetAllInGameManagerList("textbox", new string[] { "display textbox" });

            UpdateTextboxDisplay();
        }

        private void UpdateTextboxDisplay()
        {
            foreach (var e in showEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Debug.Log("showing textbox");
                    Enabler.SetActive(true);
                    Label.text = e.text1;

                    // ouch
                    switch (e.type)
                    {
                        case (int) TextboxAnchor.TopLeft:
                            Enabler.transform.localPosition = new Vector3(-XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopMiddle:
                            Enabler.transform.localPosition = new Vector3(0, YAnchor);
                            break;
                        case (int) TextboxAnchor.TopRight:
                            Enabler.transform.localPosition = new Vector3(XAnchor, YAnchor);
                            break;
                        case (int) TextboxAnchor.Left:
                            Enabler.transform.localPosition = new Vector3(-XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.Middle:
                            Enabler.transform.localPosition = new Vector3(0, 0);
                            break;
                        case (int) TextboxAnchor.Right:
                            Enabler.transform.localPosition = new Vector3(XAnchor, 0);
                            break;
                        case (int) TextboxAnchor.BottomLeft:
                            Enabler.transform.localPosition = new Vector3(-XAnchor, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomMiddle:
                            Enabler.transform.localPosition = new Vector3(0, -YAnchor);
                            break;
                        case (int) TextboxAnchor.BottomRight:
                            Enabler.transform.localPosition = new Vector3(XAnchor, -YAnchor);
                            break;
                        default:
                            Enabler.transform.localPosition = new Vector3(0, 0);
                            break;
                    }
                }
                if (prog > 1f || prog < 0f)
                {
                    Enabler.transform.localPosition = new Vector3(0, 0);
                    Enabler.SetActive(false);
                }
            }
        }
    }
}
