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
                    preFunction = delegate { Tunnel.PreStartCowbell(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 4f,
                    resizable = true,

                },
                new GameAction("countin", "Count In")
                {
                    preFunction = delegate { Tunnel.CountIn(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    defaultLength = 4f, 
                    resizable = true,
                }
            });
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
        public struct QueuedCowbell
        {
            public float beat;
            public float length;
        }
        static List<QueuedCowbell> queuedInputs = new List<QueuedCowbell>();

        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
        }

        private void Start()
        {
            driverState = 0;
            handStart = -1f;
        }

        private void Update()
        {

            var cond = Conductor.instance;
            //update hand position
            handProgress = Math.Min(Conductor.instance.songPositionInBeats - handStart, 1);


            frontHand.transform.position = handCurve.GetPoint(EasingFunction.EaseOutQuad(0, 1, handProgress));
            if (!cond.isPlaying || cond.isPaused)
            {
                return;
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                HitCowbell();
                //print("unexpected input");
                driverAnimator.Play("Angry1", -1, 0);
            }
            if (queuedInputs.Count > 0)
            {
                foreach (var input in queuedInputs)
                {
                    StartCowbell(input.beat, input.length);
                }
                queuedInputs.Clear();
            }

        }


        public void HitCowbell()
        {
            Jukebox.PlayOneShot("count-ins/cowbell");

            handStart = Conductor.instance.songPositionInBeats;
            
            cowbellAnimator.Play("Shake",-1,0);
        }

        public static void PreStartCowbell(float beat, float length)
        {
            if (GameManager.instance.currentGame == "tunnel")
            {
                instance.StartCowbell(beat, length);
            }
            else
            {
                queuedInputs.Add(new QueuedCowbell { beat = beat, length = length });
            }
        }

        public void StartCowbell(float beat, float length)
        {
            started = true;
            for(int i = 0; i < length; i++)
            {
                ScheduleInput(beat, i, InputType.STANDARD_DOWN, CowbellSuccess, CowbellMiss, CowbellEmpty);
            }
        }

        public void CowbellSuccess(PlayerActionEvent caller, float state)
        {
            HitCowbell();
            //print(state);
            if(Math.Abs(state) >= 1f)
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


        
        public static void CountIn(float beat, float length)
        {

            List<MultiSound.Sound> cuelist = new List<MultiSound.Sound>();
            

            for (int i = 0; i < length; i++)
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
            MultiSound.Play(cuelist.ToArray(), forcePlay: true);

        }



    }
}
