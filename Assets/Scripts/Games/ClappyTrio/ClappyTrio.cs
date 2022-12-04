using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbClapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("clappyTrio", "The Clappy Trio", "29E7FF", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Clap")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClappyTrio.instance.Clap(e.beat, e.length); },
                    resizable = true
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClappyTrio.instance.Bop(e.beat, e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Happy", "Whether or not they will be happy for you getting it right")
                    },
                },
                new GameAction("prepare", "Prepare Stance")
                {
                    function = delegate { ClappyTrio.instance.Prepare(eventCaller.currentEntity["toggle"] ? 3 : 0); }, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                    }
                },
                new GameAction("change lion count", "Change Lion Count")
                {
                    function = delegate { ClappyTrio.instance.ChangeLionCount((int)eventCaller.currentEntity["valA"]); }, 
                    defaultLength = 0.5f,  
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(3, 8, 3), "Lion Count", "The amount of lions")
                    }
                },
                // This is still here for backwards-compatibility but is hidden in the editor
                new GameAction("prepare_alt", "")
                {
                    function = delegate { ClappyTrio.instance.Prepare(3); }, 
                    hidden = true
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ClappyTrio;
    using UnityEngine.UIElements;

    public class ClappyTrio : Minigame
    {
        public int lionCount = 3;

        public List<GameObject> Lion;

        [SerializeField] private Sprite[] faces;

        private bool reactHappy;
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
            InitLions();
        }
        public override void OnGameSwitch(float beat)
        {
            DynamicBeatmap.DynamicEntity changeLion = GameManager.instance.Beatmap.entities.FindLast(c => c.datamodel == "clappyTrio/change lion count" && c.beat <= beat);
            if(changeLion != null)
            {
                EventCaller.instance.CallEvent(changeLion, true);
            }
        }

        private void InitLions()
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
            float resetTime = 0f;
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
                        resetTime = lastClapStart + length;
                        break;
                    }
                }
            }

            if (Conductor.instance.songPositionInBeats > resetTime + 2)
            {
                reactHappy = false;
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

        public void Bop(float beat, bool happy)
        {
            reactHappy = happy;
            if (playerHitLast)
            {
                for (int i = 0; i < Lion.Count; i++)
                {
                    if (reactHappy)
                        SetFace(i, 1);
                    else
                        SetFace(i, 0);
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
            InitLions();
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