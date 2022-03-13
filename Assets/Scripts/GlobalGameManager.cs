using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace HeavenStudio
{
    public class GlobalGameManager : MonoBehaviour
    {
        public static GlobalGameManager instance { get; set; }

        public static int loadedScene;
        public int lastLoadedScene;
        public static float fadeDuration;

        public GameObject loadScenePrefab;
        public GameObject hourGlass;

        public static string levelLocation;
        public static bool officialLevel;

        public enum Scenes : int
        {
            SplashScreen = 0,
            Menu = 1,
            Editor = 2,
            Game = 3
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            BasicCheck();

            loadedScene = 0;
            fadeDuration = 0;

            /*GameObject ui = new GameObject();
            ui.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("tempBuildUI");
            ui.GetComponent<SpriteRenderer>().sortingOrder = 1000;
            ui.layer = 5;
            ui.name = "tempBuildUI";*/
        }

        public void Awake()
        {
            Init();
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            Starpelly.OS.Windows.ChangeWindowTitle($"Heaven Studio DEMO");
        }

        public static GameObject CreateFade()
        {
            GameObject fade = new GameObject();
            DontDestroyOnLoad(fade);
            fade.transform.localScale = new Vector3(4000, 4000);
            SpriteRenderer sr = fade.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/GeneralPurpose/Square");
            sr.sortingOrder = 20000;
            fade.layer = 5;
            return fade;
        }


        public static void BasicCheck()
        {
            if (FindGGM() == null)
            {
                GameObject GlobalGameManager = new GameObject("GlobalGameManager");
                GlobalGameManager.name = "GlobalGameManager";
                GlobalGameManager.AddComponent<GlobalGameManager>();
            }
        }

        public static GameObject FindGGM()
        {
            if (GameObject.Find("GlobalGameManager") != null)
                return GameObject.Find("GlobalGameManager");
            else
                return null;
        }

        public static void LoadScene(int sceneIndex, float duration = 0.35f)
        {
            print("bruh");
            BasicCheck();
            loadedScene = sceneIndex;
            fadeDuration = duration;

            // DOTween.Clear(true);
            // SceneManager.LoadScene(sceneIndex);

            GameObject fade = CreateFade();
            fade.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
            fade.GetComponent<SpriteRenderer>().DOColor(Color.black, fadeDuration).OnComplete(() => { SceneManager.LoadScene(loadedScene); fade.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), fadeDuration).OnComplete(() => { Destroy(fade); }); });
        }
    }

}