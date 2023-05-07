using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Games;

namespace HeavenStudio.Common
{
    public class SectionMedalsManager : MonoBehaviour
    {
        public static SectionMedalsManager instance { get; private set; }

        [SerializeField] GameObject MedalsHolder;
        [SerializeField] GameObject MedalOkPrefab;
        [SerializeField] GameObject MedalMissPrefab;

        Conductor cond;
        bool isMedalsStarted = false;
        bool isMedalsEligible = true;

        // Start is called before the first frame update
        public void Start()
        {
            cond = Conductor.instance;
            GameManager.instance.onSectionChange += OnSectionChange;
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AnchorToOverlay(GameObject overlay)
        {
            transform.position = overlay.transform.position;
            transform.rotation = overlay.transform.rotation;
            transform.localScale = overlay.transform.localScale;
        }

        public void MakeIneligible()
        {
            isMedalsEligible = false;
        }

        public void Reset()
        {
            isMedalsStarted = false;
            isMedalsEligible = true;
            foreach (Transform child in MedalsHolder?.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void OnSectionChange(DynamicBeatmap.ChartSection section)
        {
            if (section == null) return;
            if (!PersistentDataManager.gameSettings.isMedalOn) return;
            if (PersistentDataManager.gameSettings.isMedalOn && !isMedalsStarted)
            {
                isMedalsStarted = true;
                isMedalsEligible = true;
            }
            else
            {
                GameObject medal = Instantiate(isMedalsEligible ? MedalOkPrefab : MedalMissPrefab, MedalsHolder.transform);
                medal.SetActive(true);
                isMedalsEligible = true;
            }
        }

        public void OnRemixEnd()
        {
            if (!PersistentDataManager.gameSettings.isMedalOn) return;
            if (PersistentDataManager.gameSettings.isMedalOn && isMedalsStarted)
            {
                GameObject medal = Instantiate(isMedalsEligible ? MedalOkPrefab : MedalMissPrefab, MedalsHolder.transform);
                medal.SetActive(true);
            }
        }
    }
}