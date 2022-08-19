using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Properties
{
    public class RemixName : MonoBehaviour
    {
        public static RemixName instance { get; private set; } = new RemixName();

        [SerializeField] private GameObject content;

        public void Refresh(string content)
        {
            Properties.instance.levelName = content;
            Debug.Log(Properties.instance.levelName);
            content = Properties.instance.levelName;
        }
    }
}