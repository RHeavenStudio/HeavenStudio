using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("tapTrial", "Tap Trial", "93ffb3", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { TapTrial.instance.Bop(eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Bop", "Whether both will bop to the beat or not")
                    }
                },
                new GameAction("tap", "Tap")
                {

                    function = delegate { TapTrial.instance.Tap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("double tap", "Double Tap")
                {

                    function = delegate { TapTrial.instance.DoubleTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("triple tap", "Triple Tap")
                {

                    function = delegate { TapTrial.instance.TripleTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 4.0f
                },
                new GameAction("jump tap prep", "Prepare Stance")
                {

                    function = delegate { TapTrial.instance.JumpTapPrep(eventCaller.currentEntity.beat); }, 
                },
                new GameAction("jump tap", "Jump Tap")
                {

                    function = delegate { TapTrial.instance.JumpTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("final jump tap", "Final Jump Tap")
                {

                    function = delegate { TapTrial.instance.FinalJumpTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("scroll event", "Scroll Background")
                {

                    function = delegate { TapTrial.instance.scrollEvent(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["flash"]); }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Scroll FX", "Will scroll"),
                        new Param("flash", false, "Flash FX", "Will flash to white"),
                    }
                },
                new GameAction("giraffe events", "Giraffe Animations")
                {

                    function = delegate { TapTrial.instance.giraffeEvent(eventCaller.currentEntity["instant"]); }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Enter?", "Giraffe will enter the scene"),
                        new Param("instant", false, "Instant", "Will the giraffe enter/exit instantly?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_TapTrial;
    using HeavenStudio.Common;

    public class TapTrial : Minigame
    {
        [Header("References")]
        public TapTrialPlayer player;
        //public GameObject tap;
        [SerializeField] List<Animator> monkeys;
        [SerializeField] List<GameObject> monkey_roots;
        [SerializeField] GameObject player_root;
        //temporary
        [SerializeField] List<GameObject> monkey_effects;
        [SerializeField] List<GameObject> player_effects;
        [SerializeField] Scroll scrollBG;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] ScrollForTap scroll;
        [SerializeField] GameObject giraffe;
        bool goBop = true, isPrep;
        float lastReportedBeat = 0f;
        bool hasJumped, isFinalJump;
        public float jumpStartTime = Single.MinValue;
        float jumpPos;
        public float time;
        bool once;
        public bool crIsRunning;
        [SerializeField] GameObject bg;
        bool giraffeIsIn;

        public static TapTrial instance { get; set; }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (goBop && !isPrep)
            {
                if (Conductor.instance.ReportBeat(ref lastReportedBeat))
                {
                    if (monkeys[0].IsPlayingAnimationName("Idle")) monkeys[0].DoScaledAnimationAsync("Bop", 0.5f);
                    if (monkeys[1].IsPlayingAnimationName("Idle")) monkeys[1].DoScaledAnimationAsync("Bop", 0.5f);
                    if (player.anim.IsPlayingAnimationName("Idle")) player.anim.DoScaledAnimationAsync("Bop", 0.5f);
                }
                else if (Conductor.instance.songPositionInBeats < lastReportedBeat)
                {
                    lastReportedBeat = Mathf.Round(Conductor.instance.songPositionInBeats);
                }
            }

            jumpPos = Conductor.instance.GetPositionFromBeat(jumpStartTime, 1f);
            if (Conductor.instance.songPositionInBeats >= jumpStartTime && Conductor.instance.songPositionInBeats < jumpStartTime + 1f)
            {
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                monkey_roots[0].transform.localPosition = new Vector3(0, 1.5f * yWeight);
                monkey_roots[1].transform.localPosition = new Vector3(0, 1.5f * yWeight);
                if (!isFinalJump)
                {
                    player_root.transform.localPosition = new Vector3(0f, 2.5f * yWeight);
                }
                else
                {
                    player_root.transform.localPosition = new Vector3(0f, 3.5f * yWeight);
                }

            }
            else
            {
                monkey_roots[0].transform.localPosition = new Vector3(0, 0);
                monkey_roots[1].transform.localPosition = new Vector3(0, 0);
                player_root.transform.localPosition = new Vector3(0, 0);
                if (hasJumped)
                {
                    //Jukebox.PlayOneShotGame("fanClub/landing_impact", pitch: UnityEngine.Random.Range(0.95f, 1f), volume: 1f / 4);
                }
                hasJumped = false;
                if (PlayerInput.Pressed() && !IsExpectingInputNow())
                {
                    player.anim.Play("Tap", 0, 0);
                    Jukebox.PlayOneShotGame("tapTrial/tonk");
                }
            }
        }

        public void Bop(bool isBopping)
        {
            goBop = isBopping;
        }

        public void Tap(float beat)
        {
            isPrep = true;
            Jukebox.PlayOneShotGame("tapTrial/ook");
            player.anim.DoScaledAnimationAsync("TapPrepare", 0.5f);

            //Monkey Tap Prepare Anim
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { monkeys[0].DoScaledAnimationAsync("TapPrepare", 0.5f); }),
                new BeatAction.Action(beat, delegate { monkeys[1].DoScaledAnimationAsync("TapPrepare", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[0].DoScaledAnimationAsync("Tap", 0.6f); particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[1].DoScaledAnimationAsync("Tap", 0.6f); }),
            });
            //CreateTap(beat);
            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, OnTap, OnTapMiss, OnEmpty);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { isPrep = false; })
            });
        }

        public void DoubleTap(float beat)
        {
            isPrep = true;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ookook",   beat),
                new MultiSound.Sound("tapTrial/ookook",   beat + 0.5f)
            });

            player.anim.DoScaledAnimationAsync("DoubleTapPrepare", 0.5f);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { monkeys[0].DoScaledAnimationAsync("DoubleTapPrepare", 0.5f); }),
                new BeatAction.Action(beat + .5f, delegate { monkeys[0].DoScaledAnimationAsync("DoubleTapPrepare_2", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[0].DoScaledAnimationAsync("DoubleTap", 0.6f); particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 1.5f, delegate { monkeys[0].DoScaledAnimationAsync("DoubleTap", 0.6f); particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 1.99f, delegate {monkeys[0].Play("Idle", 0, 0); }),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { monkeys[1].DoScaledAnimationAsync("DoubleTapPrepare", 0.5f); }),
                new BeatAction.Action(beat + .5f, delegate { monkeys[1].DoScaledAnimationAsync("DoubleTapPrepare_2", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[1].DoScaledAnimationAsync("DoubleTap", 0.6f); }),
                new BeatAction.Action(beat + 1.5f, delegate { monkeys[1].DoScaledAnimationAsync("DoubleTap", 0.6f); }),
                new BeatAction.Action(beat + 1.99f, delegate {monkeys[1].Play("Idle", 0, 0); }),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.99f, delegate { isPrep = false; })
            });

            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, OnDoubleTap, OnTapMiss, OnEmpty);
            ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, OnDoubleTap, OnTapMiss, OnEmpty);
        }

        public void TripleTap(float beat)
        {
            isPrep = true;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ooki1",   beat),
                new MultiSound.Sound("tapTrial/ooki2",   beat + 0.5f)
            });

            player.tripleOffset = 0;

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { player.anim.Play("PosePrepare_1", 0, 0);}),
                new BeatAction.Action(beat + .5f, delegate { player.anim.Play("PosePrepare_2", 0, 0);}),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { monkeys[0].Play("PostPrepare_1", 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { monkeys[0].Play("PostPrepare_2", 0, 0); }),
                new BeatAction.Action(beat + 2f, delegate { monkeys[0].Play("PostTap", 0, 0); }),
                new BeatAction.Action(beat + 2.5f, delegate { monkeys[0].Play("PostTap_2", 0, 0); }),
                new BeatAction.Action(beat + 3f, delegate { monkeys[0].Play("PostTap", 0, 0);}),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { monkeys[1].Play("PostPrepare_1", 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { monkeys[1].Play("PostPrepare_2", 0, 0); }),
                new BeatAction.Action(beat + 2f, delegate { monkeys[1].Play("PostTap", 0, 0); }),
                new BeatAction.Action(beat + 2.5f, delegate { monkeys[1].Play("PostTap_2", 0, 0);}),
                new BeatAction.Action(beat + 3f, delegate { monkeys[1].Play("PostTap", 0, 0);}),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 2.5f, delegate { particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 3f, delegate { particleEffectMonkeys(); }),
            });

            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, OnTripleTap, OnTapMiss, OnEmpty);
            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, OnTripleTap, OnTapMiss, OnEmpty);
            ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, OnTripleTap, OnTapMiss, OnEmpty);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 4f, delegate { isPrep = false; })
            });
        }

        public void JumpTap(float beat)
        {
            isPrep = true;
            hasJumped = true;
            Jukebox.PlayOneShotGame("tapTrial/jumptap1");

            player.anim.Play("JumpTap", 0, 0);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {jumpStartTime = Conductor.instance.songPositionInBeats;}),
                new BeatAction.Action(beat, delegate {monkeys[0].Play("JumpTap", 0, 0); }),
                new BeatAction.Action(beat, delegate {monkeys[1].Play("JumpTap", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { particleEffectMonkeys(); }),
                new BeatAction.Action(beat + 1f, delegate { particleEffectMonkeys_2(); }),
            });



            ScheduleInput(beat, .95f, InputType.STANDARD_DOWN, OnJumpTap, OnJumpTapMiss, OnEmpty); //why .95f? no idea, doesn't sound right w/ 1f

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { isPrep = false; })
            });
        }

        public void JumpTapPrep(float beat)
        {
            isPrep = true;
            monkeys[0].Play("JumpPrepare", 0, 0);
            monkeys[1].Play("JumpPrepare", 0, 0);
            player.anim.Play("JumpPrepare", 0, 0);
        }

        public void FinalJumpTap(float beat)
        {
            isPrep = true;
            hasJumped = true;
            isFinalJump = true;
            Jukebox.PlayOneShotGame("tapTrial/jumptap2");

            player.anim.Play("FinalJump");
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {jumpStartTime = Conductor.instance.songPositionInBeats;}),
                new BeatAction.Action(beat, delegate {monkeys[0].Play("Jump", 0, 0); }),
                new BeatAction.Action(beat, delegate {monkeys[1].Play("Jump", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[0].Play("FinalJumpTap", 0, 0); particleEffectMonkeys(); particleEffectMonkeys_2(); }),
                new BeatAction.Action(beat + 1f, delegate { monkeys[1].Play("FinalJumpTap", 0, 0); }),
            });


            ScheduleInput(beat, .95f, InputType.STANDARD_DOWN, OnJumpFinalTap, OnFinalJumpTapMiss, OnEmpty);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { isPrep = false; })
            });
        }

        public void giraffeEvent(bool instant)
        {
            float animTime = 0;
            if (instant) animTime = 1;
            if (!giraffeIsIn)
            {
                giraffe.SetActive(true);
                giraffe.GetComponent<Animator>().Play("Enter", 0, animTime);
                giraffeIsIn = true;
            }
            else if (giraffeIsIn)
            {
                giraffe.GetComponent<Animator>().Play("Exit", 0, animTime);
                giraffeIsIn = false;
            }
        }

        public void scrollEvent(bool isScrolling, bool flashToWhite)
        {
            if (isScrolling)
            {
                if (!crIsRunning) // if coroutine is not running, play the following once
                {
                    if (flashToWhite)
                    {
                        Sequence sequence = DOTween.Sequence();
                        sequence.Append(flash.DOColor(new Color(flash.color.r, flash.color.g, flash.color.b, .8f), 2f));
                        //sequence.Kill();
                    }
                    StartCoroutine(timer());
                }
            }
            else //should be the reverse of the code above
            {
                scrollBG.enabled = false;
                scrollBG.scrollSpeedY = 0;
            }
        }


        #region Player Action Scripts
        public void OnTap(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("tapTrial/tap");
            player.anim.DoScaledAnimationAsync("Tap", 0.6f);
            player_effects[0].GetComponent<ParticleSystem>().Play();
        }
        public void OnDoubleTap(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("tapTrial/tap");
            player.anim.DoScaledAnimationAsync("DoubleTap", 0.6f);
            player_effects[1].GetComponent<ParticleSystem>().Play();
        }

        public void OnTapMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("tapTrial/tapMonkey", pitch: 1.5f, volume: .3f);
        }

        public void OnJumpTapMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("tapTrial/tapMonkey", pitch: 1.5f, volume: .3f);
            player.anim.Play("JumpTap_Miss", 0, 0);
        }

        public void OnFinalJumpTapMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("tapTrial/tapMonkey", pitch: 1.5f, volume: .3f);
            player.anim.Play("FinalJump_Miss", 0, 0);
        }

        public void OnEmpty(PlayerActionEvent caller)
        {
            //empty
        }
        public void OnTripleTap(PlayerActionEvent caller, float beat)
        {
            if (player.tripleOffset % 2 == 0)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { player.anim.Play("PoseTap_L", 0, 0); })
                });
                player.tripleOffset += 1;
            }
            else
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { player.anim.Play("PoseTap_R", 0, 0); })
                });
                player.tripleOffset += 1;
            }
            player_effects[0].GetComponent<ParticleSystem>().Play();
            Jukebox.PlayOneShotGame("tapTrial/tap");
        }
        public void OnJumpTap(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("tapTrial/tap");
            player.anim.Play("JumpTap_Success", 0, 0);
            player_effects[0].GetComponent<ParticleSystem>().Play();
            player_effects[1].GetComponent<ParticleSystem>().Play();
        }
        public void OnJumpFinalTap(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("tapTrial/tap");
            player.anim.Play("FinalJump_Tap");
            player_effects[0].GetComponent<ParticleSystem>().Play();
            player_effects[1].GetComponent<ParticleSystem>().Play();
            isFinalJump = false;
        }
        #endregion

        #region Misc. Functions
        public void particleEffectMonkeys()
        {
            monkey_effects[0].GetComponent<ParticleSystem>().Play();
            monkey_effects[1].GetComponent<ParticleSystem>().Play();
        }

        public void particleEffectMonkeys_2()
        {
            monkey_effects[2].GetComponent<ParticleSystem>().Play();
            monkey_effects[3].GetComponent<ParticleSystem>().Play();
        }

        IEnumerator timer()
        {
            crIsRunning = true;
            while (scroll.scrollSpeedY < 20)
            {
                scroll.scrollSpeedY += 5f;
                yield return new WaitForSecondsRealtime(.5f);
            }
        }
        #endregion

        //this is the orig way for input handling
        //public void CreateTap(float beat, int type = 0)
        //{
        //    GameObject _tap = Instantiate(tap);
        //    _tap.transform.parent = tap.transform.parent;
        //    _tap.SetActive(true);
        //    Tap t = _tap.GetComponent<Tap>();
        //    t.startBeat = beat;
        //    t.type = type;
        //}
    }
}