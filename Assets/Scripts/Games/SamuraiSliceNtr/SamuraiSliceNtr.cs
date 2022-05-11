using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSamuraiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("samuraiSliceNtr", "Samurai Slice (DS) \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
                new GameAction("spawn object",                   delegate
                {
                    SamuraiSliceNtr.instance.ObjectIn(eventCaller.currentEntity.beat, eventCaller.currentEntity.type);
                }, 8, false, new List<Param>()
                {
                    new Param("type", SamuraiSliceNtr.ObjectType.Melon, "Object", "The object to spawn")
                }),
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
        public GameObject objectPrefab;
        public Transform objectHolder;

        public BezierCurve3D InCurve;
        public BezierCurve3D LaunchCurve;
        public BezierCurve3D LaunchHighCurve;

        public BezierCurve3D NgLaunchCurve;
        public BezierCurve3D DebrisLeftCurve;
        public BezierCurve3D DebrisRightCurve;

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
                DoStep();
            if (PlayerInput.AltPressedUp() && player.isStepping())
                DoUnStep();
            if (PlayerInput.Pressed())
                DoSlice();
        }

        public void DoStep()
        {
            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchThrough");
            player.Step(false);
            launcher.GetComponent<Animator>().Play("Launch", -1, 0);
        }

        public void DoUnStep()
        {
            player.Step(true);
            launcher.GetComponent<Animator>().Play("UnStep", -1, 0);
        }

        public void DoSlice()
        {
            if (player.isStepping())
            {
                launcher.GetComponent<Animator>().Play("UnStep", -1, 0);
            }
            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_through");
            player.Slash();
        }

        public void Bop(float beat, float length) 
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public void ObjectIn(float beat, int type = (int) ObjectType.Melon)
        {
            var mobj = GameObject.Instantiate(objectPrefab, objectHolder);
            var mobjDat = mobj.GetComponent<NtrSamuraiObject>();
            mobjDat.startBeat = beat;
            mobjDat.type = type;

            mobj.SetActive(true);

            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_in00");
        }
    }
}
