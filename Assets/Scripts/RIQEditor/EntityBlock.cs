using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class EntityBlock : MonoBehaviour
    {
        private DynamicBeatmap.DynamicEntity entity;
        
        private RectTransform rectTransform;
        private Image icon;
        private TMP_Text datamodelText;

        public void SetEntity(DynamicBeatmap.DynamicEntity entity)
        {
            this.entity = entity;
        }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            icon = transform.GetChild(1).GetComponent<Image>();
            datamodelText = transform.GetChild(2).GetComponent<TMP_Text>();
        }

        private void Start()
        {
            var game = EventCaller.instance.GetMinigame(entity.datamodel.Split(0));
            var action = EventCaller.instance.GetGameAction(game, entity.datamodel.Split(1));
            
            icon.sprite = Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{entity.datamodel.Split(0)}");
            datamodelText.text = action.displayName;
        }

        public void UpdateBlock(Timeline timeline)
        {
            rectTransform.sizeDelta = new Vector2(timeline.pixelsPerBeat * entity.length, timeline.LayerHeight());
            rectTransform.anchoredPosition = new Vector2(entity.beat * timeline.pixelsPerBeat, timeline.LayerToY(entity.track));
        }
    }
}
