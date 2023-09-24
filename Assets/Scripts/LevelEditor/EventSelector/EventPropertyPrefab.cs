using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Editor
{
    public class EventPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        public EventParameterManager parameterManager;
        public string propertyName;
        public List<PropertyCollapse> propertyCollapses = new List<PropertyCollapse>();

        public void SetProperties(string propertyName, object type, string caption) {}
        public virtual void SetCollapses(object type) { }

        public void InitProperties(string propertyName, string caption)
        {
            this.parameterManager = EventParameterManager.instance;
            this.propertyName = propertyName;
            this.caption.text = caption;
        }

        public void UpdateCollapse(object type)
        {
            foreach (var p in propertyCollapses)
            {
                foreach (var c in p.collapseables)
                {
                    c.SetActive(p.collapseOn(type) && gameObject.activeSelf);
                }
            }
        }

        public class PropertyCollapse
        {
            public List<GameObject> collapseables;
            public Func<object, bool> collapseOn;

            public PropertyCollapse(List<GameObject> collapseables, Func<object, bool> collapseOn)
            {
                this.collapseables = collapseables;
                this.collapseOn = collapseOn;
            }
        }
    }
}