using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSoccerLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceSoccer", "Space Soccer", "ff7d27", false, false, new List<GameAction>()
            {
                new GameAction("ball dispense", "Ball Dispense")
                {
                    function = delegate { SpaceSoccer.instance.Dispense(eventCaller.currentEntity.beat, !eventCaller.currentEntity["toggle"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Disable Sound", "Disables the dispense sound")
                    },
                    inactiveFunction = delegate { if (!eventCaller.currentEntity["toggle"]) { SpaceSoccer.DispenseSound(eventCaller.currentEntity.beat); } }
                },
                new GameAction("high kick-toe!", "High Kick-Toe!")
                {
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("swing", new EntityTypes.Float(0, 1, 0.5f), "Swing", "The amount of swing")
                    }
                },
                new GameAction("npc kickers enter or exit", "NPC Kickers Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.NPCKickersEnterOrExit(e.beat, e.length, e["toggle"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should Exit?", "Whether the kickers should exit or enter.")
                    },
                    resizable = true
                },
                new GameAction("npc kickers instant enter or exit", "NPC Kickers Instant Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.InstantNPCKickersEnterOrExit(e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should Exit?", "Whether the kickers should be exited or entered.")
                    },
                },
                new GameAction("scroll", "Scrolling Background") 
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.UdpateScrollSpeed(e.beat, e["x"], e["y"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>() {
                        new Param("x", new EntityTypes.Float(-100f, 100f, 22f), "Horizontal", "How many beats will it take before the background has looped once horizontally?"),
                        new Param("y", new EntityTypes.Float(-100f, 100f, 6f), "Vertical", "How many beats will it take before the background has looped once vertically?"),
                    }
                },
                // This is still here for "backwards-compatibility" but is hidden in the editor (it does absolutely nothing however)
                new GameAction("keep-up", "")
                {
                    defaultLength = 4f,
                    resizable = true,
                    hidden = true
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SpaceSoccer;
    using HeavenStudio.Common;

    public class SpaceSoccer : Minigame
    {
        [Header("Components")]
        [SerializeField] private GameObject ballRef;
        [SerializeField] private List<Kicker> kickers;
        [SerializeField] private Animator npcKickersAnim;
        [SerializeField] private SuperScroll backgroundSprite;

        [Header("Properties")]
        [SerializeField] private bool ballDispensed; //unused
        float npcMoveLength = 4f;
        float npcMoveStartBeat;
        bool npcMoving;
        string npcMoveAnimName;
        float scrollBeat;
        float scrollLengthX = 22f;
        float scrollLengthY = 6f;
        float scrollNormalizedOfssetX;
        float scrollNormalizedOfssetY;

        public static SpaceSoccer instance { get; private set; }

        private void Awake()
        {
            instance = this;
            npcKickersAnim.Play("NPCKickersExited", 0, 0);
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (npcMoving) npcKickersAnim.DoScaledAnimation(npcMoveAnimName, npcMoveStartBeat, npcMoveLength);
            float normalizedX = cond.GetPositionFromBeat(scrollBeat, scrollLengthX);
            float normalizedY = cond.GetPositionFromBeat(scrollBeat, scrollLengthY);
            backgroundSprite.NormalizedX = -normalizedX + scrollNormalizedOfssetX; 
            backgroundSprite.NormalizedY = -normalizedY + scrollNormalizedOfssetY; 
        }

        public override void OnGameSwitch(float beat)
        {
            foreach(var entity in GameManager.instance.Beatmap.entities)
            {
                if(entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if(entity.datamodel != "spaceSoccer/ball dispense" || entity.beat + entity.length <= beat) //check for dispenses that happen right before the switch
                {
                    continue;
                }
                Dispense(entity.beat, false);
                break;
            }
        }

        public void UdpateScrollSpeed(float beat, float scrollSpeedX, float scrollSpeedY) 
        {
            var cond = Conductor.instance;
            scrollNormalizedOfssetX = cond.GetPositionFromBeat(scrollBeat, scrollLengthX);
            scrollNormalizedOfssetY = cond.GetPositionFromBeat(scrollBeat, scrollLengthY);
            scrollBeat = beat;
            scrollLengthX = scrollSpeedX;
            scrollLengthY = scrollSpeedY;
        }

        public void NPCKickersEnterOrExit(float beat, float length, bool shouldExit)
        {
            npcMoving = true;
            npcMoveLength = length;
            npcMoveStartBeat = beat;
            npcMoveAnimName = shouldExit ? "NPCKickersExit" : "NPCKickersEnter";
            npcKickersAnim.DoScaledAnimation(npcMoveAnimName, npcMoveStartBeat, npcMoveLength);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.1f, delegate { npcMoving = false; }),
            });
        }

        public void InstantNPCKickersEnterOrExit(bool shouldExit)
        {
            npcKickersAnim.Play(shouldExit ? "NPCKickersExited" : "NPCKickersPresent", 0, 0);
        }

        public void Dispense(float beat, bool playSound = true)
        {
            ballDispensed = true;
            for (int i = 0; i < kickers.Count; i++)
            {
                Kicker kicker = kickers[i];
                if (i == 0) kicker.player = true;

                if (kicker.ball != null) return;

                GameObject ball = Instantiate(ballRef, transform);
                ball.SetActive(true);
                Ball ball_ = ball.GetComponent<Ball>();
                ball_.Init(kicker, beat);
                if (kicker.player && playSound)
                {
                    DispenseSound(beat);
                }
                kicker.DispenseBall(beat);

                kicker.canKick = true;
            }
        }

        public static void DispenseSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("spaceSoccer/dispenseNoise",   beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble1", beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2", beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2B",beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble3", beat + 0.75f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble4", beat + 1f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble5", beat + 1.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6", beat + 1.5f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6B",beat + 1.75f),
                }, forcePlay:true);
        }
    }

}