using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.TextboxUtilities
{
    public class TextboxObject : MonoBehaviour
    {
        [Header("Objects")]
        public TMP_Text TextboxLabel;
        public RectTransform TextboxLabelRect;
        public SpriteRenderer UL;
        public SpriteRenderer UR;
        public SpriteRenderer DL;
        public SpriteRenderer DR;

        static Vector2 textboxSize = new Vector2(3f, 0.75f);

        public void Resize(float scaleX, float scaleY)
        {
            Vector2 tScale = Vector2.Scale(textboxSize, new Vector2(scaleX, scaleY));

            UL.size = tScale;
            UR.size = tScale;
            DL.size = tScale;
            DR.size = tScale;
            TextboxLabelRect.sizeDelta = new Vector2(11.2f * scaleX, 2.2f * scaleY);
        }

        public void SetText(string text)
        {
            TextboxLabel.text = text;
        }
    }
}