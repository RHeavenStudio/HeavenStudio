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
        [NonSerialized] public double startBeat;
        bool miss = false;

        [SerializeField] public byte flyType;
        [SerializeField] float flyBeats;
        [SerializeField] float dodgeBeats;
        [SerializeField] TrickClass.TrickObjType type;
        [SerializeField] string justSound;
        [SerializeField] string missSound;

        [NonSerialized] public BezierCurve3D curve;

        private TrickClass game;

        private void Awake()
        {
            game = TrickClass.instance;

            var cond = Conductor.instance;

            float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
            transform.position = curve.GetPoint(flyPos);
            game.ScheduleInput(startBeat, dodgeBeats, TrickClass.InputAction_FlickPress, DodgeJustOrNg, DodgeMiss, DodgeThrough, CanDodge);
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

                switch (flyType)
                {
                    case 1:
                        Vector3 direction = (nextPos - lastPos).normalized;
                        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        transform.eulerAngles = new Vector3(0, 0, rotation);
                        break;
                    case 2:
                        transform.rotation = Quaternion.identity;
                        break;
                    default:
                        transform.eulerAngles = new Vector3(0, 0, transform.rotation.eulerAngles.z + (360f * Time.deltaTime * cond.SongPitch));
                        break;
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
                    Destroy(gameObject);
                    return;
                }
            }
        }

        public string GetMissSound()
        {
            return $"trickClass/{missSound}";
        }

        public string GetJustSound()
        {
            return $"trickClass/{justSound}";
        }

        public void DoObjMiss(bool ng = false)
        {
            miss = true;
            switch (type)
            {
                case TrickClass.TrickObjType.Plane:
                    startBeat += dodgeBeats;
                    curve = game.planeMissCurve;
                    flyBeats = 4f;

                    Vector3 lastPos = curve.GetPoint(0);
                    Vector3 nextPos = curve.GetPoint(0.000001f);
                    Vector3 direction = (nextPos - lastPos).normalized;
                    float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    this.transform.eulerAngles = new Vector3(0, 0, rotation);

                    transform.position = nextPos;
                    break;
                case TrickClass.TrickObjType.Shock:
                    if (!ng)
                    {
                        Destroy(gameObject);
                    }
                    break;
                default:
                    startBeat += dodgeBeats;
                    curve = game.ballMissCurve;
                    flyBeats = 1.25f;
                    break;
            }
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
                    new MultiSound.Sound(GetMissSound(), startBeat + flyBeats, volume: 0.4f),
                });
                SoundByte.PlayOneShotGame(GetMissSound(), volume: 0.6f);
                SoundByte.PlayOneShot("miss");
                DoObjMiss(true);
            }
            else
            {
                //just
                game.PlayerDodge();
                SoundByte.PlayOneShotGame(GetJustSound(), volume: 0.8f, pitch: UnityEngine.Random.Range(0.85f, 1.15f));
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(GetMissSound(), startBeat + flyBeats, volume: 0.4f),
                });
            }
        }

        public void DodgeMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame(GetMissSound());
            DoObjMiss(false);
            game.PlayerThrough(type == TrickClass.TrickObjType.Shock);
        }

        public void DodgeThrough(PlayerActionEvent caller) { }
    }
}