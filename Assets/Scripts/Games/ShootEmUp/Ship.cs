using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ShootEmUp
{
    public class Ship : MonoBehaviour
    {
        public Animator shipAnim;
        public Animator laserAnim;
        public Animator damageAnim;

        public bool isDamage = false;
        // int life = 24;

        public void Shoot()
        {
            shipAnim.DoScaledAnimationAsync("shipShoot", 1f);
            laserAnim.DoScaledAnimationAsync("laser", 1f);
        }

        public void Damage()
        {
            // if (life > 0) {
            //     life = Mathf.Max(life - 8, 0);
            // } else {
            //     // Gameover if you miss in next interval
            // }
            
            isDamage = true;
            shipAnim.DoScaledAnimationAsync("shipDamage", 1f);
            damageAnim.DoScaledAnimationAsync("damage", 0.5f);
        }

        public void DamageEnd()
        {
            isDamage = false;
        }
    }
}