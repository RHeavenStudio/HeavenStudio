using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSamuraiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("samuraiSliceNtr", "Samurai Slice (DS) \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
                //new GameAction("start bopping",                   delegate { SamuraiSliceNtr.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 1),
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_NtrSamurai;

    public class SamuraiSliceNtr : Minigame
    {
        public enum ObjectType {
            Melon,
            Fish,
            Demon
        }

        [Header("References")]
        public NtrSamurai player;
        public GameObject launcher;

        //game scene
        public static SamuraiSliceNtr instance;

        public GameEvent bop = new GameEvent();

        private void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                player.Bop();
            }

            if (PlayerInput.AltPressed())
            {
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchThrough");
                player.Step(false);
                launcher.GetComponent<Animator>().Play("Launch", -1, 0);
            }
            if (PlayerInput.AltPressedUp() && player.isStepping())
            {
                player.Step(true);
                launcher.GetComponent<Animator>().Play("UnStep", -1, 0);
            }
            if (PlayerInput.Pressed())
            {
                if (player.isStepping())
                {
                    launcher.GetComponent<Animator>().Play("UnStep", -1, 0);
                }
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_through");
                player.Slash();
            }
        }

        public void Bop(float beat, float length) 
        {
            bop.length = length;
            bop.startBeat = beat;
        }
    }
}
