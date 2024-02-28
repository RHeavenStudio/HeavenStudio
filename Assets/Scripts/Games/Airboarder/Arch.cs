using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Airboarder
{

    public class Arch : MonoBehaviour
    {
        public Airboarder game;
        
        public double targetBeat;
        public double appearBeat;

        

        [Header("Components")]

        public Animator anim;

        

        private bool isCrouch;
        // Start is called before the first frame update
        private void Awake()
        {
            double calcBeat = appearBeat;
            
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(calcBeat+28f, delegate {game.CueDuck(calcBeat);}),
                new BeatAction.Action(calcBeat+31f, delegate {game.ScheduleInput(calcBeat, 0f, Airboarder.InputAction_BasicPress, DuckSuccessArch, DuckMissArch, DuckEmptyArch);})
                
            });
            }

        // Update is called once per frame
        void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(appearBeat, 40f);
            anim.DoNormalizedAnimation("move", normalizedBeat, 0);
            anim.speed = 0;
            
            
            
        }

        public void DuckSuccessArch(PlayerActionEvent caller, float state)
        {
            game.DuckSuccess(targetBeat);
        }

        public void DuckMissArch(PlayerActionEvent caller)
        {
            game.DuckMiss(targetBeat);
        }

        public void DuckEmptyArch(PlayerActionEvent caller)
        {
            game.DuckEmpty(targetBeat);
        }
        public void CrouchSuccessArch(PlayerActionEvent caller, float state)
        {
            game.CrouchSuccess(targetBeat);
        }

        public void CrouchMissArch(PlayerActionEvent caller)
        {
            game.CrouchMiss(targetBeat);
        }

        public void CrouchEmptyArch(PlayerActionEvent caller)
        {
            game.CrouchEmpty(targetBeat);
        }        
    }
}

