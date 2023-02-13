using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HeavenStudio.Common
{
    public class OverlaysManager : MonoBehaviour
    {
        public static OverlaysManager instance { get; private set; }
        public static bool OverlaysEnabled;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void TogleOverlaysVisibility(bool visible)
        {
            OverlaysEnabled = visible;
            gameObject.SetActive(visible);
        }
    }
}