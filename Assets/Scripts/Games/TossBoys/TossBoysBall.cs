using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using UnityEditorInternal;

namespace HeavenStudio.Games.Scripts_TossBoys
{
    public class TossBoysBall : SuperCurveObject
    {
        public enum State
        {
            None,
            RedDispense,
            BlueDispense,
            YellowDispense,
            RedBlue,
            RedYellow,
            BlueRed,
            BlueYellow,
            YellowRed,
            YellowBlue,
            RedBlueDual,
            RedYellowDual,
            BlueRedDual,
            BlueYellowDual,
            YellowRedDual,
            YellowBlueDual,
            RedBlueHigh,
            RedYellowHigh,
            BlueRedHigh,
            BlueYellowHigh,
            YellowRedHigh,
            YellowBlueHigh,
            RedBlur,
            RedKeep,
            BlueBlur,
            BlueKeep,
            YellowBlur,
            YellowKeep
        }

        private State currentState;

        private float startBeat;

        private Path currentPath;

        private TossBoys game;

        private void Awake()
        {
            game = TossBoys.instance;
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                switch (currentState)
                {
                    case State.None:
                        //Do Jackshit
                        break;
                    default:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, Conductor.instance.songPositionInBeats), startBeat);
                        break;
                }
            }
        }

        public void SetState(State state, float beat)
        {
            startBeat = beat;
            currentState = state;
            currentPath = currentState switch
            {
                State.None => new Path(),
                State.RedDispense => game.GetPath("RedDispense"),
                State.BlueDispense => game.GetPath("BlueDispense"),
                State.YellowDispense => game.GetPath("YellowDispense"),
                State.RedBlue => game.GetPath("RedBlue"),
                State.RedYellow => game.GetPath("RedYellow"),
                State.BlueRed => game.GetPath("BlueRed"),
                State.BlueYellow => game.GetPath("BlueYellow"),
                State.YellowRed => game.GetPath("YellowRed"),
                State.YellowBlue => game.GetPath("YellowBlue"),
                State.RedBlueDual => game.GetPath("RedBlueDual"),
                State.RedYellowDual => game.GetPath("RedYellowDual"),
                State.BlueRedDual => game.GetPath("BlueRedDual"),
                State.BlueYellowDual => game.GetPath("BlueYellowDual"),
                State.YellowRedDual => game.GetPath("YellowRedDual"),
                State.YellowBlueDual => game.GetPath("YellowBlueDual"),
                State.RedBlueHigh => game.GetPath("RedBlueHigh"),
                State.RedYellowHigh => game.GetPath("RedYellowHigh"),
                State.BlueRedHigh => game.GetPath("BlueRedHigh"),
                State.BlueYellowHigh => game.GetPath("BlueYellowHigh"),
                State.YellowRedHigh => game.GetPath("YellowRedHigh"),
                State.YellowBlueHigh => game.GetPath("YellowBlueHigh"),
                State.RedBlur => game.GetPath("RedBlur"),
                State.RedKeep => game.GetPath("RedKeep"),
                State.BlueBlur => game.GetPath("BlueBlur"),
                State.BlueKeep => game.GetPath("BlueKeep"),
                State.YellowBlur => game.GetPath("YellowBlur"),
                State.YellowKeep => game.GetPath("YellowKeep"),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
