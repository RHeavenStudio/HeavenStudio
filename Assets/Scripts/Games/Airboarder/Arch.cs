using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Airboarder
{
    public class Arch : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject archBasic;
        [SerializeField] Animator anim;

        public Airboarder game;
        
        public double targetBeat;
        public double appearBeat;

        public float normalizedStart;

        private bool isCrouch;

        private void Awake()
        {
            game = Airboarder.instance;
        }

        public void CueDuck(double duckBeat)
        {
            game.ScheduleInput(duckBeat, 3f, Minigame.InputAction_BasicPress, DuckSuccess, DuckMiss, null);
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(duckBeat, delegate {
                    game.wantsCrouch = false;
                    game.cpu1CantBop = true;
                    game.CPU1.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                }),
                new BeatAction.Action(duckBeat + 1, delegate {
                    game.cpu2CantBop = true;
                    game.CPU1.DoScaledAnimationAsync("duck", 1f, 0, 1);
                    game.CPU2.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                    SoundByte.PlayOneShotGame("airboarder/crouch");
                    SoundByte.PlayOneShotGame("airboarder/crouchvox");
                }),
                new BeatAction.Action(duckBeat + 2, delegate {
                    game.playerCantBop = true;
                    game.CPU2.DoScaledAnimationAsync("duck", 1f, 0, 1);
                    game.Player.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                    SoundByte.PlayOneShotGame("airboarder/crouch");
                    SoundByte.PlayOneShotGame("airboarder/crouchvox");
                }),
                new BeatAction.Action(duckBeat + 2.5, delegate { game.cpu1CantBop = false; } ),
                new BeatAction.Action(duckBeat + 3.5, delegate { game.cpu2CantBop = false; } ),
                new BeatAction.Action(duckBeat + 4.5, delegate { game.playerCantBop = false; })
            });
        }

        public void CueCrouch(double crouchBeat)
        {
            game.ScheduleInput(crouchBeat, 3f, Minigame.InputAction_BasicPress, CrouchSuccess, CrouchMiss, null);
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(crouchBeat, delegate {
                    game.wantsCrouch = true;
                    game.cpu1CantBop = true;
                    game.CPU1.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                }),
                new BeatAction.Action(crouchBeat+1, delegate {
                    game.cpu2CantBop = true;
                    SoundByte.PlayOneShotGame("airboarder/crouch");
                    SoundByte.PlayOneShotGame("airboarder/crouchCharge");
                    SoundByte.PlayOneShotGame("airboarder/crouchvox");
                    game.CPU1.DoScaledAnimationAsync("charge", 1f, 0, 1);
                    game.CPU2.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                }),
                new BeatAction.Action(crouchBeat + 2, delegate {
                    SoundByte.PlayOneShotGame("airboarder/crouch");
                    game.playerCantBop = true;
                    game.CPU2.DoScaledAnimationAsync("charge", 1f, 0, 1);
                    game.Player.DoScaledAnimationAsync("letsgo", 1f, 0, 1);
                    SoundByte.PlayOneShotGame("airboarder/crouchCharge");
                    SoundByte.PlayOneShotGame("airboarder/crouchvox");
                })
            });
        }

        void Update()
        {
            float normalizedStart = game.conductor.GetPositionFromBeat(appearBeat, 40f);
            anim.DoNormalizedAnimation("move", normalizedStart, animLayer: 0);
            if (normalizedStart > 1) Destroy(gameObject);
        }

        public void DuckSuccess(PlayerActionEvent caller, float state)
        {   
            game.Player.DoScaledAnimationAsync("duck", 1f, 0, animLayer:1);
            double beat = caller.startBeat + caller.timer;
            BeatAction.New(this, new() {
                new(beat,        () => game.playerCantBop = true),
                new(beat + 1.5f, () => game.playerCantBop = false)
            });

            if (state is >= 1 or <= -1) 
            {
                anim.DoScaledAnimationAsync("shake", 1f, 0f, animLayer: 1);
                SoundByte.PlayOneShotGame("airboarder/barely");
                SoundByte.PlayOneShotGame("airboarder/barelyvox");
            }
            else 
            {
                SoundByte.PlayOneShotGame("airboarder/crouch");
                SoundByte.PlayOneShotGame("airboarder/crouchvox");
            }
        }

        public void DuckMiss(PlayerActionEvent caller)
        {
            game.Player.DoScaledAnimationAsync("hit1",1.5f, 0, 1);
            anim.DoScaledAnimationAsync("break", 1f, 0, animLayer:1);
            double beat = caller.startBeat + caller.timer;
            game.MissSound(beat);
            BeatAction.New(this, new() {
                new(beat,        () => game.playerCantBop = true),
                new(beat + 1.5f, () => game.playerCantBop = false)
            });
        }

        public void CrouchSuccess(PlayerActionEvent caller, float state)
        {
            game.Player.DoScaledAnimationAsync("charge", 1f, 0, 1);
            if (state is >= 1 or <= -1) 
            {
                anim.DoScaledAnimationAsync("shake", 1f, 0, animLayer:1);
                SoundByte.PlayOneShotGame("airboarder/barely");
                SoundByte.PlayOneShotGame("airboarder/barelyvox");
            }
            else
            {
                SoundByte.PlayOneShotGame("airboarder/crouch");
                SoundByte.PlayOneShotGame("airboarder/crouchCharge");
                SoundByte.PlayOneShotGame("airboarder/crouchvox");
            }
            game.playerCantBop = true;
        }

        public void CrouchMiss(PlayerActionEvent caller) {
            game.Player.DoScaledAnimationAsync("hit1", 1.5f, 0, 1);
            anim.DoScaledAnimationAsync("break", 1f, 0, animLayer: 1);
            double beat = caller.startBeat + caller.timer;
            game.MissSound(beat);
            BeatAction.New(this, new() {
                new(beat, ()=>game.playerCantBop = true),
                new(beat+1.5f, ()=>game.playerCantBop = false)
            });
        }
    }
}

