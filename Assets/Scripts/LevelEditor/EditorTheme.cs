using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
using TMPro;

using HeavenStudio.Common;
using HeavenStudio.Util;
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

        private void Awake()
        {
            var defaultTheme = JsonConvert.DeserializeObject<Theme>(ThemeTXT.text);
            if (File.Exists(Application.persistentDataPath + "/editorTheme.json"))
            {
                string json = File.ReadAllText(Application.persistentDataPath + "/editorTheme.json");
                if (json == "")
                {
                    PersistentDataManager.SaveTheme(ThemeTXT.text);
                    theme = defaultTheme;
                    theme.SetLayersGradient();
                    return;
                }
                try
                {
                    theme = JsonConvert.DeserializeObject<Theme>(json);

                    // Naive way of doing it? Possibly, but we should have a theme editor in the future.
                    if (defaultTheme.properties.LayerColors != null)
                    {
                        theme.properties.LayerColors = new string[]
                        {
                            theme.properties.Layer1Col,
                            theme.properties.Layer2Col,
                            theme.properties.Layer3Col,
                            theme.properties.Layer4Col,
                            theme.properties.Layer5Col
                        };
                        // Create a function for this in the future.
                        var savedTheme = JsonConvert.SerializeObject(theme, Formatting.Indented, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.None,
                            NullValueHandling = NullValueHandling.Include,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        });
                        PersistentDataManager.SaveTheme(savedTheme);
                    }
                }
                catch
                {
                    PersistentDataManager.SaveTheme(ThemeTXT.text);
                    theme = defaultTheme;
                    theme.SetLayersGradient();
                    return;
                }
            }
            else
            {
                PersistentDataManager.SaveTheme(ThemeTXT.text);
                theme = defaultTheme;
            }
            theme.SetLayersGradient();
        }

        private void Start()
        {
            if (Editor.instance == null) return;
            specialLayers.GetComponent<Image>().color = theme.properties.SpecialLayersCol.Hex2RGB();
            tempoLayer.GetComponent<Image>().color = theme.properties.TempoLayerCol.Hex2RGB();
            musicLayer.GetComponent<Image>().color = theme.properties.MusicLayerCol.Hex2RGB();
            sectionLayer.GetComponent<Image>().color = theme.properties.SectionLayerCol.Hex2RGB();


            layer.gameObject.SetActive(false);

            for (int i = 0; i < Timeline.instance.LayerCount; i++)
            {
                GameObject layer = Instantiate(this.layer.gameObject, this.layer.transform.parent);
                layer.SetActive(true);
                layer.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Track {i + 1}";

                Color c = theme.LayersGradient.Evaluate(i / (float)(Timeline.instance.LayerCount - 1));
                
                layer.GetComponent<Image>().color = c;
                Tooltip.AddTooltip(layer, $"Track {i + 1}");
            }

            Destroy(layer);
        }
    }

}