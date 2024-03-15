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

        public void Shoot()
        {
            shipAnim.Play("shipShoot", 0, 0);
            laserAnim.Play("laser", 0, 0);
        }

        public void Damage()
        {
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