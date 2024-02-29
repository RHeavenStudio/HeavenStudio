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

        public float normalizedStart;


        

        [Header("Components")]

        public Animator anim;

        

        private bool isCrouch;
        // Start is called before the first frame update
        private void Awake()
        {
            game = Airboarder.instance;

        }

        public void CueDuck(double duckBeat)
        {
            game.ScheduleInput(duckBeat, 3f, Airboarder.InputAction_BasicPress, DuckSuccess, DuckMiss, DuckEmpty);
            BeatAction.New(game, new List<BeatAction.Action>() {
                
                new BeatAction.Action(duckBeat, delegate {game.cpu1CantBop = true;} ),  
                new BeatAction.Action(duckBeat, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
                new BeatAction.Action(duckBeat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),
                new BeatAction.Action(duckBeat+ 1, delegate {game.cpu2CantBop = true;} ),
                new BeatAction.Action(duckBeat+ 1, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),
                new BeatAction.Action(duckBeat+ 1, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
                new BeatAction.Action(duckBeat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
                new BeatAction.Action(duckBeat+ 2, delegate {game.playerCantBop = true;} ),
                new BeatAction.Action(duckBeat+ 2, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),                
                new BeatAction.Action(duckBeat+ 2, delegate {game.Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
                new BeatAction.Action(duckBeat+ 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
                new BeatAction.Action(duckBeat+ 2.5, delegate {game.cpu1CantBop = false;} ),
                new BeatAction.Action(duckBeat+3.5, delegate {game.cpu2CantBop = false;} ),
                new BeatAction.Action(duckBeat+4.5, delegate {game.playerCantBop = false;})
                });

        }
  //          double calcBeat = appearBeat;
            
            
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
           // }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            float normalizedStart = Conductor.instance.GetPositionFromBeat(appearBeat, 40f);
            anim.Play("move", 0, normalizedStart);
            anim.speed = 0;
 //           if (normalizedStart > 1) 
 //           {
 //               Destroy(gameObject);}




            
        }

        public void DuckSuccess(PlayerActionEvent caller, float state)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);
            SoundByte.PlayOneShotGame("airboarder/crouch");
        }

        public void DuckMiss(PlayerActionEvent caller)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f);
            anim.DoScaledAnimationAsync("shake", 1f);
            
        }

        public void DuckEmpty(PlayerActionEvent caller)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f);
            anim.DoScaledAnimationAsync("break", 1f);

        }
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

