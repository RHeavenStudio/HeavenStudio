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

    

    [Serializable]
    public class Properties
    {
        public static Properties instance { get; private set; } = new Properties();

        //this is just copied from the beatmap lol
        public string levelName = "";
        public string levelCreator = "";
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