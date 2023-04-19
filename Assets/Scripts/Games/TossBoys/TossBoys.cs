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
                },
                new GameAction("pass", "Pass Ball")
                {
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
        Dictionary<float, WhichTossKid> passBallDict = new Dictionary<float, WhichTossKid>();
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
            SetPassBallEvents();
            SetReceiver(who);
            GetCurrentReceiver().ShowArrow(beat, 1f);
            Jukebox.PlayOneShotGame("tossBoys/ballStart");
            hatchAnim.Play("HatchOpen");
            currentBall = Instantiate(ballPrefab, transform);
            ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void SetPassBallEvents()
        {
            passBallDict.Clear();
            var passBallEvents = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "pass" });
            for (int i = 0;  i < passBallEvents.Count; i++)
            {
                if (passBallEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    if (passBallDict.ContainsKey(passBallEvents[i].beat)) continue;
                    passBallDict.Add(passBallEvents[i].beat, (WhichTossKid)passBallEvents[i]["who"]);
                }
            }
        }

        void DeterminePass(float beat)
        {
            var tempLastReceiver = lastReceiver;
            lastReceiver = currentReceiver;
            if (passBallDict.TryGetValue(beat, out var receiver))
            {
                currentReceiver = receiver;
            }
            else
            {
                currentReceiver = tempLastReceiver;
            }
            PassBall(beat);
        }

        void PassBall(float beat)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 1f;
            switch (last + current)
            {
                case "blueRed":
                case "yellowRed":
                case "redYellow":
                    secondBeat = 0.5f;
                    break;
                default:
                    secondBeat = 1f;
                    break;
            }
            Debug.Log("SecondBeat: " + secondBeat + "\n1st: " + last + current + 1 + "\n2nd: " + last + current + 2 + "\n3rd: " + last + current + 3);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + 2, beat + secondBeat),
            };
            if (secondBeat == 0.5f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + 3, beat + 1));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        #region Inputs
        void JustHitBall(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                DeterminePass(caller.timer + caller.startBeat);
                return;
            }
            GetCurrentReceiver().HitBall();
            DeterminePass(caller.timer + caller.startBeat);
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

        string GetColorBasedOnTossKid(WhichTossKid tossKid, bool capital)
        {
            switch (tossKid)
            {
                case WhichTossKid.Akachan:
                    return capital ? "Red" : "red";
                case WhichTossKid.Aokun:
                    return capital ? "Blue" : "blue";
                case WhichTossKid.Kiiyan:
                    return capital ? "Yellow" : "yellow";
                default:
                    return "";
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

