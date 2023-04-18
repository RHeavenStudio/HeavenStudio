using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class SoccerBall : FollowPath
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
                float rot = GetPathValue("rot");
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z - (rot * Time.deltaTime * (1f/conductor.pitchedSecPerBeat)));
            }
        }

        public void Init(float beat)
        {
            game.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Just, Miss, Empty);
            path = game.GetPath("SoccerIn");
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
                path = game.GetPath("SoccerNg" + (state > 0 ? "Late" : "Early"));
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
            path = game.GetPath("SoccerJust");
            game.Kick();
            Jukebox.PlayOneShotGame("doubleDate/kick");
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("doubleDate/weasel_hide");

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeats + 4f, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        void Empty(PlayerActionEvent caller) { }
    }
}
