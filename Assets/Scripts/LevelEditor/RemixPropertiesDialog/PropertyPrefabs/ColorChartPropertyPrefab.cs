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
    public class ColorChartPropertyPrefab : RemixPropertyPrefab
    {
        [Header("Color")]
        [Space(10)]
        public Button ColorBTN;
        public RectTransform ColorTable;
        public bool colorTableActive;
        public ColorPreview colorPreview;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            colorPreview.colorPicker.onColorChanged += _ =>
                parameterManager.chart[propertyName] = colorPreview.colorPicker.color;

            Color paramCol = (Color)parameterManager.chart[propertyName];

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