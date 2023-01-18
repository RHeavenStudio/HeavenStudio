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

        public enum ScaleNote
        {
            A,
            ASharp,
            B,
            C,
            CSharp,
            D,
            DSharp,
            E,
            F,
            FSharp,
            G,
            GSharp
        }

        public enum ScaleType
        {
            Major,
            Minor,
            Dorian,
            Mixolydian,
            Lydian,

        }

        

        [Header("Animators")]
        public Animator Rockets;
        public Animator Crackers;
        public Animator Bells;
        public Animator Pins;
        
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
                    switch (type2)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[7 + type2];
                                    pitch2 = possibleNotes[9 + type2];
                                    pitch3 = possibleNotes[10 + type2];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[7 + type2];
                                    pitch2 = possibleNotes[8 + type2];
                                    pitch3 = possibleNotes[10 + type2];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[7 + type2];
                                    pitch2 = possibleNotes[9 + type2];
                                    pitch3 = possibleNotes[10 + type2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[7 + type2];
                                    pitch2 = possibleNotes[9 + type2];
                                    pitch3 = possibleNotes[11 + type2];
                                    break;
                                
                            }
                            break;
                        case 7:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[2];
                                    pitch2 = possibleNotes[4];
                                    pitch3 = possibleNotes[5];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[2];
                                    pitch2 = possibleNotes[3];
                                    pitch3 = possibleNotes[5];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[2];
                                    pitch2 = possibleNotes[4];
                                    pitch3 = possibleNotes[5];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[2];
                                    pitch2 = possibleNotes[4];
                                    pitch3 = possibleNotes[6];
                                    break;
                            }
                            break;
                        case 8:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[3 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[6 + (type2 - (type2 - 1))];
                                    break;
                            }
                            break;
                        case 9:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[3 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[6 + (type2 - (type2 - 2))];
                                    break;
                            }
                            break;
                        case 10:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[3 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[6 + (type2 - (type2 - 3))];
                                    break;
                            }
                            break;
                        case 11:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    break;
                                case 1:
                                case 2:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[3 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[6 + (type2 - (type2 - 4))];
                                    break;
                            }
                            break;
                    }
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/rocket_note",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/rocket_note",   beat + 1f, pitch2),
                        new MultiSound.Sound("launchParty/rocket_note",   beat + 2f, pitch3),
                    }, forcePlay: true );


                    BeatAction.New(LaunchParty.instance.Rocket, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Rockets.Play(awakeAnim, 0, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { Rockets.Play(awakeAnim2, 0, 0);}),
                        new BeatAction.Action(beat + 2f, delegate { Rockets.Play(awakeAnim3, 0, 0);})
                    }
                    );
                    break;
                case 1:
                    switch (type2)
                    {
                        case 0:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[9];
                                    pitch2 = possibleNotes[10];
                                    pitch3 = possibleNotes[12];
                                    pitch4 = possibleNotes[14];
                                    pitch5 = possibleNotes[16];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[8];
                                    pitch2 = possibleNotes[10];
                                    pitch3 = possibleNotes[12];
                                    pitch4 = possibleNotes[13];
                                    pitch5 = possibleNotes[15];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[8];
                                    pitch2 = possibleNotes[10];
                                    pitch3 = possibleNotes[12];
                                    pitch4 = possibleNotes[14];
                                    pitch5 = possibleNotes[15];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[9];
                                    pitch2 = possibleNotes[10];
                                    pitch3 = possibleNotes[12];
                                    pitch4 = possibleNotes[14];
                                    pitch5 = possibleNotes[15];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[9];
                                    pitch2 = possibleNotes[11];
                                    pitch3 = possibleNotes[12];
                                    pitch4 = possibleNotes[14];
                                    pitch5 = possibleNotes[16];
                                    break;
                                }
                        
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[9 + type2];
                                    pitch2 = possibleNotes[10 + type2];
                                    pitch3 = possibleNotes[12 + type2];
                                    pitch4 = possibleNotes[14 + type2];
                                    pitch5 = possibleNotes[16 + type2];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[8 + type2];
                                    pitch2 = possibleNotes[10 + type2];
                                    pitch3 = possibleNotes[12 + type2];
                                    pitch4 = possibleNotes[13 + type2];
                                    pitch5 = possibleNotes[15 + type2];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[8 + type2];
                                    pitch2 = possibleNotes[10 + type2];
                                    pitch3 = possibleNotes[12 + type2];
                                    pitch4 = possibleNotes[14 + type2];
                                    pitch5 = possibleNotes[15 + type2];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[9 + type2];
                                    pitch2 = possibleNotes[10 + type2];
                                    pitch3 = possibleNotes[12 + type2];
                                    pitch4 = possibleNotes[14 + type2];
                                    pitch5 = possibleNotes[15 + type2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[9 + type2];
                                    pitch2 = possibleNotes[11 + type2];
                                    pitch3 = possibleNotes[12 + type2];
                                    pitch4 = possibleNotes[14 + type2];
                                    pitch5 = possibleNotes[16 + type2];
                                    break;
                                }
                        
                            break;
                        case 7:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[4];
                                    pitch2 = possibleNotes[5];
                                    pitch3 = possibleNotes[7];
                                    pitch4 = possibleNotes[9];
                                    pitch5 = possibleNotes[11];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[3];
                                    pitch2 = possibleNotes[5];
                                    pitch3 = possibleNotes[7];
                                    pitch4 = possibleNotes[8];
                                    pitch5 = possibleNotes[10];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[3];
                                    pitch2 = possibleNotes[5];
                                    pitch3 = possibleNotes[7];
                                    pitch4 = possibleNotes[9];
                                    pitch5 = possibleNotes[10];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[4];
                                    pitch2 = possibleNotes[5];
                                    pitch3 = possibleNotes[7];
                                    pitch4 = possibleNotes[9];
                                    pitch5 = possibleNotes[10];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[4];
                                    pitch2 = possibleNotes[6];
                                    pitch3 = possibleNotes[7];
                                    pitch4 = possibleNotes[9];
                                    pitch5 = possibleNotes[11];
                                    break;
                                }
                        
                            break;
                        case 8:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 1))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[8 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[6 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 1))];
                                    break;
                                }
                        
                            break;
                        case 9:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 2))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[8 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[6 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 2))];
                                    break;
                                }
                        
                            break;
                        case 10:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 3))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[8 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[6 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 3))];
                                    break;
                                }
                        
                            break;
                        case 11:
                            switch (type3)
                                {
                                case 0:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 4))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[8 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[3 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[6 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[11 + (type2 - (type2 - 4))];
                                    break;
                                }
                        
                            break;
                    }
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/popper_note",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/popper_note",   beat + 2/3f, pitch2),
                        new MultiSound.Sound("launchParty/popper_note",   beat + 1f, pitch3),
                        new MultiSound.Sound("launchParty/popper_note",   beat + 4/3f, pitch4),
                        new MultiSound.Sound("launchParty/popper_note",   beat + 5/3f, pitch5),
                    }, forcePlay: true);    


                    BeatAction.New(LaunchParty.instance.PartyCracker, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Crackers.Play(awakeAnim, 0, 0); }),
                        new BeatAction.Action(beat + 2/3f, delegate { Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 1f, delegate { Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 4/3f, delegate { Crackers.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 5/3f, delegate { Crackers.Play(awakeAnim, 0, 0);})
                    }
                    );
                    
                    break;
                case 2:
                    switch (type2)
                    {
                        case 0:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[5];
                                    pitch2 = possibleNotes[7];
                                    pitch3 = possibleNotes[9];
                                    pitch4 = possibleNotes[10];
                                    pitch5 = possibleNotes[12];
                                    pitch6 = possibleNotes[14];
                                    pitch7 = possibleNotes[16];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[5];
                                    pitch2 = possibleNotes[7];
                                    pitch3 = possibleNotes[8];
                                    pitch4 = possibleNotes[10];
                                    pitch5 = possibleNotes[12];
                                    pitch6 = possibleNotes[13];
                                    pitch7 = possibleNotes[15];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[5];
                                    pitch2 = possibleNotes[7];
                                    pitch3 = possibleNotes[8];
                                    pitch4 = possibleNotes[10];
                                    pitch5 = possibleNotes[12];
                                    pitch6 = possibleNotes[14];
                                    pitch7 = possibleNotes[15];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[5];
                                    pitch2 = possibleNotes[7];
                                    pitch3 = possibleNotes[9];
                                    pitch4 = possibleNotes[10];
                                    pitch5 = possibleNotes[12];
                                    pitch6 = possibleNotes[14];
                                    pitch7 = possibleNotes[15];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[5];
                                    pitch2 = possibleNotes[7];
                                    pitch3 = possibleNotes[9];
                                    pitch4 = possibleNotes[11];
                                    pitch5 = possibleNotes[12];
                                    pitch6 = possibleNotes[14];
                                    pitch7 = possibleNotes[16];
                                    break;
                            }
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[5 + type2];
                                    pitch2 = possibleNotes[7 + type2];
                                    pitch3 = possibleNotes[9 + type2];
                                    pitch4 = possibleNotes[10 + type2];
                                    pitch5 = possibleNotes[12 + type2];
                                    pitch6 = possibleNotes[14 + type2];
                                    pitch7 = possibleNotes[16 + type2];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[5 + type2];
                                    pitch2 = possibleNotes[7 + type2];
                                    pitch3 = possibleNotes[8 + type2];
                                    pitch4 = possibleNotes[10 + type2];
                                    pitch5 = possibleNotes[12 + type2];
                                    pitch6 = possibleNotes[13 + type2];
                                    pitch7 = possibleNotes[15 + type2];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[5 + type2];
                                    pitch2 = possibleNotes[7 + type2];
                                    pitch3 = possibleNotes[8 + type2];
                                    pitch4 = possibleNotes[10 + type2];
                                    pitch5 = possibleNotes[12 + type2];
                                    pitch6 = possibleNotes[14 + type2];
                                    pitch7 = possibleNotes[15 + type2];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[5 + type2];
                                    pitch2 = possibleNotes[7 + type2];
                                    pitch3 = possibleNotes[9 + type2];
                                    pitch4 = possibleNotes[10 + type2];
                                    pitch5 = possibleNotes[12 + type2];
                                    pitch6 = possibleNotes[14 + type2];
                                    pitch7 = possibleNotes[15 + type2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[5 + type2];
                                    pitch2 = possibleNotes[7 + type2];
                                    pitch3 = possibleNotes[9 + type2];
                                    pitch4 = possibleNotes[11 + type2];
                                    pitch5 = possibleNotes[12 + type2];
                                    pitch6 = possibleNotes[14 + type2];
                                    pitch7 = possibleNotes[16 + type2];
                                    break;
                            }
                            break;
                        case 7:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[0];
                                    pitch2 = possibleNotes[2];
                                    pitch3 = possibleNotes[4];
                                    pitch4 = possibleNotes[5];
                                    pitch5 = possibleNotes[7];
                                    pitch6 = possibleNotes[9];
                                    pitch7 = possibleNotes[11];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[0];
                                    pitch2 = possibleNotes[2];
                                    pitch3 = possibleNotes[3];
                                    pitch4 = possibleNotes[5];
                                    pitch5 = possibleNotes[7];
                                    pitch6 = possibleNotes[8];
                                    pitch7 = possibleNotes[10];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[0];
                                    pitch2 = possibleNotes[2];
                                    pitch3 = possibleNotes[3];
                                    pitch4 = possibleNotes[5];
                                    pitch5 = possibleNotes[7];
                                    pitch6 = possibleNotes[9];
                                    pitch7 = possibleNotes[10];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[0];
                                    pitch2 = possibleNotes[2];
                                    pitch3 = possibleNotes[4];
                                    pitch4 = possibleNotes[5];
                                    pitch5 = possibleNotes[7];
                                    pitch6 = possibleNotes[9];
                                    pitch7 = possibleNotes[10];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[0];
                                    pitch2 = possibleNotes[2];
                                    pitch3 = possibleNotes[4];
                                    pitch4 = possibleNotes[6];
                                    pitch5 = possibleNotes[7];
                                    pitch6 = possibleNotes[9];
                                    pitch7 = possibleNotes[11];
                                    break;
                            }
                            break;
                        case 8:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 1))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch6 = possibleNotes[8 + (type2 - (type2 - 1))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 1))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 1))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 1))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 1))];
                                    pitch4 = possibleNotes[6 + (type2 - (type2 - 1))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 1))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 1))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 1))];
                                    break;
                            }
                            break;
                        case 9:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 2))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch6 = possibleNotes[8 + (type2 - (type2 - 2))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 2))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 2))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 2))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[6 + (type2 - (type2 - 2))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 2))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 2))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 2))];
                                    break;
                            }
                            break;
                        case 10:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 3))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch6 = possibleNotes[8 + (type2 - (type2 - 3))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 2))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 3))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 3))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 3))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 3))];
                                    pitch4 = possibleNotes[6 + (type2 - (type2 - 3))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 3))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 3))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 3))];
                                    break;
                            }
                            break;
                        case 11:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 4))];
                                    break;
                                case 1:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch6 = possibleNotes[8 + (type2 - (type2 - 4))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 2:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[3 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 3:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[5 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch7 = possibleNotes[10 + (type2 - (type2 - 4))];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[0 + (type2 - (type2 - 4))];
                                    pitch2 = possibleNotes[2 + (type2 - (type2 - 4))];
                                    pitch3 = possibleNotes[4 + (type2 - (type2 - 4))];
                                    pitch4 = possibleNotes[6 + (type2 - (type2 - 4))];
                                    pitch5 = possibleNotes[7 + (type2 - (type2 - 4))];
                                    pitch6 = possibleNotes[9 + (type2 - (type2 - 4))];
                                    pitch7 = possibleNotes[11 + (type2 - (type2 - 4))];
                                    break;
                            }
                            break;
                    }
                    
                    var sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/bell_note",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 1f, pitch2),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 7/6f, pitch3),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 8/6f, pitch4),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 9/6f, pitch5),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 10/6f, pitch6),
                        new MultiSound.Sound("launchParty/bell_short",   beat + 11/6f, pitch7),
                    };


                    BeatAction.New(LaunchParty.instance.Bell, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Bells.Play(awakeAnim, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 7/6f, delegate { Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 8/6f, delegate { Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 9/6f, delegate { Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 10/6f, delegate { Bells.Play(awakeAnim, 0, 0);}),
                        new BeatAction.Action(beat + 11/6f, delegate { Bells.Play(awakeAnim, 0, 0);})
                    }
                    );
                    var SoundSource = MultiSound.Play(sound);
                    break;
                case 3:
                {
                    switch (type2)
                    {
                        case 0:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[10];
                                    pitch2 = possibleNotes[4];
                                    pitch3 = possibleNotes[5];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[10];
                                    pitch2 = possibleNotes[3];
                                    pitch3 = possibleNotes[5];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[11];
                                    pitch2 = possibleNotes[4];
                                    pitch3 = possibleNotes[5];
                                    break;
                            }
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[10 + type2];
                                    pitch2 = possibleNotes[4 + type2];
                                    pitch3 = possibleNotes[5 + type2];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[10 + type2];
                                    pitch2 = possibleNotes[3 + type2];
                                    pitch3 = possibleNotes[5 + type2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[11 + type2];
                                    pitch2 = possibleNotes[4 + type2];
                                    pitch3 = possibleNotes[5 + type2];
                                    break;
                            }
                            break;
                        case 8:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[6];
                                    pitch2 = possibleNotes[0];
                                    pitch3 = possibleNotes[1];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[6];
                                    pitch2 = possibleNotes[25];
                                    pitch3 = possibleNotes[1];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[7];
                                    pitch2 = possibleNotes[0];
                                    pitch3 = possibleNotes[1];
                                    break;
                            }
                            break;
                        case 9:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[7];
                                    pitch2 = possibleNotes[1];
                                    pitch3 = possibleNotes[2];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[7];
                                    pitch2 = possibleNotes[0];
                                    pitch3 = possibleNotes[2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[8];
                                    pitch2 = possibleNotes[1];
                                    pitch3 = possibleNotes[2];
                                    break;
                            }
                            break;
                        case 10:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[7 + 1];
                                    pitch2 = possibleNotes[1 + 1];
                                    pitch3 = possibleNotes[2 + 1];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[7 + 1];
                                    pitch2 = possibleNotes[0 + 1];
                                    pitch3 = possibleNotes[2 + 1];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[8 + 1];
                                    pitch2 = possibleNotes[1 + 1];
                                    pitch3 = possibleNotes[2 + 1];
                                    break;
                            }
                            break;
                        case 11:
                            switch (type3)
                            {
                                case 0:
                                    pitch1 = possibleNotes[7 + 2];
                                    pitch2 = possibleNotes[1 + 2];
                                    pitch3 = possibleNotes[2 + 2];
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    pitch1 = possibleNotes[7 + 2];
                                    pitch2 = possibleNotes[0 + 2];
                                    pitch3 = possibleNotes[2 + 2];
                                    break;
                                case 4:
                                    pitch1 = possibleNotes[8 + 2];
                                    pitch2 = possibleNotes[1 + 2];
                                    pitch3 = possibleNotes[2 + 2];
                                    break;
                            }
                            break;
                    }
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("launchParty/pin",   beat, pitch1),
                        new MultiSound.Sound("launchParty/rocket_pin_prepare",   beat, 1f, 0.75f),
                        new MultiSound.Sound("launchParty/flute",   beat, pitch2, 0.02f),
                        new MultiSound.Sound("launchParty/flute",   beat + 1/6f, pitch3, 0.02f),
                        new MultiSound.Sound("launchParty/flute",   beat + 2/6f, pitch2, 0.06f),
                        new MultiSound.Sound("launchParty/flute",   beat + 3/6f, pitch3, 0.1f),
                        new MultiSound.Sound("launchParty/flute",   beat + 4/6f, pitch2, 0.16f),
                        new MultiSound.Sound("launchParty/flute",   beat + 5/6f, pitch3, 0.22f),
                        new MultiSound.Sound("launchParty/flute",   beat + 1f, pitch2, 0.3f),
                        new MultiSound.Sound("launchParty/flute",   beat + 7/6f, pitch3, 0.4f),
                        new MultiSound.Sound("launchParty/flute",   beat + 8/6f, pitch2, 0.6f),
                        new MultiSound.Sound("launchParty/flute",   beat + 9/6f, pitch3, 0.75f),
                        new MultiSound.Sound("launchParty/flute",   beat + 10/6f, pitch2, 0.89f),
                        new MultiSound.Sound("launchParty/flute",   beat + 11/6f, pitch3),
                    });

                    BeatAction.New(LaunchParty.instance.Bowling, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Pins.Play(awakeAnim, 0, 0); }),
                    }
                    );
                    
                    
                    break;
            }
            }
            
        }
        public void RocketFourSuccess(PlayerActionEvent caller, float state)
        {   
            float fpitch = 0f;
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                float randomNumber = UnityEngine.Random.Range(0, 10);
                if (randomNumber >= 5)
                    Rockets.Play("RocketBlank");
                else
                    Rockets.Play("RocketBlankRight");
            }
            else
            {
                int notes = (int)ScaleNote.A;
                if (notes <= 6)
                {
                    fpitch = possibleNotes[notes + 12];
                
                } else {
                    fpitch = possibleNotes[notes];
                }
                Jukebox.PlayOneShotGame("launchParty/rocket_note", 0f, 1f);
                Jukebox.PlayOneShotGame("launchParty/rocket_family");
                Rockets.Play("RocketLaunch");
            }
                
            
                
            
        }

        public void RocketFourMiss(PlayerActionEvent caller)
        {
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                Rockets.Play("RocketMiss");
            
            else
                Rockets.Play("RocketMissRight");
            Jukebox.PlayOneShotGame("launchParty/miss");
            
        }

        public void RocketFourBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                Rockets.Play("RocketBlank");
            
            else
                Rockets.Play("RocketBlankRight");
        }

        public void RocketFiveSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            Jukebox.PlayOneShotGame("launchParty/popper_note", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/rocket_crackerblast");
            Crackers.Play("PopperLaunch");
        }

        public void RocketFiveMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            Crackers.Play("PopperMiss");
        }

        public void RocketFiveBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            Crackers.Play("PopperMiss");
        }

        public void RocketSevenSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            Jukebox.PlayOneShotGame("launchParty/bell_note", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/bell_blast");
            Bells.Play("BellLaunch");
        }

        public void RocketSevenMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            Bells.Play("BellMiss");
        }

        public void RocketSevenBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            Bells.Play("BellMiss");
        }

        public void RocketOneSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            float finalpitch2 = 1f;
            Jukebox.PlayOneShotGame("launchParty/pin", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/flute", 0f, finalpitch2);
            Jukebox.PlayOneShotGame("launchParty/rocket_bowling");
            Pins.Play("PinLaunch");
        }

        public void RocketOneMiss(PlayerActionEvent caller)
        {
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                Rockets.Play("PinMiss");
            
            else
                Rockets.Play("PinMissRight");
            Jukebox.PlayOneShotGame("launchParty/miss");
        }

        public void RocketOneBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            Pins.Play("PinMiss");
        }
        
    }
} 
