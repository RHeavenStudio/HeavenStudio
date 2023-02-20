using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

namespace HeavenStudio.Editor
{
    public class SpecialTimelineTempo : TabsContent
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnOpenTab()
        {
            SpecialTimeline.instance.FixObjectsVisibility();
        }

        public override void OnCloseTab()
        {
        }
    }
}