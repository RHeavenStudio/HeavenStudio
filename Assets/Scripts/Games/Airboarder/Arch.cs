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
            
            
      //      BeatAction.New(game, new List<BeatAction.Action>()
    //        {
             //   new BeatAction.Action(calcBeat+31, delegate {game.ScheduleInput(calcBeat, 0f, Airboarder.InputAction_BasicPress, DuckSuccessArch, DuckMissArch, DuckEmptyArch);}),
             //   new BeatAction.Action(calcBeat+28, delegate {game.cpu1CantBop = true;} ),  
//                new BeatAction.Action(calcBeat+28, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
//                new BeatAction.Action(calcBeat+28, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),
//                new BeatAction.Action(calcBeat+29, delegate {game.cpu2CantBop = true;} ),
//                new BeatAction.Action(calcBeat+29, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),
 //               new BeatAction.Action(calcBeat+29, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
 //               new BeatAction.Action(calcBeat+29, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
//                new BeatAction.Action(calcBeat+30, delegate {game.playerCantBop = true;} ),
//                new BeatAction.Action(calcBeat+30, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),                
//                new BeatAction.Action(calcBeat+30, delegate {game.Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
//                new BeatAction.Action(calcBeat+30, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
//                new BeatAction.Action(calcBeat+30.5, delegate {game.cpu1CantBop = false;} ),
//                new BeatAction.Action(calcBeat+31.5, delegate {game.cpu2CantBop = false;} ),
 //               new BeatAction.Action(calcBeat+32.5, delegate {game.playerCantBop = false;} )
                
         //   });
            }

        // Update is called once per frame
        void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(appearBeat, 40f);
            anim.Play("move", -1, normalizedBeat);
            anim.speed = 0;
            if (normalizedBeat > 1) Destroy(gameObject);
            
        }

  //      public void DuckSuccessArch(PlayerActionEvent caller, float state)
 //       {
 //           game.Player.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);
   //         SoundByte.PlayOneShotGame("airboarder/crouch");
  //      }

 //       public void DuckMissArch(PlayerActionEvent caller)
  //      {
      //      game.DuckMiss(targetBeat);
    //    }

  //      public void DuckEmptyArch(PlayerActionEvent caller)
 //       {
 //           game.DuckEmpty(targetBeat);
 //       }
 //       public void CrouchSuccessArch(PlayerActionEvent caller, float state)
 //       {
  //          game.CrouchSuccess(targetBeat);
 //       }

 //       public void CrouchMissArch(PlayerActionEvent caller)
 //       {
 //           game.CrouchMiss(targetBeat);
 //       }

  //      public void CrouchEmptyArch(PlayerActionEvent caller)
  //      {
  //          game.CrouchEmpty(targetBeat);
 //       }        
    }
}

