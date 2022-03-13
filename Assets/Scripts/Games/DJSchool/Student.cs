using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_DJSchool
{
    public class Student : PlayerActionObject
    {
        public Animator anim;

        [Header("Properties")]
        public float holdBeat;
        public float swipeBeat;
        public bool isHolding;
        public bool shouldBeHolding;
        public bool eligible;
        public bool missed;

        [Header("Components")]
        [SerializeField] private SpriteRenderer flash;
        [SerializeField] private GameObject flashFX;
        [SerializeField] private GameObject flashFXInverse;
        [SerializeField] private GameObject TurnTable;
        [SerializeField] private GameObject slamFX;

        private Animator tableAnim;

        private DJSchool game;

        private void Start()
        {
            game = DJSchool.instance;
            anim = GetComponent<Animator>();
            tableAnim = TurnTable.GetComponent<Animator>();
            tableAnim.speed = 0;
        }

        private void Update()
        {
            float beatToUse = shouldBeHolding ? swipeBeat : holdBeat;
            float normalizedBeat = Conductor.instance.GetPositionFromMargin(beatToUse + 2, 1);

            if (eligible)
            {
                StateCheck(normalizedBeat);

                if (normalizedBeat > Minigame.LateTime())
                {
                    eligible = false;
                    missed = true;

                    if (shouldBeHolding)
                    {
                        shouldBeHolding = false;
                    }
                    else
                    {
                        shouldBeHolding = true;
                        game.SetDJYellowHead(3);
                    }
                }
            }

            if (!isHolding)
            {
                if (PlayerInput.Pressed())
                {
                    if (!shouldBeHolding && state.perfect && eligible)
                    {
                        Hold(true);
                        eligible = false;
                    }
                    else
                    {
                        if (!shouldBeHolding)
                            eligible = false;

                        Hold(false);

                        missed = true;
                        game.SetDJYellowHead(3, true);
                    }
                }
            }
            else
            {
                if (PlayerInput.PressedUp())
                {
                    if (shouldBeHolding && state.perfect && eligible)
                    {
                        Swipe(true);
                        eligible = false;
                    }
                    else
                    {
                        if (shouldBeHolding)
                        {
                            Swipe(false);
                            eligible = false;
                        }
                        else
                        {
                            UnHold();
                        }

                        missed = true;
                        game.SetDJYellowHead(3);
                    }
                }
            }
        }

        public void Hold(bool ace)
        {
            isHolding = true;
            
            if (ace)
            {
                missed = false;
                shouldBeHolding = true;
                game.SetDJYellowHead(1);
            }

            Jukebox.PlayOneShotGame("djSchool/recordStop");
            anim.Play("Hold", 0, 0);

            if (ace)
            {
                FlashFX(true);
            }

            // Settings.GetMusicMixer().audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(0.15f);
        }

        public void UnHold()
        {
            isHolding = false;

            anim.Play("Unhold", 0, 0);

            // Settings.GetMusicMixer().audioMixer.FindSnapshot("Main").TransitionTo(0.15f);
        }

        public void Swipe(bool ace)
        {
            isHolding = false;
            
            if (ace)
            {
                missed = false;
                shouldBeHolding = false;
                Jukebox.PlayOneShotGame("djSchool/recordSwipe");
                FlashFX(false);
            }
            else
            {
                // Missed record swipe sound should play here.
            }

            anim.Play("Swipe", 0, 0);

            tableAnim.speed = 1;
            tableAnim.Play("Student_Turntable_Swipe", 0, 0);

            Instantiate(slamFX).SetActive(true);

            // Settings.GetMusicMixer().audioMixer.FindSnapshot("Main").TransitionTo(0.15f);
        }

        public override void OnAce()
        {
            if (!shouldBeHolding)
            {
                Hold(true);
            }
            else
            {
                Swipe(true);
            }

            eligible = false;
        }

        private void FlashFX(bool inverse)
        {
            GameObject prefab = flashFX;

            if (inverse)
                prefab = flashFXInverse;

            GameObject flashFX_ = Instantiate(prefab, this.transform.parent);
            flashFX_.SetActive(true);
            Destroy(flashFX_, 0.5f);

            flash.color = "D0FBFF".Hex2RGB();
            flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, 0.85f);
            flash.DOColor(new Color(flash.color.r, flash.color.g, flash.color.b, 0), 0.15f);
        }

        public void TransitionBackToIdle()
        {
            // for letting go of "hold"
            if (anim.speed == 0)
            {
                anim.speed = 1;
                anim.Play("Idle", 0, 0);
            }
        }
    }
}