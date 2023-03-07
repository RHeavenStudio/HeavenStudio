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
        [SerializeField] SpriteRenderer djYellowHeadSrpite;
        float normalXScale;
        float negativeXScale;

        void Awake()
        {
            normalXScale = djYellowHeadSrpite.transform.localScale.x;
            negativeXScale = -normalXScale;
        }

        public void ChangeHeadSprite(DJExpression expression)
        {
            if (expression == DJExpression.UpFirst && HeadSpriteCheck(DJExpression.UpSecond)) return;
            djYellowHeadSrpite.sprite = djYellowHeadSprites[(int)expression];
        }

        public bool HeadSpriteCheck(DJExpression expression)
        {
            return djYellowHeadSrpite.sprite == djYellowHeadSprites[(int)expression];
        }

        public void Reverse(bool should = false)
        {
            if (should)
            {
                djYellowHeadSrpite.transform.localScale = new Vector3(negativeXScale, normalXScale, normalXScale);
            }
            else
            {
                djYellowHeadSrpite.transform.localScale = new Vector3(normalXScale, normalXScale, normalXScale);
            }
        }
    }
}

