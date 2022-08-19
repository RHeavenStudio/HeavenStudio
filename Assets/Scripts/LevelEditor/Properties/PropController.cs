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

    public class EditInput
    {
        public static EditInput instance { get; private set; } = new EditInput();

        [Header("Properties")]
        public bool editingInputField = false;
    }

    public class PropManager : MonoBehaviour
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

        public static PropManager instance { get; set; }

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
        public static Properties instance { get; private set; } = new Properties();

        //this is just copied from the beatmap lol
        public string levelName = "asdf";
        public string levelCreator = "testCreator";
        public int Number;

        public string datamodel;

        public object this[string propertyName]
        {
            get
            {
                return typeof(Properties).GetField(propertyName).GetValue(this);
            }
            set
            {
                try
                {
                    typeof(Properties).GetField(propertyName).SetValue(this, value);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"You probably misspelled a parameter, or defined the object type wrong. Exception log: {ex}");
                }
            }
        }
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
                RemixName.instance.Refresh(Properties.instance.levelName);
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