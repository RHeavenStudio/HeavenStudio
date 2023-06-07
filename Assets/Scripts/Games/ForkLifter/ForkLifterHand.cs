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

        private List<RiqEntity> allFlickEntities = new List<RiqEntity>();

        public int currentFlickIndex;

        private void Awake()
        {
            var flickEntities = EventCaller.GetAllInGameManagerList("forkLifter", new string[] { "flick" });
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            for (int i = 0; i < flickEntities.Count; i++)
            {
                if (flickEntities[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(flickEntities[i]);
                }
            }
            allFlickEntities = tempEvents;
        }

        public void CheckNextFlick()
        {
            if (allFlickEntities.Count > 0 && currentFlickIndex >= 0 && currentFlickIndex < allFlickEntities.Count)
            {
                switch (allFlickEntities[currentFlickIndex]["type"])
                {
                    case (int)ForkLifter.FlickType.Pea:
                        ForkLifter.instance.peaPreview.sprite = ForkLifter.instance.peaSprites[0];
                        fastSprite.sprite = fastSprites[0];
                        break;
                    case (int)ForkLifter.FlickType.TopBun:
                        ForkLifter.instance.peaPreview.sprite = ForkLifter.instance.peaSprites[1];
                        fastSprite.sprite = fastSprites[0];
                        break;
                    case (int)ForkLifter.FlickType.Burger:
                        ForkLifter.instance.peaPreview.sprite = ForkLifter.instance.peaSprites[2];
                        fastSprite.sprite = fastSprites[1];
                        break;
                    case (int)ForkLifter.FlickType.BottomBun:
                        ForkLifter.instance.peaPreview.sprite = ForkLifter.instance.peaSprites[3];
                        fastSprite.sprite = fastSprites[0];
                        break;
                }
            }
            else
            {
                ForkLifter.instance.peaPreview.sprite = null;
            }
        }

        public void Prepare()
        {
            SoundByte.PlayOneShotGame("forkLifter/flickPrepare");
            GetComponent<Animator>().Play("Hand_Prepare", 0, 0);
        }
    }
}