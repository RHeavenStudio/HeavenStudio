using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        [NonSerialized] public bool NoHover = false;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void StartCategories(List<Minigames.GameAction> actions, string gameName)
        {
            NoHover = false;
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
                    lastHeaderObject.SetMaster(this);

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

            Canvas.ForceUpdateCanvases();

            float sizeY = 0;

            for (int i = 0; i < _categories.Count; i++)
            {
                if (!_categories[i].HeaderIsActive()) sizeY -= _headerOffset;
                sizeY -= _spacing - _categories[i].GetComponent<RectTransform>().rect.height;
            }

            _rectTransform.sizeDelta = new Vector3(0, sizeY, 0);
        }

        public void UpdateCategoryPositions()
        {
            float currentY = _categoryRef.transform.localPosition.y;
            for (int i = 0; i < _categories.Count; i++)
            {
                if (!_categories[i].HeaderIsActive()) currentY += _headerOffset;
                _categories[i].transform.localPosition = new Vector3(_categories[i].transform.localPosition.x, currentY);

                Canvas.ForceUpdateCanvases();
                currentY += _spacing - _categories[i].GetComponent<RectTransform>().rect.height;
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

