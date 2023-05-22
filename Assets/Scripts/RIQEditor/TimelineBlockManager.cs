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

        private void Start()
        {
            EntityTemplate.gameObject.SetActive(false);
        }

        public void Load()
        {
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                var entityBlock = Instantiate(EntityTemplate.gameObject, EntityTemplate.transform.parent).GetComponent<EntityBlock>();
                entityBlock.gameObject.SetActive(true);
                entityBlock.SetEntity(entity);
                EntityBlocks.Add(entity.ID, entityBlock);
            }
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
