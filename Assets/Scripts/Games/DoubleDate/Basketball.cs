using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class Basketball : FollowPath
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
            game.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Just, Miss, Empty);
            path = game.GetPath("BasketBallIn");
            UpdateLastRealPos();
            pathStartBeat = beat - 1f;

            Vector3 pos = GetPathPositionFromBeat(path, pathStartBeat, pathStartBeat);
            transform.position = pos;

            gameObject.SetActive(true);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeats + 3f, delegate
                {
                    Destroy(gameObject);
                }),
            });
            if (state >= 1f || state <= -1f)
            {
                UpdateLastRealPos();
                pathStartBeat = conductor.songPositionInBeats;
                path = game.GetPath("BasketBallNg" + (state > 0 ? "Late" : "Early"));
                Jukebox.PlayOneShot("miss");
                game.Kick(false);
                GetComponent<SpriteRenderer>().sortingOrder = 5;
                return;
            }
            Hit();
        }

        void Hit()
        {
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeats;
            path = game.GetPath("BasketBallJust");
            game.Kick();
            Jukebox.PlayOneShotGame("doubleDate/kick");
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("doubleDate/weasel_hide");
        }

        void Empty(PlayerActionEvent caller) { }
    }
}
