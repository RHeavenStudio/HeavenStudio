using System;
using System.Collections;
using System.Collections.Generic;
using Starpelly;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public Dictionary<Guid, EntityBlock> EntityBlocks = new();

        [SerializeField] private EntityBlock EntityTemplate;
        [SerializeField] private RectTransform GameSwitchBGTemplate;

        private List<GameSwitchBG> GameSwitchBGs = new();

        private struct GameSwitchBG
        {
            public float Beat;
            public RectTransform Rect;

            public GameSwitchBG(float beat, RectTransform rect)
            {
                Beat = beat;
                Rect = rect;
            }
        }

        private void Start()
        {
            EntityTemplate.gameObject.SetActive(false);
            GameSwitchBGTemplate.gameObject.SetActive(false);
        }

        public void Load()
        {
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                var entityBlock = Instantiate(EntityTemplate.gameObject, EntityTemplate.transform.parent).GetComponent<EntityBlock>();
                entityBlock.SetEntity(entity);
                entityBlock.GetComponents();
                entityBlock.gameObject.SetActive(false);
                EntityBlocks.Add(entity.ID, entityBlock);
            }
            
            var gameSwitches = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel.Split(1) == "switchGame");
            for (var i = 0; i < gameSwitches.Count; i++)
            {
                var gameSwitch = gameSwitches[i];

                var gsHolder = Instantiate(GameSwitchBGTemplate.gameObject, GameSwitchBGTemplate.parent).transform as RectTransform;
                gsHolder.gameObject.SetActive(true);
                
                var gameTex = Resources.Load<Texture2D>($"Sprites/Editor/GameIcons/{gameSwitch.datamodel.Split(2)}");
                gsHolder.GetChild(0).GetComponent<RawImage>().texture = gameTex;

                GameSwitchBGs.Add(new GameSwitchBG(gameSwitch.beat, gsHolder));
            }

            if (gameSwitches.Count == 0 && GameManager.instance.Beatmap.entities.Count > 0)
            {
                DynamicBeatmap.DynamicEntity firstGameEntity = null;
                foreach (var entity in GameManager.instance.Beatmap.entities)
                {
                    if (entity.datamodel.Split(0) != "vfx")
                    {
                        firstGameEntity = entity;
                        break;
                    }
                }

                if (firstGameEntity != null)
                {
                    var gsHolder = Instantiate(GameSwitchBGTemplate.gameObject, GameSwitchBGTemplate.parent).transform as RectTransform;
                    gsHolder.gameObject.SetActive(true);
                    var gameTex = Resources.Load<Texture2D>($"Sprites/Editor/GameIcons/{firstGameEntity.datamodel.Split(0)}");
                    gsHolder.GetChild(0).GetComponent<RawImage>().texture = gameTex;
                    GameSwitchBGs.Add(new GameSwitchBG(0.0f, gsHolder));
                }
            }
        }
        
        public void UpdateBlockManager()
        {
            var timeLeft = EditorMain.Instance.Timeline.timeLeft;
            var timeRight = EditorMain.Instance.Timeline.timeRight;
            foreach (var block in EntityBlocks)
            {
                var blockScr = block.Value;
                var blockBeat = blockScr.entity.beat;
                blockScr.active =
                    (blockBeat + blockScr.entity.length) > timeLeft
                    &&
                    (blockBeat) < timeRight;
                // if (!block.Value.gameObject.activeSelf) continue;
                
                blockScr.UpdateBlock(EditorMain.Instance.Timeline);
            }
            
            var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "end" });
            
            for (var i = 0; i < GameSwitchBGs.Count; i++)
            {
                var bg = GameSwitchBGs[i];
                var rect = bg.Rect;
                
                var nextBeat = 0.0f;
                if (i + 1 < GameSwitchBGs.Count)
                {
                    nextBeat = GameSwitchBGs[i + 1].Beat - bg.Beat;
                }
                else
                {
                    if (allEnds.Count > 0)
                    {
                        var firstEnd = allEnds[0];
                        nextBeat = firstEnd.beat - bg.Beat;
                    }
                    else
                    {
                        nextBeat = EditorMain.Instance.Timeline.timeRight - bg.Beat;
                    }
                }

                var newWidth = EditorMain.Instance.Timeline.pixelsPerBeat * nextBeat;
                var newX = bg.Beat * EditorMain.Instance.Timeline.pixelsPerBeat;
                rect.sizeDelta = rect.sizeDelta.ModifyX(newWidth);
                rect.anchoredPosition = rect.anchoredPosition.ModifyX(newX);

                var imgRect = rect.GetChild(0).GetComponent<RectTransform>();
                
                imgRect.anchoredPosition = imgRect.anchoredPosition.ModifyX(EditorMain.Instance.Timeline.timelineX - newX);
                imgRect.sizeDelta = imgRect.sizeDelta.ModifyX(EditorMain.Instance.Timeline.timelineWidth);
                
                imgRect.GetComponent<RawImage>().uvRect = new Rect(0, 0, EditorMain.Instance.Timeline.timelineWidth / EditorMain.Instance.Timeline.timelineHeight, 1);
            }
        }
    }
}
