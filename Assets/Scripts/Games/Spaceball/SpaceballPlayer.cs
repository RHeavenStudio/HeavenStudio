using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class SpaceballPlayer : MonoBehaviour
    {
        private Animator anim;

        private int currentHitInList = 0;

        public int costume;

        public SpriteRenderer PlayerSprite;
        public List<SpriteSheet> PlayerSpriteSheets = new List<SpriteSheet>();

        [System.Serializable]
        public class SpriteSheet
        {
            public List<Sprite> sprites;
        }

        public static SpaceballPlayer instance { get; set; }

        private void Awake()
        {
            instance = this;
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Spaceball.instance.EligibleHits.Count == 0)
                currentHitInList = 0;

            if (PlayerInput.Pressed())
            {
                Swing(null);
            }
        }

        public void SetCostume(int costume)
        {
            this.costume = costume;
            anim.Play("Idle", 0, 0);
        }

        public void Swing(SpaceballBall b)
        {
            if (b == null)
            {
                Jukebox.PlayOneShotGame("spaceball/swing");
            }
            else
            {

            }
            anim.Play("Swing", 0, 0);
        }

        public void SetSprite(int id)
        {
            PlayerSprite.sprite = PlayerSpriteSheets[costume].sprites[id];
        }
    }
}