using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class GameSelectionCategoryManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameSelectionCategory _categoryRef;

        [Header("Properties")]
        [SerializeField] private float _spacing = 0f;
        [SerializeField] private float _headerOffset = 39.8f;

        private List<GameSelectionCategory> _categories = new();

        public void StartCategories(List<Minigames.GameAction> actions, string gameName)
        {
            string lastHeader = string.Empty;
            GameSelectionCategory lastHeaderObject = null;
            float currentY = _categoryRef.transform.localPosition.y;

            // eventually add the switch games here

            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.hidden || action.actionName == "switchGame") continue;

                if ((i == 0 && gameName != "gameManager") || (gameName == "gameManager" && i == 1) || (action.categoryName != lastHeader && action.categoryName != string.Empty))
                {
                    if ((i != 0 && gameName != "gameManager") || (gameName == "gameManager" && i != 1))
                    {
                        Canvas.ForceUpdateCanvases();
                        currentY += _spacing - lastHeaderObject.GetComponent<RectTransform>().rect.height;
                    }

                    lastHeader = action.categoryName;
                    GameSelectionCategory spawnedCategory = Instantiate(_categoryRef, transform);
                    lastHeaderObject = spawnedCategory;
                    _categories.Add(spawnedCategory);

                    if (lastHeader != string.Empty)
                    {
                        spawnedCategory.SetHeaderText(lastHeader);
                    }
                    else
                    {
                        spawnedCategory.HeaderSetActive(false);
                        currentY += _headerOffset;
                    }

                    spawnedCategory.transform.localPosition = new Vector3(spawnedCategory.transform.localPosition.x, currentY);
                    spawnedCategory.gameObject.SetActive(true);
                }

                lastHeaderObject.AddEvent(action);
            }
        }

        public void DestroyCategories()
        {
            for (int i = 0; i < _categories.Count; i++)
            {
                Destroy(_categories[i].gameObject);
            }
            _categories.Clear();
        }
    }
}

