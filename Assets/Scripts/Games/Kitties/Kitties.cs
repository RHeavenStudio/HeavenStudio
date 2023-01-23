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
            return new Minigame("kitties", "Kitties! \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
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
                    defaultLength = 6f,
                }

            });;
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

        }

        public void Clap(bool isMice, bool isInverse, bool keepSpawned, float beat, int type)
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
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("FaceClap", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("FaceClap", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("FaceClap", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("FaceClap", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });
            }

            else if (!isMice)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("Clap1", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("Clap2", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("Clap1", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("Clap2", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

            }
            else
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("MiceClap1", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("MiceClap2", 0, 0);}),
                });

                BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("MiceClap1", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("MiceClap2", 0, 0);}),
                });

                BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

            }

            if (!keepSpawned)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[0].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[1].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[2].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { player.canClap = false;}),
                });
            }
        }

        public void Roll(bool keepSpawned, float beat)
        {
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

            //for (int x = 0; x < 3; x++)
            //{
            //    Debug.Log(x + " " + kitties.Length);

            //    BeatAction.New(Cats[x], new List<BeatAction.Action>()
            //    {
            //        new BeatAction.Action(beat, delegate { kitties[x].Play("RollStart", 0, 0); }),
            //        new BeatAction.Action(beat + .5f, delegate { kitties[x].Play("RollStart", 0, 0); }),
            //        new BeatAction.Action(beat + 1f, delegate { kitties[x].Play("RollStart", 0, 0); }),
            //        new BeatAction.Action(beat + 1.5f, delegate { kitties[x].Play("RollStart", 0, 0); }),
            //        });
            //}

            if (!keepSpawned)
            {
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[0].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[1].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { Cats[2].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat + 3.5f, delegate { player.canClap = false;}),
                });
            }
        }

        public void CatchFish(float beat)
        {
            player.ScheduleFish(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/fish1", beat + 2f),
                new MultiSound.Sound("kitties/fish2", beat + 2.25f),
                new MultiSound.Sound("kitties/fish3", beat + 2.5f),

            });

            BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Fish.SetActive(true); }),             
                new BeatAction.Action(beat, delegate { Fish.GetComponent<Animator>().Play("DangleFish", 0, 0); }),
                });


        }

        public void Spawn(int pos, int catNum, bool isMice, bool isInverse, bool firstSpawn)
        {
            if(firstSpawn)
            {
                ResetRotation();
                switch (pos)
                {
                    case 0:

                        if (!isInverse)
                        {
                            Cats[0].transform.position = new Vector3(-5.11f, -0.5f, 0f);
                            Cats[1].transform.position = new Vector3(.32f, -0.5f, 0f);
                            Cats[2].transform.position = new Vector3(5.75f, -0.5f, 0f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = true;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            Cats[0].transform.position = new Vector3(5.75f, -0.5f, 0f);
                            Cats[1].transform.position = new Vector3(.32f, -0.5f, 0f);
                            Cats[2].transform.position = new Vector3(-5.11f, -0.5f, 0f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = false;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                            }
                        }
                        break;

                    case 1:

                        if (!isInverse)
                        {
                            Cats[0].transform.position = new Vector3(-6.61f, 2.5f, 6f);
                            Cats[1].transform.position = new Vector3(.32f, 0.5f, 2f);
                            Cats[2].transform.position = new Vector3(4.25f, -1f, -2f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = true;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            Cats[0].transform.position = new Vector3(6.61f, 2.5f, 6f);
                            Cats[1].transform.position = new Vector3(.32f, 0.5f, 2f);
                            Cats[2].transform.position = new Vector3(-4.25f, -1f, -2f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = false;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                            }
                        }
                        break;

                    case 2:

                        if (!isInverse)
                        {
                            Cats[0].transform.position = new Vector3(4.25f, -1f, -2f);
                            Cats[1].transform.position = new Vector3(.32f, 0.5f, 2f);
                            Cats[2].transform.position = new Vector3(-6.61f, 2.5f, 6f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = true;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            Cats[0].transform.position = new Vector3(-4.25f, -1f, -2f);
                            Cats[1].transform.position = new Vector3(.32f, 0.5f, 2f);
                            Cats[2].transform.position = new Vector3(6.61f, 2.5f, 6f);

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = false;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                            }
                        }
                        break;

                    case 3:
                        if (firstSpawn)
                        {
                            var rotationVector = Cats[0].transform.rotation.eulerAngles;
                            rotationVector.z = -135;
                            Cats[0].transform.position = new Vector3(-8.21f, 3.7f, 0f);
                            Cats[0].transform.rotation = Quaternion.Euler(rotationVector);
                            Cats[1].transform.position = new Vector3(7.51f, 4.2f, 0f);
                            rotationVector.z = 135;
                            Cats[1].transform.rotation = Quaternion.Euler(rotationVector);
                            Cats[2].transform.position = new Vector3(.32f, -4.25f, 0f);
                            

                            for (int x = 0; x < 3; x++)
                            {
                                Cats[x].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = false;
                                Cats[x].transform.GetChild(1).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                                Cats[x].transform.GetChild(2).gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                            }
                        }
                        break;


                    default:
                        break;
                }
            }
            Cats[catNum].transform.GetChild(0).gameObject.SetActive(true);
            if(pos == 3)
            {
                kitties[catNum].Play("FacePopIn", 0, 0);
            }
            else if(!isMice)
                kitties[catNum].Play("PopIn", 0, 0);
            else if (catNum < 2)
            {
                kitties[catNum].Play("MicePopIn", 0, 0);
            }
            else
                kitties[catNum].Play("PopIn", 0, 0);
        }

        public void ResetRotation()
        {
            for (int i = 0; i < 3; i++)
            {
                var rotationVector = Cats[0].transform.rotation.eulerAngles;
                rotationVector.z = 0;
                Cats[i].transform.rotation = Quaternion.Euler(rotationVector);
            }
        }
    }
}