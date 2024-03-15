using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SickBeats
{
    public class Virus : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer[] renderers;
        [SerializeField] Animator virusAnim;

        [Header("Variables")]
        public double startBeat;
        public int position;
        public int life = 1;

        bool isJust = false;    // not fundamental solution

        private SickBeats game;

        public void Init()
        {
            game = SickBeats.instance;
            ChangeColor();
        }

        public void Appear()
        {
            if (startBeat >= game.gameEndBeat) return;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("sickBeats/appear"+UnityEngine.Random.Range(0, 2).ToString(), startBeat),
            });
            
            BeatAction.New(game, new() {new BeatAction.Action(startBeat, delegate {
                VirusAnim("appear");
            })});

            isJust = false;

            PlayerInput.InputAction InputAction;
            if (PlayerInput.PlayerHasControl() && PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Touch)
            {
                InputAction = SickBeats.InputAction_FlickPress;
            }
            else
            {
                InputAction = position switch {
                    0 => SickBeats.InputAction_Right,
                    1 => SickBeats.InputAction_Up,
                    2 => SickBeats.InputAction_Left,
                    3 => SickBeats.InputAction_Down,
                };
            }

            // if (GameManager.instance.autoplay)
            // {
            //     game.ScheduleAutoplayInput(startBeat, 1, InputAction, Just, Miss, Empty);
            // }
            // else
            // {
            //     game.ScheduleUserInput(startBeat, 1, InputAction, Just, Miss, Empty, CanJust);
            // }
            game.ScheduleMissableInput(startBeat, 1, InputAction, Just, Miss, Empty, CanJust);

            var dir = position;
            BeatAction.New(game, new() {
                new BeatAction.Action(startBeat, delegate { game.isPrepare[dir] = true;}),
                new BeatAction.Action((startBeat+1.5f), delegate { game.isPrepare[dir] = false;}),
            });
        }

        public void Dash()
        {
            SoundByte.PlayOneShotGame("sickBeats/dash");
            VirusAnim("dash");
        }
        public void Summon()
        {
            VirusAnim("summon");
        }

        public void Move()
        {
            position++;
            if (position <= 3)
            {
                startBeat+=2;
                Appear();
            }
            else
            {
                Kill();
            }
        }
        
        public void Kill()
        {
            game.ScoreMiss();
            
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("sickBeats/virusIn", startBeat + 2),
                new MultiSound.Sound("sickBeats/miss", startBeat + 4),
                new MultiSound.Sound("sickBeats/fadeout", startBeat + 5),
            });

            BeatAction.New(game, new() {
                new BeatAction.Action(startBeat + 2, delegate {
                    virusAnim.DoScaledAnimationAsync("laugh", 0.5f);
                    virusAnim.DoScaledAnimationAsync("enter", 0.5f);
                }),
                new BeatAction.Action(startBeat + 4, delegate {
                    virusAnim.DoScaledAnimationAsync("hide", 0.5f);
                    game.orgAnim.DoScaledAnimationAsync("damage", 0.5f);
                    game.orgAlive = false;
                }),
                new BeatAction.Action(startBeat + 5, delegate {
                    game.orgAnim.DoScaledAnimationAsync("vanish", 0.5f);
                }),
                new BeatAction.Action(startBeat + 6, delegate {
                    virusAnim.DoScaledAnimationAsync("laugh", 0.5f);
                }),
                new BeatAction.Action(startBeat + 7, delegate {
                    game.orgAnim.DoScaledAnimationAsync("idleAdd", 0.5f);
                    game.orgAnim.DoScaledAnimationAsync("appear", 0.5f);
                    game.orgAlive = true;
                    Destroy(gameObject);
                }),
            });

            Debug.Log(startBeat);
            Debug.Log(game.docShockBeat);
            if (startBeat + 6 >= game.docShockBeat + 3)
            {
                game.docShockBeat = startBeat + 6;
                BeatAction.New(game, new() {
                    new BeatAction.Action(startBeat + 6, delegate {
                        game.docShock = true;
                        game.doctorAnim.DoScaledAnimationAsync("shock0", 0.5f);
                    }),
                    new BeatAction.Action(startBeat + 7, delegate {
                        game.doctorAnim.DoScaledAnimationAsync("shock1", 0.5f);
                    }),
                    new BeatAction.Action(startBeat + 9, delegate {
                        game.docShock = false;
                        game.doctorAnim.DoScaledAnimationAsync("idle", 0.5f);
                    }),
                });
            }

        }

        private void Just(PlayerActionEvent caller, float state)
        {
            life--;
            
            var dir = position;
            BeatAction.New(game, new() {new BeatAction.Action((startBeat+1) + game.RefillBeat, delegate { game.RepopFork(dir);})});
            game.isForkPop[dir] = false;
            isJust = true;

            if (life < 0)
            {
                if (state >= 1f)
                {
                    SoundByte.PlayOneShotGame("sickBeats/bad");
                    VirusAnim("stabLate");
                    KeyAnim("stabLate");
                }
                else if (state <= -1f)
                {
                    SoundByte.PlayOneShotGame("sickBeats/bad");
                    VirusAnim("stabFast");
                    KeyAnim("stabFast");
                }
                else
                {
                    SoundByte.PlayOneShotGame("sickBeats/hit");
                    VirusAnim("stab");
                    KeyAnim("stab");

                    BeatAction.New(game, new() {new BeatAction.Action((startBeat+2), delegate {
                        game.doctorAnim.DoScaledAnimationAsync("Vsign", 0.5f);
                    })});
                }
            }
            else
            {
                SoundByte.PlayOneShotGame("sickBeats/resist");
                VirusAnim("resist");
                KeyAnim("resist");
                ChangeColor();
                Move();
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            var dir = position;
            if (dir >= 0 && dir <= 3)
            {
                game.isMiss[dir] = true;
                BeatAction.New(game, new() {new BeatAction.Action((startBeat+1.5f), delegate { game.isMiss[dir] = false;})});
            }
            Dash();
            Move();
        }

        private void Empty(PlayerActionEvent caller) { }

        private bool CanJust() { 
            if (position < 0 || position > 3) return false;
            return game.isForkPop[position] || isJust;
        }

        void VirusAnim(string animation)
        {
            virusAnim.DoScaledAnimationAsync(animation, 0.5f);
            virusAnim.DoScaledAnimationAsync(animation+position.ToString(), 0.5f);
        }
        void KeyAnim(string animation)
        {
            game.keyAnim.Play("push");
            game.forkAnims[position].DoScaledAnimationAsync(animation+position.ToString(), 0.5f);
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                double beat = cond.songPositionInBeats;
                if (life < 0 && beat >= startBeat + 3) Destroy(gameObject);
            }
        }

        void ChangeColor()
        {
            foreach (var renderer in renderers)
            {
                renderer.material = game.RecolorMats[life];
            }
        }
    }
}