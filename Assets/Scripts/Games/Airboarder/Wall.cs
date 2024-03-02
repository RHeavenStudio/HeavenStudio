using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Airboarder
{

    public class Wall : MonoBehaviour
    {
        public Airboarder game;
        
        public double targetBeat;
        public double appearBeat;

        public float normalizedWall;

        public float cueStart;

        
        

        [Header("Components")]
        [SerializeField] Animator wallAnim;

        

        private bool isCrouch;
        // Start is called before the first frame update
        private void Awake()
        {
            game = Airboarder.instance;
            

        }

        

        public void CueJump(double jumpBeat)
        {
            game.ScheduleInput(jumpBeat, 3f, Airboarder.InputAction_FlickRelease, JumpSuccess, JumphMiss, JumpEmpty);
            
            BeatAction.New(game, new List<BeatAction.Action>() {

                
                new BeatAction.Action(jumpBeat+1, delegate 
                {
                    game.CPU1.DoScaledAnimationAsync("jump", 1f, 0, 1);
                    SoundByte.PlayOneShotGame("airboarder/jump");
                    SoundByte.PlayOneShotGame("airboarder/jumpvox");
                }),
                new BeatAction.Action(jumpBeat+2, delegate {
                    game.CPU2.DoScaledAnimationAsync("jump", 1f, 0, 1);               
                    SoundByte.PlayOneShotGame("airboarder/jump");
                    SoundByte.PlayOneShotGame("airboarder/jumpvox");
                }),
                new BeatAction.Action(jumpBeat+2.5, delegate {game.cpu1CantBop = false;} ),  
                new BeatAction.Action(jumpBeat+3.5, delegate {game.cpu2CantBop = false;} ),  
            });

        }


       
            


        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            float normalizedWall = Conductor.instance.GetPositionFromBeat(appearBeat, 40f);
            wallAnim.GetComponent<Animator>().DoNormalizedAnimation("move", normalizedWall, 0);
            if (normalizedWall > 1) Destroy(gameObject);
        }

        public void JumpSuccess(PlayerActionEvent caller, float state)
        {
            game.Player.DoScaledAnimationAsync("jump", 1f, 0, 1);
            if (state is >= 1 or <= -1) 
            {
                wallAnim.DoScaledAnimationAsync("shake", 1f, 0, 1);
                SoundByte.PlayOneShotGame("airboarder/barely");
                SoundByte.PlayOneShotGame("airboarder/barelyvox");
            }
            else
            {
                SoundByte.PlayOneShotGame("airboarder/jump");
                SoundByte.PlayOneShotGame("airboarder/jumpvox");
            }

            double beat = caller.startBeat + caller.timer;
            BeatAction.New(this, new() {
                new(beat, ()=>game.playerCantBop = true),
                new(beat+1.5f, ()=>game.playerCantBop = false)});
            game.wantsCrouch = false;
        }

        public void JumphMiss(PlayerActionEvent caller){
            game.playerCantBop = true;
            game.wantsCrouch = false;
            game.Player.DoScaledAnimationAsync("hit2", 1f, 0, 1);
            wallAnim.DoScaledAnimationAsync("break", 1f, 0, 1);
            double beat = caller.startBeat + caller.timer;
            game.MissSound(beat);
            BeatAction.New(this, new() {
                new(beat+1.5f, ()=>game.playerCantBop = false)});
            
        }

        public void JumpEmpty(PlayerActionEvent caller){

        }
            


    }
}

