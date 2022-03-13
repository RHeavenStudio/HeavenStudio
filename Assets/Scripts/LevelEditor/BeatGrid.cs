using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace HeavenStudio.Editor
{
    public class BeatGrid : MonoBehaviour
    {
        private RectTransform rectTransform;

        public float snap;
        public float count;

        private float lastPosX;

        private List<GameObject> Lines = new List<GameObject>();

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            for (int i = 0; i < count; i++)
            {
                GameObject line = Instantiate(transform.GetChild(0).gameObject, transform);
                line.transform.localPosition = new Vector3(i, line.transform.localPosition.y);
                line.SetActive(true);

                Lines.Add(line);
            }
        }

        private void Update()
        {
            var pos = new Vector2(Mathf.RoundToInt(Mathf.Abs(transform.parent.localPosition.x) / 100) - 1, transform.localPosition.y);
            transform.localPosition = pos;

            if (pos.x != lastPosX)
                UpdateGridNum();

            lastPosX = transform.localPosition.x;
        }

        private void UpdateGridNum()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                float newNum = transform.localPosition.x + i;
                line.transform.GetChild(0).GetComponent<TMP_Text>().text = newNum.ToString();
            }
        }
    }
}