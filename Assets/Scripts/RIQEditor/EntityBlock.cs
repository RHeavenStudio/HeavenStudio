using Starpelly;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
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
        private Image fullHeightLine;
        private TMP_Text datamodelText;

        public bool isGameSwitch = false;

        private ObjectPool<EntityBlock> Pool;

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
            fullHeightLine = transform.GetChild(4).GetComponent<Image>();
            
            fullHeightLine.gameObject.SetActive(false);
        }

        private void Start()
        {
        }

        public void SetBlockInfo()
        {
            var game = EventCaller.instance.GetMinigame(entity.datamodel.Split(0));
            var action = EventCaller.instance.GetGameAction(game, entity.datamodel.Split(1));

            if (isGameSwitch)
            {
                bg.gameObject.SetActive(false);
                icon.gameObject.SetActive(false);
                grab.gameObject.SetActive(false);
                datamodelText.gameObject.SetActive(false);
                
                fullHeightLine.gameObject.SetActive(true);
                rectTransform.SetParent(EditorMain.Instance.Timeline.FullHeightBlocksHolder, true);
            }
            else
            {
                icon.sprite = Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{entity.datamodel.Split(0)}");
                datamodelText.text = action.displayName;
                grab.gameObject.SetActive(action.resizable);
            }
        }

        public void UpdateBlock(Timeline timeline)
        {
            /*
            if (active != wasActive)
            {
                gameObject.SetActive(active);
            }
            wasActive = active;
            if (!active) return;
            */

            if (isGameSwitch)
            {
                rectTransform.anchoredPosition = new Vector2(entity.beat * timeline.pixelsPerBeat, 0);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(timeline.pixelsPerBeat * entity.length, timeline.LayerHeight());
                rectTransform.anchoredPosition = new Vector2(entity.beat * timeline.pixelsPerBeat, timeline.LayerToY(entity.track));
                
                icon.rectTransform.sizeDelta =
                    icon.rectTransform.sizeDelta.ModifyX(Mathf.Clamp(rectTransform.sizeDelta.x - 12, 0,
                        timeline.LayerHeight() - 12));
                
                bg.color = EditorMain.Instance.Theme.LayerColorsGradient.Evaluate((float)entity.track / EditorMain.Instance.Timeline.layerCount);
            }
        }

        public void SetPool(ObjectPool<EntityBlock> pool)
        {
            Pool = pool;
        }
    }
}
