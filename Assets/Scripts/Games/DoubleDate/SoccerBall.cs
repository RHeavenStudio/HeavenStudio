using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class SoccerBall : SuperCurveObject
    {
        private DoubleDate game;
        private SuperCurveObject.Path path;
        private float pathStartBeat = float.MinValue;
        private Conductor conductor;
        private GameObject shadow;

        void Awake()
        {
            game = DoubleDate.instance;
            conductor = Conductor.instance;
        }

        void Update()
        {
            float beat = conductor.songPositionInBeats;
            float height = 0f;
            if (pathStartBeat > float.MinValue)
            {
                Vector3 pos = GetPathPositionFromBeat(path, Mathf.Max(beat, pathStartBeat), out height, pathStartBeat);
                transform.position = pos;
                float rot = GetPathValue("rot");
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z - (rot * Time.deltaTime * (1f/conductor.pitchedSecPerBeat)));
            }
            shadow.transform.position = new Vector3(transform.position.x, Mathf.Min(transform.position.y - height, game.floorHeight), transform.position.z);
            shadow.transform.localScale = Vector3.one * Mathf.Clamp(((transform.position.y) - game.shadowDepthScaleMin) / (game.shadowDepthScaleMax - game.shadowDepthScaleMin), 0f, 1f);
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
            shadow = game.MakeDropShadow();
            shadow.transform.position = new Vector3(transform.position.x, Mathf.Min(game.floorHeight, transform.position.y - offset.y), transform.position.z);
            shadow.SetActive(true);
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
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeats;
            if (state >= 1f || state <= -1f)
            {
                path = game.GetPath("SoccerNg" + (state > 0 ? "Late" : "Early"));
                Jukebox.PlayOneShot("miss");
                game.Kick(false);
                GetComponent<SpriteRenderer>().sortingOrder = 8;
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
            game.MissKick(pathStartBeat + 2.25f);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeats + 4f, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        void Empty(PlayerActionEvent caller) { }

        private void OnDestroy() {
            if (shadow != null)
            {
                Destroy(shadow);
            }
        }
    }
}
