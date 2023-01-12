using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;
namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRocketLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) 
        {
            
            return new Minigame("launchParty", "Launch Party \n<color=#eb5454>[WIP]</color>", "000000", false, false, new List<GameAction>()
            {
                new GameAction("rocket", "Launch Rocket")
                {
                    
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchRocket(e.beat, e["type"], e["type2"], e["type3"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", LaunchParty.RocketType.Family, "Rocket Model", "The rocket to launch"),
                        new Param("type2", LaunchParty.ScaleNote.A, "Scale Note", "What scale note to use"),
                        new Param("type3", LaunchParty.ScaleType.Major, "Scale Type", "What type of scale to use"),
                    }
                },
            });

        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_LaunchParty;
    public class LaunchParty : Minigame
    {
        
        [Header("Main")]
        public GameObject PadSprites;

        [Header("Rockets")]
        public GameObject Rocket;
        public GameObject PartyCracker;
        public GameObject Bell;
        public GameObject Bowling;

        [Header("Animators")]
        public Animator Rockets;
        public Animator Crackers;
        public Animator Bells;
        public Animator Pins;
        
        [Header("Outcasts")]
        public PlayerActionEvent padLaunch;

        public enum RocketType
        {
            Family,
            PartyCracker,
            Bell,
            Bowling,
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

        
        [Header("Positions")]
        public Transform SpawnRoot;
        public static LaunchParty instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            Vector3 spawnPos = SpawnRoot.position;
            GameObject mobj = Instantiate(PadSprites, SpawnRoot.parent);
            mobj.SetActive(true);

        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        public GameObject CreateRocketInstance(float beat, string awakeAnim, RocketScript.RocketType type)
        {
            
            GameObject mobk = Instantiate(Rocket, SpawnRoot.parent);
            RocketScript mobkDat = mobk.GetComponent<RocketScript>();
            mobkDat.startBeat = beat;
            mobkDat.awakeAnim = awakeAnim;
            mobkDat.type = type;
            mobk.SetActive(true);
            return mobk;

        }
        
        public void LaunchRocket(float beat, int type, int type2, int type3)
            {   float pitch1 = 1f;
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
                if (type == 0)
                {   switch (type2)
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


                    BeatAction.New(Rocket, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Rockets.Play("Rocket3", 0, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { Rockets.Play("Rocket2", 0, 0);}),
                        new BeatAction.Action(beat + 2f, delegate { Rockets.Play("Rocket1", 0, 0);})
                    }
                    );
                }
                if (type == 1)
                {
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


                    BeatAction.New(PartyCracker, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Crackers.Play("Popper5", 0, 0); }),
                        new BeatAction.Action(beat + 2/3f, delegate { Crackers.Play("Popper4", 0, 0);}),
                        new BeatAction.Action(beat + 1f, delegate { Crackers.Play("Popper3", 0, 0);}),
                        new BeatAction.Action(beat + 4/3f, delegate { Crackers.Play("Popper2", 0, 0);}),
                        new BeatAction.Action(beat + 5/3f, delegate { Crackers.Play("Popper1", 0, 0);})
                    }
                    );
                    
                    
                }
                
                if (type == 2)
                {
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


                    BeatAction.New(Bell, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Bells.Play("Bell7", 0, 0); }),
                        new BeatAction.Action(beat + 1f, delegate { Bells.Play("Bell6", 0, 0);}),
                        new BeatAction.Action(beat + 7/6f, delegate { Bells.Play("Bell5", 0, 0);}),
                        new BeatAction.Action(beat + 8/6f, delegate { Bells.Play("Bell4", 0, 0);}),
                        new BeatAction.Action(beat + 9/6f, delegate { Bells.Play("Bell3", 0, 0);}),
                        new BeatAction.Action(beat + 10/6f, delegate { Bells.Play("Bell2", 0, 0);}),
                        new BeatAction.Action(beat + 11/6f, delegate { Bells.Play("Bell1", 0, 0);})
                    }
                    );
                    var SoundSource = MultiSound.Play(sound);
                    
                }
                if (type == 3)
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
                    var sound = new MultiSound.Sound[]
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
                    };

                    BeatAction.New(Bowling, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Pins.Play("Pin1", 0, 0); }),
                    }
                    );
                    var SoundSource = MultiSound.Play(sound);
                    
                }
            }

}}


        


