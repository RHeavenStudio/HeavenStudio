using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    public class GameSelectionCategory : MonoBehaviour
    {
        [SerializeField] private NewGameSelectionEvent _gameSelectionEventRef;
        [SerializeField] private GameObject _headerObject;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private RectTransform _collapseImageTrans;

        private bool _isOpen = true;

        private GameSelectionCategoryManager _manager;

        private List<NewGameSelectionEvent> _gameSelectionEventList = new();

        public void SetMaster(GameSelectionCategoryManager manager)
        {
            _manager = manager;
        }

        public void SetHeaderText(string text)
        {
            _headerText.text = text;
        }

        public void HeaderSetActive(bool active)
        {
            _headerObject.SetActive(active);
        }

        public bool HeaderIsActive()
        {
            return _headerObject.activeSelf;
        }

        public void AddEvent(Minigames.GameAction action)
        {
            NewGameSelectionEvent spawnedEvent = Instantiate(_gameSelectionEventRef, transform);
            spawnedEvent.SetText(action.displayName);
            spawnedEvent.SetActiveGearIcon(action.parameters != null && action.parameters.Count > 0);
            spawnedEvent.gameObject.SetActive(true);
            _gameSelectionEventList.Add(spawnedEvent);
        }

        public void ToggleCollapse()
        {
            _isOpen = !_isOpen;

            _collapseImageTrans.localEulerAngles = new Vector3(0, 0, _isOpen ? -90 : 0);

            foreach (var e in _gameSelectionEventList)
            {
                e.gameObject.SetActive(_isOpen);
            }
            _manager.UpdateCategoryPositions();
        }
    }
}

