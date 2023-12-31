using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class Meat : MonoBehaviour
    {
        [SerializeField] Transform startPosition;
        [SerializeField] Transform startPositionAlt;
        [SerializeField] Transform hitPosition;
        [SerializeField] Transform missPosition;
        [SerializeField] float meatFlyHeight = 1f;
        [SerializeField] float meatFlyHeightAlt = 1f;
        [SerializeField] bool showAltCurve;

        [NonSerialized] public double startBeat;
        [NonSerialized] public MeatType meatType;
        [NonSerialized] public MeatGrinder.Reaction tackReaction;
        [NonSerialized] public MeatGrinder.Reaction bossReaction;

        private bool isHit = false;

        // const float meatStart = 0;
        // const float meatEnd = 3.43f;

        [Header("Animators")]
        private Animator anim;
        private SpriteRenderer sr;
        [SerializeField] private Sprite[] meats;

        public enum MeatType
        {
            DarkMeat,
            LightMeat,
            BaconBall,
        }

        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
            anim = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
            // anim.writeDefaultValuesOnDisable = false;
        }

        private void Start()
        {
            sr.sprite = meats[(int)meatType];

            game.ScheduleInput(startBeat, 1, MeatGrinder.InputAction_Press, Hit, Miss, Nothing);
        }

        private void Update()
        {
            Conductor cond = Conductor.instance;
            if (!isHit)
            {
                double startTime = cond.GetSongPosFromBeat(startBeat);
                double hitTime = cond.GetSongPosFromBeat(startBeat + 1);
                double missTime = hitTime + MeatGrinder.ngLateTime;
                double currentTime = cond.songPositionAsDouble;
                Vector3 lastPos = transform.position;
                Vector3 startPos = meatType == MeatType.LightMeat ? startPositionAlt.position : startPosition.position;

                float hitAlongMissRatio = Vector3.Dot((startPos - missPosition.position), (startPos - hitPosition.position));
                hitAlongMissRatio /= Vector3.Dot((startPos - missPosition.position), (startPos - missPosition.position));
                Vector3 hitOnMissPos = startPos + ((missPosition.position - startPos) * hitAlongMissRatio);

                float prog;

                if (currentTime >= hitTime)
                {
                    prog = (float)((currentTime - hitTime) / (missTime - hitTime));
                    transform.position = Vector3.Lerp(hitOnMissPos, missPosition.position, prog);

                    prog = (prog * (1 - hitAlongMissRatio)) + hitAlongMissRatio;
                }
                else
                {
                    prog = (float)((currentTime - startTime) / (hitTime - startTime));
                    transform.position = Vector3.Lerp(startPos, hitOnMissPos, prog);

                    prog *= hitAlongMissRatio;
                }
                float yMul = prog * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                transform.position += (meatType == MeatType.LightMeat ? meatFlyHeightAlt : meatFlyHeight) * yWeight * Vector3.up;
                // point towards the next position
                if (cond.isPlaying)
                {
                    transform.right = transform.position - lastPos;
                }
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            anim.enabled = true;
            isHit = true;
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync(meatType.ToString() + "Hit", 0.5f);

            bool isBarely = state is >= 1f or <= -1f;

            game.bossAnnoyed = isBarely;
            SoundByte.PlayOneShotGame("meatGrinder/" + (isBarely ? "tink" : "meatHit"));
            game.TackAnim.DoScaledAnimationAsync("TackHit" + (isBarely ? "Barely" : "Success"), 0.5f);

            (float rangeStart, float rangeEnd) = meatType switch { // im not good enough at math to figure out how to make this an equation
                MeatType.DarkMeat => (0, 0.333f),
                MeatType.LightMeat => (0.334f, 0.666f),
                MeatType.BaconBall or _ => (0.667f, 1),
            };
            var sheetAnim = game.MeatSplash.textureSheetAnimation;
            var main = game.MeatSplash.main;
            sheetAnim.frameOverTime = new ParticleSystem.MinMaxCurve(rangeStart, rangeEnd);
            main.simulationSpeed = 0.5f / Conductor.instance.pitchedSecPerBeat;

            // has a probability of zero normally so it's not played with Play() but this exposes it to the editor
            var emission = game.MeatSplash.emission;
            game.MeatSplash.Emit(new ParticleSystem.EmitParams(), (int)emission.GetBurst(0).count.constant);

            if (tackReaction.expression > 0) {
                BeatAction.New(game, new List<BeatAction.Action>() {
                    new(startBeat + tackReaction.beat + 1, delegate { game.DoExpressions(tackReaction.expression); }),
                });
            }
            if (bossReaction.expression > 0) {
                BeatAction.New(game, new List<BeatAction.Action>() {
                    new(startBeat + bossReaction.beat + 1, delegate { game.DoExpressions(0, bossReaction.expression); }),
                });
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            anim.enabled = true;
            game.bossAnnoyed = true;
            SoundByte.PlayOneShotGame("meatGrinder/miss");

            game.TackAnim.DoScaledAnimationAsync("TackMiss" + meatType.ToString(), 0.5f);
            game.TackAnim.SetBool("tackMeated", true);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);

            Destroy(gameObject);
        }

        private void Nothing(PlayerActionEvent caller) { }

        public void SetSprite()
        {
            sr.sprite = meats[(int)meatType];
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Callback to draw gizmos that are pickable and always drawn.
        /// </summary>
        private void OnDrawGizmos()
        {
            // draw a line showing the direction from the miss position to the hit position
            if (startPosition == null) return;
            if (startPositionAlt == null) return;

            Vector3 startPos = showAltCurve ? startPositionAlt.position : startPosition.position;
            if (hitPosition != null && missPosition != null && startPos != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(startPos, hitPosition.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(startPos, missPosition.position);
                Gizmos.color = Color.green;
                Vector3 direction = (hitPosition.position - missPosition.position).normalized;
                Gizmos.DrawRay(missPosition.position, direction * 10);

                // project start position -> hit position onto start position -> miss position
                float hitAlongMissRatio = Vector3.Dot((startPos - missPosition.position), (startPos - hitPosition.position));
                hitAlongMissRatio /= Vector3.Dot((startPos - missPosition.position), (startPos - missPosition.position));
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(startPos, (missPosition.position - startPos) * hitAlongMissRatio);

                DrawCurveGizmo(startPos, missPosition.position, Vector3.up * (showAltCurve ? meatFlyHeightAlt : meatFlyHeight), Color.yellow, 0.1f);
            }
        }

        void DrawCurveGizmo(Vector3 start, Vector3 end, Vector3 height, Color colour, float interval)
        {
            Gizmos.color = colour;
            Vector3 lastPos = start;
            Vector3 pos;
            float t, yMul, yWeight;
            for (float i = 0; i < 1; i += interval)
            {
                t = i;
                pos = Vector3.Lerp(start, end, t);

                yMul = t * 2f - 1f;
                yWeight = -(yMul * yMul) + 1f;

                pos += height * yWeight;
                Gizmos.DrawLine(lastPos, pos);
                lastPos = pos;
            }
            t = 1f;
            pos = end;

            yMul = t * 2f - 1f;
            yWeight = -(yMul * yMul) + 1f;

            pos += height * yWeight;
            Gizmos.DrawLine(lastPos, pos);
        }
    }
}
