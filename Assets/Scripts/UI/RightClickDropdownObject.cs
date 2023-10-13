using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenStudio.Common
{
    public class RightClickDropdownObject : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Event[] _events;
        [Header("Components")]
        [SerializeField] private Transform _masterTrans;
        [SerializeField] private RectTransform _dropDown;

        private Button _dropDownButton;

        private RectTransform _currentDropDown;

        private bool _beingHovered;

        private void Awake()
        {
            _dropDownButton = _dropDown.GetChild(0).GetChild(0).GetComponent<Button>();
        }

        private void OnDestroy()
        {
            if (_currentDropDown != null) Destroy(_currentDropDown.gameObject);
        }

        private void OnDisable()
        {
            if (_currentDropDown != null) Destroy(_currentDropDown.gameObject);
        }

        private void Update()
        {
            if (_beingHovered) return;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (_currentDropDown != null) Destroy(_currentDropDown.gameObject);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _events.Length == 0) return;
            _currentDropDown = Instantiate(_dropDown, _masterTrans);
            _currentDropDown.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            foreach(var e in _events)
            {
                Button spawnedButton = Instantiate(_dropDownButton, _currentDropDown.GetChild(0));
                spawnedButton.onClick.AddListener(new UnityAction(() => 
                { 
                    e.action.Invoke();
                    if (_currentDropDown != null) Destroy(_currentDropDown.gameObject);
                }));
                spawnedButton.transform.GetChild(0).GetComponent<TMP_Text>().text = e.name;
                spawnedButton.gameObject.SetActive(true);

                EventTrigger eventTrigger = spawnedButton.GetComponent<EventTrigger>();
                EventTrigger.Entry enter = new();
                enter.eventID = EventTriggerType.PointerEnter;
                enter.callback.AddListener(new UnityAction<BaseEventData>((BaseEventData e) => { _beingHovered = true; }));
                eventTrigger.triggers.Add(enter);

                EventTrigger.Entry exit = new();
                exit.eventID = EventTriggerType.PointerExit;
                exit.callback.AddListener(new UnityAction<BaseEventData>((BaseEventData e) => { _beingHovered = false; }));
                eventTrigger.triggers.Add(exit);
            }
        }

        [System.Serializable]
        private struct Event
        {
            public string name;
            public UnityEvent action;
        }
    }
}

