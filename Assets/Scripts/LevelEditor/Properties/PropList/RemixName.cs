using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Properties
{
    public class RemixName : MonoBehaviour
    {
        [SerializeField] private GameObject content;

        public void Refresh(string content)
        {
            Properties.instance.levelName = content;
            Debug.Log(Properties.instance.levelName);
        }
    }
}