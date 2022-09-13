using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
using TMPro;

using Starpelly;

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
            theme = JsonConvert.DeserializeObject<Theme>(ThemeTXT.text);
        }

        private void Start()
        {
            specialLayers.GetComponent<Image>().color = theme.properties.SpecialLayersCol.Hex2RGB();
            tempoLayer.GetComponent<Image>().color = theme.properties.TempoLayerCol.Hex2RGB();
            musicLayer.GetComponent<Image>().color = theme.properties.MusicLayerCol.Hex2RGB();
            sectionLayer.GetComponent<Image>().color = theme.properties.SectionLayerCol.Hex2RGB();
            Tooltip.AddTooltip(specialLayers.gameObject, $"All Special Tracks");
            Tooltip.AddTooltip(tempoLayer.gameObject, $"Tempo Track");
            Tooltip.AddTooltip(musicLayer.gameObject, $"Music Volume Track");
            Tooltip.AddTooltip(sectionLayer.gameObject, $"Remix Sections Track");


            layer.gameObject.SetActive(false);

            for (int i = 0; i < Timeline.instance.LayerCount; i++)
            {
                GameObject layer = Instantiate(this.layer.gameObject, this.layer.transform.parent);
                layer.SetActive(true);
                layer.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Track {i + 1}";

                Color c = Color.white;

                switch (i)
                {
                    case 0:
                        c = theme.properties.Layer1Col.Hex2RGB();
                        break;
                    case 1:
                        c = theme.properties.Layer2Col.Hex2RGB();
                        break;
                    case 2:
                        c = theme.properties.Layer3Col.Hex2RGB();
                        break;
                    case 3:
                        c = theme.properties.Layer4Col.Hex2RGB();
                        break;
                    case 4:
                        c = theme.properties.Layer5Col.Hex2RGB();
                        break;
                }

                layer.GetComponent<Image>().color = c;
                Tooltip.AddTooltip(layer, $"Track {i + 1}");
            }
            Destroy(layer);
        }
    }

}