using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class SoccerBall : SuperCurveObject
    {
        private DoubleDate game;
        private SuperCurveObject.Path path;
        private double pathStartBeat = double.MinValue;
        private Conductor conductor;
        private GameObject shadow;
        private bool _jump;

        void Awake()
        {
            game = DoubleDate.instance;
            conductor = Conductor.instance;
        }

        void Update()
        {
            double beat = conductor.songPositionInBeatsAsDouble;
            double height = 0f;
            if (pathStartBeat > double.MinValue)
            {
                Vector3 pos = GetPathPositionFromBeat(path, Math.Max(beat, pathStartBeat), out height, pathStartBeat);
                transform.position = pos;
                float rot = GetPathValue("rot");
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z - (rot * Time.deltaTime * (1f/conductor.pitchedSecPerBeat)));
            }
            shadow.transform.position = new Vector3(transform.position.x, (float) Math.Min(transform.position.y - height, game.floorHeight), transform.position.z);
            shadow.transform.localScale = Vector3.one * Mathf.Clamp(((transform.position.y) - game.shadowDepthScaleMin) / (game.shadowDepthScaleMax - game.shadowDepthScaleMin), 0f, 1f);
        }

        public void Init(double beat, bool shouldJump)
        {
            _jump = shouldJump;
            game.ScheduleInput(beat, 1f, DoubleDate.InputAction_FlickPress, Just, Miss, Empty);
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
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 3f, delegate
                {
                    Destroy(gameObject);
                }),
            });
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeatsAsDouble;
            if (state >= 1f || state <= -1f)
            {
                path = game.GetPath("SoccerNg" + (state > 0 ? "Late" : "Early"));
                SoundByte.PlayOneShot("miss");
                game.Kick(false);
                GetComponent<SpriteRenderer>().sortingOrder = 8;
                return;
            }
            Hit();
        }

        void Hit()
        {
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeatsAsDouble;
            path = game.GetPath("SoccerJust");
            game.Kick(true, false, true, _jump);
            SoundByte.PlayOneShotGame("doubleDate/kick");
        }

        void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("doubleDate/weasel_hide");
            game.MissKick(pathStartBeat + 2.25f);

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 4f, delegate
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
