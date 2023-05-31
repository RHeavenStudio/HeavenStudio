using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Starpelly;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

namespace HeavenStudio.RIQEditor
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public Dictionary<Guid, EntityBlock> EntityBlocks = new();

        [SerializeField] private EntityBlock EntityTemplate;
        [SerializeField] private RectTransform GameSwitchBGTemplate;

        private List<GameSwitchBG> GameSwitchBGs = new();

        public ObjectPool<EntityBlock> Pool;

        private DynamicBeatmap.DynamicEntity entityToSet;
        
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

            Pool = new ObjectPool<EntityBlock>(CreateBlock, OnTakeBlockFromPool, OnReturnBlockToPool, OnDestroyBlock, true, 125, 1500);
        }

        private EntityBlock CreateBlock()
        {
            EntityBlock block = Instantiate(EntityTemplate.gameObject, EditorMain.Instance.Timeline.BlocksHolder).GetComponent<EntityBlock>();
            block.GetComponents();
            block.SetPool(Pool);

            return block;
        }

        private void OnTakeBlockFromPool(EntityBlock block)
        {
            block.SetEntity(entityToSet);
            block.SetBlockInfo();
            
            block.gameObject.SetActive(true);
            EntityBlocks.Add(entityToSet.ID, block);
        }

        private void OnReturnBlockToPool(EntityBlock block)
        {
            EntityBlocks.Remove(block.entity.ID);
            block.gameObject.SetActive(false);
        }

        private void OnDestroyBlock(EntityBlock block)
        {
            Destroy(block.gameObject);
        }

        public void Load()
        {
            var timeLeft = EditorMain.Instance.Timeline.timeLeft;
            var timeRight = EditorMain.Instance.Timeline.timeRight;
            
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                var left = (entity.beat + entity.length) > timeLeft;
                var right = (entity.beat) < timeRight;
                var active = left && right;
                
                if (!active)
                    continue;

                /*
                var isGameSwitch = entity.datamodel.Split(1) == "switchGame";
                var holder = (isGameSwitch)
                    ? EditorMain.Instance.Timeline.FullHeightBlocksHolder
                    : EditorMain.Instance.Timeline.BlocksHolder;
                var entityBlock = Instantiate(EntityTemplate.gameObject, holder).GetComponent<EntityBlock>();
                entityBlock.SetEntity(entity);
                entityBlock.GetComponents();
                entityBlock.gameObject.SetActive(false);
                entityBlock.isGameSwitch = isGameSwitch;
                EntityBlocks.Add(entity.ID, entityBlock);
                */

                entityToSet = entity;
                Pool.Get();
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
            // Problem regarding smooth zooming on edges, investigate.
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                var left = (entity.beat + entity.length) > timeLeft;
                var right = (entity.beat) < timeRight;
                var active = left && right;

                if (active)
                {
                    if (!EntityBlocks.ContainsKey(entity.ID))
                    {
                        entityToSet = entity;
                        Pool.Get();
                        EntityBlocks[entity.ID].UpdateBlock(EditorMain.Instance.Timeline);
                    }
                    else
                    {
                        EntityBlocks[entity.ID].UpdateBlock(EditorMain.Instance.Timeline);
                    }
                }
                else
                {
                    if (EntityBlocks.ContainsKey(entity.ID))
                        Pool.Release(EntityBlocks[entity.ID]);

                    if (!right)
                        break;
                }
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
