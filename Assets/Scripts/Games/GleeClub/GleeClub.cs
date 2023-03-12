using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrGleeClubLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("gleeClub", "Glee Club", "FFFFFF", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_GleeClub;
    public class GleeClub : Minigame
    {
        [Header("Components")]
        [SerializeField] ChorusKid leftChorusKid;
        [SerializeField] ChorusKid middleChorusKid;
        [SerializeField] ChorusKid playerChorusKid;
        public static GleeClub instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (!PlayerInput.Pressing() && Conductor.instance.isPlaying)
            {
                playerChorusKid.StartSinging();
            }
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                playerChorusKid.StopSinging();
            }
            if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
            {
                playerChorusKid.StartSinging();
            }
        }
    }
}

