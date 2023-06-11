using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Fillbots
{
    public enum BotSize
    {
        Small,
        Medium, 
        Large
    }

    public enum BotVariant
    {
        Normal,
        HoneyBee,
        WarioWare
    }

    public class NtrFillbot : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private BotSize size;
        [SerializeField] private BotVariant variant;
        public double holdLength = 4f;
        [SerializeField] private float limbFallHeight = 15f;

        [Header("Body Parts")]
        [SerializeField] private Animator fullBody;
        [SerializeField] private Animator legs;
        private Transform legsTrans;
        [SerializeField] private Animator body;
        private Transform bodyTrans;
        [SerializeField] private Animator head;
        private Transform headTrans;

        [SerializeField] private Animator fillAnim;

        private float legsPosY;
        private float bodyPosY;
        private float headPosY;

        private double startBeat = -1;

        private bool legsHaveFallen;
        private bool bodyHasFallen;
        private bool headHasFallen;

        private Fillbots game;

        private float startPosX;

        private GameEvent beepEvent;

        private PlayerActionEvent releaseEvent;

        private Sound fillSound;

        private bool holding;

        private void OnDestroy()
        {
            if (fillSound != null) fillSound.KillLoop(0);
        }

        private void Awake()
        {
            game = Fillbots.instance;
            legsTrans = legs.GetComponent<Transform>();
            headTrans = head.GetComponent<Transform>();
            bodyTrans = body.GetComponent<Transform>();

            legsPosY = legsTrans.position.y;
            headPosY = headTrans.position.y;
            bodyPosY = bodyTrans.position.y;

            legsTrans.position = new Vector3(legsTrans.position.x, legsTrans.position.y + limbFallHeight);
            headTrans.position = new Vector3(headTrans.position.x, headTrans.position.y + limbFallHeight);
            bodyTrans.position = new Vector3(bodyTrans.position.x, bodyTrans.position.y + limbFallHeight);

            startPosX = transform.position.x;
        }

        public void Init(double beat)
        {
            startBeat = beat;

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { legs.Play("Impact", 0, 0); legsHaveFallen = true; legsTrans.position = new Vector3(legsTrans.position.x, legsPosY); }),
                new BeatAction.Action(beat + 1, delegate { body.Play("Impact", 0, 0); bodyHasFallen = true; bodyTrans.position = new Vector3(bodyTrans.position.x, bodyPosY);}),
                new BeatAction.Action(beat + 2, delegate { head.Play("Impact", 0, 0); headHasFallen = true; headTrans.position = new Vector3(headTrans.position.x, headPosY);}),
                new BeatAction.Action(beat + 3, delegate
                {
                    fullBody.gameObject.SetActive(true);
                    legs.gameObject.SetActive(false);
                    head.gameObject.SetActive(false);
                    body.gameObject.SetActive(false);
                })
            });

            string sizePrefix = size switch
            {
                BotSize.Small => "small",
                BotSize.Medium => "medium",
                BotSize.Large => "big",
                _ => throw new System.NotImplementedException()
            };

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("fillbots/" + sizePrefix + "Fall", beat),
                new MultiSound.Sound("fillbots/" + sizePrefix + "Fall", beat + 1),
                new MultiSound.Sound("fillbots/" + sizePrefix + "Fall", beat + 2),
            });

            game.ScheduleInput(startBeat, 4, InputType.STANDARD_DOWN, JustHold, HoldOut, HoldOut);
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (startBeat != -1)
                {
                    if (!legsHaveFallen)
                    {
                        float normalizedBeat = cond.GetPositionFromBeat(startBeat - 0.25, 0.25);
                        float lerpedY = Mathf.Lerp(legsPosY + limbFallHeight, legsPosY, normalizedBeat);
                        legsTrans.position = new Vector3(legsTrans.position.x, Mathf.Clamp(lerpedY, legsPosY, legsPosY + limbFallHeight));
                    }
                    if (!bodyHasFallen)
                    {
                        float normalizedBeat = cond.GetPositionFromBeat(startBeat - 0.25 + 1, 0.25);
                        float lerpedY = Mathf.Lerp(bodyPosY + limbFallHeight, bodyPosY, normalizedBeat);
                        bodyTrans.position = new Vector3(bodyTrans.position.x, Mathf.Clamp(lerpedY, bodyPosY, bodyPosY + limbFallHeight));
                    }
                    if (!headHasFallen)
                    {
                        float normalizedBeat = cond.GetPositionFromBeat(startBeat - 0.25 + 2, 0.25);
                        float lerpedY = Mathf.Lerp(headPosY + limbFallHeight, headPosY, normalizedBeat);
                        headTrans.position = new Vector3(headTrans.position.x, Mathf.Clamp(lerpedY, headPosY, headPosY + limbFallHeight));
                    }
                }

                if (beepEvent != null && beepEvent.enabled && cond.ReportBeat(ref beepEvent.lastReportedBeat))
                {
                    if (beepEvent.lastReportedBeat < beepEvent.startBeat + beepEvent.length)
                    {
                        SoundByte.PlayOneShotGame("fillbots/beep");
                    }
                    fullBody.DoScaledAnimationAsync("HoldBeat", 1f);
                    game.filler.DoScaledAnimationAsync("HoldBeat", 1f);
                }

                if (holding)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(startBeat + 4, holdLength);

                    fillAnim.DoNormalizedAnimation("Fill", Mathf.Clamp(normalizedBeat, 0, 1));
                    if (PlayerInput.PressedUp() && !game.IsExpectingInputNow(InputType.STANDARD_UP))
                    {
                        fullBody.Play("Dead", 0, 0);
                        holding = false;
                    }
                }
            }
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                fullBody.Play("HoldBarely", 0, 0);
                return;
            }
            holding = true;
            fullBody.DoScaledAnimationAsync("Hold", 1f);
            game.filler.DoScaledAnimationAsync("Hold", 0.5f);
            SoundByte.PlayOneShotGame("fillbots/armExtension");
            SoundByte.PlayOneShotGame("fillbots/beep");
            fillSound = SoundByte.PlayOneShotGame("fillbots/water", -1, 1 / ((float)holdLength * 0.25f), 1, true);
            releaseEvent = game.ScheduleInput(startBeat + 4, holdLength, InputType.STANDARD_UP, JustRelease, OutRelease, OutRelease);
            beepEvent = new GameEvent()
            {
                startBeat = startBeat + 4,
                lastReportedBeat = startBeat + 4,
                length = (float)holdLength,
                enabled = true
            };
        }

        private void HoldOut(PlayerActionEvent caller)
        {

        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            fillSound.KillLoop(0);
            beepEvent.enabled = false;
            holding = false;
            game.filler.DoScaledAnimationAsync("Release", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SoundByte.PlayOneShotGame("fillbots/armRetraction");
            fullBody.DoScaledAnimationAsync("Release", 1f);
            string sizePrefix = size switch
            {
                BotSize.Small => "small",
                BotSize.Medium => "medium",
                BotSize.Large => "big",
                _ => throw new System.NotImplementedException()
            };
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("fillbots/" + sizePrefix + "Move", caller.startBeat + caller.timer + 0.5),
                new MultiSound.Sound("fillbots/" + sizePrefix + "OK1", caller.startBeat + caller.timer + 0.5),
                new MultiSound.Sound("fillbots/" + sizePrefix + "OK2", caller.startBeat + caller.timer + 1),
            });
        }

        private void OutRelease(PlayerActionEvent caller)
        {

        }
    }
}

