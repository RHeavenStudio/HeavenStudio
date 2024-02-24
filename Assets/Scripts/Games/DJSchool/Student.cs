using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using DG.Tweening;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DJSchool
{
    public class Student : MonoBehaviour
    {
        public Animator anim;
        public static bool soundFX;

        [Header("Properties")]
        public bool isHolding;
        public bool shouldBeHolding;
        public bool missed;
        public bool swiping;
        bool canBoo = true;

        [Header("Components")]
        [SerializeField] private SpriteRenderer flash;
        [SerializeField] private GameObject flashFX;
        [SerializeField] private GameObject flashFXInverse;
        [SerializeField] private GameObject TurnTable;
        //[SerializeField] private GameObject slamFX;
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

        public void ForceHold()
        {
            isHolding = true;

            missed = false;
            shouldBeHolding = true;

            anim.Play("Hold", -1, 1);
            tableAnim.DoScaledAnimationAsync("Student_Turntable_Hold", 0.5f);

            if (soundFX)
            {
                mixer.audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(.1f);
            }
        }

        void EnableBoo()
        {
            canBoo = true;
        }

        #region onHold
        public void OnHitHold(PlayerActionEvent caller, float beat)
        {
            isHolding = true;

            missed = false;
            shouldBeHolding = true;

            SoundByte.PlayOneShotGame("djSchool/recordStop");

            anim.DoScaledAnimationAsync("Hold", 0.5f);
            tableAnim.DoScaledAnimationAsync("Student_Turntable_StartHold", 0.5f);

            if (soundFX)
            {
                mixer.audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(.1f);
            }
            FlashFX(true);
        }

        public void OnMissHold(PlayerActionEvent caller)
        {
            //isHolding = true;
            if (canBoo)
            {
                Sound booSound = SoundByte.PlayOneShotGame("djSchool/boo", -1, 1, 0.8f);
                CancelInvoke();
                canBoo = false;
                Invoke("EnableBoo", 1f);
            }

            if (!game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpFirst) && !game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpSecond))
            {
                game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.CrossEyed);
                if (game.djYellowHolding || game.andStop) game.djYellowScript.Reverse();
                else game.djYellowScript.Reverse(true);
            }
            missed = true;
        }

        public void OnMissHoldForPlayerInput()
        {
            isHolding = true;

            missed = true;

            SoundByte.PlayOneShotGame("djSchool/recordStop");

            anim.DoScaledAnimationAsync("Hold", 0.5f);
            tableAnim.DoScaledAnimationAsync("Student_Turntable_StartHold", 0.5f);
            if (soundFX)
            {
                mixer.audioMixer.FindSnapshot("DJSchool_Hold").TransitionTo(.1f);
            }
            if (!game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpFirst) && !game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpSecond))
            {
                game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.CrossEyed);
                if (game.djYellowHolding || game.andStop) game.djYellowScript.Reverse();
                else game.djYellowScript.Reverse(true);
            }
        }
        #endregion

        public void OnEmpty(PlayerActionEvent caller)
        {
            //empty
        }

        public void UnHold()
        {
            isHolding = false;

            anim.DoScaledAnimationAsync("Unhold", 0.5f);
            if (canBoo)
            {
                Sound booSound = SoundByte.PlayOneShotGame("djSchool/boo", -1, 1, 0.8f);
                CancelInvoke();
                canBoo = false;
                Invoke("EnableBoo", 1f);
            }
            missed = true;
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);
            tableAnim.DoScaledAnimationAsync("Student_Turntable_Idle", 0.5f);
            if (!game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpFirst) && !game.djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.UpSecond))
            {
                game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.CrossEyed);
                if (game.djYellowHolding || game.andStop) game.djYellowScript.Reverse();
                else game.djYellowScript.Reverse(true);
            }
        }

        #region onSwipe
        public void OnHitSwipeCheer(PlayerActionEvent caller, float beat)
        {
            OnHitSwipe(caller, beat);
            SoundByte.PlayOneShotGame("djSchool/cheer", caller.timer + caller.startBeat + 1f, 1, 0.8f);
        }
        public void OnHitSwipe(PlayerActionEvent caller, float beat)
        {
            game.shouldBeHolding = false;
            isHolding = false;
            swiping = true;
            SoundByte.PlayOneShotGame("djSchool/recordSwipe");
            anim.Play("Swipe", 0, 0);
            if (beat >= 1f || beat <= -1f) missed = true;
            if (!missed)
            {
                missed = false;
                shouldBeHolding = false;
                FlashFX(false);

                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 4f, delegate { swiping = false; }),
                });
                game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.UpSecond);
                game.djYellowScript.Reverse();
                game.smileBeat = caller.timer + caller.startBeat + 1f;

                tableAnim.speed = 1;
                tableAnim.DoScaledAnimationAsync("Student_Turntable_Swipe", 0.5f);

                //Instantiate(slamFX, this.transform.parent).SetActive(true);
                mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);
            }
            else
            {
                OnMissSwipeForPlayerInput(caller.timer + caller.startBeat + 1);
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 4f, delegate { swiping = false; }),
                });
                tableAnim.speed = 1;
                tableAnim.DoScaledAnimationAsync("Student_Turntable_Swipe", 0.5f);

                //Instantiate(slamFX, this.transform.parent).SetActive(true);
                mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);
            }

        }

        public void OnMissSwipe(PlayerActionEvent caller)
        {
            isHolding = false;
            //swiping = false;
            missed = true;
            // mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);
            if (canBoo)
            {
                Sound booSound = SoundByte.PlayOneShotGame("djSchool/boo", caller.timer + caller.startBeat + 1f, 1, 0.8f);
                CancelInvoke();
                canBoo = false;
                Invoke("EnableBoo", 1);
            }
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1, delegate
                {
                    if (game.BeatIsInBopRegion(caller.timer + caller.startBeat + 1))
                    {
                        game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.CrossEyed);
                        if (game.djYellowHolding) game.djYellowScript.Reverse();
                        else game.djYellowScript.Reverse(true);
                    }
                })
            });
        }

        public void OnMissSwipeForPlayerInput(double beat)
        {
            isHolding = false;

            missed = true;
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (game.BeatIsInBopRegion(beat))
                    {
                        game.djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.CrossEyed);
                        if (game.djYellowHolding) game.djYellowScript.Reverse();
                        else game.djYellowScript.Reverse(true);
                    }
                })
            });
        }

        public void OnFlickSwipe()
        {
            anim.Play("Swipe", 0, 0);
            tableAnim.speed = 1;
            tableAnim.DoScaledAnimationAsync("Student_Turntable_Swipe", 0.5f);

            isHolding = false;

            missed = true;

            OnMissSwipeForPlayerInput(Conductor.instance.songPositionAsDouble + 1);
            SoundByte.PlayOneShotGame("djSchool/recordSwipe");
        }

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
            mixer.audioMixer.FindSnapshot("Main").TransitionTo(.04f);
        }
    }
}