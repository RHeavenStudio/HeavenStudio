using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class GameSelectionCategory : MonoBehaviour
    {
        [SerializeField] private NewGameSelectionEvent _gameSelectionEventRef;
        [SerializeField] private GameObject _headerObject;
        [SerializeField] private TMP_Text _headerText;

        public void SetHeaderText(string text)
        {
            _headerText.text = text;
        }

        public void HeaderSetActive(bool active)
        {
            _headerObject.SetActive(active);
        }

        public void AddEvent(Minigames.GameAction action)
        {
            NewGameSelectionEvent spawnedEvent = Instantiate(_gameSelectionEventRef, transform);
            spawnedEvent.SetText(action.displayName);
            spawnedEvent.SetActiveGearIcon(action.parameters != null && action.parameters.Count > 0);
            spawnedEvent.gameObject.SetActive(true);
        }
    }
}

