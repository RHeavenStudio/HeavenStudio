using DG.Tweening;
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
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("catchyTune", "Catchy Tune \n<color=#eb5454>[WIP]</color>", "B4E6F6", false, false, new List<GameAction>()
            {
                new GameAction("orange", "Orange")
                {
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the orange falls down"),
                        new Param("smile", false, "Smile", "If the characters smile with the heart message after catching")
                    },
                    preFunction = delegate {var e = eventCaller.currentEntity; CatchyTune.PreDropFruit(e.beat, e["side"], e["smile"], false); },
                },

                new GameAction("pineapple", "Pineapple")
                {
                    defaultLength = 8f,
                    parameters = new List<Param>()
                    {
                        new Param("side", CatchyTune.Side.Left, "Side", "The side the pineapple falls down"),
                        new Param("smile", false, "Smile", "If the characters smile with the heart message after catching")
                    },
                    preFunction = delegate {var e = eventCaller.currentEntity; CatchyTune.PreDropFruit(e.beat, e["side"], e["smile"], true); },
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
                },
                new GameAction("background", "Background")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchyTune.instance.changeBG(e["BG"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("BG", CatchyTune.Background.Long, "BG", "The background to change to")
                    },
                }
            },
            new List<string>() {"ctr", "normal"},
            "ctrcatchy", "en", "ver0",
            new List<string>(){},
            new List<string>(){}
            );
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

        public enum Background
        {
            Short,
            Long
        }


        [Header("Animators")]
        public Animator plalinAnim; // Left d-pad
        public Animator alalinAnim; // right A button

        [Header("References")]
        public GameObject orangeBase;
        public GameObject pineappleBase;
        public Transform fruitHolder;
        public GameObject heartMessage;

        public GameObject bg1;
        public GameObject bg2;

        // when to stop playing the catch animation
        private float stopCatchLeft = 0f;
        private float stopCatchRight = 0f;

        private float startSmile = 0f;
        private float stopSmile = 0f;

        private bool bopLeft = true;
        private bool bopRight = true;
        public GameEvent bop = new GameEvent();

        public static CatchyTune instance;
        static List<QueuedFruit> queuedFruits = new List<QueuedFruit>();
        struct QueuedFruit
        {
            public float beat;
            public int side;
            public bool smile;
            public bool isPineapple;
        }

        private void Awake()
        {
            instance = this;
        }

        const float orangeoffset = 0.5f;
        const float pineappleoffset = 0.5f;

        private void Update()
        {
            Conductor cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedFruits.Count > 0)
                {
                    foreach (var fruit in queuedFruits)
                    {
                        DropFruit(fruit.beat, fruit.side, fruit.smile, fruit.isPineapple);
                    }
                    queuedFruits.Clear();
                }

                // print(stopCatchLeft + " " + stopCatchRight);
                // print("current beat: " + conductor.songPositionInBeats);
                if (stopCatchLeft > 0 && stopCatchLeft <= cond.songPositionInBeats)
                {
                    plalinAnim.Play("idle", 0, 0);
                    stopCatchLeft = 0;
                }

                if (stopCatchRight > 0 && stopCatchRight <= cond.songPositionInBeats)
                {
                    alalinAnim.Play("idle", 0, 0);
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
                    plalinAnim.Play("stopsmile", 1, 0);
                    alalinAnim.Play("stopsmile", 1, 0);
                    stopSmile = 0;
                    heartMessage.SetActive(false);
                }

                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (bopLeft && stopCatchLeft == 0)
                    {
                        plalinAnim.SetTrigger("bop");
                    }

                    if (bopRight && stopCatchRight == 0)
                    {
                        alalinAnim.Play("bop", 0, 0);
                    }
                }

                if (!IsExpectingInputNow())
                {
                    if (PlayerInput.GetAnyDirectionDown())
                    {
                        catchWhiff(false);
                    }
                    if (PlayerInput.Pressed())
                    {
                        catchWhiff(true);
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

        //minenice: experiment to test preFunction
        public static void PreDropFruit(float beat, int side, bool smile, bool isPineapple)
        {
            float spawnBeat = beat - 1f;
            beat = beat - (isPineapple ? 2f : 1f);
            if (GameManager.instance.currentGame == "catchyTune")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(spawnBeat, delegate { if (instance != null) instance.DropFruit(beat, side, smile, isPineapple); }),
                });
            }
            else
            {
                queuedFruits.Add(new QueuedFruit()
                {
                    beat = beat,
                    side = side,
                    smile = smile,
                    isPineapple = isPineapple
                });
            }

            if (side == (int)Side.Left || side == (int)Side.Both)
            {
                Fruit.PlaySound(beat, false, isPineapple);
            }
            if (side == (int)Side.Right || side == (int)Side.Both)
            {
                Fruit.PlaySound(beat, true, isPineapple);
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

        public void changeBG(int bg)
        {
            if (bg == 0)
            {
                bg1.SetActive(true);
                bg2.SetActive(false);
            }
            else
            {
                bg1.SetActive(false);
                bg2.SetActive(true);
            }
        }

        public void catchSuccess(bool side, bool isPineapple, bool smile, float beat)
        {
            string anim = isPineapple ? "catchPineapple" : "catchOrange";

            if (side)
            {
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
            // not the right sound at all but need an accurate rip
            Jukebox.PlayOneShotGame("catchyTune/fruitThrough");

            float beat = Conductor.instance.songPositionInBeats;

            string fruitType = isPineapple ? "Pineapple" : "Orange";
            
            if (side)
            {
                alalinAnim.Play("miss" + fruitType, 0, 0);
                stopCatchRight = beat + 0.7f;
            }
            else
            {
                plalinAnim.Play("miss" + fruitType, 0, 0);
                stopCatchLeft = beat + 0.7f;
            }
        }

        public void catchWhiff(bool side)
        {
            Jukebox.PlayOneShotGame("catchyTune/whiff");
            whiffAnim(side);
        }

        public void catchBarely(bool side)
        {
            if (side)
            {
                Jukebox.PlayOneShotGame("catchyTune/barely right");
            }
            else
            {
                Jukebox.PlayOneShotGame("catchyTune/barely left");
            }

            whiffAnim(side);
        }

        public void whiffAnim(bool side)
        {
            float beat = Conductor.instance.songPositionInBeats;
            
            if (side)
            {
                alalinAnim.Play("whiff", 0, 0);
                stopCatchRight = beat + 0.5f;
            }
            else
            {
                plalinAnim.Play("whiff", 0, 0);
                stopCatchLeft = beat + 0.5f;
            }
        }
    }
}
