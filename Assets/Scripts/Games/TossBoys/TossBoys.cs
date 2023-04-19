using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTossBoysLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tossBoys", "Toss Boys", "9cfff7", false, false, new List<GameAction>()
            {
                new GameAction("dispense", "Dispense")
                {
                    function = delegate { var e = eventCaller.currentEntity; TossBoys.instance.Dispense(e.beat, e["who"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?")
                    }
                }
            });
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TossBoys;
    using UnityEngine.UI.Extensions;

    public class TossBoys : Minigame
    {
        public enum KidChoice
        {
            Akachan = 0,
            Aokun = 1,
            Kiiyan = 2
        }
        public enum WhichTossKid
        {
            None = -1,
            Akachan = 0,
            Aokun = 1,
            Kiiyan = 2
        }
        [Header("Components")]
        [SerializeField] TossKid akachan;
        [SerializeField] TossKid aokun;
        [SerializeField] TossKid kiiyan;
        [SerializeField] Animator hatchAnim;
        [SerializeField] TossBoysBall ballPrefab;

        [Header("Properties")]
        WhichTossKid lastReceiver = WhichTossKid.None;
        WhichTossKid currentReceiver = WhichTossKid.None;
        public TossBoysBall currentBall = null;

        public static TossBoys instance;

        private void Awake()
        {
            instance = this;            
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    akachan.HitBall(false);
                }
                if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
                {
                    aokun.HitBall(false);
                }
                if (PlayerInput.GetAnyDirectionDown() && !IsExpectingInputNow(InputType.DIRECTION_DOWN))
                {
                    kiiyan.HitBall(false);
                }
            }
        }

        public void Dispense(float beat, int who)
        {
            if (currentBall != null) return;
            SetReceiver(who);
            GetCurrentReceiver().ShowArrow(beat, 1f);
            Jukebox.PlayOneShotGame("tossBoys/ballStart");
            hatchAnim.Play("HatchOpen");
            currentBall = Instantiate(ballPrefab, transform);
            ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        #region Inputs
        void JustHitBall(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                return;
            }
            GetCurrentReceiver().HitBall();
            lastReceiver = currentReceiver;
        }

        void Miss(PlayerActionEvent caller)
        {
            GetCurrentReceiver().Miss();
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        void Empty(PlayerActionEvent caller) { }
        #endregion

        #region HelperFunctions
        public TossKid GetCurrentReceiver()
        {
            switch (currentReceiver)
            {
                case WhichTossKid.Akachan:
                    return akachan;
                case WhichTossKid.Aokun:
                    return aokun;
                case WhichTossKid.Kiiyan:
                    return kiiyan;
                default:
                    return null;
            }
        }

        public void SetReceiver(int who)
        {
            currentReceiver = (WhichTossKid)who;
        }

        InputType GetInputTypeBasedOnCurrentReceiver()
        {
            switch (currentReceiver)
            {
                case WhichTossKid.Akachan:
                    return InputType.STANDARD_DOWN;
                case WhichTossKid.Aokun:
                    return InputType.STANDARD_ALT_DOWN;
                case WhichTossKid.Kiiyan:
                    return InputType.DIRECTION_DOWN;
                default:
                    return InputType.ANY;
            }
        }
        #endregion
    }
}

