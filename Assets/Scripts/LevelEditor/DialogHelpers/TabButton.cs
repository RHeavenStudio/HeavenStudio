using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class TabButton : MonoBehaviour
    {
        public GameObject Content;

        public void OnClick()
        {
            var tabsManager = transform.parent.GetComponent<TabsManager>();
            tabsManager.SetActiveContent(Content);
        }
    }
}