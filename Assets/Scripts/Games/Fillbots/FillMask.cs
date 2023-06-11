using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Fillbots
{
    public class FillMask : MonoBehaviour
    {
        [SerializeField] private SpriteMask mask;

        [SerializeField] private Sprite[] sprites;

        public void SetMask(int i)
        {
            mask.sprite = sprites[i];
        }
    }
}
