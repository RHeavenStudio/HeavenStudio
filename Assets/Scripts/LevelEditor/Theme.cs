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

            public List<string> LayerColors = new List<string>();

            public string EventSelectedCol;
            public string EventNormalCol;

            public string BeatMarkerCol;
            public string CurrentTimeMarkerCol;

            public string BoxSelectionCol;
            public string BoxSelectionOutlineCol;
        }
    }
}