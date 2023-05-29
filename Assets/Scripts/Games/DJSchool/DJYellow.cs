using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_DJSchool
{
    public class DJYellow : MonoBehaviour
    {
        public enum DJExpression
        {
            NeutralLeft = 0,
            NeutralRight = 1,
            CrossEyed = 2,
            Happy = 3,
            Focused = 4,
            UpFirst = 5,
            UpSecond = 6,
        }
        [SerializeField] List<Sprite> djYellowHeadSprites = new List<Sprite>();
        [SerializeField] SpriteRenderer djYellowHeadSprite;
        float normalXScale;
        float negativeXScale;

        void Awake()
        {
            normalXScale = djYellowHeadSprite.transform.localScale.x;
            negativeXScale = -normalXScale;
        }

        public void ChangeHeadSprite(DJExpression expression)
        {
            if (expression == DJExpression.UpFirst && HeadSpriteCheck(DJExpression.UpSecond)) return;
            djYellowHeadSprite.sprite = djYellowHeadSprites[(int)expression];
        }

        public bool HeadSpriteCheck(DJExpression expression)
        {
            return djYellowHeadSprite.sprite == djYellowHeadSprites[(int)expression];
        }

        public void Reverse(bool should = false)
        {
            djYellowHeadSprite.transform.localScale = new Vector3(should ? negativeXScale : normalXScale, normalXScale, normalXScale);
        }
    }
}

