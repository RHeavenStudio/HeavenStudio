using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

namespace HeavenStudio.Common
{
    public class ChartSectionDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text SectionText;
        [SerializeField] private Slider SectionProgress;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(GameManager.instance);
            GameManager.instance.onSectionChange += OnSectionChange;
            GameManager.instance.onBeatChanged += OnBeatChanged;
            gameObject.SetActive(GameManager.instance.currentSection != null);
        }

        // Update is called once per frame
        void Update()
        {
            SectionProgress.value = GameManager.instance.sectionProgress;
        }

        public void OnBeatChanged(float beat)
        {
            gameObject.SetActive(GameManager.instance.currentSection != null);
            SectionProgress.value = GameManager.instance.sectionProgress;
        }

        public void OnSectionChange(DynamicBeatmap.ChartSection section)
        {
            if (section != null)
            {
                gameObject.SetActive(true);
                SectionText.text = section.sectionName;
                SectionProgress.value = GameManager.instance.sectionProgress;

                if (section.startPerfect && GoForAPerfect.instance != null && GoForAPerfect.instance.perfect && !GoForAPerfect.instance.gameObject.activeSelf)
                {
                    GoForAPerfect.instance.Enable();
                }
            }
        }
    }
}