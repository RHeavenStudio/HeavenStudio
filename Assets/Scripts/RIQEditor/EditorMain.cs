using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class EditorMain : MonoBehaviour
    {
        public static EditorMain Instance { get; private set; }

        public Camera EditorCamera;
        public Canvas MainCanvas, TimelineCanvas;
        
        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Debug.LogError("There shouldn't be more than 1 EditorMain!");
        }

        public void Init()
        {
            GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
            GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
            
            GameManager.instance.Init();
        }
    }
}
