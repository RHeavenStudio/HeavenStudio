using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Rod : MonoBehaviour
    {
        public double startBeat, lengthBeat, currentBeat, endBeat;
        public int currentPos, nextPos;
        public int ID;
        public BezierCurve3D[] curve;
        private BezierCurve3D currentCurve;
        private Animator rodAnim;
        public bool isShoot = false;
        public Square[] Squares;
        private bool isPreShoot = false; 
        private bool isPreOut = false;
        private bool isMiss = false; 

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            rodAnim = GetComponent<Animator>();
            currentBeat = startBeat;
            BounceRecursion(startBeat, lengthBeat, currentPos, nextPos);
            setParameters(currentPos, nextPos);
        }
        void Update()
        {
            var cond = Conductor.instance;
            rodAnim.speed = 0.5f / cond.pitchedSecPerBeat / (float)lengthBeat;
            if (currentCurve is not null)
            {
                float curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat);
                if (curveProg > 1) curveProg = 1 + (curveProg-1)*0.5f;
                if (currentPos <= nextPos || isMiss) {
                    transform.position = currentCurve.GetPoint(curveProg);
                } else {
                    transform.position = currentCurve.GetPoint(1 - curveProg);
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
                        setParameters(currentPos, nextPos);
                        BounceRecursion(beat + length, length, nextPos, futurePos);
                    })
                });    
            } else if (nextPos < 0 || nextPos > 3) {
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
                        setParameters(currentPos, nextPos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                        game.spawnedRods.Remove(this);
                        Destroy(gameObject);
                    })
                });    
            } else if (nextPos == 2) {
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
                        setParameters(currentPos, nextPos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                    })
                });
                if (isShoot && beat + length == endBeat) {
                    SoundByte.PlayOneShotGame("builtToScaleRvl/prepare");
                    SoundByte.PlayOneShotGame("builtToScaleRvl/playerRetract", beat);
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate
                        {
                            game.blockAnims[nextPos].Play("prepare", 0, 0);
                        })
                    });
                    game.ScheduleInput(beat, length, BuiltToScaleRvl.InputAction_FlickAltPress, ShootOnHit, ShootOnMiss, Empty);
                }
                else game.ScheduleInput(beat, length, BuiltToScaleRvl.InputAction_BasicPress, BounceOnHit, BounceOnMiss, Empty);
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
                        setParameters(currentPos, nextPos);
                        BounceRecursion(beat + length, length, nextPos, futurePos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                    })
                });
            }
        }
        int getFuturePos(int currentPos, int nextPos)
        {
            if (nextPos == 0) return isPreOut ? -1 : 1;
            else if (nextPos == 3) return isPreOut ? 4 : 2;
            else if (currentPos < nextPos) return nextPos + 1;
            else if (currentPos > nextPos) return nextPos - 1;
            return nextPos;
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

        public void BounceOnHit(PlayerActionEvent caller, float state)
        {
            int futurePos = getFuturePos(this.currentPos, this.nextPos);
            BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, futurePos);
        }
        public void BounceOnMiss(PlayerActionEvent caller)
        {
            currentCurve = curve[5];
            currentBeat = Conductor.instance.songPositionInBeats;
            rodAnim.SetFloat("speed", -1f);
            isMiss = true;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(currentBeat + lengthBeat, delegate
                {
                    Destroy(gameObject);
                })
            });
        }

        public void PreShoot(double beat)
        {
            isPreShoot = true;
        }

        public void PreOut(double beat)
        {
            isPreOut = true;
        }
        public void ShootRod(double beat)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/prepare", beat);
        }

        public void ShootOnHit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/shoot");
            game.blockAnims[nextPos].Play("shoot", 0, 0);
            foreach (var square in Squares) {
                Destroy(square.gameObject);
            }
            game.SpawnAssembled();
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(currentBeat + lengthBeat, delegate
                {
                    game.spawnedRods.Remove(this);
                    Destroy(gameObject);
                })
            });
        }
        public void ShootOnMiss(PlayerActionEvent caller)
        {
            game.blockAnims[nextPos].Play("shoot", 0, 0);
            game.spawnedRods.Remove(this);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(currentBeat + lengthBeat, delegate
                {
                    game.spawnedRods.Remove(this);
                    Destroy(gameObject);
                })
            });
        }

        public void Empty(PlayerActionEvent caller) {}
    }
}