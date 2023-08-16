using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TrickClass
{
    public class MobTrickObj : MonoBehaviour
    {
        public bool flyType;
        public double startBeat;
        bool miss = false;

        float flyBeats;
        float dodgeBeats;
        public int type;

        [NonSerialized] public BezierCurve3D curve;
        PlayerActionEvent hitProg;

        private TrickClass game;

        private void Awake()
        {
            game = TrickClass.instance;
            flyBeats = flyType ? 4f : 2f;
            dodgeBeats = flyType ? 2f : 1f;

            var cond = Conductor.instance;

            float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
            transform.position = curve.GetPoint(flyPos);
            hitProg = game.ScheduleInput(startBeat, dodgeBeats, InputType.STANDARD_DOWN, DodgeJustOrNg, DodgeMiss, DodgeThrough, CanDodge);
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);

            if (flyPos <= 1f) 
            {
                if (!miss)
                {
                    flyPos *= 0.95f;
                }
                Vector3 lastPos = transform.position;
                Vector3 nextPos = curve.GetPoint(flyPos);

                if (flyType)
                {
                    Vector3 direction = (nextPos - lastPos).normalized;
                    float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    this.transform.eulerAngles = new Vector3(0, 0, rotation);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * Time.deltaTime));
                }

                transform.position = nextPos;
            }
            else
            {
                transform.position = curve.GetPoint(miss ? 1f : 0.95f);
            }

            if (flyPos > 1f)
            {
                if (Conductor.instance.GetPositionFromBeat(startBeat, flyBeats + 1f) >= 1f)
                {
                    GameObject.Destroy(gameObject);
                    return;
                }
            }
        }

        public string GetDodgeSound()
        {
            switch (type)
            {
                default:
                    return "trickClass/ball_impact";
            }
        }

        public void DoObjMiss()
        {
            miss = true;
            switch (type)
            {
                case (int) TrickClass.TrickObjType.Plane:
                    curve = game.planeMissCurve;
                    flyBeats = 4f;

                    Vector3 lastPos = curve.GetPoint(0);
                    Vector3 nextPos = curve.GetPoint(0.000001f);
                    Vector3 direction = (nextPos - lastPos).normalized;
                    float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    this.transform.eulerAngles = new Vector3(0, 0, rotation);

                    transform.position = nextPos;
                    break;
                default:
                    curve = game.ballMissCurve;
                    flyBeats = 1.25f;
                    break;
            }
            startBeat += dodgeBeats;
        }

        public bool CanDodge()
        {
            return game.playerCanDodge <= Conductor.instance.songPositionInBeatsAsDouble;
        }

        public void DodgeJustOrNg(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f)
            {
                //NG
                game.PlayerDodgeNg();
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound(GetDodgeSound(), startBeat + flyBeats, volume: 0.4f), 
                });
                SoundByte.PlayOneShotGame(GetDodgeSound(), volume: 0.6f);
                SoundByte.PlayOneShot("miss");
                DoObjMiss();
            }
            else
            {
                //just
                game.PlayerDodge();
                SoundByte.PlayOneShotGame("trickClass/player_dodge_success", volume: 0.8f, pitch: UnityEngine.Random.Range(0.85f, 1.15f));
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound(GetDodgeSound(), startBeat + flyBeats, volume: 0.4f), 
                });
            }
        }

        public void DodgeMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame(GetDodgeSound());
            DoObjMiss();
            game.PlayerThrough();
        }

        public void DodgeThrough(PlayerActionEvent caller) {}
    }
}