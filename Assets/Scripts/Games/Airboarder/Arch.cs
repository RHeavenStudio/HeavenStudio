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

        public float cueStart;

        
        

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
                new BeatAction.Action(duckBeat, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(duckBeat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),
                new BeatAction.Action(duckBeat+ 1, delegate {game.cpu2CantBop = true;} ),
                new BeatAction.Action(duckBeat+ 1, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f, 0, 1);}),
                new BeatAction.Action(duckBeat+ 1, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),   
                new BeatAction.Action(duckBeat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}), 
                new BeatAction.Action(duckBeat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouchvox");}),             
                new BeatAction.Action(duckBeat+ 2, delegate {game.playerCantBop = true;} ),
                new BeatAction.Action(duckBeat+ 2, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f, 0, 1);}),                
                new BeatAction.Action(duckBeat+ 2, delegate {game.Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(duckBeat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),   
                new BeatAction.Action(duckBeat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouchvox");}),               
                new BeatAction.Action(duckBeat+ 2.5, delegate {game.cpu1CantBop = false;} ),
                new BeatAction.Action(duckBeat+3.5, delegate {game.cpu2CantBop = false;} ),
                new BeatAction.Action(duckBeat+4.5, delegate {game.playerCantBop = false;})
                });

        }

        public void CueCrouch(double crouchBeat)
        {
            game.wantsCrouch = true;
            game.ScheduleInput(crouchBeat, 3f, Airboarder.InputAction_BasicPress, CrouchSuccess, CrouchMiss, CrouchEmpty);

            
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(crouchBeat, delegate {game.cpu1CantBop = true;}),
                new BeatAction.Action(crouchBeat, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(crouchBeat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),

                new BeatAction.Action(crouchBeat+1, delegate {game.cpu2CantBop = true;} ),  
                new BeatAction.Action(crouchBeat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouchCharge");}),
                new BeatAction.Action(crouchBeat+1, delegate {game.CPU1.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f, 0, 1);}),
                new BeatAction.Action(crouchBeat+1, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}), 
                            
                new BeatAction.Action(crouchBeat+2, delegate {game.CPU2.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f, 0, 1);}),                
                new BeatAction.Action(crouchBeat+2, delegate {game.Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),                
                new BeatAction.Action(crouchBeat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouchCharge");})
            });

        }

       
            


        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            float normalizedStart = Conductor.instance.GetPositionFromBeat(appearBeat, 40f);
            float horizArch = (5*normalizedStart) - 140;
            anim.GetComponent<Animator>().DoNormalizedAnimation("move", normalizedStart, 0);
            if (normalizedStart > 1) Destroy(gameObject);
        }

        public void DuckSuccess(PlayerActionEvent caller, float state)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f, 0, 1);
            SoundByte.PlayOneShotGame("airboarder/crouch");
            SoundByte.PlayOneShotGame("airboarder/crouchvox");
            double beat = caller.startBeat + caller.timer;
            BeatAction.New(this, new() {
                new(beat+1.5f, ()=>game.playerCantBop = false)});
        }

        public void DuckMiss(PlayerActionEvent caller)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f, 0, 1);
            anim.DoScaledAnimationAsync("shake", 1f, 0, 1);
            double beat = caller.startBeat + caller.timer;
            BeatAction.New(this, new() {
                new(beat+1.5f, ()=>game.playerCantBop = false)});
            
        }

        public void DuckEmpty(PlayerActionEvent caller)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f, 0, 1);
            anim.DoScaledAnimationAsync("break", 1f, 0, 1);
            double beat = caller.startBeat + caller.timer;
            BeatAction.New(this, new() {
                new(beat+1.5f, ()=>game.playerCantBop = false)});
            

        }

        public void CrouchSuccess(PlayerActionEvent caller, float state)
        {
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f, 0, 1);
            SoundByte.PlayOneShotGame("airboarder/crouchCharge");
            game.playerCantBop = true;
        }

        public void CrouchMiss(PlayerActionEvent caller){
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f, 0, 1);
            anim.DoScaledAnimationAsync("shake", 1f, 0, 1);
        }

        public void CrouchEmpty(PlayerActionEvent caller){
            game.Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f, 0, 1);
            anim.DoScaledAnimationAsync("break", 1f, 0, 1);
        }


    }
}

