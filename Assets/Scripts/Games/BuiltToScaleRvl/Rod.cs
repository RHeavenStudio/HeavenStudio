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
        public double startBeat, lengthBeat, currentBeat;
        public int currentPos, nextPos;
        public int ID;
        public BezierCurve3D[] curve;
        static readonly Dictionary<(int, int), int> curveMap = new Dictionary<(int, int), int> {
            {(-1, 0), 0}, {(0, -1), 0},     // 01
            {(0, 1), 1}, {(1, 0), 1},       // 12
            {(1, 2), 2}, {(2, 1), 2},       // 23
            {(2, 3), 3}, {(3, 2), 3},       // 34
            {(3, 4), 4}, {(4, 3), 4},       // 45
            {(0, 0), 5},                    // 11
            {(1, 1), 6},                    // 22
            {(2, 2), 7},                    // 33
            {(3, 3), 8},                    // 44
            {(-1, 1), 9}, {(1, -1), 9},     // 02
            {(0, 2), 10}, {(2, 0), 10},     // 13
            {(1, 3), 11}, {(3, 1), 11},     // 24
            {(2, 4), 12}, {(4, 2), 12},     // 35
            {(-1, 2), 13}, {(2, -1), 13},   // 03
            {(0, 3), 10}, {(3, 0), 14},     // 14
            {(1, 4), 11}, {(4, 1), 15},     // 25
            {(-1, 3), 16}, {(3, -1), 16},   // 04
            {(0, 4), 17}, {(4, 0), 17},     // 15
        };
        private BezierCurve3D currentCurve;
        private Animator rodAnim;
        public bool isShoot = false;
        public Square[] Squares;
        private bool isMiss = false;
        public int time, shootTime = int.MaxValue;
        public BuiltToScaleRvl.CustomBounceItem[] customBounce;

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
            if (currentPos < 0 || currentPos > 3) {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        this.currentBeat = beat;
                        this.time++;
                        setParameters(currentPos, nextPos);
                        int followingPos = BuiltToScaleRvl.getFollowingPos(currentPos, nextPos, time, customBounce);
                        BounceRecursion(beat + length, length, nextPos, followingPos);
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
                        this.time++;
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
                        this.time++;
                        setParameters(currentPos, nextPos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                    })
                });
                if (isShoot && time + 1 == shootTime) {
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
                        this.time++;
                        setParameters(currentPos, nextPos);
                        int followingPos = BuiltToScaleRvl.getFollowingPos(currentPos, nextPos, time, customBounce);
                        BounceRecursion(beat + length, length, nextPos, followingPos);
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        game.blockAnims[currentPos].Play("idle", 0, 0);
                    })
                });
            }
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

            currentCurve = curve[curveMap[(currentPos, nextPos)]];
            if ((currentPos==-1 && nextPos==0) || (currentPos==0 && nextPos==-1)) {
                currentCurve = curve[0];    // 01
            } else if ((currentPos==0 && nextPos==1) || (currentPos==1 && nextPos==0)) {
                currentCurve = curve[1];    // 12
            } else if ((currentPos==1 && nextPos==2) || (currentPos==2 && nextPos==1)) {
                currentCurve = curve[2];    // 23
            } else if ((currentPos==2 && nextPos==3) || (currentPos==3 && nextPos==2)) {
                currentCurve = curve[3];    // 34
            } else if ((currentPos==3 && nextPos==4) || (currentPos==4 && nextPos==3)) {
                currentCurve = curve[4];    // 45
            }
            else if ((currentPos==0 && nextPos==0)) {
                currentCurve = curve[5];    // 11
            } else if ((currentPos==1 && nextPos==1)) {
                currentCurve = curve[6];    // 22
            } else if ((currentPos==2 && nextPos==2)) {
                currentCurve = curve[7];    // 33
            } else if ((currentPos==3 && nextPos==3)) {
                currentCurve = curve[8];    // 44
            }
            else if ((currentPos==-1 && nextPos==1) || (currentPos==1 && nextPos==-1)) {
                currentCurve = curve[9];    // 02
            } else if ((currentPos==0 && nextPos==2) || (currentPos==2 && nextPos==0)) {
                currentCurve = curve[10];   // 13
            } else if ((currentPos==1 && nextPos==3) || (currentPos==3 && nextPos==1)) {
                currentCurve = curve[11];   // 24
            } else if ((currentPos==2 && nextPos==4) || (currentPos==4 && nextPos==2)) {
                currentCurve = curve[12];   // 35
            }
            else if ((currentPos==-1 && nextPos==2) || (currentPos==2 && nextPos==-1)) {
                currentCurve = curve[13];   // 03
            } else if ((currentPos==0 && nextPos==3) || (currentPos==3 && nextPos==0)) {
                currentCurve = curve[14];   // 14
            } else if ((currentPos==1 && nextPos==4) || (currentPos==4 && nextPos==1)) {
                currentCurve = curve[15];   // 25
            }
            else if ((currentPos==-1 && nextPos==3) || (currentPos==3 && nextPos==-1)) {
                currentCurve = curve[16];   // 04
            } else if ((currentPos==0 && nextPos==4) || (currentPos==4 && nextPos==0)) {
                currentCurve = curve[17];   // 15
            }
        }

        public void BounceOnHit(PlayerActionEvent caller, float state)
        {
            int followingPos = BuiltToScaleRvl.getFollowingPos(this.currentPos, this.nextPos, this.time, this.customBounce);
            BounceRecursion(currentBeat + lengthBeat, lengthBeat, nextPos, followingPos);
        }
        public void BounceOnMiss(PlayerActionEvent caller)
        {
            currentCurve = curve[^1];       // miss
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