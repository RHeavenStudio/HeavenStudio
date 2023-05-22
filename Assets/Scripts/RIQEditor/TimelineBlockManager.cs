using System;
using System.Collections;
using System.Collections.Generic;
using Starpelly;
using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public Dictionary<Guid, EntityBlock> EntityBlocks = new();

        [SerializeField] private EntityBlock EntityTemplate;

        private void Start()
        {
            EntityTemplate.gameObject.SetActive(false);
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
        }
    }
}
