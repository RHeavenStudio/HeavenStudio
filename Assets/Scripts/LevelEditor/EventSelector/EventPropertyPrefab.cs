using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;
using Jukebox;

namespace HeavenStudio.Editor
{
    public class EventPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        protected string _captionText;
        public EventParameterManager parameterManager;
        public string propertyName;
        public List<PropertyCollapse> propertyCollapses = new List<PropertyCollapse>();

        public void SetProperties(string propertyName, object type, string caption) {}
        public virtual void SetCollapses(object type) { }

        public void InitProperties(string propertyName, string caption)
        {
            this.parameterManager = EventParameterManager.instance;
            this.propertyName = propertyName;

            _captionText = caption;

            this.caption.text = _captionText;
        }

        public void UpdateCollapse(object type)
        {
            foreach (var p in propertyCollapses)
            {
                foreach (var c in p.collapseables)
                {
                    if (c != null) c.SetActive(p.collapseOn(type, p.entity) && gameObject.activeSelf);
                }
            }
        }

        public class PropertyCollapse
        {
            public List<GameObject> collapseables;
            public Func<object, RiqEntity, bool> collapseOn;
            public RiqEntity entity;

            public PropertyCollapse(List<GameObject> collapseables, Func<object, RiqEntity, bool> collapseOn, RiqEntity entity)
            {
                this.collapseables = collapseables;
                this.collapseOn = collapseOn;
                this.entity = entity;
            }
        }
    }
}