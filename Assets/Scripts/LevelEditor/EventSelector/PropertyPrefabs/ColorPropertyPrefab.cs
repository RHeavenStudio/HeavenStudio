using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Starpelly;

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

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            colorPreview.colorPicker.onColorChanged += _ =>
                parameterManager.entity[propertyName] = colorPreview.colorPicker.color;

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
                    }
                }
            }
        }
    }
}