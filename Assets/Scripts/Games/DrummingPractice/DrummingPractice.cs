using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrDrummingLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("drummingPractice", "Drumming Practice", "2BCF33", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetBop(e.beat, e.length, e["bop"], e["autoBop"]); }, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the drummers bop?"),
                        new Param("autoBop", true, "Bop (Auto)", "Should the drummers auto bop?")
                    }
                },
                new GameAction("drum", "Hit Drum")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.Prepare(e.beat, e["toggle"]); }, 
                    defaultLength = 2f, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Applause", "Whether or not an applause should be played on a successful hit")
                    }
                },
                new GameAction("set mii", "Set Miis")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetMiis(e["type"], e["type2"], e["type3"], e["toggle"]); }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>()
                    {
                        new Param("type", DrummingPractice.MiiType.Random, "Player Mii", "The Mii that the player will control"),
                        new Param("type2", DrummingPractice.MiiType.Random, "Left Mii", "The Mii on the left"),
                        new Param("type3", DrummingPractice.MiiType.Random, "Right Mii", "The Mii on the right"),
                        new Param("toggle", false, "Set All to Player", "Sets all Miis to the Player's Mii")
                    }
                },
                new GameAction("move npc drummers", "NPC Drummers Enter or Exit")
                {
                    function = delegate {var e = eventCaller.currentEntity; DrummingPractice.instance.NPCDrummersEnterOrExit(e.beat, e.length, e["exit"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("exit", false, "Exit?", "Should the NPC drummers exit or enter?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the movement have?")
                    }
                },
                new GameAction("set background color", "Set Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; DrummingPractice.instance.SetBackgroundColor(e["colorA"], e["colorB"], e["colorC"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", new Color(43/255f, 207/255f, 51/255f), "Color A", "The top-most color of the background gradient"),
                        new Param("colorB", new Color(1, 1, 1), "Color B", "The bottom-most color of the background gradient"),
                        new Param("colorC", new Color(1, 247/255f, 0), "Streak Color", "The color of streaks that appear on a successful hit")
                    }
                }
            });
        }
    }
}


namespace HeavenStudio.Games
{
    using Scripts_DrummingPractice;

    public class DrummingPractice : Minigame
    {
        public enum MiiType
        {
            Random = -1,
            GuestA,
            GuestB,
            GuestC,
            GuestD,
            GuestE,
            GuestF,
            Matt,
            Tsunku,
            Marshal
        }

        [Header("References")]
        public SpriteRenderer background;
        public SpriteRenderer backgroundGradient;
        public SpriteRenderer[] streaks;
        public Drummer player;
        public Drummer leftDrummer;
        public Drummer rightDrummer;
        public GameObject hitPrefab;
        [SerializeField] Animator NPCDrummers;

        [Header("Variables")]
        float movingLength;
        float movingStartBeat;
        bool isMoving;
        string moveAnim;
        EasingFunction.Ease lastEase;
        bool goBop = true;

        public GameEvent bop = new GameEvent();
        public int count = 0;

        public static DrummingPractice instance;

        private void Awake()
        {
            instance = this;
            SetMiis();
        }
        
        public override void OnGameSwitch(float beat)
        {
            var changeMii = GameManager.instance.Beatmap.entities.FindLast(c => c.datamodel == "drummingPractice/set mii" && c.beat <= beat);
            if(changeMii != null)
            {
                EventCaller.instance.CallEvent(changeMii, true);
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (goBop)
                {
                    Bop();
                }
            }

            if (isMoving && cond.isPlaying && !cond.isPaused) 
            {
                float normalizedBeat = cond.GetPositionFromBeat(movingStartBeat, movingLength);
                if (normalizedBeat >= 0 && normalizedBeat <= 1f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float newPos = func(0f, 1f, normalizedBeat);
                    NPCDrummers.DoNormalizedAnimation(moveAnim, newPos);
                }
            }

            foreach (SpriteRenderer streak in streaks)
            {
                Color col = streak.color;
                streak.color = new Color(col.r, col.g, col.b, Mathf.Lerp(col.a, 0, 3.5f * Time.deltaTime));
            }
        }

        public void NPCDrummersEnterOrExit(float beat, float length, bool exit, int ease)
        {
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = exit ? "NPCDrummersExit" : "NPCDrummersEnter";
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.01f, delegate { isMoving = false; })
            });
        }

        public void SetBop(float beat, float length, bool shouldBop, bool autoBop)
        {
            goBop = autoBop;
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate { Bop(); })
                    });
                }
            }
        }

        public void Bop()
        {
            player.Bop();
            leftDrummer.Bop();
            rightDrummer.Bop();
        }

        public void Prepare(float beat, bool applause)
        {
            int type = count % 2;
            player.Prepare(beat, type);
            leftDrummer.Prepare(beat, type);
            rightDrummer.Prepare(beat, type);
            count++;

            SetFaces(0);
            Jukebox.PlayOneShotGame("drummingPractice/prepare");

            GameObject hit = Instantiate(hitPrefab);
            hit.transform.parent = hitPrefab.transform.parent;
            hit.SetActive(true);
            DrummerHit h = hit.GetComponent<DrummerHit>();
            h.startBeat = beat;
            h.applause = applause;
        }

        public void SetFaces(int type)
        {
            player.SetFace(type);
            leftDrummer.SetFace(type);
            rightDrummer.SetFace(type);
        }

        public void SetMiis(int playerFace = (int) MiiType.Random, int leftFace = (int) MiiType.Random, int rightFace = (int) MiiType.Random, bool all = false)
        {
            if (playerFace == (int) MiiType.Random)
            {
                do
                {
                    player.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                }
                while (player.mii == leftFace || player.mii == rightFace);
            }
            else
                player.mii = playerFace;

            if (all && playerFace != -1)
            {
                leftDrummer.mii = playerFace;
                rightDrummer.mii = playerFace;
            }
            else
            {
                if (leftFace == (int) MiiType.Random)
                {
                    do
                    {
                        leftDrummer.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                    }
                    while (leftDrummer.mii == player.mii);
                }
                else
                    leftDrummer.mii = leftFace;

                if (rightFace == (int) MiiType.Random)
                {
                    do
                    {
                        rightDrummer.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                    }
                    while (rightDrummer.mii == leftDrummer.mii || rightDrummer.mii == player.mii);
                }
                else
                    rightDrummer.mii = rightFace;
            }

            SetFaces(0);
        }

        public void SetBackgroundColor(Color col1, Color col2, Color col3)
        {
            backgroundGradient.color = col1;
            background.color = col2;
            foreach(SpriteRenderer streak in streaks)
            {
                streak.color = new Color(col3.r, col3.g, col3.b, streak.color.a);
            }
        }

        public void Streak()
        {
            foreach (SpriteRenderer streak in streaks)
            {
                Color col = streak.color;
                streak.color = new Color(col.r, col.g, col.b, 0.7f);
            }
        }

    }
}