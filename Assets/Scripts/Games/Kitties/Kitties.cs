using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrTeppanLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("kitties", "Kitties!", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Cat Clap")
                {
                    function = delegate { Kitties.instance.Clap(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle1"], eventCaller.currentEntity["toggle2"],
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Spawn", "The way in which the kitties will spawn"),
                        new Param("toggle", false, "Mice", "Replaces kitties as mice"),
                        new Param("toggle1", false, "Invert Direction", "Inverts the direction they clap in"),
                        new Param("toggle2", false, "Keep Cats Spawned", "Sets whether or not cats stay spawned after their cue"),
                    }
                },

                new GameAction("roll", "Roll")
                    {
                        function = delegate { Kitties.instance.Roll(eventCaller.currentEntity["toggle"], eventCaller.currentEntity.beat);  },

                        defaultLength = 4f,

                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Keep Cats spawned", "Sets whether or not cats stay spawned after their cue"),
                        }
                    },

                new GameAction ("fish", "Fish")
                {
                    function = delegate { Kitties.instance.CatchFish(eventCaller.currentEntity.beat); },
                    defaultLength = 4f,
                },

                new GameAction("instantSpawn", "Instant Spawn")
                {
                    function = delegate { Kitties.instance.InstantSpawn(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle1"],
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Spawn", "The way in which the kitties will spawn"),
                        new Param("toggle", false, "Mice", "Replaces kitties as mice"),
                        new Param("toggle1", false, "Invert Direction", "Inverts the direction they clap in"),
                        new Param("toggle2", false, "Keep Cats Spawned", "Sets whether or not cats stay spawned after their cue"),
                    }
                },

                new GameAction("bgcolor", "Background Color")
                {
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        Kitties.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]); 
                    },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.white, "Start Color"),
                        new Param("colorEnd", Color.white, "End Color"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    }
                }
            },
            new List<string>() {"ctr", "normal"},
            "ctrteppan", "en",
            new List<string>() {}
            );
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_Kitties;
    public class Kitties : Minigame
    {
        public CtrTeppanPlayer player;
        public Animator[] kitties;
        public GameObject[] Cats;

        public GameObject Fish;
        public bool isInverted;
        public SpriteRenderer background;

        public Vector3[] positions;
        public float[] rotationAngles;

        public enum SpawnType
        {
            Straight,
            DownDiagonal,
            UpDiagonal,
            CloseUp
        }

        public static Kitties instance;

        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            BackgroundColorUpdate();
        }

        public void Clap(bool isMice, bool isInverse, bool keepSpawned, double beat, int type)
        {
            player.ScheduleClap(beat, type);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/nya1", beat),
                new MultiSound.Sound("kitties/nya2", beat + .75f),
                new MultiSound.Sound("kitties/nya3", beat + 1.5f)
            });

            if(type == 3)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("FaceClap", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("FaceClap", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("FaceClap", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("FaceClap", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });
            }

            else if (!isMice)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("Clap1", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("Clap2", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("Clap1", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("Clap2", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

            }
            else
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("MiceClap1", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("MiceClap2", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("MiceClap1", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("MiceClap2", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

            }

            if (!keepSpawned)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat + 3.5f, delegate { RemoveCats(false);})
                });
            }
        }

        public void Roll(bool keepSpawned, double beat)
        {
            if (!player.canClap)
                return;
            player.ScheduleRoll(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/roll1", beat),
                new MultiSound.Sound("kitties/roll2", beat + .5f),
                new MultiSound.Sound("kitties/roll3", beat + 1f),
                new MultiSound.Sound("kitties/roll4", beat + 1.5f)

            });
            BeatAction.New(Cats[0], new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[0].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[0].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[0].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[0].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 2f, delegate { kitties[0].Play("Rolling", 0, 0); }),
                    new BeatAction.Action(beat + 2.75f, delegate { kitties[0].Play("RollEnd", 0, 0); })
                    });

            BeatAction.New(Cats[1], new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[1].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[1].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[1].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[1].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 2f, delegate { kitties[1].Play("Rolling", 0, 0); }),
                    new BeatAction.Action(beat + 2.75f, delegate { kitties[1].Play("RollEnd", 0, 0); })
                    });

            BeatAction.New(Cats[2], new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[2].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[2].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[2].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[2].Play("RollStart", 0, 0); }),
                    new BeatAction.Action(beat + 2f, delegate { player.ScheduleRollFinish(beat); })
                    });

            if (!keepSpawned)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat + 3.5f, delegate { RemoveCats(false);})
                });
            }
        }

        public void CatchFish(double beat)
        {
            //if (!player.canClap)
            //    return;
            
            player.ScheduleFish(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/fish1", beat + 2f),
                new MultiSound.Sound("kitties/fish2", beat + 2.25f),
                new MultiSound.Sound("kitties/fish3", beat + 2.5f),

            });

            BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { if (isInverted)
                    Fish.transform.localScale = new Vector3(-1f, 1f, 1f);
                    else
                        Fish.transform.localScale = new Vector3(1f, 1f, 1f); }),
                new BeatAction.Action(beat, delegate { Fish.SetActive(true); }),             
                new BeatAction.Action(beat, delegate { Fish.GetComponent<Animator>().DoScaledAnimationAsync("FishDangle", 0.5f); }),
                new BeatAction.Action(beat + 2f, delegate { kitties[0].Play("FishNotice", 0, 0);  }),
                new BeatAction.Action(beat + 2.25f, delegate { kitties[1].Play("FishNotice2", 0, 0);  }),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[2].Play("FishNotice3", 0, 0);  })
                });

                BeatAction.New(Fish, new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat + 4f, delegate { Fish.SetActive(false);})
                });


        }

        public void Spawn(int pos, int catNum, bool isMice, bool isInverse, bool firstSpawn, bool instant)
        {
            ResetRotation(catNum);
            if (firstSpawn)
            {
                isInverted = isInverse;
                switch (pos)
                {
                    case 0:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(-5.11f, -1.25f, 0f);
                            positions[1] = new Vector3(.32f, -1.25f, 0f);
                            positions[2] = new Vector3(5.75f, -1.25f, 0f);
                        }
                        else
                        {
                            positions[0] = new Vector3(5.75f, -1.25f, 0f);
                            positions[1] = new Vector3(.32f, -1.25f, 0f);
                            positions[2] = new Vector3(-5.11f, -1.5f, 0f);
                        }
                        break;

                    case 1:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(-6.61f, 1.75f, 6f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(4.25f, -1.75f, -2f);
                        }
                        else
                        {
                            positions[0] = new Vector3(6.61f, 1.75f, 6f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(-4.25f, -1.75f, -2f);
                        }
                        break;

                    case 2:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(4.25f, -1.75f, -2f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(-6.61f, 1.75f, 6f);
                        }
                        else
                        {
                            positions[0] = new Vector3(-4.25f, -1.75f, -2f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(6.61f, 1.75f, 6f);
                        }
                        break;

                    case 3:
                        if (firstSpawn)
                        {
                            rotationAngles = new float[] { -135f, 135f, 0f };
                            positions[0] = new Vector3(-8.21f, 3.7f, 0f);
                            positions[1] = new Vector3(7.51f, 4.2f, 0f);
                            positions[2] = new Vector3(.32f, -4.25f, 0f);
                        }
                        break;


                    default:
                        break;
                }
            }
            Cats[catNum].transform.position = positions[catNum];
            if (pos != 3)
            {
                if (!isInverse)
                    Cats[catNum].transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    Cats[catNum].transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            else
            {
                var rotationVector = Cats[catNum].transform.rotation.eulerAngles;
                rotationVector.z = rotationAngles[catNum];
                Cats[catNum].transform.rotation = Quaternion.Euler(rotationVector);
                Cats[catNum].transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            Cats[catNum].transform.GetChild(0).gameObject.SetActive(true);

            if (!instant)
            {
                if (pos == 3)
                {
                    kitties[catNum].Play("FacePopIn", 0, 0);
                }
                else if (!isMice)
                    kitties[catNum].Play("PopIn", 0, 0);
                else if (catNum < 2)
                {
                    kitties[catNum].Play("MicePopIn", 0, 0);
                }
                else
                    kitties[catNum].Play("PopIn", 0, 0);
            }
            else
            {
                if(pos == 3)
                    kitties[catNum].Play("FaceIdle", 0, 0);
                else if (!isMice)
                    kitties[catNum].Play("Idle", 0, 0);
                else if (catNum < 2)
                    kitties[catNum].Play("MiceIdle", 0, 0);
                else
                    kitties[catNum].Play("Idle", 0, 0);
            }
        }

        public void ResetRotation(int catNum)
        {
            var rotationVector = Cats[0].transform.rotation.eulerAngles;
            rotationVector.z = 0;
            Cats[catNum].transform.rotation = Quaternion.Euler(rotationVector);
        }

        public void RemoveCats(bool fishing)
        {
            if (!fishing)
            {
                for (int x = 0; x < 3; x++)
                {
                    Cats[x].transform.GetChild(0).gameObject.SetActive(false);
                }
            }
            else
                Fish.SetActive(false);
                player.canClap = false;
        }

        public void InstantSpawn(bool isMice, bool isInverse, double beat, int pos)
        {
            BeatAction.New(Cats[0], new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { Spawn(pos, 0, isMice, isInverse, true, true); }),
                new BeatAction.Action(beat, delegate { Spawn(pos, 1, isMice, isInverse, true, true); }),
                new BeatAction.Action(beat, delegate { Spawn(pos, 2, isMice, isInverse, true, true); })
            });
            player.canClap = true;
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart = Color.white; //obviously put to the default color of the game
        private Color colorEnd = Color.white;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            background.color = new Color(newR, newG, newB);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("kitties", new string[] { "bgcolor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }
    }
}