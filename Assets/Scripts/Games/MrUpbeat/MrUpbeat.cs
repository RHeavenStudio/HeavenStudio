using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbUpbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("mrUpbeat", "Mr. Upbeat", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { MrUpbeat.instance.SetInterval(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f, 
                    resizable = true, 
                    inactiveFunction = delegate { MrUpbeat.Beep(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }
                },
                new GameAction("go", "Start Stepping")
                {
                    function = delegate { MrUpbeat.instance.Go(eventCaller.currentEntity.beat);  }, 
                    defaultLength = 4f, 
                    resizable = true
                },
                new GameAction("ding!", "Finish Stepping")
                {
                    function = delegate { MrUpbeat.instance.Ding(eventCaller.currentEntity["toggle"]); },
                    defaultLength = 0.5f, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Applause")
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MrUpbeat;

    public class MrUpbeat : Minigame
    {
        [Header("References")]
        public GameObject metronome;
        public UpbeatMan man;
        public GameObject bt;
        private static MultiSound beeps; //only used when this game isn't active.

        public GameEvent beat = new GameEvent();
        public bool canGo = false;
        public int beatCount = 0;

        public float beatOffset = 0f;

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;
            canGo = false;
            man.stepTimes = 0;
            SetInterval(0);
            var pos = Conductor.instance.songPositionInBeats;
            StartCoroutine(Upbeat(pos - Mathf.Round(pos)));

            List<float> gos = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "mrUpbeat/go").Select(c => c.beat).ToList();

            if (gos.Count > 0)
            {
                var nextInterval = gos.IndexOf(Mathp.GetClosestInList(gos, Conductor.instance.songPositionInBeats));
                beatOffset = gos[nextInterval];
            }
        }

        private void Update()
        {
            List<DynamicBeatmap.DynamicEntity> gos = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "mrUpbeat/go");
            for (int i = 0; i < gos.Count; i++)
            {
                if ((gos[i].beat - 0.15f) <= Conductor.instance.songPositionInBeats && (gos[i].beat + gos[i].length) - 0.15f > Conductor.instance.songPositionInBeats)
                {
                    canGo = true;
                    break;
                } else
                {
                    canGo = false;
                }
            }

            if (canGo)
            {
                var songPos = Conductor.instance.songPositionInBeats - beatOffset;
                metronome.transform.eulerAngles = new Vector3(0, 0, 270 - Mathf.Cos(Mathf.PI * songPos) * 60);
            }

            if (Conductor.instance.ReportBeat(ref beat.lastReportedBeat))
            {
                StartCoroutine(Upbeat());
                if (canGo)
                {
                    if (beatCount % 2 == 0)
                        Jukebox.PlayOneShotGame("mrUpbeat/metronomeRight");
                    else
                        Jukebox.PlayOneShotGame("mrUpbeat/metronomeLeft");

                    Beat(Mathf.Round(Conductor.instance.songPositionInBeats));
                }
            }
        }

        public override void OnGameSwitch(float beat)
        {
            foreach (var entity in GameManager.instance.Beatmap.entities)
            {
                if (entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if (entity.datamodel != "mrUpbeat/prepare" || entity.beat + entity.length < beat) //check for prepares that happen before the switch
                {
                    continue;
                }
                SetInterval(entity.beat);
                break;
            }
            if(beeps != null)
            {
                beeps.Delete(); //the beeps are only for when the game isn't active
                beeps = null;
            }
        }

        public void SetInterval(float beat)
        {
            beatCount = 0;
            man.targetBeat = beat + 320f;
            man.Idle();
        }

        public void Go(float beat)
        {
            beatCount = 0;
        }

        public void Ding(bool applause)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/ding");
            if (applause)
                Jukebox.PlayOneShot("applause");
        }

        public void Beat(float beat)
        {
            beatCount++;

            GameObject _beat = Instantiate(bt);
            _beat.transform.parent = bt.transform.parent;
            _beat.SetActive(true);
            UpbeatStep s = _beat.GetComponent<UpbeatStep>();
            s.startBeat = beat;
            s.beatOffset = beatOffset;
        }

        private IEnumerator Upbeat(float offset = 0)
        {
            yield return new WaitForSeconds(Conductor.instance.secPerBeat * 0.5f - offset);
            man.Blip();
        }

        public static void Beep(float beat, float length)
        {
            if(GameManager.instance.currentGame == "mrUpbeat") //this function is only meant for making beeps while the game is inactive
            {
                return;
            }
            if (beeps != null)
            {
                beeps.Delete();
            }
            MultiSound.Sound[] beepSounds = new MultiSound.Sound[Mathf.CeilToInt(length)];
            for(int i = 0; i < beepSounds.Length; i++)
            {
                beepSounds[i] = new MultiSound.Sound("mrUpbeat/blip", beat + 0.5f + i);
            }
            beeps = MultiSound.Play(beepSounds, forcePlay:true);
        }
    }
}