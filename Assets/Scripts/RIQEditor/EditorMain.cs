using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class EditorMain : MonoBehaviour
    {
        public static EditorMain Instance { get; private set; }

        public Camera EditorCamera;
        public Canvas MainCanvas, TimelineCanvas;

        public Timeline Timeline;
        
        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;

        [Header("GridGameSelector")] 
        [SerializeField] private GameObject SpecialIconPrefab;
        [SerializeField] private GameObject NormalIconPrefab;

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

            foreach (var minigame in EventCaller.instance.minigames)
            {
                var pref = minigame.fxOnly ? SpecialIconPrefab : NormalIconPrefab;
                var gameIcon = Instantiate(pref, pref.transform.parent);
                gameIcon.GetComponent<Image>().sprite = GameIcon(minigame.name);
            }
            SpecialIconPrefab.SetActive(false);
            NormalIconPrefab.SetActive(false);
        }

        #region Custom

        public static Sprite GameIcon(string name)
        {
            return Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{name}");
        }

        public static Texture GameIconMask(string name)
        {
            return Resources.Load<Texture>($"Sprites/Editor/GameIcons/{name}_mask");
        }

        #endregion
    }
}
