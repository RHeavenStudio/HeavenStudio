using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Starpelly;
using Newtonsoft.Json;
using HeavenStudio.Games;

namespace HeavenStudio.Editor
{

    public class Properties
    {
        string levelName = "";
        string levelCreator = "";
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
                //Debug.Log("property 1 =" + (string)(levelName));
            }
            else
            {
                properties = new Properties();
            }

        }
    }
}