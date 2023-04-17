using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class Football : FollowPath
    {
        private DoubleDate game;
        
        private FollowPath.Path path;
        private float pathStartBeat = float.MinValue;
        private Conductor conductor;

        void Awake()
        {
            game = DoubleDate.instance;
            conductor = Conductor.instance;
        }

        void Update()
        {
            float beat = conductor.songPositionInBeats;
            if (pathStartBeat > float.MinValue)
            {
                Vector3 pos = GetPathPositionFromBeat(path, Mathf.Max(beat, pathStartBeat), pathStartBeat);
                transform.position = pos;
            }
        }

        public void Init(float beat)
        {
            game.ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, Just, Miss, Empty);
            path = game.GetPath("FootBallIn");
            UpdateLastRealPos();
            pathStartBeat = beat - 1f;

            Vector3 pos = GetPathPositionFromBeat(path, pathStartBeat, pathStartBeat);
            transform.position = pos;

            gameObject.SetActive(true);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                UpdateLastRealPos();
                pathStartBeat = conductor.songPositionInBeats;
                path = game.GetPath("FootBallNg" + (state > 0 ? "Late" : "Early"));
                Jukebox.PlayOneShot("miss");
                game.Kick(false);
                GetComponent<SpriteRenderer>().sortingOrder = 5;
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(conductor.songPositionInBeats + 4f, delegate
                    {
                        Destroy(gameObject);
                    }),
                });
                return;
            }
            Hit();
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeats + 2f, delegate
                {
                    GetComponent<SpriteRenderer>().sortingOrder = -5;
                    transform.localScale *= 0.25f;
                    path = game.GetPath("FootBallFall");
                    UpdateLastRealPos();
                    pathStartBeat = conductor.songPositionInBeats + 2f;
                }),
                new BeatAction.Action(conductor.songPositionInBeats + 12f, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        void Hit()
        {
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeats;
            path = game.GetPath("FootBallJust");
            game.Kick(true, true);
            Jukebox.PlayOneShotGame("doubleDate/footballKick");
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("doubleDate/weasel_hit");
            Jukebox.PlayOneShotGame("doubleDate/weasel_scream");

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeats + 5f, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        void Empty(PlayerActionEvent caller) { }
    }
}
