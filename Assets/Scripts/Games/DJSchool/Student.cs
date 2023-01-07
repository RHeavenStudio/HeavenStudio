using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
        public bool swiping;
        public bool soundFX;

        [Header("Components")]
        [SerializeField] private SpriteRenderer flash;
        [SerializeField] private GameObject flashFX;
        [SerializeField] private GameObject flashFXInverse;
        [SerializeField] private GameObject TurnTable;
        [SerializeField] private GameObject slamFX;
        AudioMixerGroup mixer;

        private Animator tableAnim;

        private DJSchool game;

        public void Init()
        {
            game = DJSchool.instance;
            anim = GetComponent<Animator>();
            tableAnim = TurnTable.GetComponent<Animator>();
            tableAnim.speed = 0;
            mixer = Resources.Load<AudioMixer>("MainMixer").FindMatchingGroups("Music")[0];
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
        }

        private void Update()
        {
            
        }

        #region old hold
        //public void Hold(bool ace)
        //{
        //    isHolding = true;

        //    if (ace)
        //    {
        //        missed = false;
        //        shouldBeHolding = true;
        //        game.SetDJYellowHead(1);
        //    }

        //    Jukebox.PlayOneShotGame("djSchool/recordStop");

        //    anim.Play("Hold", 0, 0);
        //    tableAnim.Play("Student_Turntable_Hold", 0, 0);
        //    if (ace)
        //    {
        //        if (soundFX)
        //        {
        //            Conductor.instance.djSchoolHold.TransitionTo(.01f);
        //        }

        //        FlashFX(true);
        //    }

        //    // Settings.GetMusicMixer().audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(0.15f);
        //}
        #endregion

        #region onHold
        public void OnHitHold(PlayerActionEvent caller, float beat)
        {
            isHolding = true;

            missed = false;
            shouldBeHolding = true;
            game.SetDJYellowHead(1);

            Jukebox.PlayOneShotGame("djSchool/recordStop");

            anim.Play("Hold", 0, 0);
            //tableAnim.Play("Student_Turntable_Hold", 0, 0);

            if (soundFX)
            {
                mixer.audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(.01f);
            }
            FlashFX(true);
        }

        public void OnMissHold(PlayerActionEvent caller)
        {
            //isHolding = true;

            missed = true;
            game.SetDJYellowHead(3, true);
        }

        public void OnMissHoldForPlayerInput()
        {
            isHolding = true;

            missed = true;
            game.SetDJYellowHead(3, true);

            Jukebox.PlayOneShotGame("djSchool/recordStop");

            anim.Play("Hold", 0, 0);
            //tableAnim.Play("Student_Turntable_Hold", 0, 0);
        }
        #endregion

        public void OnEmpty(PlayerActionEvent caller)
        {
            //empty
        }

        public void UnHold()
        {
            isHolding = false;

            anim.Play("Unhold", 0, 0);
            missed = true;
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
        }

        #region onSwipe
        public void OnHitSwipe(PlayerActionEvent caller, float beat)
        {
            if (!missed)
            {
                isHolding = false;

                missed = false;
                shouldBeHolding = false;
                Jukebox.PlayOneShotGame("djSchool/recordSwipe");
                FlashFX(false);
                swiping = true;

                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { anim.Play("Swipe", 0, 0); }),
                    new BeatAction.Action(beat + 4f, delegate { swiping = false; }),
                });
                //anim.Play("Swipe", 0, 0);

                tableAnim.speed = 1;
                tableAnim.Play("Student_Turntable_Swipe", 0, 0);

                Instantiate(slamFX, this.transform.parent).SetActive(true);
                mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
            }
            else
            {
                OnMissSwipeForPlayerInput();
                Jukebox.PlayOneShotGame("djSchool/recordSwipe");
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { anim.Play("Swipe", 0, 0); }),
                    new BeatAction.Action(beat + 4f, delegate { swiping = false; }),
                });
                //anim.Play("Swipe", 0, 0);

                tableAnim.speed = 1;
                tableAnim.Play("Student_Turntable_Swipe", 0, 0);

                Instantiate(slamFX, this.transform.parent).SetActive(true);
                mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
            }
            
        }

        public void OnMissSwipe(PlayerActionEvent caller)
        {
            isHolding = false;
            //swiping = false;
            missed = true;
            game.SetDJYellowHead(3);
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
        }

        public void OnMissSwipeForPlayerInput()
        {
            isHolding = false;

            missed = true;
            game.SetDJYellowHead(3);
            //swiping = false;
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);

        }

        #endregion

        #region old swipe
        //public void Swipe(bool ace)
        //{
        //    isHolding = false;

        //    if (ace)
        //    {
        //        missed = false;
        //        shouldBeHolding = false;
        //        Jukebox.PlayOneShotGame("djSchool/recordSwipe");
        //        FlashFX(false);
        //        swiping = true;
        //    }
        //    else
        //    {
        //        // Missed record swipe sound should play here.
        //    }

        //    anim.Play("Swipe", 0, 0);

        //    tableAnim.speed = 1;
        //    tableAnim.Play("Student_Turntable_Swipe", 0, 0);

        //    Instantiate(slamFX).SetActive(true);
        //    Conductor.instance.normal.TransitionTo(.01f);
        //    // Settings.GetMusicMixer().audioMixer.FindSnapshot("Main").TransitionTo(0.15f);
        //    swiping = false;

        //}

        //public override void OnAce()
        //{
        //    if (!shouldBeHolding)
        //    {
        //        //Hold(true);
        //    }
        //    else
        //    {
        //        Conductor.instance.normal.TransitionTo(.01f);
        //        //Swipe(true);
        //    }

        //    eligible = false;
        //}

        #endregion

        private void FlashFX(bool inverse)
        {
            GameObject prefab = flashFX;

            if (inverse)
                prefab = flashFXInverse;

            GameObject flashFX_ = Instantiate(prefab, this.transform.parent);
            flashFX_.SetActive(true);
            flash.color = "D0FBFF".Hex2RGB();
            flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, 0.85f);
            flash.DOColor(new Color(flash.color.r, flash.color.g, flash.color.b, 0), 0.15f);
            Destroy(flashFX_, 0.5f);         
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


        //Not sure but will do?
        private void OnDestroy()
        {
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.01f);
        }
    }
}