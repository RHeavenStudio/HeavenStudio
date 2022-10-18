using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public abstract class TabsContent : MonoBehaviour
    {
        public abstract void OnOpenTab();
        public abstract void OnCloseTab();
    }
}