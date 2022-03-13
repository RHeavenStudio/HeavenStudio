using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    using Scripts_ClappyTrio;

    public class ClappyTrio : Minigame
    {
        public int lionCount = 3;

        public List<GameObject> Lion;

        [SerializeField] private Sprite[] faces;

        private bool isClapping;
        private float currentClappingLength;
        private float lastClapStart;
        private int clapIndex;

        private ClappyTrioPlayer ClappyTrioPlayer;

        public bool playerHitLast = false;

        public static ClappyTrio instance { get; set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            float startPos = -3.066667f;
            float maxWidth = 12.266668f;

            for (int i = 0; i < lionCount; i++)
            {
                GameObject lion;
                if (i == 0)
                    lion = Lion[0];
                else
                    lion = Instantiate(Lion[0], Lion[0].transform.parent);

                lion.transform.localPosition = new Vector3(startPos + ((maxWidth / (lionCount + 1)) * (i + 1)), 0);

                if (i > 0)
                    Lion.Add(lion);

                if (i == lionCount - 1)
                    ClappyTrioPlayer = lion.AddComponent<ClappyTrioPlayer>();
            }
        }

        private void Update()
        {
            if (isClapping)
            {
                float songPosBeat = Conductor.instance.songPositionInBeats;

                for (int i = 0; i < Lion.Count; i++)
                {
                    float length = currentClappingLength * (i);
                    float lengthplusone = (currentClappingLength * (i + 1));

                    // i spent like 25 minutes trying to figure out what was wrong with this when i forgot to subtract the currentClapLength :(
                    if (i == Lion.Count - 1)
                    {
                        length = 0;
                    }

                    if (songPosBeat > lastClapStart + length && songPosBeat < lastClapStart + lengthplusone && clapIndex == i)
                    {
                        if (i == Lion.Count - 1)
                        {
                            ClappyTrioPlayer.SetClapAvailability(lastClapStart + (currentClappingLength * (i - 1)), currentClappingLength);

                            clapIndex = 0;
                            isClapping = false;
                            currentClappingLength = 0;
                            ClappyTrioPlayer.clapStarted = false;
                        } else
                        {
                            SetFace(i, 4);
                            Lion[i].GetComponent<Animator>().Play("Clap", 0, 0);

                            // lazy fix rn
                            if (i > 0)
                                Jukebox.PlayOneShotGame("clappyTrio/middleClap");
                            else
                                Jukebox.PlayOneShotGame("clappyTrio/leftClap");

                            clapIndex++;
                        }
                        break;
                    }
                }
            }
        }

        public void Clap(float beat, float length)
        {
            ClappyTrioPlayer.clapStarted = true;
            ClappyTrioPlayer.canHit = true; // this is technically a lie, this just restores the ability to hit

            playerHitLast = false;
            isClapping = true;
            lastClapStart = beat;
            currentClappingLength = length;
        }

        public void Prepare(int type)
        {
            for (int i = 0; i < Lion.Count; i++)
            {
                SetFace(i, type);
            }
            PlayAnimationAll("Prepare");
            Jukebox.PlayOneShotGame("clappyTrio/ready");
        }

        public void Bop(float beat)
        {
            if (playerHitLast)
            {
                for (int i = 0; i < Lion.Count; i++)
                {
                    SetFace(i, 1);
                }
            } else
            {
                var a = EventCaller.GetAllInGameManagerList("clappyTrio", new string[] { "clap" });
                var b = a.FindAll(c => c.beat < beat);

                if (b.Count > 0)
                {
                    for (int i = 0; i < Lion.Count; i++)
                    {
                        if (i == Lion.Count - 1)
                        {
                            SetFace(i, 0);
                        } else
                        {
                            SetFace(i, 2);
                        }
                    }
                }
            }
            PlayAnimationAll("Bop");
        }

        public void ChangeLionCount(int lions)
        {
            for(int i=1; i<lionCount; i++)
            {
                Destroy(Lion[i]);
            }
            Lion.RemoveRange(1, lionCount - 1);
            lionCount = lions;
            SetFace(0, 0);
            Start();
            PlayAnimationAll("Idle");
        }

        private void PlayAnimationAll(string anim)
        {
            for (int i = 0; i < Lion.Count; i++)
            {
                Lion[i].GetComponent<Animator>().Play(anim, -1, 0);
            }
        }

        public void SetFace(int lion, int type)
        {
            Lion[lion].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = faces[type];
        }
    }
}