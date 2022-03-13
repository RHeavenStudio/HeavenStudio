using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_WizardsWaltz
{
    public class MagicFX : MonoBehaviour
    {
        public Animator animator;
        public SpriteRenderer spriteRenderer;
        public GameObject shimmer;

        public void Start()
        {
            int order = (int)Math.Round((transform.position.z - 2) * 1000);
            spriteRenderer.sortingOrder = order;
            shimmer.GetComponent<SpriteRenderer>().sortingOrder = order;
            animator.Play("Magic", 0, 0);

            Rigidbody2D rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 2.5f;
        }

        public void Kill()
        {
            Destroy(shimmer);
            Destroy(gameObject);
        }

    }
}