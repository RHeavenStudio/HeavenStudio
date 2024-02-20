using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox;
using UnityEngine;

namespace HeavenStudio
{
    public class EntityTypes
    {
        public struct Integer
        {
            public int min;
            public int val;
            public int max;

            public Integer(int min, int max, int val = 0)
            {
                this.min = min;
                this.val = val;
                this.max = max;
            }
        }

        public struct Float
        {
            public float min;
            public float val;
            public float max;

            public Float(float min, float max, float val = 0)
            {
                this.min = min;
                this.val = val;
                this.max = max;
            }
        }

        // this will eventually replace Float and Integer
        public struct Number
        {
            public float snap;
            public float min;
            public float val;
            public float max;

            public Number(float snap, float min, float max, float val = 0)
            {
                this.snap = snap;
                this.min = min;
                this.val = val;
                this.max = max;
            }

            public Number(float min, float max, float val = 0)
            {
                this.snap = 0.001f;
                this.min = min;
                this.val = val;
                this.max = max;
            }
        }

        public struct Button
        {
            public string defaultLabel;
            public Func<RiqEntity, string> onClick;

            public Button(string defaultLabel, Func<RiqEntity, string> onClick)
            {
                this.defaultLabel = defaultLabel;
                this.onClick = onClick;
            }
        }

        public struct Dropdown
        {
            public int defaultValue;
            public List<string> values;

            public Dropdown(int defaultValue, List<string> values)
            {
                this.defaultValue = defaultValue;
                this.values = values.ToList();
            }

            public Dropdown(Enum value)
            {
                this.defaultValue = 0;
                // Debug.Log(value);
                // Debug.Log(value.GetType());
                // Debug.Log(value.GetType().DeclaringType);
                this.values = Enum.GetNames(value.GetType()).ToList();
            }
        }

        public struct Resource
        {
            public enum ResourceType
            {
                Image,
                Audio,
                MSMD,
                AssetBundle
            }

            public string path;
            public string name;
            public ResourceType type;

            public Resource(ResourceType type, string path, string name)
            {
                this.type = type;
                this.path = path;
                this.name = name;
            }
        }
    }
}