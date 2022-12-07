using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bread2Unity
{
    public class Bread2UnityGUI : EditorWindow
    {
        private const string EditorFolderName = "bread2unity";
        private const float Width = 657;
        private const float Height = 442;
        private readonly List<string> _animationsIndexes = new List<string>();
        private readonly List<PrefabData> _prefabDataList = new List<PrefabData>();
        private DataModel _animation;
        private GameObject _prefab;
        private Vector2 _scrollPosition;
        private bool _shouldRotate;

        public void OnGUI()
        {
            // Logo
            var logo =
                (Texture)AssetDatabase.LoadAssetAtPath($"Assets/Editor/{EditorFolderName}/logo.png", typeof(Texture));
            GUILayout.Box(logo, GUILayout.ExpandWidth(true), GUILayout.Height(60));
            GUILayout.Space(30);

            var desc = EditorStyles.label;
            desc.wordWrap = true;
            desc.fontStyle = FontStyle.BoldAndItalic;
            // Description
            GUILayout.Box(
                "bread2unity is a tool built with the purpose of converting RH Megamix animations to unity. And to generally speed up development by a lot.",
                desc);

            GUILayout.Space(60);
            EditorGUIUtility.labelWidth = 100;

            // Prefab Selector
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Prefab");
            _prefab = (GameObject)EditorGUILayout.ObjectField(_prefab, typeof(GameObject), false);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Prefab data input
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(60));
            var plusGuiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 35,
                alignment = TextAnchor.MiddleCenter
            };
            var minusGuiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 35,
                alignment = TextAnchor.MiddleCenter
            };
            for (var i = 0; i < _prefabDataList.Count; i++)
            {
                var prefabData = _prefabDataList[i];
                GUILayout.BeginHorizontal();
                prefabData.Name = EditorGUILayout.TextField("Name", prefabData.Name);
                _animationsIndexes[i] = EditorGUILayout.TextField("Animations", _animationsIndexes[i]);
                prefabData.SpriteIndex =
                    EditorGUILayout.IntField("Sprite Index", prefabData.SpriteIndex);
                if (GUILayout.Button("-", minusGuiStyle, GUILayout.Height(15), GUILayout.Width(15)))
                {
                    _prefabDataList.RemoveAt(i);
                    _animationsIndexes.RemoveAt(i);
                    Repaint();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Separator();

            // Add line button
            if (GUILayout.Button("+", plusGuiStyle, GUILayout.Height(40), GUILayout.Width(40)))
            {
                _prefabDataList.Add(new PrefabData("", 0));
                _animationsIndexes.Add("");
            }

            GUILayout.Space(12f);

            // Rotate check box
            _shouldRotate = GUILayout.Toggle(_shouldRotate, "Rotate Spritesheet");

            GUILayout.Space(12f);

            // Create button
            if (GUILayout.Button("Generate Assets") && _prefab != null)
            {
                //Choose png and bccad files
                var bccadFilePath = EditorUtility.OpenFilePanel("Open BCCAD File", null, "bccad");
                if (!string.IsNullOrEmpty(bccadFilePath))
                {
                    var bccadFileFolder = Path.GetDirectoryName(bccadFilePath);
                    var pngFilePath = EditorUtility.OpenFilePanel("Open Texture File", bccadFileFolder, "png");
                    if (!string.IsNullOrEmpty(pngFilePath))
                    {
                        var bccad = BCCAD.Read(File.ReadAllBytes(bccadFilePath));
                        var spriteTexture =
                            SpriteCreator.ComputeSprites(bccad, pngFilePath, _prefab.name, _shouldRotate);
                        //Create prefab from prefab data
                        for (var i = 0; i < _prefabDataList.Count; i++)
                        {
                            List<int> animationIndexes;
                            var prefabData = _prefabDataList[i];
                            if (_animationsIndexes[i].Equals(""))
                                animationIndexes = new List<int>();
                            else
                                animationIndexes = _animationsIndexes[i].Split(',').Select(int.Parse).ToList();
                            prefabData.Animations =
                                animationIndexes.Select(index => bccad.Animations[index]).ToList();
                        }

                        PrefabCreator.CreatePrefab(_prefab, bccad, _prefabDataList, spriteTexture);
                    }
                }
            }

            GUILayout.Space(12f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bread Download", GUILayout.Height(40)))
                Application.OpenURL("https://github.com/rhmodding/bread");

            GUILayout.EndHorizontal();
        }

        public void CreateGUI()
        {
            _animationsIndexes.Add("");
            _prefabDataList.Add(new PrefabData("", 0));
        }


        [MenuItem("Tools/bread2unity")]
        public static void ShowWindow()
        {
            var window = GetWindow<Bread2UnityGUI>("bread2unity");
            var x = Screen.currentResolution.width / 2f - Width;
            var y = Screen.currentResolution.height / 2f - Height;
            window.position = new Rect(x, y, Width, Height);
        }
    }
}