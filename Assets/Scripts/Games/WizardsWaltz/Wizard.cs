using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WizardsWaltz
{
    public class Wizard : MonoBehaviour
    {
        public Animator animator;
        public GameObject shadow;

        private WizardsWaltz game;
        private float songPos;

        public void Init()
        {
            game = WizardsWaltz.instance;
        }

        void Update()
        {
            songPos = (float)(Conductor.instance.songPositionInBeatsAsDouble - game.wizardBeatOffset);
            var am = game.beatInterval / 2f;
            var x = Mathf.Sin(Mathf.PI * songPos / am) * game.xRange;
            var y = Mathf.Cos(Mathf.PI * songPos / am) * game.yRange;
            var z = Mathf.Cos(Mathf.PI * songPos / am) * game.zRange;
            //var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f;

            transform.position = new Vector3(x, 3f - y * 0.5f, z);
            shadow.transform.position = new Vector3(x, game.plantYOffset + y * 1.5f, z);

            /*var xscale = scale;
            if (y > 0) xscale *= -1;
            transform.localScale = new Vector3(xscale, scale, 1);
            shadow.transform.localScale = new Vector3(scale, scale, 1);*/

            transform.localScale = new Vector3((y > 0) ? -1 : 1, 1, 1);
        }

        private void LateUpdate()
        {
            if (PlayerInput.Pressed(true))
            {
                animator.Play("Magic", 0, 0);
                SoundByte.PlayOneShotGame("wizardsWaltz/wand");
            }
        }

        public void Idle()
        {
            animator.Play("Idle", 0, 0);
        }

        public void Magic(Plant plant, bool hit)
        {
            animator.Play("Magic", 0, 0);

            if(plant == null)
            {
                // TODO: Play empty A press sound
                return;
            }
            if (hit)
            {
                SoundByte.PlayOneShotGame("wizardsWaltz/grow");
                plant.Bloom();
                game.girl.Happy();
            }
            else
            {
                SoundByte.PlayOneShot("miss");
                plant.Eat();
                game.girl.Sad();
            }
        }

    }
}