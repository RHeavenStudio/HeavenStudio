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
                new GameAction("cowbell", "Start Cowbell")
                {
                    preFunction = delegate { Tunnel.PreStartCowbell(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 1f,
                    resizable = false,

                },
                new GameAction("tunnel", "Tunnel")
                {
                    function = delegate { if (Tunnel.instance != null) {
                        var e = eventCaller.currentEntity;
                        Tunnel.instance.StartTunnel(e.beat, e.length, e["volume"] / 100f, e["duration"]);
                    } },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("duration", new EntityTypes.Float(0, 8, 2), "Fade Duration", "The duration of the volume fade in beats"),
                        new Param("volume", new EntityTypes.Float(0, 200, 10), "Volume", "The volume to fade to"),
                    }
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
        const double PostTunnelScrnTime = 0.25;
        public static Tunnel instance { get; set; }

        [Header("Backgrounds")]
        [SerializeField] Transform bg;
        [SerializeField] float bgScrollTime;

        [SerializeField] Material tunnelLightMaterial;
        [SerializeField] Color tunnelTint;
        [SerializeField] Color tunnelScreen;
        [SerializeField] GameObject tunnelWall;
        [SerializeField] SpriteRenderer tunnelWallRenderer;
        [SerializeField] float tunnelChunksPerSec;
        [SerializeField] float tunnelWallChunkSize;

        Vector3 tunnelStartPos;
        Sound tunnelSoundRight, tunnelSoundMiddle, tunnelSoundLeft;

        [Header("References")]
        [SerializeField] GameObject frontHand;

        [Header("Animators")]
        [SerializeField] Animator cowbellAnimator;
        [SerializeField] Animator driverAnimator;

        [Header("Curves")]
        [SerializeField] BezierCurve3D handCurve;

        GameEvent cowbell = new GameEvent();

        float bgStartX;
        float fadeDuration = 2f;
        double tunnelStartTime = double.MinValue;
        double tunnelEndTime = double.MinValue;
        double lastCowbell = double.MaxValue;

        float handStart;
        float handProgress;
        bool inTunnel;

        public struct QueuedCowbell
        {
            public double beat;
            public float length;
        }
        static List<QueuedCowbell> queuedInputs = new List<QueuedCowbell>();

        private void Awake()
        {
            instance = this;
            tunnelStartPos = new Vector3(tunnelWallChunkSize, 0, 0);
        }

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (Conductor.instance != null && !(Conductor.instance.isPlaying || Conductor.instance.isPaused))
            {
                Conductor.instance.FadeMinigameVolume(Conductor.instance.songPositionInBeatsAsDouble, 0, 1);
                tunnelLightMaterial.SetColor("_Color", Color.white);
                tunnelLightMaterial.SetColor("_AddColor", Color.black);

                tunnelSoundRight?.Stop();
                tunnelSoundMiddle?.Stop();
                tunnelSoundLeft?.Stop();
            }
        }

        private void Start()
        {
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

            if (lastCowbell + 1 <= cond.songPositionInBeatsAsDouble)
            {
                lastCowbell++;
                ScheduleInput(lastCowbell, 1, InputAction_BasicPress, CowbellSuccess, CowbellMiss, CowbellEmpty);
            }

            // bg.localPosition = new Vector3(bgStartX - (2 * bgStartX * (((float)Time.realtimeSinceStartupAsDouble % bgScrollTime) / bgScrollTime)), 0, 0);
            if (tunnelWall.activeSelf)
            {
                tunnelWall.transform.localPosition = tunnelStartPos - new Vector3(tunnelChunksPerSec * tunnelWallChunkSize * (float)(cond.songPositionAsDouble - tunnelStartTime), 0, 0);
            }
            if (inTunnel && cond.songPositionAsDouble >= tunnelEndTime + PostTunnelScrnTime)
            {
                cond.FadeMinigameVolume(cond.GetBeatFromSongPos(tunnelEndTime + PostTunnelScrnTime), fadeDuration, 1);
                tunnelLightMaterial.SetColor("_Color", Color.white);
                tunnelLightMaterial.SetColor("_AddColor", Color.black);
                inTunnel = false;
            }
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
            lastCowbell = beat - 1;
            ScheduleInput(lastCowbell, 1, InputAction_BasicPress, CowbellSuccess, CowbellMiss, CowbellEmpty);
        }

        public void CowbellSuccess(PlayerActionEvent caller, float state)
        {
            HitCowbell();
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
            driverAnimator.Play("Angry1", -1, 0);
        }

        public void CowbellEmpty(PlayerActionEvent caller) { }

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

        public void StartTunnel(double beat, double length, float volume = 0.1f, float fadeDuration = 2f)
        {
            Conductor cond = Conductor.instance;
            if (cond.songPositionAsDouble < tunnelEndTime + PostTunnelScrnTime)
            {
                return;
            }
            double targetBeat = beat + length;
            tunnelStartTime = cond.GetSongPosFromBeat(beat);
            tunnelEndTime = cond.GetSongPosFromBeat(targetBeat);
            // tunnel chunks can be divided into quarters
            double durationSec = Math.Ceiling((tunnelEndTime - tunnelStartTime) * 4 * tunnelChunksPerSec) * 0.25 / tunnelChunksPerSec;

            tunnelWallRenderer.size = new Vector2((float)durationSec * tunnelWallChunkSize * tunnelChunksPerSec, 13.7f);
            tunnelWall.transform.localPosition = tunnelStartPos;
            tunnelWall.SetActive(true);
            this.fadeDuration = fadeDuration;
            cond.FadeMinigameVolume(beat, fadeDuration, volume);

            tunnelSoundRight?.Stop();
            tunnelSoundMiddle?.Stop();
            tunnelSoundLeft?.Stop();

            tunnelSoundRight = SoundByte.PlayOneShotGame("tunnel/tunnelRight", beat, looping: true);
            tunnelSoundMiddle = SoundByte.PlayOneShotGame("tunnel/tunnelMiddle", beat + (6 / 48f), looping: true);
            tunnelSoundLeft = SoundByte.PlayOneShotGame("tunnel/tunnelLeft", beat + (12 / 48f), looping: true);

            double tunnelEnd = cond.GetBeatFromSongPos(tunnelEndTime + PostTunnelScrnTime);
            tunnelSoundRight.SetLoopParams(tunnelEnd, 0.1);
            tunnelSoundMiddle.SetLoopParams(tunnelEnd + (6 / 48f), 0.1);
            tunnelSoundLeft.SetLoopParams(tunnelEnd + (12 / 48f), 0.25);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(cond.GetBeatFromSongPos(tunnelStartTime + 0.25), delegate {
                    tunnelLightMaterial.SetColor("_Color", tunnelTint);
                    tunnelLightMaterial.SetColor("_AddColor", tunnelScreen);
                }),
            });
            inTunnel = true;
        }
    }
}
