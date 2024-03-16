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
            shipAnim.Play("shipShoot", 0, 0);
            laserAnim.Play("laser", 0, 0);
        }

        public void Damage()
        {
            // if (life > 0) {
            //     life = Mathf.Max(life - 8, 0);
            // } else {
            //     // Gameover if you miss in next interval
            // }
            
            isDamage = true;
            shipAnim.Play("shipDamage", 0, 0);
            damageAnim.Play("damage", 0, 0);
        }

        public void DamageEnd()
        {
            isDamage = false;
        }
    }
}