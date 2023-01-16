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
                    function = delegate { Kitties.instance.Clap(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle"], 
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Spawn", "The way in which the kitties will spawn"),
                        new Param("toggle", false, "Mice", "Replaces kitties as mice"),
                        new Param("toggle", false, "Invert Direction", "Inverts the direction they clap in"),
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
        public GameObject[] Cats;
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

        public void Clap(bool isMice, bool isRightToLeft, float beat, int type = (int)SpawnType.Straight)
        {
            player.ScheduleClap(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/nya1", beat),
                new MultiSound.Sound("kitties/nya2", beat + .75f),
                new MultiSound.Sound("kitties/nya3", beat + 1.5f)
            });
                BeatAction.New(Cats[0], new List<BeatAction.Action>()
            {
                    new BeatAction.Action(beat, delegate { Cats[0].transform.GetChild(0).gameObject.SetActive(true);}),
                });

            BeatAction.New(Cats[1], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .75f, delegate { Cats[1].transform.GetChild(0).gameObject.SetActive(true);}),
                });

            BeatAction.New(Cats[2], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate { Cats[2].transform.GetChild(0).gameObject.SetActive(true);}),
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
    }
}