using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using System.Linq;

namespace HeavenStudio.Games.Global
{
    public class Flash : MonoBehaviour
    {
        [NonSerialized] public float startBeat;
        [NonSerialized] public float length;

        [NonSerialized] public Color startColor;
        [NonSerialized] public Color endColor;

        [NonSerialized] public EasingFunction.Ease ease;
        [NonSerialized] private EasingFunction.Function func;

        [NonSerialized] private Image spriteRenderer;

        [SerializeField] private Color currentCol;

        [NonSerialized] private List<DynamicBeatmap.DynamicEntity> allFadeEvents = new List<DynamicBeatmap.DynamicEntity>();

        private void Awake()
        {
            spriteRenderer = GetComponent<Image>();
            spriteRenderer.color = currentCol;
            func = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(float beat)
        {
            allFadeEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "flash" });
            // backwards-compatibility baybee
            allFadeEvents.AddRange(EventCaller.GetAllInGameManagerList("gameManager", new string[] { "flash" }));
            allFadeEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            FindFadeFromBeat(beat);
        }

        private void FindFadeFromBeat(float beat)
        {
            Color startCol = Color.white;
            Color endCol = Color.white;

            bool override_ = false;

            if (allFadeEvents.Count > 0)
            {
                DynamicBeatmap.DynamicEntity startEntity = null;

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
                        Color colA = startEntity["colorA"];
                        Color colB = startEntity["colorB"];

                        startCol = new Color(colA.r, colA.g, colA.b, startEntity["valA"]);
                        endCol = new Color(colB.r, colB.g, colB.b, startEntity["valB"]);
                    }

                    SetFade(startEntity.beat, startEntity.length, startCol, endCol, (EasingFunction.Ease) startEntity["ease"]);
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
            FindFadeFromBeat(Conductor.instance.songPositionInBeats);
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, length);
            // normalizedBeat = Mathf.Clamp01(normalizedBeat);

            currentCol = new Color(func(startColor.r, endColor.r, normalizedBeat), func(startColor.g, endColor.g, normalizedBeat), func(startColor.b, endColor.b, normalizedBeat), func(startColor.a, endColor.a, normalizedBeat));
            spriteRenderer.color = currentCol;
        }
    }
}