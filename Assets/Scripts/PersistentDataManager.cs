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
        public enum PerfectChallengeType
        {
            Off,        // no perfect challenge
            Arcade,     // "arcade rule"
            Legacy,     // "legacy rule"
            On          // "megamix rule"
        }

        [NonSerialized] public static GameSettings gameSettings;

        public static void CreateDefaultSettings()
        {
            gameSettings = new GameSettings(
                false,
                1,
                GlobalGameManager.DEFAULT_SCREEN_SIZES[1].width,
                GlobalGameManager.DEFAULT_SCREEN_SIZES[1].height,
                0.8f,
                256,
                44100,
                true,
                true,
                PerfectChallengeType.On,
                true,
                false
            );

            // disable if platform is mac
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                gameSettings.discordRPCEnable = false;
            else
                gameSettings.discordRPCEnable = true;
            
            gameSettings.timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>()
            {
                OverlaysManager.TimingDisplayComponent.CreateDefaultDual()
            };
            gameSettings.skillStarComponents = new List<OverlaysManager.SkillStarComponent>()
            {
                OverlaysManager.SkillStarComponent.CreateDefault()
            };
            gameSettings.sectionComponents = new List<OverlaysManager.SectionComponent>()
            {
                OverlaysManager.SectionComponent.CreateDefault()
            };
            
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
            // default settings constructor
            public GameSettings(
                bool isFullscreen = false,
                int resolutionIndex = 0,
                int resolutionWidth = 1280,
                int resolutionHeight = 720,
                float masterVolume = 0.8f,
                int dspSize = 256,
                int sampleRate = 44100,
                bool editorCursorEnable = true,
                bool discordRPCEnable = true,
                PerfectChallengeType perfectChallengeType = PerfectChallengeType.On,
                bool isMedalOn = true,
                bool timingDisplayMinMode = false,
                bool overlaysInEditor = true
                )
            {
                this.isFullscreen = isFullscreen;

                this.resolutionIndex = resolutionIndex;
                this.resolutionWidth = resolutionWidth;
                this.resolutionHeight = resolutionHeight;

                this.masterVolume = masterVolume;
                this.dspSize = dspSize;
                this.sampleRate = sampleRate;

                this.editorCursorEnable = editorCursorEnable;
                if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                    this.discordRPCEnable = false;
                else
                    this.discordRPCEnable = true;

                this.perfectChallengeType = perfectChallengeType;
                this.isMedalOn = isMedalOn;
                this.timingDisplayMinMode = timingDisplayMinMode;
                this.overlaysInEditor = overlaysInEditor;

                this.timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>()
                {
                    OverlaysManager.TimingDisplayComponent.CreateDefaultDual()
                };
                this.skillStarComponents = new List<OverlaysManager.SkillStarComponent>()
                {
                    OverlaysManager.SkillStarComponent.CreateDefault()
                };
                this.sectionComponents = new List<OverlaysManager.SectionComponent>()
                {
                    OverlaysManager.SectionComponent.CreateDefault()
                };
            }

            // Display / Audio Settings
            public bool isFullscreen;

            public int resolutionIndex;
            public int resolutionWidth;
            public int resolutionHeight;

            public float masterVolume;
            public int dspSize;
            public int sampleRate;

            // Editor Settings
            public bool editorCursorEnable;
            public bool discordRPCEnable;

            // Gameplay Settings
            public PerfectChallengeType perfectChallengeType;
            public bool isMedalOn;
            public bool timingDisplayMinMode;
            public bool overlaysInEditor;
            public List<OverlaysManager.TimingDisplayComponent> timingDisplayComponents;
            public List<OverlaysManager.SkillStarComponent> skillStarComponents;
            public List<OverlaysManager.SectionComponent> sectionComponents;
        }
    }
}