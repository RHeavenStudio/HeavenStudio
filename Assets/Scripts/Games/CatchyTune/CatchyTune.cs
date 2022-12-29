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
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("catchyTune", "Catchy Tune", "B4E6F6", false, false, new List<GameAction>()
            {
                new GameAction("orange", "Orange")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], false); }, 
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the orange falls down")
                    },
                },

                new GameAction("pineapple", "Pineapple")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.DropFruit(e.beat, e["side"], true); }, 
                    defaultLength = 8f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the pineapple falls down")
                    },
                },
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

        [Header("Curves")]
        public BezierCurve3D leftCurve;
        public BezierCurve3D rightCurve;



        public static CatchyTune instance;

        private void Awake()
        {
            instance = this;
        }

        // private void Update()
        // {
        //     headAndBodyAnim.SetBool("ShouldOpenMouth", foodHolder.childCount != 0);

        //     if (PlayerInput.GetAnyDirectionDown())
        //     {
        //         headAndBodyAnim.Play("BiteL", 0, 0);
        //     }
        //     else if (PlayerInput.Pressed())
        //     {
        //         headAndBodyAnim.Play("BiteR", 0, 0);
        //     }
        // }

        List<DynamicBeatmap.DynamicEntity> spawnedOrangeEvents = new List<DynamicBeatmap.DynamicEntity>();

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
        }

        
        // private void LateUpdate()
        // {
        //     if (squashing)
        //     {
        //         var dState = donutBagAnim.GetCurrentAnimatorStateInfo(0);
        //         var cState = cakeBagAnim.GetCurrentAnimatorStateInfo(0);

        //         bool noDonutSquash = dState.IsName("DonutIdle");
        //         bool noCakeSquash = cState.IsName("CakeIdle");

        //         if (noDonutSquash && noCakeSquash)
        //         {
        //             squashing = false;
        //             bagsAnim.Play("Idle", 0, 0);
        //         }
        //     }
        // }

        // public void SpawnTreat(float beat, bool isCake)
        // {
        //     var objectToSpawn = isCake ? cakeBase : donutBase;
        //     var newTreat = GameObject.Instantiate(objectToSpawn, foodHolder);
            
        //     var treatComp = newTreat.GetComponent<Treat>();
        //     treatComp.startBeat = beat;
        //     treatComp.curve = isCake ? cakeCurve : donutCurve;

        //     newTreat.SetActive(true);

        //     Jukebox.PlayOneShotGame(isCake ? "blueBear/cake" : "blueBear/donut");

        //     SquashBag(isCake);
        // }

        public void DropFruit(float beat, int side, bool isPineapple)
        {
            var objectToSpawn = isPineapple ? pineappleBase : orangeBase;

            print("side = " + side);

            if (side == (int)Side.Left || side == (int)Side.Both)
            {
                DropFruitSingle(beat, false, objectToSpawn);
            }

            if (side == (int)Side.Right || side == (int)Side.Both)
            {
                DropFruitSingle(beat, true, objectToSpawn);
            }
            

            //float fruittimer = isPineapple ? 10f : 6f;

            //if (side == Side.Left || side == Side.Both) {
            //    PlayerActionEvent fruitcatch = ScheduleInput(beat, fruittimer, InputType.DIRECTION_DOWN, catchOrangeSuccess, isPineapple ? catchPineappleMiss : catchOrangeMiss, isPineapple ? catchPineappleMiss : catchOrangeMiss);
            //}
            //if (side == Side.Right || side == Side.Both) {
            //    PlayerActionEvent fruitcatch = ScheduleInput(beat, fruittimer, InputType.STANDARD_DOWN, isPineapple ? catchPineappleSuccess : catchOrangeSuccess, isPineapple ? catchPineappleMiss : catchOrangeMiss, isPineapple ? catchPineappleMiss : catchOrangeMiss);
            //}

            

        }

        public void DropFruitSingle(float beat, bool side, GameObject objectToSpawn)
        {

            var newFruit = GameObject.Instantiate(objectToSpawn, fruitHolder);
            var fruitComp = newFruit.GetComponent<Fruit>();
            fruitComp.startBeat = beat;
            fruitComp.side = side;
            newFruit.SetActive(true);

            print("dropped fruit");
        }


        public void catchOrangeSuccess(PlayerActionEvent caller)
        {
            plalinAnim.Play("catchOrange", 0, 0);
            Jukebox.PlayOneShotGame("catchyTune/catchSuccess");
        }
        public void catchOrangeMiss(PlayerActionEvent caller)
        {
            return;
        }

        public void catchPineappleSuccess(PlayerActionEvent caller)
        {
            //alalinAnim.Play("catchPineapple", 0, 0);
            Jukebox.PlayOneShotGame("catchyTune/catchSuccess");
        }

        public void catchPineappleMiss(PlayerActionEvent caller)
        {
            return;
        }

        // void CatchOrangePlalin()
        // {
        //     plalinAnim.Play("catchOrange", 0, 0);
        // }
        // void CatchOrangeAlalin()
        // {
        //     alalinAnim.Play("catchOrange", 0, 0);
        // }


    }
}
