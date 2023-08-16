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
            gameObject.SetActive(GameManager.instance.currentSection != null);
        }

        // Update is called once per frame
        void Update()
        {
            SectionProgress.value = (float) GameManager.instance.sectionProgress;
        }

        public void OnBeatChanged(double beat)
        {
            gameObject.SetActive(GameManager.instance.currentSection != null);
            SectionProgress.value = (float) GameManager.instance.sectionProgress;
        }

        public void OnSectionChange(RiqEntity section)
        {
            if (section != null)
            {
                gameObject.SetActive(true);
                SectionText.text = section["sectionName"];
                SectionProgress.value = (float) GameManager.instance.sectionProgress;

                if (PersistentDataManager.gameSettings.perfectChallengeType == PersistentDataManager.PerfectChallengeType.Off) return;
                if (!OverlaysManager.OverlaysEnabled) return;
                if (section["startPerfect"] && GoForAPerfect.instance != null && GoForAPerfect.instance.perfect && !GoForAPerfect.instance.gameObject.activeSelf)
                {
                    GoForAPerfect.instance.Enable(section.beat);
                }
            }
        }
    }
}