using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;

namespace HeavenStudio.Games
{
        public class RocketScript : PlayerActionObject
    {
        public string awakeAnim;
        public string awakeAnim2;
        public string awakeAnim3;
        public string awakeAnim4;
        public string awakeAnim5;
        public string awakeAnim6;
        public string awakeAnim7;
        public float startBeat;
        public RocketType type;
        public enum RocketType
        {
            Family,
            PartyCracker,
            Bell,
            Bowling
        }

        
        
        // Start is called before the first frame update
        void Awake()
        {
            PlayerActionEvent onHit;
            switch(type)
            {
                case RocketType.Family:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 3f, InputType.STANDARD_DOWN, RocketFourSuccess, RocketFourMiss, RocketFourBlank);
                    break;
                case RocketType.PartyCracker:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketFiveSuccess, RocketFiveMiss, RocketFiveBlank);
                    break;
                case RocketType.Bell:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketSevenSuccess, RocketSevenMiss, RocketSevenBlank);
                    break;
                case RocketType.Bowling:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketOneSuccess, RocketOneMiss, RocketOneBlank);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
            float pitch1 = 1f;
            float pitch2 = 1f;
            float pitch3 = 1f;
            float pitch4 = 1f;
            float pitch5 = 1f;
            float pitch6 = 1f;
            float pitch7 = 1f;

            float[] possibleNotes = new float[] {0.7491535384383408f, 0.7937005259840998f, 0.8408964152537145f, 0.8908987181403393f, 
            0.9438743126816935f, 1, 1.0594630943592953f, 1.122462048309373f, 1.189207115002721f, 1.2599210498948732f, 
            1.3348398541700344f, 1.4142135623730951f, 1.4983070768766815f, 1.5874010519681994f, 1.681792830507429f, 
            1.7817974362806785f, 1.8877486253633868f, 2, 2.1189261887185906f, 2.244924096618746f, 
            2.378414230005442f, 2.5198420997897464f, 2.6696797083400687f, 2.8284271247461903f, 2.996614153753363f, 0.7071067811865476f};
            // all values in possibleNotes are pitch multipliers needed for notes E4 to E#6
            // for finding pitches, all indexes are the pitches' semitones - 4
            // notes increase chromatically
        void Sounds(float beat, int type, int type2, int type3)
        {
            
            switch (type)
            {
                case 0:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/VT_CL",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 1f, pitch2),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 2f, pitch3),
                    }, forcePlay: true );


                    BeatAction.New(LaunchParty.instance.Rocket, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { LaunchParty.instance.Rockets.Play(awakeAnim, 0, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { LaunchParty.instance.Rockets.Play(awakeAnim2, 0, 0);}),
                        new BeatAction.Action(beat + 2f, delegate { LaunchParty.instance.Rockets.Play(awakeAnim3, 0, 0);})
                    }
                    );
                    break;
                case 1:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/VT_CL",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 2/3f, pitch2),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 1f, pitch3),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 4/3f, pitch4),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 5/3f, pitch5),
                    }, forcePlay: true);    


                    BeatAction.New(LaunchParty.instance.PartyCracker, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { LaunchParty.instance.Crackers.Play(awakeAnim, 0, 0); }),
                        new BeatAction.Action(beat + 2/3f, delegate { LaunchParty.instance.Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 1f, delegate { LaunchParty.instance.Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 4/3f, delegate { LaunchParty.instance.Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 5/3f, delegate { LaunchParty.instance.Crackers.Play(awakeAnim, 0, 0);})
                    }
                    );
                    
                    break;
                case 2:
                    var sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/VT_CL",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 1f, pitch2),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 7/6f, pitch3),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 8/6f, pitch4),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 9/6f, pitch5),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 10/6f, pitch6),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 11/6f, pitch7),
                    };


                    BeatAction.New(LaunchParty.instance.Bell, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 7/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 8/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 9/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 10/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 11/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);})
                    }
                    );
                    var SoundSource = MultiSound.Play(sound);
                    break;
                case 3:
                {
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/VT_CL",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_pin_prepare",   beat, 1f, 0.75f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat, pitch2, 0.02f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 1/6f, pitch3, 0.02f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 2/6f, pitch2, 0.06f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 3/6f, pitch3, 0.1f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 4/6f, pitch2, 0.16f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 5/6f, pitch3, 0.22f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 1f, pitch2, 0.3f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 7/6f, pitch3, 0.4f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 8/6f, pitch2, 0.6f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 9/6f, pitch3, 0.75f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 10/6f, pitch2, 0.89f),
                        new MultiSound.Sound("launchParty/VT_CL",   beat + 11/6f, pitch3),
                    });

                    BeatAction.New(LaunchParty.instance.Bowling, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { LaunchParty.instance.Pins.Play(awakeAnim, 0, 0); }),
                    }
                    );
                    
                    
                    break;
            }
            }
            
        }
        public void RocketFourSuccess(PlayerActionEvent caller, float state)
        {   
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                float randomNumber = UnityEngine.Random.Range(0, 10);
                if (randomNumber >= 5)
                    LaunchParty.instance.Rockets.Play("RocketBlank");
                else
                    LaunchParty.instance.Rockets.Play("RocketBlankRight");
            }
            else
            {
                Jukebox.PlayOneShotGame("launchParty/VT_CL", 0f, 1f);
                Jukebox.PlayOneShotGame("launchParty/rocket_family");
                LaunchParty.instance.Rockets.Play("RocketLaunch");
            }
                
            
                
            
        }

        public void RocketFourMiss(PlayerActionEvent caller)
        {
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                LaunchParty.instance.Rockets.Play("RocketMiss");
            
            else
                LaunchParty.instance.Rockets.Play("RocketMissRight");
            Jukebox.PlayOneShotGame("launchParty/miss");
            
        }

        public void RocketFourBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                LaunchParty.instance.Rockets.Play("RocketBlank");
            
            else
                LaunchParty.instance.Rockets.Play("RocketBlankRight");
        }

        public void RocketFiveSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("launchParty/VT_CL", 0f, 1f);
            Jukebox.PlayOneShotGame("launchParty/rocket_crackerblast");
            LaunchParty.instance.Crackers.Play("PopperLaunch");
        }

        public void RocketFiveMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Crackers.Play("PopperMiss");
        }

        public void RocketFiveBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Crackers.Play("PopperMiss");
        }

        public void RocketSevenSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("launchParty/VT_CL", 0f, 1f);
            Jukebox.PlayOneShotGame("launchParty/bell_blast");
            LaunchParty.instance.Bells.Play("BellLaunch");
        }

        public void RocketSevenMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Bells.Play("BellMiss");
        }

        public void RocketSevenBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Bells.Play("BellMiss");
        }

        public void RocketOneSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("launchParty/VT_CL", 0f, 1f);
            Jukebox.PlayOneShotGame("launchParty/VT_CL", 0f, 1f);
            Jukebox.PlayOneShotGame("launchParty/rocket_bowling");
            LaunchParty.instance.Pins.Play("PinLaunch");
        }

        public void RocketOneMiss(PlayerActionEvent caller)
        {
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                LaunchParty.instance.Rockets.Play("PinMiss");
            
            else
                LaunchParty.instance.Rockets.Play("PinMissRight");
            Jukebox.PlayOneShotGame("launchParty/miss");
        }

        public void RocketOneBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Pins.Play("PinMiss");
        }
        
    }
} 
