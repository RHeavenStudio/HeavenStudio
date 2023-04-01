using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlDoubleDateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("doubleDate", "Romantic Dates", "ef854a", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the two couples bop?"),
                        new Param("autoBop", false, "Bop (Auto)", "Should the two couples auto bop?")
                    }
                },
                new GameAction("soccer", "Soccer Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnSoccerBall(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("basket", "Basket Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnBasketBall(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("football", "Football")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnFootBall(e.beat); },
                    defaultLength = 2.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        [Header("Prefabs")]
        [SerializeField] SoccerBall soccer;
        [SerializeField] Basketball basket;
        [SerializeField] Football football;
        [SerializeField] GameObject leaves;
        [Header("Components")]
        [SerializeField] Animator boyAnim;
        [SerializeField] Animator girlAnim;
        [SerializeField] DoubleDateWeasels weasels;
        [SerializeField] Animator treeAnim;
        [Header("Variables")]
        bool shouldBop = true;
        bool canBop = true;
        GameEvent bop = new GameEvent();
        public static DoubleDate instance;
        
        private void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1) && shouldBop)
                {
                    SingleBop();
                }
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Jukebox.PlayOneShotGame("doubleDate/kick_whiff");
                Kick(true, true, false);
            }
        }

        public void ToggleBop(bool go)
        {
            canBop = go;
        }

        public void Bop(float beat, float length, bool goBop, bool autoBop)
        {
            shouldBop = autoBop;
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate { SingleBop(); })
                    });
                }
            }
        }

        void SingleBop()
        {
            if (canBop)
            {
                boyAnim.DoScaledAnimationAsync("IdleBop", 1f);
            }
            girlAnim.DoScaledAnimationAsync("GirlBop", 1f);
            weasels.Bop();
        }

        public void Kick(bool hit = true, bool forceNoLeaves = false, bool weaselsHappy = true)
        {
            if (hit)
            {
                boyAnim.DoScaledAnimationAsync("Kick", 1f);
                if (weaselsHappy) weasels.Happy();
                if (!forceNoLeaves)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeats + 1f, delegate
                        {
                            GameObject spawnedLeaves = Instantiate(leaves, transform);
                            spawnedLeaves.SetActive(true);
                            treeAnim.DoScaledAnimationAsync("TreeRustle", 1f);
                        })
                    });
                }
            }
            else
            {
                boyAnim.DoScaledAnimationAsync("Barely", 1f);
            }
        }

        public void SpawnSoccerBall(float beat)
        {
            SoccerBall spawnedBall = Instantiate(soccer, instance.transform);
            spawnedBall.Init(beat);
            Jukebox.PlayOneShotGame("doubleDate/soccerBounce", beat);
        }

        public void SpawnBasketBall(float beat)
        {
            Basketball spawnedBall = Instantiate(basket, instance.transform);
            spawnedBall.Init(beat);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/basketballBounce", beat),
                new MultiSound.Sound("doubleDate/basketballBounce", beat + 0.75f),
            });
        }

        public void SpawnFootBall(float beat)
        {
            Football spawnedBall = Instantiate(football, instance.transform);
            spawnedBall.Init(beat);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/footballBounce", beat),
                new MultiSound.Sound("doubleDate/footballBounce", beat + 0.75f),
            });
        }
    }

}