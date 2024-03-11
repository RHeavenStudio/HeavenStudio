using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_PowerCalligraphy
{
    public class Writing : MonoBehaviour
    {
        [Serializable]
        public struct PatternItem
        {
            public double beat;
            public SoundType soundType;
            public float soundVolume;
            public StrokeType stroke;
            public FudeType fudeAnim;
        }
        
        public enum SoundType {
            None = 0,
            brushTap,
            brush1,
            brush2,
            brush3,
            reShout,
            comma1,
            comma2,
            comma3,
        }

        public enum StrokeType {
            None = 0,
            TOME = 1,
            HANE,
            HARAI,
        }
        
        public enum FudeType {
            None = 0,
            Release,
            Tap,
            Prepare,
        }

        public double startBeat;
        public double nextBeat;
        [SerializeField] PatternItem[] AnimPattern;
        
        private Animator paperAnim;
        private SortingGroup paperSort;

        public Vector3 scrollSpeed;
        Vector3 scrollRate => scrollSpeed / (Conductor.instance.pitchedSecPerBeat * 2f);
        
        public bool onGoing = false;
        bool isFinish = false;
        int process_num;
        StrokeType stroke;
        public int Stroke { get { return (int)stroke; }}

        private PowerCalligraphy game;

        public void Init()
        {
            game = PowerCalligraphy.instance;
            paperAnim = GetComponent<Animator>();
            paperSort = GetComponent<SortingGroup>();
            nextBeat = AnimPattern[^1].beat;
        }

        public void Play()
        {
            paperSort.sortingOrder++;

            var sounds = new List<MultiSound.Sound>();
            var actions = new List<BeatAction.Action>();
            
            int anim_num = 0;
            foreach (var item in AnimPattern)
            {
                double itemBeat = startBeat + item.beat;
                string sound = item.soundType switch {
                    SoundType.brushTap => "powerCalligraphy/brushTap",
                    SoundType.brush1 => "powerCalligraphy/brush1",
                    SoundType.brush2 => "powerCalligraphy/brush2",
                    SoundType.brush3 => "powerCalligraphy/brush3",
                    SoundType.reShout => "powerCalligraphy/reShout",
                    SoundType.comma1 => "powerCalligraphy/comma1",
                    SoundType.comma2 => "powerCalligraphy/comma2",
                    SoundType.comma3 => "powerCalligraphy/comma3",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sound)) sounds.Add(new MultiSound.Sound(sound, itemBeat, volume:item.soundVolume));
                
                int current_anim_num;
                switch (item.fudeAnim)
                {
                    case FudeType.Release:
                        anim_num++;
                        current_anim_num = anim_num;
                        actions.Add(new BeatAction.Action(itemBeat, delegate { Anim(current_anim_num); game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);}));
                        break;
                    case FudeType.Tap:
                        anim_num++;
                        current_anim_num = anim_num;
                        actions.Add(new BeatAction.Action(itemBeat, delegate { Anim(current_anim_num); game.fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);}));
                        break;
                    case FudeType.Prepare:
                        actions.Add(new BeatAction.Action(itemBeat, delegate { game.fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}));
                        break;
                    default:
                        break;
                }

                int current_anim_num_1;
                switch(item.stroke)
                {
                    case StrokeType.TOME:
                        anim_num++;
                        current_anim_num_1 = anim_num;
                        actions.Add(new BeatAction.Action(itemBeat, delegate {
                            Halt(); stroke = StrokeType.TOME; process_num = current_anim_num_1;}));
                        actions.Add(new BeatAction.Action(itemBeat, delegate { onGoing = true;}));
                        game.ScheduleInput(itemBeat, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                        break;
                    case StrokeType.HANE:
                        anim_num++;
                        current_anim_num_1 = anim_num;
                        actions.Add(new BeatAction.Action(itemBeat, delegate {
                            Sweep(); stroke = StrokeType.HANE; process_num = current_anim_num_1;}));
                        actions.Add(new BeatAction.Action(itemBeat+1, delegate { onGoing = true;}));
                        game.ScheduleInput(itemBeat, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                        break;
                    case StrokeType.HARAI:
                        anim_num++;
                        current_anim_num_1 = anim_num;
                        actions.Add(new BeatAction.Action(itemBeat, delegate {
                            Sweep(); stroke = StrokeType.HARAI; process_num = current_anim_num_1;}));
                        actions.Add(new BeatAction.Action(itemBeat+1, delegate { onGoing = true;}));
                        game.ScheduleInput(itemBeat, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                        break;
                    default:
                        break;
                }
            }
            actions.Add(new BeatAction.Action(startBeat + nextBeat, delegate { Finish();}));

            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray());
            if (actions.Count > 0) BeatAction.New(game, actions);
        }

        // TOME
        private void Halt()
        {
            game.fudeAnim.Play("fude-halt");
            SoundByte.PlayOneShotGame("powerCalligraphy/releaseB1");
        }
        // HANE HARAI
        private void Sweep()
        {
            game.fudeAnim.Play("fude-sweep");
            SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
        }
        private void Finish()
        {
            isFinish = true;
            game.fudeAnim.Play("fude-none");
            paperAnim.enabled = false;
        }

        private void writeSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f)
                ProcessInput("late");
            else if (state <= -1f)
                ProcessInput("fast");
            else
                ProcessInput("just");
        }

        private void writeMiss(PlayerActionEvent caller)
        {
            if (onGoing)
                Miss();
        }

        private void Empty(PlayerActionEvent caller) { }

        bool CanSuccess()
        {
            return onGoing;
        }

        public void ProcessInput(string input)
        {
            onGoing = false;
            Anim(process_num, input);

            switch (input)
            {
                case "just":
                    switch (stroke) {
                        case StrokeType.TOME:
                            game.fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            SoundByte.PlayOneShotGame("powerCalligraphy/releaseB2");
                            break;
                            
                        case StrokeType.HANE:
                        case StrokeType.HARAI:
                            game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                            break;
                    }
                    break;
                    
                case "late":
                case "fast":
                    switch (stroke) {   // WIP
                        case StrokeType.TOME:
                            game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            SoundByte.PlayOneShotGame("powerCalligraphy/8");
                            break;
                        case StrokeType.HANE:
                            game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            SoundByte.PlayOneShotGame("powerCalligraphy/6");    
                            break;
                        case StrokeType.HARAI:
                            game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            SoundByte.PlayOneShotGame("powerCalligraphy/9");
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        
        public void Miss()
        {
            onGoing = false;
            SoundByte.PlayOneShotGame("powerCalligraphy/7");    // WIP
            Anim(process_num, "miss");

            switch (stroke) {
                case StrokeType.TOME:
                    game.fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    break;
                    
                case StrokeType.HANE:
                case StrokeType.HARAI:
                    game.fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;
            }
        }

        private void Anim(int num, string str = "")
        {
            string pattern = num.ToString() + str;

            game.fudePosAnim.DoScaledAnimationAsync(pattern, 0.5f);
            paperAnim.DoScaledAnimationAsync(pattern, 0.5f);  
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (isFinish)
                {
                    double beat = cond.songPositionInBeats;
                    // Paper scroll.
                    var paperPos = transform.localPosition;
                    transform.localPosition = paperPos + (scrollRate * Time.deltaTime);
                    if (beat >= startBeat + 24) Destroy(gameObject);
                }
            }
        }
    }
}