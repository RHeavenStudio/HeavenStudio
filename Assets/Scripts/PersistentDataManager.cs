using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Newtonsoft.Json;

namespace HeavenStudio.Common
{
    public static class PersistentDataManager
    {
        [NonSerialized] public static GameSettings gameSettings;

        public static void CreateDefaultSettings()
        {
            gameSettings = new GameSettings();

            gameSettings.isFullscreen = false;

            gameSettings.resolutionIndex = 0;
            gameSettings.resolutionWidth = GlobalGameManager.DEFAULT_SCREEN_SIZES[gameSettings.resolutionIndex].width;
            gameSettings.resolutionHeight = GlobalGameManager.DEFAULT_SCREEN_SIZES[gameSettings.resolutionIndex].height;

            gameSettings.masterVolume = 0.8f;

            gameSettings.editorCursorEnable = true;

            // disable if platform is mac
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                gameSettings.discordRPCEnable = false;
            else
                gameSettings.discordRPCEnable = true;

            SaveSettings();
        }

        public static void LoadSettings()
        {
            if (File.Exists(Application.persistentDataPath + "/settings.json"))
            {
                string json = File.ReadAllText(Application.persistentDataPath + "/settings.json");
                gameSettings = JsonUtility.FromJson<GameSettings>(json);
            }
            else
            {
                CreateDefaultSettings();
            }
        }

        public static void SaveSettings()
        {
            string json = JsonUtility.ToJson(gameSettings);
            // save json to persistentDataPath
            FileStream file = File.Create(Application.persistentDataPath + "/settings.json");
            file.Write(System.Text.Encoding.ASCII.GetBytes(json), 0, json.Length);
            file.Close();
        }

        public static void SaveTheme(string json)
        {
            // save json to persistentDataPath
            FileStream file = File.Create(Application.persistentDataPath + "/editorTheme.json");
            file.Write(System.Text.Encoding.ASCII.GetBytes(json), 0, json.Length);
            file.Close();
        }

        [Serializable]
        public struct GameSettings
        {
            // Display / Audio Settings
            public bool isFullscreen;

            public int resolutionIndex;
            public int resolutionWidth;
            public int resolutionHeight;

            public float masterVolume;

            // Editor Settings
            public bool editorCursorEnable;
            public bool discordRPCEnable;
        }
    }

}