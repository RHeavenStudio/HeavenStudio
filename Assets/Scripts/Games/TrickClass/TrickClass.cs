using DG.Tweening;
using NaughtyBezierCurves;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class MobTrickLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("trickClass", "Trick on the Class", "C0171D", false, false, new List<GameAction>()
            {
                new GameAction("toss", "Toss Object")
                {
                    function = delegate
                    {
                        TrickClass.instance.TossObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);
                    }, 
                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", TrickClass.TrickObjType.Ball, "Object", "The object to toss")
                    }
                },
                new GameAction("bop", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; TrickClass.instance.Bop(e.beat, e.length); },
                    resizable = true, 
                    hidden = true
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    /**
        mob_Trick
    **/

    using Scripts_TrickClass;
    public class TrickClass : Minigame
    {
        public enum TrickObjType {
            Ball,
            Plane,
        }

        [Header("Objects")]
        public Animator playerAnim;
        public Animator girlAnim;
        public Animator warnAnim;

        [Header("References")]
        public GameObject ballPrefab;
        public GameObject planePrefab;
        public GameObject shockPrefab;
        public Transform objHolder;

        [Header("Curves")]
        public BezierCurve3D ballTossCurve;
        public BezierCurve3D ballMissCurve;
        public BezierCurve3D planeTossCurve;
        public BezierCurve3D planeMissCurve;
        public BezierCurve3D shockTossCurve;

        public static TrickClass instance;
        public GameEvent bop = new GameEvent();

        public float playerCanDodge = Single.MinValue;
        float playerBopStart = Single.MinValue;
        float girlBopStart = Single.MinValue;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats > playerBopStart)
                    playerAnim.DoScaledAnimationAsync("Bop");

                if (cond.songPositionInBeats > girlBopStart)
                    girlAnim.DoScaledAnimationAsync("Bop");
            }

            if (PlayerInput.Pressed() && !IsExpectingInputNow() && (playerCanDodge <= Conductor.instance.songPositionInBeats))
            {
                PlayerDodge(true);
                playerCanDodge = Conductor.instance.songPositionInBeats + 0.6f;
            }

            // bruh
            var tossEvents = GameManager.instance.Beatmap.entities.FindAll(en => en.datamodel == "trickClass/toss");
            for (int i = 0; i < tossEvents.Count; i++)
            {
                var e = tossEvents[i];
                float timeToEvent = e.beat - cond.songPositionInBeats;
                warnAnim.Play("NoPose", -1, 0);
                if (timeToEvent > 0f && timeToEvent <= 1f)
                {
                    string anim = "WarnBall";
                    switch (e["type"])
                    {
                        case (int) TrickObjType.Plane:
                            anim = "WarnPlane";
                            break;
                        default:
                            anim = "WarnBall";
                            break;
                    }
                    warnAnim.DoScaledAnimation(anim, e.beat - 1f, 1f);
                    break;
                }
            }
        }

        public void Bop(float beat, float length)
        {
            bop.startBeat = beat;
            bop.length = length;
        }

        public void TossObject(float beat, int type)
        {
            switch (type)
            {
                case (int) TrickObjType.Plane:
                    Jukebox.PlayOneShotGame("trickClass/girl_toss_plane");
                    break;
                default:
                    Jukebox.PlayOneShotGame("trickClass/girl_toss_ball");
                    break;
            }
            SpawnObject(beat, type);

            girlAnim.DoScaledAnimationAsync("Throw");
            girlBopStart = Conductor.instance.songPositionInBeats + 0.75f;
        }

        public void SpawnObject(float beat, int type)
        {
            GameObject objectToSpawn;
            BezierCurve3D curve;
            bool isPlane = false;
            switch (type)
            {
                case (int) TrickObjType.Plane:
                    objectToSpawn = planePrefab;
                    curve = planeTossCurve;
                    isPlane = true;
                    break;
                default:
                    objectToSpawn = ballPrefab;
                    curve = ballTossCurve;
                    break;
            }
            var mobj = GameObject.Instantiate(objectToSpawn, objHolder);
            var thinker = mobj.GetComponent<MobTrickObj>();

            thinker.startBeat = beat;
            thinker.flyType = isPlane;
            thinker.curve = curve;
            thinker.type = type;

            mobj.SetActive(true);
        }

        public void PlayerDodge(bool slow = false)
        {
            if (playerCanDodge > Conductor.instance.songPositionInBeats) return;

            //anim
            Jukebox.PlayOneShotGame("trickClass/player_dodge");
            playerAnim.DoScaledAnimationAsync("Dodge", slow ? 0.6f : 1f);
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            
        }

        public void PlayerDodgeNg()
        {
            playerAnim.DoScaledAnimationAsync("DodgeNg");
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeats + 0.15f;
        }

        public void PlayerThrough()
        {
            playerAnim.DoScaledAnimationAsync("Through");
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeats + 0.15f;
        }
    }
}