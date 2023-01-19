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
                    function = delegate { Kitties.instance.Clap(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle1"], 
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Spawn", "The way in which the kitties will spawn"),
                        new Param("toggle", false, "Mice", "Replaces kitties as mice"),
                        new Param("toggle1", false, "Invert Direction", "Inverts the direction they clap in"),
                    }
                },

                new GameAction("remove", "Remove Cats")
                {
                    function = delegate { Kitties.instance.Remove(eventCaller.currentEntity.beat); },

                    defaultLength = .5f,
                }
            });
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

        private bool isSpawned = false;
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

        public void Clap(bool isMice, bool isInverse, float beat, int type)
        {
            player.ScheduleClap(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/nya1", beat),
                new MultiSound.Sound("kitties/nya2", beat + .75f),
                new MultiSound.Sound("kitties/nya3", beat + 1.5f)
            });
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].Play("Clap1", 0, 0);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].Play("Clap2", 0, 0);}),
                });

            BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse);}),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[1].Play("Clap1", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { kitties[1].Play("Clap2", 0, 0);}),
                });

            BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse);}),
                new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

        }

        public void Remove(float beat)
        {
            BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Cats[0].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat, delegate { Cats[1].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat, delegate { Cats[2].transform.GetChild(0).gameObject.SetActive(false);}),
                    new BeatAction.Action(beat, delegate { player.canClap = false;}),
                });
        }

        public void Spawn(int pos, int catNum, bool isMice, bool isInverse)
        {
            
            switch(pos)
            {
                case 0:

                    if (!isInverse)
                    {
                        Cats[0].transform.position = new Vector3(-5.11f, -0.5f, 0f);
                        Cats[1].transform.position = new Vector3(.32f, -0.5f, 0f);
                        Cats[2].transform.position = new Vector3(5.75f, -0.5f, 0f);

                        for (int x = 0; x < 3; x++)
                        {
                            Cats[catNum].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = true;
                            Cats[catNum].transform.GetChild(1).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                        }
                    }
                    else
                    {
                        Cats[0].transform.position = new Vector3(5.75f, -0.5f, 0f);
                        Cats[1].transform.position = new Vector3(.32f, -0.5f, 0f);
                        Cats[2].transform.position = new Vector3(-5.11f, -0.5f, 0f);

                        for (int x = 0; x < 3; x++)
                        {
                            Cats[catNum].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().flipX = false;
                            Cats[catNum].transform.GetChild(1).gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                        }
                    }
                            break;
                default:
                    break;
            }
            Cats[catNum].transform.GetChild(0).gameObject.SetActive(true);
            kitties[catNum].Play("PopIn", 0, 0);
        }

        public void SpawnPosition(int pos, int catNum, bool isInverse)
        {

        }
    }
}