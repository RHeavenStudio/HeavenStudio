using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
using TMPro;

using Starpelly;

using HeavenStudio.Common;
using HeavenStudio.Editor.Track;
using System;

namespace HeavenStudio.Editor
{
    public class EditorTheme : MonoBehaviour
    {
        public TextAsset ThemeTXT;
        public static Theme theme;

        [Header("Components")]
        [SerializeField] private Image layer;
        [SerializeField] private Image nodeLayer;
        [SerializeField] private RectTransform LayersHolder, NodeLayersHolder;
        [SerializeField] private Image specialLayers;
        [SerializeField] private Image tempoLayer;
        [SerializeField] private Image musicLayer;
        [SerializeField] private Image sectionLayer;

        public static Gradient LayersGradient;

        private void Awake()
        {
            /*
            if (File.Exists(Application.persistentDataPath + "/editorTheme.json"))
            {
                string json = File.ReadAllText(Application.persistentDataPath + "/editorTheme.json");
                theme = JsonConvert.DeserializeObject<Theme>(json);
            }
            else
            {
                PersistentDataManager.SaveTheme(ThemeTXT.text);
                theme = JsonConvert.DeserializeObject<Theme>(ThemeTXT.text);
            }
            */

            PersistentDataManager.SaveTheme(ThemeTXT.text);
            theme = JsonConvert.DeserializeObject<Theme>(ThemeTXT.text);
        }

        private void Start()
        {
            if (Editor.instance == null) return;
            specialLayers.GetComponent<Image>().color = theme.properties.SpecialLayersCol.Hex2RGB();
            tempoLayer.GetComponent<Image>().color = theme.properties.TempoLayerCol.Hex2RGB();
            musicLayer.GetComponent<Image>().color = theme.properties.MusicLayerCol.Hex2RGB();
            sectionLayer.GetComponent<Image>().color = theme.properties.SectionLayerCol.Hex2RGB();
            Tooltip.AddTooltip(specialLayers.gameObject, $"All Special Tracks");
            Tooltip.AddTooltip(tempoLayer.gameObject, $"Tempo Track");
            Tooltip.AddTooltip(musicLayer.gameObject, $"Music Volume Track");
            Tooltip.AddTooltip(sectionLayer.gameObject, $"Remix Sections Track");

            LayersGradient = new UnityEngine.Gradient();
            var colorKeys = new List<GradientColorKey>();

            for (int i = 0; i < EditorTheme.theme.properties.LayerColors.Count; i++)
                colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.LayerColors[i].Hex2RGB(),
                    i / (float)EditorTheme.theme.properties.LayerColors.Count));

            LayersGradient.colorKeys = colorKeys.ToArray();

            layer.gameObject.SetActive(false);
            nodeLayer.gameObject.SetActive(false);

            for (int i = 0; i < Timeline.instance.LayerCount; i++)
            {
                GameObject layer = Instantiate(this.layer.gameObject, this.layer.transform.parent);
                layer.SetActive(true);
                layer.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Track {i + 1}";

                Color c = TrackToThemeColour(i);

                layer.GetComponent<Image>().color = c;
                Tooltip.AddTooltip(layer, $"Track {i + 1}");
            }

            var nodeNames = Enum.GetNames(typeof(NodeType));
            for (int i = 0; i < nodeNames.Length; i++)
            {
                CreateNodeLayer(nodeNames[i].Replace("_", " "), i);
            }

            Destroy(layer);
        }


        private void Update()
        {
            for (int i = 1; i < nodeLayer.transform.parent.childCount; i++)
            {
                var ai = i - 1;
                var button = nodeLayer.transform.parent.GetChild(i).GetComponent<Button>();
                if (ai == Editor.instance.currentNodeLayer)
                {
                    var bc = button.colors;
                    var c = LayersGradient.Evaluate(ai / (float)nodeLayer.transform.parent.childCount);
                    bc.normalColor = c;
                    bc.disabledColor = c;
                    button.colors = bc;

                    button.interactable = false;
                }
                else
                {
                    var bc = button.colors;
                    bc.normalColor = "171717".Hex2RGB();
                    button.colors = bc;

                    button.interactable = true;
                }
            }

            if (Editor.instance.currentTimeline == 1)
            {
                LayersHolder.gameObject.SetActive(false);
                NodeLayersHolder.gameObject.SetActive(true);
            }
            else
            {
                LayersHolder.gameObject.SetActive(true);
                NodeLayersHolder.gameObject.SetActive(false);
            }
        }

        private void CreateNodeLayer(string name, int index)
        {
            GameObject nodeLayer = Instantiate(this.nodeLayer.gameObject, this.nodeLayer.transform.parent);
            nodeLayer.SetActive(true);
            nodeLayer.transform.GetChild(0).GetComponent<TMP_Text>().text = name;


            nodeLayer.GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    Editor.instance.currentNodeLayer = index;
                });

            Tooltip.AddTooltip(nodeLayer, name);
        }


        public static string TrackToThemeColourStr(int track)
        {
            return TrackToThemeColour(track).Color2Hex();
        }

        public static Color TrackToThemeColour(int track)
        {
            return LayersGradient.Evaluate((float)track / Timeline.instance.LayerCount);
        }
    }

}