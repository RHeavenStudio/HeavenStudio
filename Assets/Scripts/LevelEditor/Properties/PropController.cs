using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

using Starpelly;
using Newtonsoft.Json;
using HeavenStudio.Games;

namespace HeavenStudio.Properties
{

    public class EventParameterManager : MonoBehaviour
    {
        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;

        public Beatmap.Entity entity;

        public bool active;

        private int childCountAtStart;

        public bool canDisable = true;

        public static EventParameterManager instance { get; set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            childCountAtStart = transform.childCount;
        }

    }

        [Serializable]
    public class Properties
    {
        //this is just copied from the beatmap lol
        public string levelName = "test";
        public string levelCreator = "testCreator";
        public int Number;

    }

    public class PropController
    {
        public static PropController instance { get; private set; } = new PropController();

        public Properties properties = new Properties();

        public void LoadProperties(string json = "")
        {
            if (json != "")
            {
                properties = JsonConvert.DeserializeObject<Properties>(json);
                Debug.Log("levelName = " + (properties.levelName));
                Debug.Log("levelCreator = " + (properties.levelCreator));
            }
            else
            {
                properties = new Properties();
            }

        }
    }
}