using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Rod : MonoBehaviour
    {
        [System.NonSerialized] public double startBeat, lengthBeat, currentBeat;
        [System.NonSerialized] public int currentPos, nextPos;
        [System.NonSerialized] public int ID;
        private BezierCurve3D currentCurve;
        private Animator rodAnim;
        [System.NonSerialized] public bool isShoot = false;
        public Square[] Squares;
        private bool isMiss = false;
        private bool isNearlyMiss = false;
        [System.NonSerialized] public int time, endTime = int.MaxValue;
        [System.NonSerialized] public BuiltToScaleRvl.CustomBounceItem[] customBounce;
        public float missAngle, fallingAngle;

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            rodAnim = GetComponent<Animator>();
            currentBeat = startBeat;
            time = 0;
            BounceRecursion(startBeat, lengthBeat, currentPos, nextPos);
            setParameters(currentPos, nextPos);
            fallingAngle = fallingAngle * UnityEngine.Random.Range(-1f, 1f);
        }
        void Update()
        {
            var cond = Conductor.instance;
            rodAnim.speed = 0.5f / cond.pitchedSecPerBeat / (float)lengthBeat;
            transform.localEulerAngles = new Vector3(0, 0, 0);
            if (currentCurve is not null)
            {
                float curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat);
                if (curveProg > 1) curveProg = 1 + (curveProg-1)*0.5f;
                if (isMiss) {
                    transform.position = currentCurve.GetPoint(curveProg);
                    transform.localEulerAngles = new Vector3(0, 0, fallingAngle*curveProg);
                } else if (currentPos <= nextPos) {
                    transform.position = currentCurve.GetPoint(curveProg);
                    if (isNearlyMiss) transform.localEulerAngles = new Vector3(0, 0, missAngle*(1 - curveProg));
                } else {
                    transform.position = currentCurve.GetPoint(1 - curveProg);
                    if (isNearlyMiss) transform.localEulerAngles = new Vector3(0, 0, missAngle*(1 - curveProg));
                }
            }
        }

        private void BounceRecursion(double beat, double length, int currentPos, int nextPos, bool playBounce = true)
        {
            var actions = new List<BeatAction.Action>();
            
            if (BuiltToScaleRvl.IsPositionInRange(currentPos) && playBounce)
            {
                actions.Add(new BeatAction.Action(beat, () => game.PlayBlockBounce(currentPos, beat + length)));
            }

            actions.Add(new BeatAction.Action(beat, delegate
            {
                this.currentBeat = beat;
                this.time++;
                setParameters(currentPos, nextPos);
            }));

            if (!BuiltToScaleRvl.IsPositionInRange(nextPos))
            {
                actions.Add(new BeatAction.Action(beat + length, () => End()));
            }
            else if (nextPos == 2)
            {
                if (isShoot && time + 1 == endTime) {
                    actions.Add(new BeatAction.Action(beat, () => game.PlayBlockPrepare(nextPos, beat + length)));
                    game.ScheduleInput(beat, length, BuiltToScaleRvl.InputAction_FlickAltPress, ShootOnHit, ShootOnMiss, Empty, CanShootHit);
                }
                else {
                    game.ScheduleInput(beat, length, BuiltToScaleRvl.InputAction_BasicPress, BounceOnHit, BounceOnMiss, Empty, CanBounceHit);
                }
            }
            else
            {
                actions.Add(new BeatAction.Action(beat, delegate
                {
                    int followingPos = BuiltToScaleRvl.getFollowingPos(currentPos, nextPos, time, customBounce);
                    BounceRecursion(beat + length, length, nextPos, followingPos);
                }));
            }

            if (BuiltToScaleRvl.IsPositionInRange(currentPos))
            {
                actions.Add(new BeatAction.Action(beat + length, () => game.PlayBlockIdle(currentPos, beat + length)));
            }
            
            BeatAction.New(game, actions);
        }
        
        void setParameters(int currentPos, int nextPos)
        {
            this.currentPos = currentPos;
            this.nextPos = nextPos;

            if (currentPos < nextPos) {
                rodAnim.SetFloat("speed", 1f);
            } else if (currentPos > nextPos){
                rodAnim.SetFloat("speed", -1f);
            }
            if (isShoot && time == endTime) {
                Debug.Log($"{currentPos} {nextPos}");
                currentCurve = game.curve[BuiltToScaleRvl.curveMapHigh[(currentPos, nextPos)]];
            } else if (BuiltToScaleRvl.IsPositionInRange(nextPos)) {
                currentCurve = game.curve[BuiltToScaleRvl.curveMap[(currentPos, nextPos)]];
            } else {
                currentCurve = game.curve[BuiltToScaleRvl.curveMapOut[(currentPos, nextPos)]];
            }
        }

        private void BounceOnHit(PlayerActionEvent caller, float state)
        {
            int followingPos = BuiltToScaleRvl.getFollowingPos(currentPos, nextPos, time, customBounce);
            if (state >= 1f || state <= -1f)
            {
                isNearlyMiss = true;
                BeatAction.New(game, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + 2*lengthBeat, () => isNearlyMiss = false)});
                game.PlayBlockBounceNearlyMiss(nextPos);
                BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, followingPos, false);
                return;
            }

            game.PlayBlockBounce(nextPos, currentBeat + 2*lengthBeat);
            BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, followingPos, false);
        }
        private void BounceOnMiss(PlayerActionEvent caller)
        {
            Falling();
        }
        private bool CanBounceHit()
        {
            return !game.isPlayerOpen;
        }
        private void Falling()
        {
            int missCurveIndex = (currentPos > nextPos) ? 0 : 1;
            currentCurve = game.missCurve[missCurveIndex];

            currentBeat = Conductor.instance.songPositionInBeats;
            rodAnim.SetFloat("speed", -1f);
            isMiss = true;
            game.PlayBlockBounceMiss(nextPos);
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(currentBeat + lengthBeat*0.2f, delegate {
                    GetComponent<SpriteRenderer>().sortingOrder = 1;
                }),
                new BeatAction.Action(currentBeat + lengthBeat, delegate {
                    game.PlayBlockIdle(nextPos, currentBeat + lengthBeat);
                    End();
                })
            });
        }

        private void ShootOnHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Falling();
                return;
            }

            game.PlayBlockShoot(nextPos);
            foreach (var square in Squares) {
                Destroy(square.gameObject);
            }
            game.SpawnAssembled();
            End();
        }
        private void ShootOnMiss(PlayerActionEvent caller)
        {
            if (game.isPlayerPrepare)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 1;
                game.PlayBlockShootMiss(nextPos);
                BeatAction.New(game, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + 2*lengthBeat, () => End())});
            }
            else
            {
                Falling();
            }
        }
        private bool CanShootHit()
        {
            return game.isPlayerPrepare;
        }

        private void Empty(PlayerActionEvent caller) {}

        void End()
        {
            Destroy(gameObject);
        }
    }
}