using Starpelly;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class EntityBlock : MonoBehaviour
    {
        public DynamicBeatmap.DynamicEntity entity { get; private set; }
        public bool active;
        private bool wasActive;
        
        private RectTransform rectTransform;
        private Image bg;
        private Image icon;
        private Image grab;
        private TMP_Text datamodelText;

        public void SetEntity(DynamicBeatmap.DynamicEntity entity)
        {
            this.entity = entity;
        }
        
        public void GetComponents()
        {
            rectTransform = GetComponent<RectTransform>();
            bg = transform.GetChild(0).GetComponent<Image>();
            grab = transform.GetChild(1).GetComponent<Image>();
            icon = transform.GetChild(2).GetComponent<Image>();
            datamodelText = transform.GetChild(3).GetComponent<TMP_Text>();
        }

        private void Start()
        {
            var game = EventCaller.instance.GetMinigame(entity.datamodel.Split(0));
            var action = EventCaller.instance.GetGameAction(game, entity.datamodel.Split(1));
            
            icon.sprite = Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{entity.datamodel.Split(0)}");
            datamodelText.text = action.displayName;
            grab.gameObject.SetActive(action.resizable);
        }

        public void UpdateBlock(Timeline timeline)
        {
            if (active != wasActive)
            {
                gameObject.SetActive(active);
            }
            wasActive = active;
            if (!active) return;
            
            rectTransform.sizeDelta = new Vector2(timeline.pixelsPerBeat * entity.length, timeline.LayerHeight());
            rectTransform.anchoredPosition = new Vector2(entity.beat * timeline.pixelsPerBeat, timeline.LayerToY(entity.track));
            
            icon.rectTransform.sizeDelta =
                icon.rectTransform.sizeDelta.ModifyX(Mathf.Clamp(rectTransform.sizeDelta.x - 12, 0,
                    timeline.LayerHeight() - 12));
            
            bg.color = EditorMain.Instance.Theme.LayerColorsGradient.Evaluate((float)entity.track / EditorMain.Instance.Timeline.layerCount);
        }
    }
}
