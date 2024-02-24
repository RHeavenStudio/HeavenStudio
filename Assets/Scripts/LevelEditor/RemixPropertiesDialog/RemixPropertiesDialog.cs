using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeavenStudio.Editor.Track;
using Jukebox;
using Jukebox.Legacy;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class RemixPropertiesDialog : Dialog
    {
        [Header("General References")]
        [SerializeField] TabsManager tabsManager;
        [SerializeField] Sprite returnIcon;
        [SerializeField] Color returnColor;
        [SerializeField] Sprite saveIcon;
        [SerializeField] Color saveColor;
        [SerializeField] Image returnButtonImage;
        [SerializeField] GameObject onSaveButton;

        [Header("Containers")]
        [SerializeField] ChartInfoProperties[] containers;

        [Header("Tabs")]
        [SerializeField] private TabsManager.TabsEntry[] tabs;

        [Header("Property Prefabs")]
        [SerializeField] public GameObject IntegerP;
        [SerializeField] public GameObject FloatP;
        [SerializeField] public GameObject BooleanP;
        [SerializeField] public GameObject DropdownP;
        [SerializeField] public GameObject ColorP;
        [SerializeField] public GameObject StringP;
        [SerializeField] public GameObject ImageP;

        [Header("Layout Prefabs")]
        [SerializeField] public GameObject DividerP;
        [SerializeField] public GameObject HeaderP;
        [SerializeField] public GameObject SubHeaderP;
        [SerializeField] public GameObject ResultDialogP;

        [NonSerialized] public RiqBeatmap chart;
        List<GameObject> tabContents;
        bool saveAfterClose = false, saveAs = false;

        private void Start() { }

        public void SwitchPropertiesDialog()
        {
            if (dialog.activeSelf)
            {
                Editor.instance.canSelect = true;
                Editor.instance.inAuthorativeMenu = false;
                Editor.instance.editingInputField = false;
                dialog.SetActive(false);

                tabsManager.CleanTabs();
                tabContents = null;
            }
            else
            {
                ResetAllDialogs();
                Editor.instance.canSelect = false;
                Editor.instance.inAuthorativeMenu = true;
                Editor.instance.editingInputField = true;
                dialog.SetActive(true);

                chart = GameManager.instance.Beatmap;
                chart["propertiesmodified"] = true;
                SetSaveOnClose(false);

                tabContents = tabsManager.GenerateTabs(tabs);
                foreach (var tab in tabContents)
                {
                    tab.GetComponent<ChartInfoProperties>().Init(this);
                }
            }
        }

        public void CloseAndSave()
        {
            if (saveAfterClose)
            {
                Editor.instance.SaveRemix(saveAs);
            }
            SwitchPropertiesDialog();
        }

        public void SetSaveOnClose(bool saveAfterClose, bool saveAs = false)
        {
            this.saveAfterClose = saveAfterClose;
            this.saveAs = saveAs;

            onSaveButton.SetActive(saveAfterClose);
        }

        public void SetupDialog(PropertyTag[] tags, ChartInfoProperties container)
        {
            chart = GameManager.instance.Beatmap;
            chart["propertiesmodified"] = true;

            foreach (PropertyTag property in tags)
            {
                if (chart.data.properties.ContainsKey(property.tag))
                {
                    container.AddParam(this, property.tag, chart.data.properties[property.tag], property.label, property.isReadOnly);
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
                    else if (property.tag == "resultmessagediag")
                    {
                        container.AddResultMessageEditor(this);
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