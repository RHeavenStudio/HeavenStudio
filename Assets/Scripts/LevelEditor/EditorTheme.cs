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

namespace HeavenStudio.Editor
{
    public class EditorTheme : MonoBehaviour
    {
        public TextAsset ThemeTXT;
        public static Theme theme;

        [Header("Components")]
        [SerializeField] private Image layer;
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

            for (int i = 0; i < Timeline.instance.LayerCount; i++)
            {
                GameObject layer = Instantiate(this.layer.gameObject, this.layer.transform.parent);
                layer.SetActive(true);
                layer.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Track {i + 1}";

                Color c = TrackToThemeColour(i);

                layer.GetComponent<Image>().color = c;
                Tooltip.AddTooltip(layer, $"Track {i + 1}");
            }
            Destroy(layer);
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