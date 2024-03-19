using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Rod : MonoBehaviour
    {
        public double startBeat, lengthBeat, currentBeat;
        public int currentPos, nextPos;
        public BezierCurve3D[] curve;
        private BezierCurve3D currentCurve;
        private Animator rodAnim;

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            rodAnim = GetComponent<Animator>();
            currentBeat = startBeat;
            BounceRecursion(startBeat, lengthBeat, currentPos, nextPos);
            setCurve(currentPos, nextPos);
        }
        void Update()
        {
            var cond = Conductor.instance;
            rodAnim.speed = 0.5f / cond.pitchedSecPerBeat / (float)lengthBeat;
            if (currentCurve is not null)
            {
                float curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat);
                if (curveProg <= 1) {
                    if (currentPos <= nextPos) {
                        transform.position = currentCurve.GetPoint(curveProg);
                    } else {
                        transform.position = currentCurve.GetPoint(1 - curveProg);
                    }
                }
            }
        }

        private void BounceRecursion(double beat, double length, int currentPos, int nextPos)
        {
            int futurePos = getFuturePos(currentPos, nextPos);
            if (currentPos < 0 || currentPos > 3) {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        this.currentBeat = beat;
                        this.currentPos = currentPos;
                        this.nextPos = nextPos;
                        setCurve(currentPos, nextPos);
                        BounceRecursion(beat + length, length, nextPos, futurePos);
                    })
                });    
            } else {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        SoundByte.PlayOneShotGame(currentPos switch {
                            0 => "builtToScaleRvl/left",
                            1 => "builtToScaleRvl/middleLeft",
                            2 => "builtToScaleRvl/middleRight",
                            3 => "builtToScaleRvl/right",
                            _ => throw new System.NotImplementedException()
                        }, beat);
                        game.blockAnims[currentPos].Play("bounce", 0, 0);
                        this.currentBeat = beat;
                        this.currentPos = currentPos;
                        this.nextPos = nextPos;
                        if (currentPos < nextPos) {
                            rodAnim.SetFloat("speed", 1f);
                        } else if (currentPos > nextPos){
                            rodAnim.SetFloat("speed", -1f);
                        }
                        
                        setCurve(currentPos, nextPos);
                        BounceRecursion(beat + length, length, nextPos, futurePos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                    })
                });
            }

            // game.ScheduleInput(beat, length, game.InputAction_BasicPress, BounceOnHit, BounceOnMiss, Empty);
        }
        int getFuturePos(int currentPos, int nextPos)
        {
            if (nextPos == 0) return 1;
            else if (nextPos == 3) return 2;
            else if (currentPos < nextPos) return nextPos + 1;
            else if (currentPos > nextPos) return nextPos - 1;
            return nextPos;
        }
        void setCurve(int currentPos, int nextPos)
        {
            if ((currentPos==-1 && nextPos==0) || (currentPos==0 && nextPos==-1)) {
                currentCurve = curve[0];
            } else if ((currentPos==0 && nextPos==1) || (currentPos==1 && nextPos==0)) {
                currentCurve = curve[1];
            } else if ((currentPos==1 && nextPos==2) || (currentPos==2 && nextPos==1)) {
                currentCurve = curve[2];
            } else if ((currentPos==2 && nextPos==3) || (currentPos==3 && nextPos==2)) {
                currentCurve = curve[3];
            } else if ((currentPos==3 && nextPos==4) || (currentPos==4 && nextPos==3)) {
                currentCurve = curve[4];
            }
        }

        public void ShootRod(double beat)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/prepare", beat);
            game.ScheduleInput(beat, 2f, BuiltToScaleRvl.InputAction_FlickAltPress, ShootOnHit, ShootOnMiss, Empty);
        }

        public void BounceOnHit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/middleRight");
            game.blockAnims[2].Play("bounce", 0, 0);
        }
        public void BounceOnMiss(PlayerActionEvent caller)
        {
            
        }

        public void ShootOnHit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/shoot");
        }
        public void ShootOnMiss(PlayerActionEvent caller)
        {
            
        }

        public void Empty(PlayerActionEvent caller) {}
    }
}