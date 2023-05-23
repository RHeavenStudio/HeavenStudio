using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class EditorTheme
    {
        public Color[] LayerColors = new Color[] {};
        
        [JsonIgnore] public Gradient LayerColorsGradient { get; private set; }

        public void LoadColors()
        {
            var grad = new Gradient();
            var gradientColKeys = new List<GradientColorKey>();
            for (var i = 0; i < LayerColors.Length; i++)
            {
                var col = LayerColors[i];
                gradientColKeys.Add(new GradientColorKey(col, (float)i / LayerColors.Length));
            }

            grad.colorKeys = gradientColKeys.ToArray();
            LayerColorsGradient = grad;
        }
    }
}
