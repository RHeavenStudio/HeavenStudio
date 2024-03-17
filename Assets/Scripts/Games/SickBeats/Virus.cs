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

        private SickBeats game;

        public void Init()
        {
            game = SickBeats.instance;
            ChangeColor();
        }

        public void Appear()
        {
            var InputAction = position switch {
                0 => SickBeats.InputAction_Right,
                1 => SickBeats.InputAction_Up,
                2 => SickBeats.InputAction_Left,
                3 => SickBeats.InputAction_Down,
            };
            
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("sickBeats/appear", startBeat, pitch: UnityEngine.Random.Range(0.9f, 1.1f)),
            });
            
            BeatAction.New(game, new() {new BeatAction.Action(startBeat, delegate {
                VirusAnim("appear");
            })});

            game.ScheduleInput(startBeat, 1, InputAction, Just, Miss, Empty, CanJust);
        }

        public void Dash()
        {
            SoundByte.PlayOneShotGame("sickBeats/whoosh");
            VirusAnim("dash");
        }
        public void Come()
        {
            VirusAnim("come");
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
            BeatAction.New(game, new() {
                new BeatAction.Action((startBeat+1) + game.RefillBeat, delegate {
                    virusAnim.DoScaledAnimationAsync("laugh", 0.5f);
                    virusAnim.DoScaledAnimationAsync("enter", 0.5f);
                }),
                new BeatAction.Action((startBeat+3) + game.RefillBeat, delegate {
                    virusAnim.DoScaledAnimationAsync("hide", 0.5f);
                    game.orgAnim.DoScaledAnimationAsync("damage", 0.5f);
                    game.orgAlive = false;
                }),
                new BeatAction.Action((startBeat+4) + game.RefillBeat, delegate {
                    game.orgAnim.DoScaledAnimationAsync("vanish", 0.5f);
                }),
                new BeatAction.Action((startBeat+5) + game.RefillBeat, delegate {
                    virusAnim.DoScaledAnimationAsync("laugh", 0.5f);
                    game.docShock = true;
                    game.doctorAnim.DoScaledAnimationAsync("shock0", 0.5f);
                }),
                new BeatAction.Action((startBeat+6) + game.RefillBeat, delegate {
                    game.orgAnim.DoScaledAnimationAsync("idleAdd", 0.5f);
                    game.orgAnim.DoScaledAnimationAsync("appear", 0.5f);
                    game.orgAlive = true;
                    Destroy(gameObject);
                    game.doctorAnim.DoScaledAnimationAsync("shock1", 0.5f);
                }),
                new BeatAction.Action((startBeat+8) + game.RefillBeat, delegate {
                    game.docShock = false;
                    game.doctorAnim.DoScaledAnimationAsync("idle", 0.5f);
                }),
            });
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            life--;
            
            var dir = position;
            BeatAction.New(game, new() {new BeatAction.Action((startBeat+1) + game.RefillBeat, delegate { game.RepopFork(dir);})});
            game.isForkPop[dir] = false;


            if (life < 0)
            {
                if (state >= 1f)
                {
                    VirusAnim("stabLate");
                    KeyAnim("stabLate");
                }
                else if (state <= -1f)
                {
                    VirusAnim("stabFast");
                    KeyAnim("stabFast");
                }
                else
                {
                    SoundByte.PlayOneShotGame("sickBeats/stab");
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
            Dash();
            Move();
        }

        private void Empty(PlayerActionEvent caller) { }

        private bool CanJust() { 
            if (position < 0 || position > 3) return false;
            return game.isForkPop[position];
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
            renderers[0].material = game.RecolorMats[life];

            renderers[1].material = game.RecolorMats[life];
            
            Color newColor = game.color[life];
            renderers[2].color = new Color(newColor.r, newColor.g, newColor.b, renderers[2].color.a);

        }
    }
}