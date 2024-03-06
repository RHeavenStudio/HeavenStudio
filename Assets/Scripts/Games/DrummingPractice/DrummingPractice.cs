using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrDrummingLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("drummingPractice", "Drumming Practice", "36d23e", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetBop(e.beat, e.length, e["bop"], e["autoBop"]); }, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the drummers should bop for the duration of this event."),
                        new Param("autoBop", false, "Bop (Auto)", "Toggle if the drummers should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("drum", "Hit Drum")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.Prepare(e.beat, e["toggle"]); }, 
                    defaultLength = 2f, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Applause", "Toggle if applause should be played on a successful hit.")
                    }
                },
                new GameAction("set mii", "Set Miis")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetMiis(e["type"], e["type2"], e["type3"], e["toggle"]); }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Set All To Player", "Toggle if all Miis should be set to the player's (middle) Mii.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "type", "type2", "type3" })
                        }),
                        new Param("type", DrummingPractice.MiiType.Random, "Player Mii", "Set the Mii that the player will control."),
                        new Param("type2", DrummingPractice.MiiType.Random, "Left Mii", "Set the Mii on the left."),
                        new Param("type3", DrummingPractice.MiiType.Random, "Right Mii", "Set the Mii on the right."),
                    }
                },
                new GameAction("move npc drummers", "NPC Drummers")
                {
                    function = delegate {var e = eventCaller.currentEntity; DrummingPractice.instance.NPCDrummersEnterOrExit(e.beat, e.length, e["exit"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("exit", false, "Exit", "Toggle if the NPC drummers should enter or exit the scene."),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("set background color", "Background Appearance")
                {
                    function = delegate {var e = eventCaller.currentEntity;
                    DrummingPractice.instance.BackgroundColor(e.beat, e.length, e["colorAStart"], e["colorA"], e["colorBStart"], e["colorB"], e["colorC"], e["ease"]); }, 
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorAStart", new Color(43/255f, 207/255f, 51/255f), "Top Color Start", "Set the top-most color of the background gradient at the start of the event."),
                        new Param("colorA", new Color(43/255f, 207/255f, 51/255f), "Top Color End", "Set the top-most color of the background gradient at the end of the event."),
                        new Param("colorBStart", new Color(1, 1, 1), "Bottom Color Start", "Set the bottom-most color of the background gradient at the start of the event."),
                        new Param("colorB", new Color(1, 1, 1), "Bottom Color End", "Set the bottom-most color of the background gradient at the end of the event."),
                        new Param("colorC", new Color(1, 247/255f, 0), "Streak Color", "Set the color of the streaks that appear upon a successful hit."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                }
            },
            new List<string>() {"ctr", "normal"},
            "ctrintro", "en",
            new List<string>() {}
            );
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
        double movingStartBeat;
        bool isMoving;
        string moveAnim;
        EasingFunction.Ease lastEase;
        public int count = 0;

        public static DrummingPractice instance;

        private void Awake()
        {
            instance = this;
            SetMiis();
            SetupBopRegion("drummingPractice", "bop", "autoBop");
        }
        
        public override void OnGameSwitch(double beat)
        {
            var changeMii = GameManager.instance.Beatmap.Entities.FindLast(c => c.datamodel == "drummingPractice/set mii" && c.beat <= beat);
            if(changeMii != null)
            {
                EventCaller.instance.CallEvent(changeMii, true);
            }
            PersistColor(beat);
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                Bop();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;

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

            BackgroundColorUpdate();
        }

        public void NPCDrummersEnterOrExit(double beat, float length, bool exit, int ease)
        {
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = exit ? "NPCDrummersExit" : "NPCDrummersEnter";
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.01f, delegate { isMoving = false; })
            });
        }

        public void SetBop(double beat, float length, bool shouldBop, bool autoBop)
        {
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
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

        public void Prepare(double beat, bool applause)
        {
            int type = count % 2;
            player.Prepare(beat, type);
            leftDrummer.Prepare(beat, type);
            rightDrummer.Prepare(beat, type);
            count++;

            SetFaces(0);
            SoundByte.PlayOneShotGame("drummingPractice/prepare");

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

        private ColorEase tColorEase = new(new Color(43 / 255f, 207 / 255f, 51 / 255f)); // top gradient color
        private ColorEase bColorEase = new(Color.white); // bottom gradient color

        //call this in update
        private void BackgroundColorUpdate()
        {
            backgroundGradient.color = tColorEase.GetColor();
            background.color = bColorEase.GetColor();
        }

        public void BackgroundColor(double beat, float length, Color colorStartSetT, Color colorEndSetT, Color colorStartSetB, Color colorEndSetB, Color setStreak, int ease)
        {
            tColorEase = new ColorEase(beat, length, colorStartSetT, colorEndSetT, ease);
            bColorEase = new ColorEase(beat, length, colorStartSetB, colorEndSetB, ease);

            foreach (SpriteRenderer streak in streaks) {
                streak.color = new Color(setStreak.r, setStreak.g, setStreak.b, streak.color.a);
            }
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("drummingPractice", new string[] { "set background color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorAStart"], lastEvent["colorA"], lastEvent["colorBStart"], lastEvent["colorB"], lastEvent["colorC"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
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