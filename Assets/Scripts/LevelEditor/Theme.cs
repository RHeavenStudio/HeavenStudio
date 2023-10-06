using Newtonsoft.Json;
using Starpelly;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Editor
{
    [Serializable]
    public class Theme
    {
        public string name;
        public Properties properties;
        
        [Serializable]
        public class Properties
        {
            public string SpecialLayersCol;
            public string TempoLayerCol;
            public string MusicLayerCol;
            public string SectionLayerCol;

            public string Layer1Col;
            public string Layer2Col;
            public string Layer3Col;
            public string Layer4Col;
            public string Layer5Col;

            public string[] LayerColors = null;

            public string EventSelectedCol;
            public string EventNormalCol;

            public string BeatMarkerCol;
            public string CurrentTimeMarkerCol;

            public string BoxSelectionCol;
            public string BoxSelectionOutlineCol;
        }

        [JsonIgnore]
        public Gradient LayersGradient { get; private set; }

        public void SetLayersGradient()
        {
            if (properties.LayerColors == null) return;

            LayersGradient = new Gradient();
            var colorKeys = new List<GradientColorKey>();
            for (var i = 0; i < properties.LayerColors.Length; i++)
            {
                var color = properties.LayerColors[i];
                colorKeys.Add(new GradientColorKey(color.Hex2RGB(), (float)i / (properties.LayerColors.Length - 1)));
            }

            LayersGradient.colorKeys = colorKeys.ToArray();
        }
    }
}