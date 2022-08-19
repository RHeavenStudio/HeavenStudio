using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using TMPro;

namespace HeavenStudio.Properties
{
    public class RemixName : MonoBehaviour
    {
        public static RemixName instance { get; private set; } = new RemixName();

        [SerializeField] private GameObject content;
        [SerializeField] private TMP_InputField inputFieldString;

        public void Refresh(string content)
        {
            Properties.instance.levelName = content;
            Debug.Log(Properties.instance.levelName);
            inputFieldString.text = (string)(Properties.instance.levelName);
        }
    }
}