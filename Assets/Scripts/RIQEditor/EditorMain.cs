using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class EditorMain : MonoBehaviour
    {
        public static EditorMain Instance { get; private set; }

        public Camera EditorCamera;
        public Canvas MainCanvas, TimelineCanvas;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Debug.LogError("There shouldn't be more than 1 EditorMain!");
        }
    }
}
