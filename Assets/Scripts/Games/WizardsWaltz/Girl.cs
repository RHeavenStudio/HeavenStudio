using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_WizardsWaltz
{
    public class Girl : MonoBehaviour
    {

        public Animator animator;

        public GameObject[] flowers;
        private int flowerCount = 0;

        public void Happy()
        {
            animator.Play("Happy", 0, 0);
            SetFlowers(1);
        }

        public void Sad()
        {
            animator.Play("Sad", 0, 0);
            SetFlowers(-1);
        }

        public void SetFlowers(int add = 0)
        {
            flowerCount = Mathf.Clamp(flowerCount + add, 0, flowers.Length);
            for (int i = 0; i < flowers.Length; i++)
            {
                flowers[i].SetActive(i < flowerCount);
            }
        }
    }
}