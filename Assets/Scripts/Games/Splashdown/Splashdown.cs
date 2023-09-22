using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrSplashdownLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("splashdown", "Splashdown", "327BF5", false, false, new List<GameAction>()
            {
                new GameAction("dive", "Dive")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.GoDown(e.beat, e.length); },
                    resizable = true
                },
                new GameAction("appear", "Appear")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.GoUp(e.beat, e.length, e["type"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", new EntityTypes.Integer(1, 3, 1), "Type")
                    }
                },
                new GameAction("jump", "Jump")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.Jump(e.beat, e.length, e["dolphin"]); },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("dolphin", true, "Dolphin")
                    }
                },
                new GameAction("together", "Together Jump")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.TogetherJump(e.beat, e["al"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("al", false, "Alley-Oop!")
                    }
                },
                new GameAction("togetherR9", "Together Jump (Remix 9)")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.TogetherJumpRemix9(e.beat, e["al"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("al", false, "Alley-Oop!")
                    }
                },
                new GameAction("intro", "Intro")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.Intro(e.beat, e.length); },
                    resizable = true,
                    defaultLength = 8
                },
                new GameAction("amount", "Synchrette Amount")
                {
                    function = delegate { Splashdown.instance.SpawnSynchrettes(eventCaller.currentEntity["amount"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(3, 5, 3), "Amount")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Splashdown;

    public class Splashdown : Minigame
    {
        public static Splashdown instance;
        [Header("References")]
        [SerializeField] private Transform synchretteHolder;
        [SerializeField] private NtrSynchrette synchrettePrefab;
        [SerializeField] private Animator crowdAnim;
        [Header("Properties")]
        [SerializeField] private float synchretteDistance;

        private List<NtrSynchrette> currentSynchrettes = new List<NtrSynchrette>();
        private NtrSynchrette player;

        private void Awake()
        {
            instance = this;
            SpawnSynchrettes(3);
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused && !IsIntroing())
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
                {
                    SoundByte.PlayOneShot("miss");
                    SoundByte.PlayOneShotGame("splashdown/downPlayer");
                    player.GoDown();
                    ScoreMiss();
                }

                if ((PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease))
                    || (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease)))
                {
                    SoundByte.PlayOneShot("miss");
                    player.Appear(true);
                    SoundByte.PlayOneShotGame("splashdown/upPlayer");
                    ScoreMiss();
                }
            }
        }

        public void SpawnSynchrettes(int amount)
        {
            if (currentSynchrettes.Count > 0)
            {
                foreach (var synchrette in currentSynchrettes)
                {
                    Destroy(synchrette.gameObject);
                }
                currentSynchrettes.Clear();
            }
            if (player != null) Destroy(player.gameObject);
            float startPos = -((amount / 2) * synchretteDistance) + ((amount % 2 == 0) ? synchretteDistance / 2 : 0);

            for (int i = 0; i < amount; i++)
            {
                NtrSynchrette spawnedSynchrette = Instantiate(synchrettePrefab, synchretteHolder);
                spawnedSynchrette.transform.localPosition = new Vector3(startPos + (synchretteDistance * i), spawnedSynchrette.transform.localPosition.y, 0);
                if (i < amount - 1) currentSynchrettes.Add(spawnedSynchrette);
                else player = spawnedSynchrette;
            }
        }

        private double introBeat = -1;
        private float introLength = 0;

        private bool IsIntroing()
        {
            float normalized = Conductor.instance.GetPositionFromBeat(introBeat, introLength);
            return normalized >= 0f && normalized <= 1f;
        }

        public void Intro(double beat, float length)
        {
            introBeat = beat;
            introLength = length;
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < length - 1; i++)
            {
                actions.Add(new BeatAction.Action(beat + i, delegate
                {
                    foreach (var synchrette in currentSynchrettes)
                    {
                        synchrette.Bop();
                    }
                    player.Bop();
                }));
            }
            actions.Add(new BeatAction.Action(beat + length - 1, delegate
            {
                foreach (var synchrette in currentSynchrettes)
                {
                    synchrette.JumpIntoWater(beat + length - 1);
                }
                player.JumpIntoWater(beat + length - 1);
            }));
            SoundByte.PlayOneShotGame("splashdown/start", beat + length - 0.25);
            BeatAction.New(instance, actions);
        }

        public void GoDown(double beat, float length)
        {
            if (IsIntroing()) return;
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < currentSynchrettes.Count; i++)
            {
                NtrSynchrette synchretteToDive = currentSynchrettes[i];
                double diveBeat = beat + (i * length);
                actions.Add(new BeatAction.Action(diveBeat, delegate
                {
                    synchretteToDive.GoDown();
                }));
                SoundByte.PlayOneShotGame("splashdown/whistle", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/downOthers", diveBeat);
            }
            BeatAction.New(instance, actions);
            SoundByte.PlayOneShotGame("splashdown/whistle", beat + (currentSynchrettes.Count * length));
            ScheduleInput(beat, currentSynchrettes.Count * length, InputAction_BasicPress, JustDown, Out, Out);
        }

        public void GoUp(double beat, float length, int appearType)
        {
            if (IsIntroing()) return;
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < currentSynchrettes.Count; i++)
            {
                NtrSynchrette synchretteToDive = currentSynchrettes[i];
                double diveBeat = beat + (i * length);
                actions.Add(new BeatAction.Action(diveBeat, delegate
                {
                    synchretteToDive.Appear(false, appearType);
                }));
                SoundByte.PlayOneShotGame("splashdown/whistle", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/upOthers", diveBeat);
            }
            BeatAction.New(instance, actions);
            SoundByte.PlayOneShotGame("splashdown/whistle", beat + (currentSynchrettes.Count * length));
            switch (appearType)
            {
                case 1:
                    ScheduleInput(beat, currentSynchrettes.Count * length, InputAction_BasicRelease, JustUp1, Out, Out);
                    break;
                case 2:
                    ScheduleInput(beat, currentSynchrettes.Count * length, InputAction_BasicRelease, JustUp2, Out, Out);
                    break;
                case 3:
                    ScheduleInput(beat, currentSynchrettes.Count * length, InputAction_BasicRelease, JustUp3, Out, Out);
                    break;
            }
        }

        public void Jump(double beat, float length, bool dolphin)
        {
            if (IsIntroing()) return;
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < currentSynchrettes.Count; i++)
            {
                NtrSynchrette synchretteToDive = currentSynchrettes[i];
                double diveBeat = beat + (i * length);
                actions.Add(new BeatAction.Action(diveBeat, delegate
                {
                    synchretteToDive.Jump(diveBeat, false, !dolphin);
                }));
                SoundByte.PlayOneShotGame("splashdown/yeah", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/jumpOthers", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/rollOthers", diveBeat + 1);
                SoundByte.PlayOneShotGame("splashdown/splashOthers", diveBeat + 1.75);
            }
            BeatAction.New(instance, actions);
            SoundByte.PlayOneShotGame("splashdown/yeah", beat + (currentSynchrettes.Count * length));
            ScheduleInput(beat, currentSynchrettes.Count * length, InputAction_FlickRelease, dolphin ? JustJump : JustJumpNoDolphin, Out, Out);
        }

        public void TogetherJump(double beat, bool alleyoop)
        {
            if (IsIntroing()) return;
            SoundByte.PlayOneShotGame("splashdown/together");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2, delegate
                {
                    foreach (var synchrette in currentSynchrettes)
                    {
                        synchrette.Jump(beat + 2, false, alleyoop);
                    }
                })
            });
            if (alleyoop)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("splashdown/jumpOthers", beat + 2),
                    new MultiSound.Sound("splashdown/alleyOop1", beat + 2.5, 1, 1, false, 0.014),
                    new MultiSound.Sound("splashdown/alleyOop2", beat + 2.75),
                    new MultiSound.Sound("splashdown/alleyOop3", beat + 3, 1, 1, false, 0.014),
                    new MultiSound.Sound("splashdown/splashOthers", beat + 3.75),
                });
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("splashdown/jumpOthers", beat + 2),
                    new MultiSound.Sound("splashdown/rollOthers", beat + 3),
                    new MultiSound.Sound("splashdown/splashOthers", beat + 3.75),
                });
            }
            ScheduleInput(beat, 2, InputAction_FlickRelease, alleyoop ? JustJumpNoRollSound : JustJump, Out, Out);
        }

        public void TogetherJumpRemix9(double beat, bool alleyoop)
        {
            if (IsIntroing()) return;
            SoundByte.PlayOneShotGame("splashdown/togetherRemix9");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate
                {
                    foreach (var synchrette in currentSynchrettes)
                    {
                        synchrette.Jump(beat + 1, false, alleyoop);
                    }
                })
            });
            if (alleyoop)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("splashdown/jumpOthers", beat + 1),
                    new MultiSound.Sound("splashdown/alleyOop1", beat + 1.5, 1, 1, false, 0.014),
                    new MultiSound.Sound("splashdown/alleyOop2", beat + 1.75),
                    new MultiSound.Sound("splashdown/alleyOop3", beat + 2, 1, 1, false, 0.014),
                    new MultiSound.Sound("splashdown/splashOthers", beat + 2.75),
                });
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("splashdown/jumpOthers", beat + 1),
                    new MultiSound.Sound("splashdown/rollOthers", beat + 2),
                    new MultiSound.Sound("splashdown/splashOthers", beat + 2.75),
                });
            }
            ScheduleInput(beat, 1, InputAction_FlickRelease, alleyoop ? JustJumpNoRollSound : JustJump, Out, Out);
        }

        private void JustDown(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/downPlayer");
            player.GoDown();
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
            }
        }

        private void JustUp1(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/upPlayer");
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                player.Appear(true);
                return;
            }
            player.Appear(false, 1);
        }

        private void JustUp2(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/upPlayer");
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                player.Appear(true);
                return;
            }
            player.Appear(false, 2);
        }

        private void JustUp3(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/upPlayer");
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                player.Appear(true);
                return;
            }
            player.Appear(false, 3);
        }

        private void JustJump(PlayerActionEvent caller, float state)
        {
            double diveBeat = caller.timer + caller.startBeat;
            SoundByte.PlayOneShotGame("splashdown/jumpPlayer");
            SoundByte.PlayOneShotGame("splashdown/splashPlayer", diveBeat + 1.75);
            if (state >= 1f || state <= -1f)
            {
                player.Jump(diveBeat, true);
                return;
            }
            SoundByte.PlayOneShotGame("splashdown/rollPlayer", diveBeat + 1);
            player.Jump(diveBeat);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(diveBeat + 1.75, delegate { crowdAnim.DoScaledAnimationAsync("CrowdCheer", 0.5f); }),
                new BeatAction.Action(diveBeat + 4, delegate { crowdAnim.Play("CrowdIdle", 0, 0); })
            });
        }

        private void JustJumpNoDolphin(PlayerActionEvent caller, float state)
        {
            double diveBeat = caller.timer + caller.startBeat;
            SoundByte.PlayOneShotGame("splashdown/jumpPlayer");
            SoundByte.PlayOneShotGame("splashdown/splashPlayer", diveBeat + 1.75);
            if (state >= 1f || state <= -1f)
            {
                player.Jump(diveBeat, true, true);
                return;
            }
            SoundByte.PlayOneShotGame("splashdown/rollPlayer", diveBeat + 1);
            player.Jump(diveBeat, false, true);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(diveBeat + 1.75, delegate { crowdAnim.DoScaledAnimationAsync("CrowdCheer", 0.5f); }),
                new BeatAction.Action(diveBeat + 4, delegate { crowdAnim.Play("CrowdIdle", 0, 0); })
            });
        }

        private void JustJumpNoRollSound(PlayerActionEvent caller, float state)
        {
            double diveBeat = caller.timer + caller.startBeat;
            SoundByte.PlayOneShotGame("splashdown/jumpPlayer");
            SoundByte.PlayOneShotGame("splashdown/splashPlayer", diveBeat + 1.75);
            if (state >= 1f || state <= -1f)
            {
                player.Jump(diveBeat, true, true);
                return;
            }
            player.Jump(diveBeat, false, true);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(diveBeat + 1.75, delegate { crowdAnim.DoScaledAnimationAsync("CrowdCheer", 0.5f); }),
                new BeatAction.Action(diveBeat + 4, delegate { crowdAnim.Play("CrowdIdle", 0, 0); })
            });
        }

        private void Out(PlayerActionEvent caller) { }
    }
}

