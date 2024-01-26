using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio
{
    public class EventCaller : MonoBehaviour
    {
        public GameManager gameManager { get; private set; }
        public Transform GamesHolder;
        public RiqEntity currentEntity = new RiqEntity();
        public string currentSwitchGame;

        public delegate void EventCallback();

        public static EventCaller instance { get; private set; }

        public Dictionary<string, Minigames.Minigame> minigames = new();


        public Minigames.Minigame GetMinigame(string gameName)
        {
            if (!minigames.ContainsKey(gameName))
            {
                return null;
            }
            return minigames[gameName];
        }

        public Minigames.GameAction GetGameAction(Minigames.Minigame game, string action)
        {
            return game.actions.Find(c => c.actionName == action);
        }

        public Minigames.GameAction GetGameAction(string gameName, string action)
        {
            if (minigames.ContainsKey(gameName))
            {
                return minigames[gameName].actions.Find(c => c.actionName == action);
            }
            else
            {
                Debug.LogWarning($"Game {gameName} not found!");
                return null;
            }
        }

        public Minigames.Param GetGameParam(Minigames.Minigame game, string action, string param)
        {
            return GetGameAction(game, action).parameters.Find(c => c.propertyName == param);
        }

        public Minigames.Param GetGameParam(string gameName, string action, string param)
        {
            return GetGameAction(gameName, action).parameters.Find(c => c.propertyName == param);
        }

        public void Init(GameManager mgr)
        {
            gameManager = mgr;
            instance = this;

            currentEntity = new RiqEntity();

            Minigames.Init(this);
        }

        public void CallEvent(RiqEntity entity, bool gameActive)
        {
            string[] details = entity.datamodel.Split('/');
            Minigames.Minigame game = minigames[details[0]];
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
            Minigames.Minigame game = minigames[details[0]];
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

        public static List<RiqEntity> GetAllInGameManagerList(string gameName, string[] include)
        {
            List<RiqEntity> temp1 = instance.gameManager.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<RiqEntity> temp2 = new List<RiqEntity>();
            foreach (string s in include)
            {
                temp2.AddRange(temp1.FindAll(c => c.datamodel.Split('/')[1].Equals(s)));
            }
            return temp2;
        }

        public static List<RiqEntity> GetAllInGameManagerListExclude(string gameName, string[] exclude)
        {
            List<RiqEntity> temp1 = instance.gameManager.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<RiqEntity> temp2 = new List<RiqEntity>();
            foreach (string s in exclude)
            {
                temp2.AddRange(temp1.FindAll(c => !c.datamodel.Split('/')[1].Equals(s)));
            }
            return temp2;
        }

        public static List<Minigames.Minigame> FXOnlyGames()
        {
            return instance.minigames.Values.ToList().FindAll(c => c.fxOnly);
        }
    }
}