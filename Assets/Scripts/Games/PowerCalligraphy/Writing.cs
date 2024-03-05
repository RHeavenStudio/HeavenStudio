using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_PowerCalligraphy
{
    public class Writing : MonoBehaviour
    {
        // Declaring the same enum in another class is not beautiful.
        public enum LetterType
        {
            re,
            ten,
            chikara,
            onore,
            sun,
            kokoro,
            tsurunihamushi,
            tsurunihamushi_korean,
        }

        public double targetBeat;
        public int type;
        public Animator paperAnim;
        public Animator fudePosAnim;
        public Animator fudeAnim;

        float scrollRateX => 6f / (Conductor.instance.pitchedSecPerBeat * 2f);
        float scrollRateY => -10f / (Conductor.instance.pitchedSecPerBeat * 2f);
        public bool onGoing = false;
        bool isEnd = false;
        int num;
        Sound releaseSound = null;

        private PowerCalligraphy game;

        public void Init()
        {
            game = PowerCalligraphy.instance;

            paperAnim.Play(type switch {
                (int)LetterType.re => "paper-re00",
                (int)LetterType.ten => "paper-ten00",
                (int)LetterType.chikara => "paper-chikara00",
                (int)LetterType.onore => "paper-onore00",
                (int)LetterType.sun => "paper-sun00",
                (int)LetterType.kokoro => "paper-kokoro00",
                (int)LetterType.tsurunihamushi => "paper-tsurunihamushi00",
                (int)LetterType.tsurunihamushi_korean => "paper-tsurunihamushi_kr00",
            });
        }

        public void Play()
        {
            switch(type)
            {
                case (int)LetterType.re:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/reShout", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+2f, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            fudePosAnim.DoScaledAnimationAsync("fudePos-re01", 0.5f);
                            paperAnim.Play("paper-re01");
                        }),
                        new BeatAction.Action(targetBeat+3f, delegate {
                            fudePosAnim.DoScaledAnimationAsync("fudePos-re02", 0.5f);
                            paperAnim.Play("paper-re02");
                            }),
                        new BeatAction.Action(targetBeat+4f, delegate
                        {
                            onGoing = true;
                            fudeAnim.DoScaledAnimationAsync("fude-sweep", 0.5f);
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                            if (releaseSound is null) Debug.Log("!?");
                        }),
                        new BeatAction.Action(targetBeat+6f, delegate { paperAnim.Play("paper-re04-end");}),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;
                
                case (int)LetterType.ten:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/comma1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/comma2", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/comma2", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/comma3", targetBeat+4f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+2f, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+3f, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+4f, delegate { fudePosAnim.DoScaledAnimationAsync("fudePos-ten01", 0.5f);}),
                        new BeatAction.Action(targetBeat+5f, delegate
                        {
                            onGoing = true;
                            fudeAnim.DoScaledAnimationAsync("fude-pause", 0.5f);
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseB1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+6f, delegate { paperAnim.Play("paper-ten02-end");}),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.chikara:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+0.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+3f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+4f, delegate
                        {
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.onore:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1.5f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+4f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+4f, delegate
                        {
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.sun:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+0.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+2f, delegate
                        {
                            num = 1;
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+5f, delegate
                        {
                            num = 2;
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseB1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+2f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.kokoro:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+4.5f, volume:0.3f), // +Agb
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+1f, delegate
                        {
                            num = 1;
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+5f, delegate
                        {
                            num = 2;
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseB1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+7f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+1f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.tsurunihamushi:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6.5f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7.25f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+7.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+8f, delegate
                        {
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+11f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+8f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case (int)LetterType.tsurunihamushi_korean:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),     // korean
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2.5f),     // korean
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+3.25f),    // korean
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6.5f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7.25f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+7.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat+8f, delegate
                        {
                            onGoing = true;
                            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
                        }),
                        new BeatAction.Action(targetBeat+11f, delegate
                        { 
                            isEnd = true;
                            fudeAnim.Play("fude-none");
                            paperAnim.enabled = false;
                        }),
                    });
                    game.ScheduleInput(targetBeat+8f, 2f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;
            }
        }

        private void writeSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f)
                Late();
            else if (state <= -1f)
                Fast();
            else
                Just();
        }

        private void writeMiss(PlayerActionEvent caller)
        {
            if (onGoing)
                Miss();
        }

        private void Empty(PlayerActionEvent caller) { }

        bool CanSuccess()
        {
            return onGoing;
        }

        public void Just()
        {
            onGoing = false;
            switch(type)
            {
                case (int)LetterType.re:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    paperAnim.Play("paper-re03-just");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-re03-just", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    break;
                
                case (int)LetterType.ten:
                    fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                    paperAnim.Play("paper-ten01-just");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-ten02-just", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseB2");
                    break;

                case (int)LetterType.chikara:
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    break;

                case (int)LetterType.onore:
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    break;

                case (int)LetterType.sun:
                    if (num==1) {
                        SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    } else {
                        SoundByte.PlayOneShotGame("powerCalligraphy/releaseB2");
                    }
                    break;

                case (int)LetterType.kokoro:
                    if (num==1) {
                        SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    } else {
                        SoundByte.PlayOneShotGame("powerCalligraphy/releaseB2");
                    }
                    break;

                case (int)LetterType.tsurunihamushi:
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    break;

                case (int)LetterType.tsurunihamushi_korean:
                    SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                    break;
            }
        }
        public void Late()
        {
            onGoing = false;
            switch(type)
            {
                case (int)LetterType.re:
                    paperAnim.Play("paper-re03-late");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-re03-late", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/6");    // WIP  HANE-miss?
                    break;
                
                case (int)LetterType.ten:
                    paperAnim.Play("paper-ten01-late");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-ten02-late", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/8");    // WIP  TOME-miss?
                    break;

                case (int)LetterType.chikara:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;

                case (int)LetterType.onore:
                    SoundByte.PlayOneShotGame("powerCalligraphy/6");    // WIP  HANE-miss?
                    break;

                case (int)LetterType.sun:
                    break;

                case (int)LetterType.kokoro:
                    break;

                case (int)LetterType.tsurunihamushi:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;

                case (int)LetterType.tsurunihamushi_korean:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;
            }
        }
        public void Fast()
        {
            onGoing = false;
            switch(type)
            {
                case (int)LetterType.re:
                    paperAnim.Play("paper-re03-fast");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-re03-fast", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/6");    // WIP  HANE-miss?
                    break;
                
                case (int)LetterType.ten:
                    paperAnim.Play("paper-ten01-fast");
                    fudePosAnim.DoScaledAnimationAsync("fudePos-ten02-fast", 0.5f);
                    SoundByte.PlayOneShotGame("powerCalligraphy/8");    // WIP  TOME-miss?
                    break;

                case (int)LetterType.chikara:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;

                case (int)LetterType.onore:
                    SoundByte.PlayOneShotGame("powerCalligraphy/6");    // WIP  HANE-miss?
                    break;

                case (int)LetterType.sun:
                    break;

                case (int)LetterType.kokoro:
                    break;

                case (int)LetterType.tsurunihamushi:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;

                case (int)LetterType.tsurunihamushi_korean:
                    SoundByte.PlayOneShotGame("powerCalligraphy/9");    // WIP  HARAI-miss?
                    break;
            }

            if (releaseSound is null) Debug.Log("!");
            if (releaseSound is not null)
            {
                Debug.Log("?");
                releaseSound.Stop();
                releaseSound = null;
            }
        }
        public void Miss()
        {
            onGoing = false;
            SoundByte.PlayOneShotGame("powerCalligraphy/7");    // WIP
            switch(type)
            {
                case (int)LetterType.re:
                    break;
                
                case (int)LetterType.ten:
                    fudePosAnim.DoScaledAnimationAsync("fudePos-ten02-miss", 0.5f);
                    break;

                case (int)LetterType.chikara:
                    break;

                case (int)LetterType.onore:
                    break;

                case (int)LetterType.sun:
                    break;

                case (int)LetterType.kokoro:
                    break;

                case (int)LetterType.tsurunihamushi:
                    break;

                case (int)LetterType.tsurunihamushi_korean:
                    break;
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (isEnd)
                {
                    double beat = cond.songPositionInBeats;
                    // Paper scroll.
                    var paperPos = transform.localPosition;
                    var newPaperX = paperPos.x + (scrollRateX * Time.deltaTime);
                    var newPaperY = paperPos.y + (scrollRateY * Time.deltaTime);
                    transform.localPosition = new Vector3(newPaperX, newPaperY, paperPos.z);
                    if (beat >= targetBeat + 13) Destroy(gameObject);
                }
            }
        }
    }
}