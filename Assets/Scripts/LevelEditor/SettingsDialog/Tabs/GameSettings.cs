using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class GameSettings : TabsContent
    {
        [SerializeField] Toggle perfectChallengeToggle;
        [SerializeField] Toggle sectionMedalsToggle;
        [SerializeField] Toggle timingDispMinModeToggle;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPerfectChallengeToggleChanged()
        {
            PersistentDataManager.gameSettings.perfectChallengeType = perfectChallengeToggle.isOn ? PersistentDataManager.PerfectChallengeType.On : PersistentDataManager.PerfectChallengeType.Off;
        }

        public void OnSectionMedalsToggleChanged()
        {
            PersistentDataManager.gameSettings.isMedalOn = sectionMedalsToggle.isOn;
        }

        public void OnTimingDispMinModeToggleChanged()
        {
            PersistentDataManager.gameSettings.timingDisplayMinMode = timingDispMinModeToggle.isOn;
        }

        public override void OnOpenTab()
        {
            perfectChallengeToggle.isOn = PersistentDataManager.gameSettings.perfectChallengeType > 0;
            sectionMedalsToggle.isOn = PersistentDataManager.gameSettings.isMedalOn;
            timingDispMinModeToggle.isOn = PersistentDataManager.gameSettings.timingDisplayMinMode;
        }

        public override void OnCloseTab()
        {
        }
    }
}