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

        private GameSelectionCategory _category;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetCategory(GameSelectionCategory category)
        {
            _category = category;
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
            _category.NoHover = true;
        }

        public void PointerUp()
        {
            _animator.Play("GameEventDragEnd", 0, 0);
            _category.NoHover = false;
        }

        public void PointerEnter()
        {
            if (_category.NoHover) return;
            _animator.Play("GameEventEnter", 0, 0);
        }

        public void PointerExit()
        {
            if (_category.NoHover) return;
            _animator.Play("GameEventExit", 0, 0);
        }
    }
}

