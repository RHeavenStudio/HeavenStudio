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

        public void ChangeHeadSprite(DJExpression expression)
        {
            djYellowHeadSrpite.sprite = djYellowHeadSprites[(int)expression];
        }
    }
}

