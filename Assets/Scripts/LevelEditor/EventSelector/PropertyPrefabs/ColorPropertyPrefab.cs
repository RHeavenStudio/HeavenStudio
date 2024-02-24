using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;
using HeavenStudio.Editor;

namespace HeavenStudio.Editor
{
    public class ColorPropertyPrefab : EventPropertyPrefab
    {
        [Header("Color")]
        [Space(10)]
        public Button ColorBTN;
        public RectTransform ColorTable;
        public bool colorTableActive;
        public ColorPreview colorPreview;
        public TMP_InputField hex;

        private Color _defaultColor;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            hex.onSelect.AddListener(
                _ =>
                    Editor.instance.editingInputField = true
            );
            hex.onEndEdit.AddListener(
                _ =>
                {;
                    Editor.instance.editingInputField = false;
                }
            );

            colorPreview.colorPicker.onColorChanged += _ => 
            {
                parameterManager.entity[propertyName] = colorPreview.colorPicker.color;
                if (colorPreview.colorPicker.color != _defaultColor)
                {
                    this.caption.text = _captionText + "*";
                }
                else
                {
                    this.caption.text = _captionText;
                }
            };

            _defaultColor = (Color)type;

            Color paramCol = parameterManager.entity[propertyName];

            ColorBTN.onClick.AddListener(
                () =>
                {
                    ColorTable.gameObject.SetActive(true);
                    colorTableActive = true;
                    colorPreview.ChangeColor(paramCol);
                }
            );

            colorPreview.ChangeColor(paramCol);
            ColorTable.gameObject.SetActive(false);
        }

        public void ResetValue()
        {
            colorPreview.ChangeColor(_defaultColor);
        }

        public override void SetCollapses(object type)
        {
            colorPreview.colorPicker.onColorChanged += _ => UpdateCollapse(colorPreview.colorPicker.color);
            UpdateCollapse(colorPreview.colorPicker.color);
        }

        private void Update()
        {
            if (colorTableActive)
            {
                if (!Editor.MouseInRectTransform(ColorTable))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ColorTable.gameObject.SetActive(false);
                        colorTableActive = false;
                        Editor.instance.editingInputField = false;
                    }
                }
            }
        }
    }
}