using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace HeavenStudio.Common
{
    public class OverlaysManager : MonoBehaviour
    {
        public static OverlaysManager instance { get; private set; }
        public static bool OverlaysEnabled;

        const float WIDTH_SPAN = 10f;
        const float HEIGHT_SPAN = 10f * (9f / 16f);

        [Header("Prefabs")]
        [SerializeField] GameObject TimingDisplayPrefab;
        [SerializeField] GameObject SkillStarPrefab;
        [SerializeField] GameObject ChartSectionPrefab;

        [Header("Components")]
        [SerializeField] Transform ComponentHolder;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            RefreshOverlaysLayout();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void TogleOverlaysVisibility(bool visible)
        {
            OverlaysEnabled = visible;
            StaticCamera.instance.ToggleOverlayView(visible);
        }

        public void RefreshOverlaysLayout()
        {
            List<OverlaysManager.OverlayOption> lytElements = new List<OverlaysManager.OverlayOption>();
            foreach (var c in PersistentDataManager.gameSettings.timingDisplayComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.skillStarComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.sectionComponents) { lytElements.Add(c); }

            foreach (Transform child in ComponentHolder.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var c in lytElements)
            {
                if (c is TimingDisplayComponent) { 
                    Debug.Log("TimingDisplayComponent");
                    c.CreateElement(TimingDisplayPrefab, ComponentHolder); 
                }
                else if (c is SkillStarComponent) { 
                    Debug.Log("SkillStarComponent");
                    c.CreateElement(SkillStarPrefab, ComponentHolder); 
                }
                else if (c is SectionComponent) { 
                    Debug.Log("SectionComponent");
                    c.CreateElement(ChartSectionPrefab, ComponentHolder); 
                }
                c.PositionElement();
            }
        }

        
        [Serializable]
        public class TimingDisplayComponent : OverlayOption
        {
            public enum TimingDisplayType
            {
                Dual,
                Single,
            }

            [NonSerialized] GameObject go2;
            [SerializeField] public TimingDisplayType tdType;

            public TimingDisplayComponent(TimingDisplayType type, bool enable, Vector2 position, float scale, float rotation)
            {
                tdType = type;
                this.enable = enable;
                this.position = position;
                this.scale = scale;
                this.rotation = rotation;
            }

            public override void CreateElement(GameObject prefab, Transform holder)
            {
                go = Instantiate(prefab, holder);
                go2 = Instantiate(prefab, holder);
            }

            public override void PositionElement()
            {
                if (go != null)
                {
                    switch (tdType)
                    {
                        case TimingDisplayType.Dual:
                            go.transform.localPosition = position * new Vector2(WIDTH_SPAN, HEIGHT_SPAN) * new Vector2(-1, 1);
                            go.transform.localScale = Vector3.one * scale;
                            go.transform.localRotation = Quaternion.Euler(0, 0, -rotation);

                            go2.transform.localPosition = position * new Vector2(WIDTH_SPAN, HEIGHT_SPAN);
                            go2.transform.localScale = Vector3.one * scale;
                            go2.transform.localRotation = Quaternion.Euler(0, 0, rotation);

                            go.SetActive(enable);
                            go2.SetActive(enable);
                            break;
                        case TimingDisplayType.Single:
                            go.transform.localPosition = position * new Vector2(WIDTH_SPAN, HEIGHT_SPAN);
                            go.transform.localScale = Vector3.one * scale;
                            go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                            go.SetActive(enable);
                            break;
                    }
                }
            } 

            public static TimingDisplayComponent CreateDefaultDual()
            {
                return new TimingDisplayComponent(TimingDisplayType.Dual, true, new Vector2(-0.84f, 0), 1f, 0f);
            }

            public static TimingDisplayComponent CreateDefaultSingle()
            {
                return new TimingDisplayComponent(TimingDisplayType.Single, true, new Vector2(0, -0.8f), 1f, 90f);
            }
        }

        [Serializable]
        public class SkillStarComponent : OverlayOption
        {
            public SkillStarComponent(bool enable, Vector2 position, float scale, float rotation)
            {
                this.enable = enable;
                this.position = position;
                this.scale = scale;
                this.rotation = rotation;
            }

            public override void PositionElement()
            {
                if (go != null)
                {
                    go.transform.localPosition = position * new Vector2(WIDTH_SPAN, HEIGHT_SPAN);
                    go.transform.localScale = Vector3.one * scale;
                    go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                    go.SetActive(enable);
                }
            }

            public static SkillStarComponent CreateDefault()
            {
                return new SkillStarComponent(true, new Vector2(0.75f, -0.7f), 1f, 0f);
            }
        }

        [Serializable]
        public class SectionComponent : OverlayOption
        {
            public SectionComponent(bool enable, Vector2 position, float scale, float rotation)
            {
                this.enable = enable;
                this.position = position;
                this.scale = scale;
                this.rotation = rotation;
            }

            public override void PositionElement()
            {
                if (go != null)
                {
                    go.transform.localPosition = position * new Vector2(WIDTH_SPAN, HEIGHT_SPAN);
                    go.transform.localScale = Vector3.one * scale;
                    go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                    go.SetActive(enable);
                }
            }

            public static SectionComponent CreateDefault()
            {
                return new SectionComponent(true, new Vector2(0.7f, 0.765f), 1f, 0f);
            }
        }

        [Serializable]
        public abstract class OverlayOption
        {
            static long uuidCounter = 0;
            [NonSerialized] protected GameObject go;
            [NonSerialized] public long uuid = GenerateUUID();
            [SerializeField] public bool enable;
            [SerializeField] public Vector2 position;
            [SerializeField] public float scale;
            [SerializeField] public float rotation;

            static long GenerateUUID()
            {
                return uuidCounter++;
            }

            public virtual void CreateElement(GameObject prefab, Transform holder)
            {
                go = Instantiate(prefab, holder);
            }

            public abstract void PositionElement();
        }
    }
}