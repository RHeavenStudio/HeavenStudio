using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeavenStudio
{
    public class EventCaller : MonoBehaviour
    {
        public Transform GamesHolder;
        public DynamicBeatmap.DynamicEntity currentEntity = new DynamicBeatmap.DynamicEntity();
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

            currentEntity = new DynamicBeatmap.DynamicEntity();

            Minigames.Init(this);

            List<Minigames.Minigame> minigamesInBeatmap = new List<Minigames.Minigame>();
            for (int i = 0; i < GameManager.instance.Beatmap.entities.Count; i++)
            {
                //go through every entity in the timeline and add the game that they're from to the minigamesInBeatmap list (ignore entities from FX only categories, i.e. Game Manager and Count-Ins)
                Minigames.Minigame game = GetMinigame(GameManager.instance.Beatmap.entities[i].datamodel.Split('/')[0]);
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

        public void CallEvent(DynamicBeatmap.DynamicEntity entity, bool gameActive)
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

        public void CallPreEvent(DynamicBeatmap.DynamicEntity entity)
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

        public static List<DynamicBeatmap.DynamicEntity> GetAllInGameManagerList(string gameName, string[] include)
        {
            List<DynamicBeatmap.DynamicEntity> temp1 = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<DynamicBeatmap.DynamicEntity> temp2 = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < temp1.Count; i++)
            {
                if (include.Any(temp1[i].datamodel.Split('/')[1].Contains))
                {
                    temp2.Add(temp1[i]);
                }
            }
            return temp2;
        }

        public static List<DynamicBeatmap.DynamicEntity> GetAllInGameManagerListExclude(string gameName, string[] exclude)
        {
            List<DynamicBeatmap.DynamicEntity> temp1 = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
            List<DynamicBeatmap.DynamicEntity> temp2 = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < temp1.Count; i++)
            {
                if (!exclude.Any(temp1[i].datamodel.Split('/')[1].Contains))
                {
                    temp2.Add(temp1[i]);
                }
            }
            return temp2;
        }

        public static List<DynamicBeatmap.DynamicEntity> GetAllPlayerEntities(string gameName)
        {
            return GameManager.instance.playerEntities.FindAll(c => c.datamodel.Split('/')[0] == gameName);
        }

        public static List<DynamicBeatmap.DynamicEntity> GetAllPlayerEntitiesExcept(string gameName)
        {
            return GameManager.instance.playerEntities.FindAll(c => c.datamodel.Split('/')[0] != gameName);
        }

        // elaborate as fuck, boy
        public static List<DynamicBeatmap.DynamicEntity> GetAllPlayerEntitiesExceptBeforeBeat(string gameName, float beat)
        {
            return GameManager.instance.playerEntities.FindAll(c => c.datamodel.Split('/')[0] != gameName && c.beat < beat);
        }

        public static List<Minigames.Minigame> FXOnlyGames()
        {
            return instance.minigames.FindAll(c => c.fxOnly == true).ToList();
        }
    }
}