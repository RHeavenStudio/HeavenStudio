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

        private List<Beatmap.Entity> showEvents = new List<Beatmap.Entity>();   //shows / hides textbox 
        private List<Beatmap.Entity> positionEvents = new List<Beatmap.Entity>();
        Textbox instance;

        [Header("Objects")]
        public TMP_Text Label;
        public SpriteRenderer UL;
        public SpriteRenderer UR;
        public SpriteRenderer DL;
        public SpriteRenderer DR;

        public float startBeat;
        public float length;
        public float TextboxWidth = 1f;
        public float TextboxHeight = 1f;

        private void Awake()
        {
            instance = this;
        }

        public void OnBeatChanged(float beat)
        {
            showEvents = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "display textbox" });
            positionEvents = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "position textbox" });
        }
    }
}
