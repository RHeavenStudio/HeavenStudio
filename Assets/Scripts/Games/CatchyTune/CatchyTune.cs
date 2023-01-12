using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrCatchLoader
    {
        // minigame menu items
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("catchyTune", "Catchy Tune \n<color=#eb5454>[WIP]</color>", "B4E6F6", false, false, new List<GameAction>()
            {
                new GameAction("orange", "Orange")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], e["smile"], false); }, 
                    defaultLength = 5f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the orange falls down"),
                        new Param("smile", false, "Smile", "If the characters smile with the heart message after catching")
                    },
                },

                new GameAction("pineapple", "Pineapple")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], e["smile"], true); }, 
                    defaultLength = 9f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the pineapple falls down"),
                        new Param("smile", false, "Smile", "If the characters smile with the heart message after catching")
                    },
                },

                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.Bop(e.beat, e["left"], e["right"]); }, 
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("left" , true, "Left", "Plalin bops head"),
                        new Param("right", true, "Right", "Alalin bops head")
                    },
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_CatchyTune;
    public class CatchyTune : Minigame
    {

        public enum Side
        {
            Left,
            Right,
            Both
        }


        [Header("Animators")]
        public Animator plalinAnim; // Left d-pad
        public Animator alalinAnim; // right A button

        [Header("References")]
        public GameObject orangeBase;
        public GameObject pineappleBase;
        public Transform fruitHolder;

        public GameObject heartMessage;

        // when to stop playing the catch animation
        private float stopCatchLeft = 0f;
        private float stopCatchRight = 0f;

        private float startSmile = 0f;
        private float stopSmile = 0f;

        private bool bopLeft = true;
        private bool bopRight = true;
        public GameEvent bop = new GameEvent();

        public static CatchyTune instance;

        private void Awake()
        {
            instance = this;
        }

        const float orangeoffset = 0.5f;
        const float pineappleoffset = 0.5f;

        private void Update()
        {

            // doesnt work here since i need the parameter and i dont know how to get it

            // if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            //     return;

            // var currentBeat = Conductor.instance.songPositionInBeats;

            // var orangeEvents = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "catchyTune/orange");
            // for (int i = 0; i < orangeEvents.Count; i++)
            // {
            //     var ev = orangeEvents[i];
            //     if (spawnedOrangeEvents.Contains(ev)) continue; // Don't spawn the same oranges multiple times.

            //     var spawnBeat = ev.beat - orangeoffset;
            //     if (currentBeat > spawnBeat && currentBeat < ev.beat + 4f)
            //     {
            //         DropFruit(currentBeat, ev[side], false);
            //         spawnedOrangeEvents.Add(ev);
            //         break;
            //     }
            // }

            // if (PlayerInput.GetAnyDirectionDown())
            // {
            //     plalinAnim.Play("catchOrange", 0, 0);
            // }
            // else if (PlayerInput.Pressed())
            // {
            //     alalinAnim.Play("catchOrange", 0, 0);
            // }

            

            Conductor cond = Conductor.instance;

            

            if (cond.isPlaying && !cond.isPaused)
            {
                // print(stopCatchLeft + " " + stopCatchRight);
                // print("current beat: " + conductor.songPositionInBeats);
                if (stopCatchLeft > 0 && stopCatchLeft <= cond.songPositionInBeats)
                {
                    plalinAnim.SetTrigger("stopCatch");
                    stopCatchLeft = 0;
                }

                if (stopCatchRight > 0 && stopCatchRight <= cond.songPositionInBeats)
                {
                    alalinAnim.SetTrigger("stopCatch");
                    stopCatchRight = 0;
                }

                if (startSmile > 0 && startSmile <= cond.songPositionInBeats)
                {
                    //print("smile start");
                    plalinAnim.Play("smile", 1, 0);
                    alalinAnim.Play("smile", 1, 0);
                    startSmile = 0;
                    heartMessage.SetActive(true);
                }

                if (stopSmile > 0 && stopSmile <= cond.songPositionInBeats)
                {
                    //print("smile stop");
                    plalinAnim.SetTrigger("stopSmile");
                    alalinAnim.SetTrigger("stopSmile");
                    stopSmile = 0;
                    heartMessage.SetActive(false);
                }

                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (bopLeft && stopCatchLeft == 0)
                    {
                        plalinAnim.Play("bop", 0, 0);
                    }

                    if (bopRight && stopCatchRight == 0)
                    {
                        alalinAnim.Play("bop", 0, 0);
                    }
                }

            }


        }


        public void DropFruit(float beat, int side, bool smile, bool isPineapple)
        {
            var objectToSpawn = isPineapple ? pineappleBase : orangeBase;

            if (side == (int)Side.Left || side == (int)Side.Both)
            {
                DropFruitSingle(beat, false, smile, objectToSpawn);
            }

            if (side == (int)Side.Right || side == (int)Side.Both)
            {
                DropFruitSingle(beat, true, smile, objectToSpawn);
            }
            

        }

        public void DropFruitSingle(float beat, bool side, bool smile, GameObject objectToSpawn)
        {

            var newFruit = GameObject.Instantiate(objectToSpawn, fruitHolder);
            var fruitComp = newFruit.GetComponent<Fruit>();
            fruitComp.startBeat = beat;
            fruitComp.side = side;
            fruitComp.smile = smile;
            newFruit.SetActive(true);
        }

        public void Bop(float beat, bool left, bool right)
        {
            bopLeft = left;
            bopRight = right;
        }


        public void catchSuccess(bool side, bool isPineapple, bool smile, float beat)
        {

            string anim = isPineapple ? "catchPineapple" : "catchOrange";

            if (side) {
                alalinAnim.Play(anim, 0, 0);
                stopCatchRight = beat + 0.9f;
            }
            else
            {
                plalinAnim.Play(anim, 0, 0);
                stopCatchLeft = beat + 0.9f;
            }

            if (smile)
            {
                startSmile = beat + 1f;
                stopSmile = beat + 2f;
            }
        }

        public void catchMiss(bool side, bool isPineapple)
        {
            return;
        }



    }
}
