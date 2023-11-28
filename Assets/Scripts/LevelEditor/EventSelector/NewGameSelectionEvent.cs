using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class NewGameSelectionEvent : MonoBehaviour
    {
        [NonSerialized] public int index;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private GameObject _gearIcon;

        private Animator _animator;

        private GameSelectionCategoryManager _manager;

        private bool _shouldDoExitHoverAnim = true;

        private Minigames.GameAction _action;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetMaster(GameSelectionCategoryManager manager)
        {
            _manager = manager;
        }

        public void SetAction(Minigames.GameAction action)
        {
            _action = action;
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void SetTextColor(Color color)
        {
            _text.color = color;
        }

        public void SetActiveGearIcon(bool a)
        {
            _gearIcon.SetActive(a);
        }

        public void PointerDown()
        {
            _animator.Play("GameEventDrag", 0, 0);
            _manager.NoHover = true;
            _shouldDoExitHoverAnim = false;
        }

        public void PointerUp()
        {
            _animator.Play("GameEventDragEnd", 0, 0);
            _manager.NoHover = false;
        }

        public void PointerEnter()
        {
            if (_manager.NoHover) return;
            _animator.Play("GameEventEnter", 0, 0);
            _shouldDoExitHoverAnim = true;
        }

        public void PointerExit()
        {
            if (_manager.NoHover || !_shouldDoExitHoverAnim) return;
            _animator.Play("GameEventExit", 0, 0);
        }
    }
}

