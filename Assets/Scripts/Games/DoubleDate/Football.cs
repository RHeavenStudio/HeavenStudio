using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class Football : SuperCurveObject
    {
        private DoubleDate game;
        
        private SuperCurveObject.Path path;
        private double pathStartBeat = double.MinValue;
        private Conductor conductor;
        private GameObject shadow;

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

        public void Init(double beat)
        {
            game.ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, Just, Miss, Empty);
            path = game.GetPath("FootBallInNoHit");  // there's a second path for footballs that hit the weasels, use that if the weasels haven't been hit recently
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
            if (state >= 1f || state <= -1f)
            {
                UpdateLastRealPos();
                pathStartBeat = conductor.songPositionInBeatsAsDouble;
                path = game.GetPath("FootBallNg" + (state > 0 ? "Late" : "Early"));
                SoundByte.PlayOneShot("miss");
                game.Kick(false);
                GetComponent<SpriteRenderer>().sortingOrder = 8;
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 4f, delegate
                    {
                        Destroy(gameObject);
                    }),
                });
                return;
            }
            Hit();
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 1f, delegate
                {
                    shadow.SetActive(false);
                    GetComponent<SpriteRenderer>().sortingOrder = -5;
                    transform.localScale *= 0.25f;
                    path = game.GetPath("FootBallFall");
                    UpdateLastRealPos();
                    pathStartBeat = conductor.songPositionInBeatsAsDouble + 1f;
                }),
                new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 12f, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        void Hit()
        {
            UpdateLastRealPos();
            pathStartBeat = conductor.songPositionInBeatsAsDouble;
            path = game.GetPath("FootBallJust");
            game.Kick(true, true, jump: true);
            SoundByte.PlayOneShotGame("doubleDate/footballKick");
        }

        void Miss(PlayerActionEvent caller)
        {
            if (conductor.songPositionInBeatsAsDouble > game.lastHitWeasel + 2.25f)
            {
                path = game.GetPath("FootBallIn");
                float impact = GetPointTimeByTag(path, "impact");
                if (impact > 0)
                {
                    GetComponent<SpriteRenderer>().sortingOrder = 8;
                    SoundByte.PlayOneShotGame("doubleDate/weasel_hit", pathStartBeat + impact);
                    SoundByte.PlayOneShotGame("doubleDate/weasel_scream", pathStartBeat + impact);
                    game.MissKick(pathStartBeat + impact, true);
                }
            }

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(conductor.songPositionInBeatsAsDouble + 5f, delegate
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
