using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

using Jukebox;

namespace HeavenStudio.Editor
{
    public class EventPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        protected string _captionText;
        public EventParameterManager parameterManager;
        public RiqEntity entity;
        public string propertyName;
        public List<PropertyCollapse> propertyCollapses = new List<PropertyCollapse>();

        public virtual void SetProperties(string propertyName, object type, string caption)
        {
            this.parameterManager = EventParameterManager.instance;

            entity = parameterManager.entity;
            this.propertyName = propertyName;
            this.caption.text = _captionText = caption;
        }
        public virtual void SetCollapses(object type) { }

        public void UpdateCollapse(object type)
        {
            foreach (var p in propertyCollapses)
            {
                if (p.collapseables.Count > 0) { // there could be a better way to do it, but for now this works
                    foreach (var c in p.collapseables) {
                        if (c != null) c.SetActive(p.collapseOn(type, p.entity) && gameObject.activeSelf);
                    }
                } else {
                    _ = p.collapseOn(type, p.entity);
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