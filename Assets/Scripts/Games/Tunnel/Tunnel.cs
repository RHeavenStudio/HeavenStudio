using DG.Tweening;
using NaughtyBezierCurves;
using  HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrTunnelLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tunnel", "Tunnel \n<color=#eb5454>[WIP]</color>", "B4E6F6", false, false, new List<GameAction>()
            {


                new GameAction("cowbell", "Cowbell")
                {
                    function = delegate { Tunnel.instance.StartCowbell(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity.length); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Driver can stop", "Lets the driver stop if the player makes too many mistakes"),
                    }

                },
                new GameAction("countin", "Count In")
                {
                    function = delegate { Tunnel.instance.CountIn(eventCaller.currentEntity.beat,  eventCaller.currentEntity.length); }, 
                    defaultLength = 3f, 
                    resizable = true,
                }

            }
            //new List<string>() {"ntr", "aim"},
            //"ntrcoin", "en",
            //new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class Tunnel : Minigame
    {

        public static Tunnel instance { get; set; }

        

        [Header("Backgrounds")]
        public SpriteRenderer fg;
        public SpriteRenderer bg;

        Tween bgColorTween;
        Tween fgColorTween;


        [Header("References")]
        public GameObject frontHand;


        [Header("Animators")]
        public Animator cowbellAnimator;
        public Animator driverAnimator;

        [Header("Curves")]
        public BezierCurve3D handCurve;


        public GameEvent cowbell = new GameEvent();


        public int driverState;

        public float handStart;
        public float handProgress;
        public bool started;

        private void Awake()
        {
            instance = this;
        }


        private void Start()
        {
            driverState = 0;
            handStart = -1f;
        }

        private void Update()
        {

            var cond = Conductor.instance;
            if (cond.ReportBeat(ref cowbell.lastReportedBeat, cowbell.startBeat % 1))
            {
                if (cond.songPositionInBeats >= cowbell.startBeat && cond.songPositionInBeats < cowbell.startBeat + cowbell.length)
                {
                    ScheduleInput(cond.songPositionInBeats, 1, InputType.STANDARD_DOWN, CowbellSuccess, CowbellMiss, CowbellEmpty);
                }
            }


            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                HitCowbell();
                //print("unexpected input");
                driverAnimator.Play("Angry1", -1, 0);
            }


            //update hand position
            handProgress = Math.Min(Conductor.instance.songPositionInBeats - handStart, 1);


            frontHand.transform.position = handCurve.GetPoint(EasingFunction.EaseOutQuad(0, 1, handProgress));

        }

        private void LateUpdate()
        {
            //nothing
        }


        public void HitCowbell()
        {
            Jukebox.PlayOneShot("count-ins/cowbell");

            handStart = Conductor.instance.songPositionInBeats;
            
            cowbellAnimator.Play("Shake",-1,0);
        }

        public void StartCowbell(float beat, bool driverStops, float length)
        {
            started = true;

            cowbell.length = length;
            cowbell.startBeat = beat;


            //for (int i = 1; i <= length; i++)
            //{
            //ScheduleInput(beat, i, InputType.STANDARD_DOWN, CowbellSuccess, CowbellMiss, CowbellEmpty);
            //}

            
        }

        public void CowbellSuccess(PlayerActionEvent caller, float state)
        {
            HitCowbell();
            //print(state);
            if(Math.Abs(state) >= 0.5)
            {
                driverAnimator.Play("Disturbed", -1, 0);

            }
            else
            {
                driverAnimator.Play("Idle", -1, 0);
            }

        }


        public void CowbellMiss(PlayerActionEvent caller)
        {
            //HitCowbell();

            driverAnimator.Play("Angry1", -1, 0);
        }

        public void CowbellEmpty(PlayerActionEvent caller)
        {
            //HitCowbell();
        }


        
        public void CountIn(float beat, float length)
        {

            List<MultiSound.Sound> cuelist = new List<MultiSound.Sound>();
            

            for (int i = 0; i <= length; i++)
            {
                if(i % 2 == 0)
                {
                    //Jukebox.PlayOneShotGame("tunnel/en/one", beat+i);
                    //print("cueing one at " + (beat + i));
                    cuelist.Add(new MultiSound.Sound("tunnel/en/one", beat + i));
                }
                else
                {
                    //Jukebox.PlayOneShotGame("tunnel/en/two", beat+i);
                    //print("cueing two at " + (beat + i));
                    cuelist.Add(new MultiSound.Sound("tunnel/en/two", beat + i));
                }
                
            }
            MultiSound.Play(cuelist.ToArray());

        }



    }
}
