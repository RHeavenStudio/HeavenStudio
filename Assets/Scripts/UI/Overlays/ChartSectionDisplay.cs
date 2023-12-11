using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

using HeavenStudio.Editor;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Common
{
    public class ChartSectionDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text SectionText;
        [SerializeField] private Slider SectionProgress;

        // Start is called before the first frame update
        void Start()
        {
            GameManager.instance.onSectionChange += OnSectionChange;
            GameManager.instance.onBeatChanged += OnBeatChanged;
            gameObject.SetActive(GameManager.instance.lastSection != null);
        }

        // Update is called once per frame
        void Update()
        {
            SectionProgress.value = (float) GameManager.instance.SectionProgress;
        }

        public void OnBeatChanged(double beat)
        {
            gameObject.SetActive(GameManager.instance.lastSection != null);
            SectionProgress.value = (float) GameManager.instance.SectionProgress;
        }

        public void OnSectionChange(RiqEntity newSection, RiqEntity lastSection)
        {
            if (newSection != null)
            {
                gameObject.SetActive(true);
                SectionText.text = newSection["sectionName"];
                SectionProgress.value = (float) GameManager.instance.SectionProgress;
            }
        }
    }
}