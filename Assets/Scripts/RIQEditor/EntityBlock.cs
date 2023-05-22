using System;
using System.Collections;
using System.Collections.Generic;
using Starpelly;
using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class EntityBlock : MonoBehaviour
    {
        private DynamicBeatmap.DynamicEntity entity;
        
        private RectTransform rectTransform;

        public void SetEntity(DynamicBeatmap.DynamicEntity entity)
        {
            this.entity = entity;
        }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void UpdateBlock(Timeline timeline)
        {
            rectTransform.sizeDelta = rectTransform.sizeDelta.ModifyX(timeline.pixelsPerBeat);
        }
    }
}
