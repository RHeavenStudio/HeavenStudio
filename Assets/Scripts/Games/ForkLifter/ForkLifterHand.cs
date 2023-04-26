using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ForkLifter
{
    public class ForkLifterHand : MonoBehaviour
    {
        public SpriteRenderer fastSprite;

        public Sprite[] fastSprites;

        private List<DynamicBeatmap.DynamicEntity> allFlickEntities = new List<DynamicBeatmap.DynamicEntity>();

        public int currentFlickIndex;

        private void Awake()
        {
            var flickEntities = EventCaller.GetAllInGameManagerList("forkLifter", new string[] { "flick" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < flickEntities.Count; i++)
            {
                if (flickEntities[i].beat >= Conductor.instance.songPositionInBeats)
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
            Jukebox.PlayOneShotGame("forkLifter/flickPrepare");
            GetComponent<Animator>().Play("Hand_Prepare", 0, 0);
        }
    }
}