using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using DG.Tweening;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSamuraiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("samuraiSliceNtr", "Samurai Slice (DS)", "b6b5b6", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; SamuraiSliceNtr.instance.Bop(e.beat, e.length, e["whoBops"], e["whoBopsAuto"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", SamuraiSliceNtr.WhoBops.Both, "Who Bops?", "Who will bop?"),
                        new Param("whoBopsAuto", SamuraiSliceNtr.WhoBops.None, "Who Bops? (Auto)", "Who will automatically begin bopping?")
                    }
                },
                new GameAction("melon", "Melon")
                {
                    function = delegate
                    {
                        SamuraiSliceNtr.instance.ObjectIn(eventCaller.currentEntity.beat, (int)SamuraiSliceNtr.ObjectType.Melon, (int) eventCaller.currentEntity["valA"], eventCaller.currentEntity["2b2t"]);
                    }, 
                    defaultLength = 5,
                    parameters = new List<Param>()
                    {
                        new Param("2b2t", false, "Melon2B2T", "Should the melon be reskinned as the 2B2T melon?"),
                        new Param("valA", new EntityTypes.Integer(0, 30, 1), "Money", "The amount of coins the melon spills out when sliced"),
                    }
                },
                new GameAction("fish", "Fish")
                {
                    function = delegate
                    {
                        SamuraiSliceNtr.instance.ObjectIn(eventCaller.currentEntity.beat, (int)SamuraiSliceNtr.ObjectType.Fish, (int) eventCaller.currentEntity["valA"]);
                    },
                    defaultLength = 7,
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(0, 30, 1), "Money", "The amount of coins the fish spills out when sliced"),
                    }
                },
                new GameAction("demon", "Demon")
                {
                    function = delegate
                    {
                        SamuraiSliceNtr.instance.ObjectIn(eventCaller.currentEntity.beat, (int)SamuraiSliceNtr.ObjectType.Demon, (int) eventCaller.currentEntity["valA"]);
                    },
                    defaultLength = 7,
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(0, 30, 1), "Money", "The amount of coins the demon spills out when sliced"),
                    }
                },
                //backwards compatibility
                new GameAction("spawn object", "Toss Object")
                {
                    function = delegate
                    {
                        SamuraiSliceNtr.instance.ObjectIn(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], (int) eventCaller.currentEntity["valA"]);
                    },
                    defaultLength = 8,
                    parameters = new List<Param>()
                    {
                        new Param("type", SamuraiSliceNtr.ObjectType.Melon, "Object", "The object to spawn"),
                        new Param("valA", new EntityTypes.Integer(0, 30, 1), "Money", "The amount of coins the melon spills out when sliced"),
                    },
                    hidden = true
                },
            },
            new List<string>() {"ntr", "normal"},
            "ntrsamurai", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using JetBrains.Annotations;
    using Scripts_NtrSamurai;

    public class SamuraiSliceNtr : Minigame
    {
        public enum ObjectType {
            Melon,
            Fish,
            Demon,
            Melon2B2T,
        }

        public enum WhoBops
        {
            Samurai = 2,
            Children = 1,
            Both = 0,
            None = 3
        }

        [Header("References")]
        public NtrSamurai player;
        public GameObject launcher;
        public GameObject objectPrefab;
        public GameObject childParent;
        public Transform objectHolder;

        public BezierCurve3D InCurve;
        public BezierCurve3D LaunchCurve;
        public BezierCurve3D LaunchHighCurve;

        public BezierCurve3D NgLaunchCurve;
        public BezierCurve3D DebrisLeftCurve;
        public BezierCurve3D DebrisRightCurve;
        public BezierCurve3D NgDebrisCurve;

        //game scene
        public static SamuraiSliceNtr instance;

        public GameEvent bop = new GameEvent();

        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltDown =
            new("NtrSamuraiAltDown", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltUp =
            new("NtrSamuraiAltUp", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchBasicRelease, IA_BatonAltRelease);

        private void Awake()
        {
            instance = this;
            SetupBopRegion("samuraiSliceNtr", "bop", "whoBopsAuto", false);
        }

        public override void OnBeatPulse(double beat)
        {
            int whoBopsAuto = BeatIsInBopRegionInt(beat);
            bool goBopSamurai = whoBopsAuto == (int)WhoBops.Samurai || whoBopsAuto == (int)WhoBops.Both;
            bool goBopChild = whoBopsAuto == (int)WhoBops.Children || whoBopsAuto == (int)WhoBops.Both;

            if (goBopSamurai) player.Bop();
            if (goBopChild) childParent.GetComponent<NtrSamuraiChild>().Bop();
        }

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_AltDown))
                DoStep();
            if (PlayerInput.GetIsAction(InputAction_AltUp) && player.isStepping())
                DoUnStep();
            if (PlayerInput.GetIsAction(InputAction_FlickPress))
                DoSlice();
        }

        public void Bop(double beat, float length, int whoBops, int whoBopsAuto)
        {
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate { BopSingle(whoBops); })
                });
            }
        }

        void BopSingle(int whoBops)
        {
            switch (whoBops)
            {
                case (int)WhoBops.Samurai:
                    player.Bop();
                    break;
                case (int)WhoBops.Children:
                    childParent.GetComponent<NtrSamuraiChild>().Bop();
                    break;
                case (int)WhoBops.Both:
                    player.Bop();
                    childParent.GetComponent<NtrSamuraiChild>().Bop();
                    break;
                default:
                    break;
            }
        }

        public void DoStep()
        {
            SoundByte.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchThrough");
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
            SoundByte.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_through");
            player.Slash();
        }

        public void Bop(double beat, float length) 
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public void ObjectIn(double beat, int type = (int) ObjectType.Melon, int value = 1, bool funnyMinecraft = false)
        {
            var mobj = GameObject.Instantiate(objectPrefab, objectHolder);
            var mobjDat = mobj.GetComponent<NtrSamuraiObject>();
            mobjDat.startBeat = beat;
            mobjDat.type = funnyMinecraft ? (int)ObjectType.Melon2B2T : type;
            mobjDat.holdingCash = value;

            mobj.SetActive(true);

            SoundByte.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_in00");
        }

        public NtrSamuraiChild CreateChild(double beat)
        {
            var mobj = GameObject.Instantiate(childParent, objectHolder);
            var mobjDat = mobj.GetComponent<NtrSamuraiChild>();
            mobjDat.startBeat = beat;
            mobjDat.isMain = false;

            mobjDat.Bop();

            mobj.SetActive(true);
            mobj.GetComponent<SortingGroup>().sortingOrder = 7;

            return mobjDat;
        }
    }
}
