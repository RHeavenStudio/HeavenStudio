using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HeavenStudio.Util;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Games.Scripts_ForkLifter
{
    public class ForkLifterHand : MonoBehaviour
    {
        public SpriteRenderer fastSprite;

        public Sprite[] fastSprites;

        public List<RiqEntity> allFlickEntities = new List<RiqEntity>();

        public int currentFlickIndex;

        ForkLifter game;

        private void Awake()
        {
            game = ForkLifter.instance;
        }

        public void CheckNextFlick()
        {
            if (allFlickEntities.Count > 0 && currentFlickIndex >= 0 && currentFlickIndex < allFlickEntities.Count)
            {
                int nextType = allFlickEntities[currentFlickIndex]["type"];
                game.peaPreview.sprite = game.peaSprites[nextType];
                fastSprite.sprite = fastSprites[nextType == (int)ForkLifter.FlickType.Burger ? 1 : 0];
            } else {
                game.peaPreview.sprite = null;
            }
        }

        public void Prepare(bool mute)
        {
            if (!mute) SoundByte.PlayOneShotGame("forkLifter/flickPrepare");
            GetComponent<Animator>().Play("Hand_Prepare", 0, 0);
        }
    }
}