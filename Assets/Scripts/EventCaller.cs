using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio
{
    public class EventCaller : MonoBehaviour
    {
        public Transform GamesHolder;
        public RiqEntity currentEntity = new RiqEntity();
        public string currentSwitchGame;

        public delegate void EventCallback();

        public static EventCaller instance { get; private set; }

        public List<Minigames.Minigame> minigames = new List<Minigames.Minigame>();

        public Minigames.Minigame GetMinigame(string gameName)
        {
            return minigames.Find(c => c.name == gameName);
        }

        public Minigames.GameAction GetGameAction(Minigames.Minigame game, string action)
        {
            return game.actions.Find(c => c.actionName == action);
        }

        public Minigames.Param GetGameParam(Minigames.Minigame game, string action, string param)
        {
            return GetGameAction(game, action).parameters.Find(c => c.propertyName == param);
        }

        public void Init()
        {
            instance = this;

            currentEntity = new RiqEntity();

            Minigames.Init(this);

            List<Minigames.Minigame> minigamesInBeatmap = new List<Minigames.Minigame>();
            for (int i = 0; i < GameManager.instance.Beatmap.Entities.Count; i++)
            {
                //go through every entity in the timeline and add the game that they're from to the minigamesInBeatmap list (ignore entities from FX only categories, i.e. Game Manager and Count-Ins)
                Minigames.Minigame game = GetMinigame(GameManager.instance.Beatmap.Entities[i].datamodel.Split('/')[0]);
                if (!minigamesInBeatmap.Contains(game) && !FXOnlyGames().Contains(game))
                {
                    minigamesInBeatmap.Add(game);
                }
            }

            for (int i = 0; i < minigamesInBeatmap.Count; i++)
            {
                // minigames[minigames.FindIndex(c => c.name == minigamesInBeatmap[i].name)].holder = Resources.Load<GameObject>($"Games/{minigamesInBeatmap[i].name}");
            }
        }

        private void Update()
        {
            
        }

        public void CallEvent(RiqEntity entity, bool gameActive)
        {
            string[] details = entity.datamodel.Split('/');
            Minigames.Minigame game = minigames.Find(c => c.name == details[0]);
            try
            {
                currentEntity = entity;

                if (details.Length > 2) currentSwitchGame = details[2];

                Minigames.GameAction action = game.actions.Find(c => c.actionName == details[1]);
                if (gameActive)
                {
                    action.function.Invoke();
                }
                else
                {
                    action.inactiveFunction.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Event not found! May be spelled wrong or it is not implemented.\n" + ex);
            }
        }

        public void CallPreEvent(RiqEntity entity)
        {
            string[] details = entity.datamodel.Split('/');
            Minigames.Minigame game = minigames.Find(c => c.name == details[0]);
            try
            {
                currentEntity = entity;

                Minigames.GameAction action = game.actions.Find(c => c.actionName == details[1]);
                action.preFunction.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Event not found! May be spelled wrong or it is not implemented.\n" + ex);
            }
        }

        static bool StringStartsWith(string a, string b)
        {
            int aLen = a.Length;
            int bLen = b.Length;
        
            int ap = 0; int bp = 0;
        
            while (ap < aLen && bp < bLen && a [ap] == b [bp])
            {
                ap++;
                bp++;
            }
        
            return (bp == bLen);
        }

        public static bool IsGameSwitch(RiqEntity entity)
        {
            return StringStartsWith(entity.datamodel, "gameManager/switchGame");
        }

        public static List<RiqEntity> GetAllInGameManagerList(string gameName, string[] include)
        {
            List<RiqEntity> temp1 = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<RiqEntity> temp2 = new List<RiqEntity>();
            foreach (string s in include)
            {
                temp2.AddRange(temp1.FindAll(c => c.datamodel.Split('/')[1].Equals(s)));
            }
            return temp2;
        }

        public static List<RiqEntity> GetAllInGameManagerListExclude(string gameName, string[] exclude)
        {
            List<RiqEntity> temp1 = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<RiqEntity> temp2 = new List<RiqEntity>();
            foreach (string s in exclude)
            {
                temp2.AddRange(temp1.FindAll(c => !c.datamodel.Split('/')[1].Equals(s)));
            }
            return temp2;
        }

        public static List<Minigames.Minigame> FXOnlyGames()
        {
            return instance.minigames.FindAll(c => c.fxOnly == true).ToList();
        }
    }
}