using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System.Linq;

namespace HeavenStudio.Games.Global
{
    public class Flash : MonoBehaviour
    {
        public float startBeat;
        public float length;

        public Color startColor;
        public Color endColor;

        public EasingFunction.Ease ease;
        private EasingFunction.Function func;

        private SpriteRenderer spriteRenderer;

        [SerializeField] private Color currentCol;

        private List<Beatmap.Entity> allFadeEvents = new List<Beatmap.Entity>();

        private void Start()
        {
            this.gameObject.transform.SetParent(GameManager.instance.gameObject.transform);
            gameObject.layer = LayerMask.NameToLayer("Flash");
            this.gameObject.transform.localScale = new Vector3(1, 1);

            spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.color = startColor;
            spriteRenderer.sortingOrder = 30001;
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/GeneralPurpose/Square");

            func = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);

            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(float beat)
        {
            // I really need to create a class for objects that are constant like the Spaceball Camera
            // startColor = new Color(1, 1, 1, 0);
            // endColor = new Color(1, 1, 1, 0);

            allFadeEvents = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "flash" });
            Test(beat);
        }

        private void Test(float beat)
        {
            Color startCol = Color.white;
            Color endCol = Color.white;

            bool override_ = false;

            if (allFadeEvents.Count > 0)
            {
                Beatmap.Entity startEntity = null;

                for (int i = 0; i < allFadeEvents.Count; i++)
                {
                    if (allFadeEvents[i].beat <= beat)
                    {
                        startEntity = allFadeEvents[i];
                    }
                    else if (i == 0 && allFadeEvents[i].beat > beat)
                    {
                        startEntity = allFadeEvents[i];
                        override_ = true;
                        startCol = new Color(1, 1, 1, 0);
                        endCol = new Color(1, 1, 1, 0);
                    }
                }

                if (startEntity != null)
                {
                    if (!override_)
                    {
                        Color colA = startEntity.colorA;
                        Color colB = startEntity.colorB;

                        startCol = new Color(colA.r, colA.g, colA.b, startEntity.valA);
                        endCol = new Color(colB.r, colB.g, colB.b, startEntity.valB);
                    }

                    SetFade(startEntity.beat, startEntity.length, startCol, endCol, startEntity.ease);
                }
            }
        }

        public void SetFade(float beat, float length, Color startCol, Color endCol, EasingFunction.Ease ease)
        {
            this.startBeat = beat;
            this.length = length;
            this.startColor = startCol;
            this.endColor = endCol;
            this.ease = ease;
            func = EasingFunction.GetEasingFunction(ease);
        }

        private void Update()
        {
            Test(Conductor.instance.songPositionInBeats);
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, length);
            // normalizedBeat = Mathf.Clamp01(normalizedBeat);

            currentCol = new Color(func(startColor.r, endColor.r, normalizedBeat), func(startColor.g, endColor.g, normalizedBeat), func(startColor.b, endColor.b, normalizedBeat), func(startColor.a, endColor.a, normalizedBeat));
            spriteRenderer.color = currentCol;
        }
    }
}