using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class RemixPropertiesDialog : Dialog
    {
        [Header("General References")]
        [SerializeField] TabsManager tabsManager;

        [Header("Containers")]
        [SerializeField] ChartInfoProperties[] containers;

        public DynamicBeatmap chart;

        private void Start() { }

        public void SwitchPropertiesDialog()
        {
            if (dialog.activeSelf)
            {
                tabsManager.CloseContent();
                Editor.instance.canSelect = true;
                Editor.instance.inAuthorativeMenu = false;
                dialog.SetActive(false);
            }
            else
            {
                ResetAllDialogs();

                foreach (var container in containers)
                {
                    container.Init(this);
                }

                tabsManager.OpenContent();
                Editor.instance.canSelect = false;
                Editor.instance.inAuthorativeMenu = true;
                dialog.SetActive(true);

                chart = GameManager.instance.Beatmap;
                chart["propertiesmodified"] = true;
            }
        }

        public void SetupDialog(PropertyTag[] tags, ChartInfoProperties container)
        {
            chart = GameManager.instance.Beatmap;
            chart["propertiesmodified"] = true;

            foreach (PropertyTag property in tags)
            {
                if (chart.properties.ContainsKey(property.tag))
                {
                    container.AddParam(this, property.tag, chart.properties[property.tag], property.label, property.isReadOnly);
                }
                else
                {
                    if (property.tag == "divider")
                    {
                        container.AddDivider(this);
                    }
                    else if (property.tag == "header")
                    {
                        container.AddHeader(this, property.label);
                    }
                    else if (property.tag == "subheader")
                    {
                        container.AddSubHeader(this, property.label);
                    }
                    else
                    {
                        Debug.LogWarning("Property Menu generation Warning: Property " + property.tag + " not found, skipping...");
                    }
                }
            }
        }

        private void CleanDialog() {}

        private void Update() {}

        [Serializable]
        public class PropertyTag
        {
            public string tag;
            public string label;
            public bool isReadOnly;
        }
    }
}