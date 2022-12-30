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
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], false); }, 
                    defaultLength = 5f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the orange falls down")
                    },
                },

                new GameAction("pineapple", "Pineapple")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], true); }, 
                    defaultLength = 9f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the pineapple falls down")
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

        // when to stop playing the catch animation
        private float stopCatchLeft = 0f;
        private float stopCatchRight = 0f;

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

            Conductor conductor = Conductor.instance;
            if (conductor.isPlaying && !conductor.isPaused)
            {
                // print(stopCatchLeft + " " + stopCatchRight);
                // print("current beat: " + conductor.songPositionInBeats);
                if (stopCatchLeft > 0 && stopCatchLeft <= conductor.songPositionInBeats)
                {
                    plalinAnim.SetTrigger("stopCatch");
                    stopCatchLeft = 0;
                }

                if (stopCatchRight > 0 && stopCatchRight <= conductor.songPositionInBeats)
                {
                    alalinAnim.SetTrigger("stopCatch");
                    stopCatchRight = 0;
                }


            }


        }


        public void DropFruit(float beat, int side, bool isPineapple)
        {
            var objectToSpawn = isPineapple ? pineappleBase : orangeBase;

            if (side == (int)Side.Left || side == (int)Side.Both)
            {
                DropFruitSingle(beat, false, objectToSpawn);
            }

            if (side == (int)Side.Right || side == (int)Side.Both)
            {
                DropFruitSingle(beat, true, objectToSpawn);
            }
            

        }

        public void DropFruitSingle(float beat, bool side, GameObject objectToSpawn)
        {

            var newFruit = GameObject.Instantiate(objectToSpawn, fruitHolder);
            var fruitComp = newFruit.GetComponent<Fruit>();
            fruitComp.startBeat = beat;
            fruitComp.side = side;
            newFruit.SetActive(true);
        }

        public void Bop(float beat, bool left, bool right)
        {
            if (left && stopCatchLeft == 0)
            {
                plalinAnim.Play("bop", 0, 0);
            }

            if (right && stopCatchRight == 0)
            {
                alalinAnim.Play("bop", 0, 0);
            }
        }


        public void catchSuccess(bool side, bool isPineapple, float beat)
        {

            if (side) {
                alalinAnim.Play("catchOrange", 0, 0);
                stopCatchRight = beat + 0.9f;
            }
            else
            {
                plalinAnim.Play("catchOrange", 0, 0);
                stopCatchLeft = beat + 0.9f;
            }
        }

        public void catchMiss(bool side, bool isPineapple)
        {
            return;
        }



    }
}
