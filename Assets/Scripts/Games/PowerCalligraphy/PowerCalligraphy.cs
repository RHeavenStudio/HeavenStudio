using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbPowerCalligraphy
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("powerCalligraphy", "Power Calligraphy", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("write", "Write")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, e["type"], e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, e["type"]); },
                    parameters = new List<Param>() 
                    {
                        new Param("type", PowerCalligraphy.LetterType.re, "Type", "Choose the letter to write."),
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
            },
            new List<string>() { "agb", "normal" }, "agbCalligraphy", "en", new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_PowerCalligraphy;
    public class PowerCalligraphy : Minigame
    {
        [Header("Prefabs")]
        [SerializeField] List<GameObject> basePapers = new List<GameObject>();

        [Header("Components")]
        public Transform paperHolder;
        public Animator fudePosAnim;

        public static Nullable<QueuedPaper> queuedPaper = null;

        public enum LetterType
        {
            re,
            ten,
            chikara,
            onore,
            sun,
            kokoro,
            tsurunihamushi,
            tsurunihamushi_korean,
        }
        public struct QueuedPaper
        {
            public double beat;
            public int type;
        }


        public static PowerCalligraphy instance = null;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        Writing nowPaper;
        bool isPrepare = false;
        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (queuedPaper is not null)
            {
                Prepare(queuedPaper.Value.beat, queuedPaper.Value.type);
                queuedPaper = null;
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (nowPaper.onGoing)
                {
                    nowPaper.Fast();
                    ScoreMiss();
                }
            }
        }

        private void SpawnPaper(double beat, int type)
        {
            nowPaper = Instantiate(basePapers[type], paperHolder).GetComponent<Writing>();
            nowPaper.targetBeat = beat;
            nowPaper.type = type;
            nowPaper.Init();

            nowPaper.gameObject.SetActive(true);

        }

        public void Write(double beat, int type)
        {
            if (!isPrepare) Prepare(beat, type);
            nowPaper.Play();
            isPrepare=false;
        }

        public void QueuePaper(double beat, int type, bool prepare)
        {
            if (GameManager.instance.currentGame != "powerCalligraphy" || prepare)
            {
                queuedPaper = new QueuedPaper()
                {
                    beat = beat,
                    type = type,
                };
            }
            else if(Conductor.instance.songPositionInBeats < beat)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat-1, delegate{ Prepare(beat, type);})
                });
            }
        }
        public void Prepare(double beat, int type)
        {
            SpawnPaper(beat, type);
            fudePosAnim.Play(type switch {
                (int)LetterType.re => "fudePos-re00",
                (int)LetterType.ten => "fudePos-ten00",
                (int)LetterType.chikara => "fudePos-chikara00",
                (int)LetterType.onore => "fudePos-onore00",
                (int)LetterType.sun => "fudePos-sun00",
                (int)LetterType.kokoro => "fudePos-kokoro00",
                (int)LetterType.tsurunihamushi => "fudePos-tsurunihamushi00",
                (int)LetterType.tsurunihamushi_korean => "fudePos-tsurunihamushi_kr00",
            });
            isPrepare = true;
        }

        private void Success(PlayerActionEvent caller, float state)
        {
        }

        private void Miss(PlayerActionEvent caller)
        {
        }

        private void Empty(PlayerActionEvent caller) { }

        bool CanSuccess()
        {
            return false;
        }
    }
}