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
        public double holdLength = 4;
        [SerializeField] private float limbFallHeight = 15f;

        [Header("Body Parts")]
        [SerializeField] private Animator fullBody;
        [SerializeField] private Animator legs;
        private Transform legsTrans;
        [SerializeField] private Animator body;
        private Transform bodyTrans;
        [SerializeField] private Animator head;
        private Transform headTrans;

        private float legsPosY;
        private float bodyPosY;
        private float headPosY;

        private double startBeat = -1;

        private bool legsHaveFallen;
        private bool bodyHasFallen;
        private bool headHasFallen;

        private Fillbots game;

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
            }
        }
    }
}

