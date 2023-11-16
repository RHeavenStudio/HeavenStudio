using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
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
            return new Minigame("tunnel", "Tunnel \n<color=#eb5454>[WIP]</color>", "c00000", false, false, new List<GameAction>()
            {
                new GameAction("cowbell", "Cowbell")
                {
                    preFunction = delegate { Tunnel.PreStartCowbell(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 4f,
                    resizable = true,

                },
                new GameAction("tunnel", "Start Tunnel")
                {
                    function = delegate { if (Tunnel.instance != null) { Tunnel.instance.StartTunnel(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); } },
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("countin", "Count In")
                {
                    preFunction = delegate { Tunnel.CountIn(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 4f,
                    resizable = true,
                }
            },
            new List<string>() { "ntr", "keep" },
            "ntrtunnel", "en",
            new List<string>() { "en" }
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
        [SerializeField] Transform bg;
        [SerializeField] float bgScrollTime;

        [SerializeField] GameObject tunnelWall;
        [SerializeField] SpriteRenderer tunnelWallRenderer;
        [SerializeField] float tunnelChunksPerSec;
        [SerializeField] float tunnelWallChunkSize;

        Tween bgColorTween;
        Tween fgColorTween;


        [Header("References")]
        [SerializeField] GameObject frontHand;


        [Header("Animators")]
        [SerializeField] Animator cowbellAnimator;
        [SerializeField] Animator driverAnimator;

        [Header("Curves")]
        [SerializeField] BezierCurve3D handCurve;


        GameEvent cowbell = new GameEvent();


        int driverState;

        float bgStartX;

        float handStart;
        float handProgress;
        bool started;
        public struct QueuedCowbell
        {
            public double beat;
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
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Start()
        {
            driverState = 0;
            handStart = -1f;
            tunnelWall.SetActive(false);
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
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
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

            bg.position = new Vector3(bgStartX - (2 * bgStartX * (((float)Time.realtimeSinceStartupAsDouble % bgScrollTime) / bgScrollTime)), 0, 0);
            tunnelWall.transform.position -= new Vector3(tunnelChunksPerSec * tunnelWallChunkSize * Time.deltaTime * cond.SongPitch, 0, 0);
        }


        public void HitCowbell()
        {
            SoundByte.PlayOneShot("count-ins/cowbell");

            handStart = Conductor.instance.songPositionInBeats;

            cowbellAnimator.Play("Shake", -1, 0);
        }

        public static void PreStartCowbell(double beat, float length)
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

        public void StartCowbell(double beat, float length)
        {
            started = true;
            for (int i = 0; i < length; i++)
            {
                ScheduleInput(beat, i, InputAction_BasicPress, CowbellSuccess, CowbellMiss, CowbellEmpty);
            }
        }

        public void CowbellSuccess(PlayerActionEvent caller, float state)
        {
            HitCowbell();
            //print(state);
            if (Math.Abs(state) >= 1f)
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



        public static void CountIn(double beat, float length)
        {

            List<MultiSound.Sound> cuelist = new List<MultiSound.Sound>();


            for (int i = 0; i < length; i++)
            {
                if (i % 2 == 0)
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


        public void StartTunnel(double beat, double length)
        {
            double targetBeat = beat + length;
            double startTimeSec = Conductor.instance.GetSongPosFromBeat(beat);
            double targetTimeSec = Conductor.instance.GetSongPosFromBeat(targetBeat);
            // tunnel chunks can be divided into quarters
            double durationSec = Math.Ceiling((targetTimeSec - startTimeSec) * 4) * 0.25;

            tunnelWallRenderer.size = new Vector2((float)durationSec * tunnelWallChunkSize * tunnelChunksPerSec, 13.7f);
            tunnelWall.transform.position = new Vector3(tunnelWallChunkSize, 0, 0);
            tunnelWall.SetActive(true);
        }
    }
}
