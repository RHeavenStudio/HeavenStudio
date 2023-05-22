using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.RIQEditor
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public Dictionary<Guid, EntityBlock> EntityBlocks = new();

        [SerializeField] private EntityBlock EntityTemplate;
        
        public void Load()
        {
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                var entityBlock = Instantiate(EntityTemplate.gameObject, EntityTemplate.transform.parent).GetComponent<EntityBlock>();
                entityBlock.SetEntity(entity);
                EntityBlocks.Add(entity.ID, entityBlock);
            }
            
            EntityTemplate.gameObject.SetActive(false);
        }
        
        private void Update()
        {
            foreach (var block in EntityBlocks)
            {
                if (!block.Value.gameObject.activeSelf) continue;
                
                block.Value.UpdateBlock(EditorMain.Instance.Timeline);
            }
        }
    }
}
