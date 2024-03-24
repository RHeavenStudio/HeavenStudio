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
        private bool isMissShoot = false;
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
        }
        void Update()
        {
            var cond = Conductor.instance;
            rodAnim.speed = 0.5f / cond.pitchedSecPerBeat / (float)lengthBeat;
            transform.localEulerAngles = new Vector3(0, 0, 0);
            if (currentCurve is not null)
            {
                float curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat);
                if (curveProg > 1 && !isMissShoot) curveProg = 1 + (curveProg-1)*0.5f;
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
                actions.Add(new BeatAction.Action(beat, () => game.PlayBlockBounce(currentPos)));
            }

            actions.Add(new BeatAction.Action(beat, delegate
            {
                this.currentBeat = beat;
                this.time++;
                setParameters(currentPos, nextPos);
            }));

            if (!BuiltToScaleRvl.IsPositionInRange(nextPos))
            {
                actions.Add(new BeatAction.Action(beat + length, () => RemoveAndDestroy()));
            }
            else if (nextPos == 2)
            {
                if (isShoot && time + 1 == endTime) {
                    actions.Add(new BeatAction.Action(beat, () => game.PlayBlockPrepare(nextPos)));
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
                actions.Add(new BeatAction.Action(beat + length, () => game.PlayBlockIdle(currentPos)));
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
            if (BuiltToScaleRvl.IsPositionInRange(nextPos)) {
                currentCurve = game.curve[BuiltToScaleRvl.curveMap[(currentPos, nextPos)]];
            } else {
                currentCurve = game.curve[BuiltToScaleRvl.curveMapOut[(currentPos, nextPos)]];
            }
        }

        private void BounceOnHit(PlayerActionEvent caller, float state)
        {
            int followingPos = BuiltToScaleRvl.getFollowingPos(this.currentPos, this.nextPos, this.time, this.customBounce);
            if (state >= 1f || state <= -1f)
            {
                isNearlyMiss = true;
                BeatAction.New(this, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + 2*lengthBeat, () => isNearlyMiss = false)});
                game.PlayBlockBounceNearlyMiss(this.nextPos);
                BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, followingPos, false);
                return;
            }

            game.PlayBlockBounce(this.nextPos);
            BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, followingPos, false);
        }
        private void BounceOnMiss(PlayerActionEvent caller)
        {
            currentCurve = game.curve[^1];       // miss
            currentBeat = Conductor.instance.songPositionInBeats;
            rodAnim.SetFloat("speed", -1f);
            isMiss = true;
            game.PlayBlockBounceMiss(this.nextPos);
            BeatAction.New(this, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + lengthBeat, delegate {
                game.PlayBlockIdle(this.nextPos);
                RemoveAndDestroy();
            })});
        }
        private bool CanBounceHit()
        {
            return !game.isPlayerOpen;
        }

        private void ShootOnHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                currentCurve = game.curve[^1];       // miss
                currentBeat = Conductor.instance.songPositionInBeats;
                rodAnim.SetFloat("speed", -1f);
                isMiss = true;
                game.PlayBlockShootNearlyMiss(this.nextPos);
                BeatAction.New(this, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + lengthBeat, () => RemoveAndDestroy())});
                return;
            }

            game.PlayBlockShoot(nextPos);
            foreach (var square in Squares) {
                Destroy(square.gameObject);
            }
            game.SpawnAssembled();
            RemoveAndDestroy();
        }
        private void ShootOnMiss(PlayerActionEvent caller)
        {
            if (game.isPlayerPrepare)
            {
                isMissShoot = true;
                GetComponent<SpriteRenderer>().sortingOrder = 1;
                game.PlayBlockShootMiss(nextPos);
                BeatAction.New(this, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + 2*lengthBeat, () => RemoveAndDestroy())});
            }
            else
            {
                currentCurve = game.curve[^1];       // miss
                currentBeat = Conductor.instance.songPositionInBeats;
                rodAnim.SetFloat("speed", -1f);
                isMiss = true;
                game.PlayBlockBounceMiss(this.nextPos);
                BeatAction.New(this, new List<BeatAction.Action>() {new BeatAction.Action(currentBeat + lengthBeat, () => RemoveAndDestroy())});
            }
        }
        private bool CanShootHit()
        {
            return game.isPlayerPrepare;
        }

        private void Empty(PlayerActionEvent caller) {}

        void RemoveAndDestroy()
        {
            game.spawnedRods.Remove(this);
            Destroy(gameObject);
        }
    }
}