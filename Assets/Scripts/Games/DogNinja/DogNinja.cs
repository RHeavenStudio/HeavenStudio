using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("dogNinja", "Dog Ninja \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", true, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { DogNinja.instance.Bop(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("Toggle", true, "Bop", "Whether to bop to the beat or not"),
                    }
                },
                new GameAction("ThrowObjectLeft", "Throw Left Object")
                {
                    function = delegate { DogNinja.instance.ThrowObjectLeft(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Right Object")
                {
                    function = delegate { DogNinja.instance.ThrowObjectRight(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
                    }
                },
                new GameAction("CutEverything", "Cut Everything!")
                {
                    function = delegate { DogNinja.instance.CutEverything(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    function = delegate { DogNinja.instance.HereWeGo(eventCaller.currentEntity.beat); },
                    defaultLength = 2,
                    inactiveFunction = delegate { DogNinja.instance.HereWeGo(eventCaller.currentEntity.beat); },
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DogNinja;
    public class DogNinja : Minigame
    {
        [Header("Animators")]
        public Animator BirdAnim; // Bird flying in and out
        public Animator DogAnim; // Dog misc animations
        
        [Header("References")]
        public GameObject ObjectLeftBase;
        public GameObject ObjectRightBase;
        public GameObject HalvesBase;
        public GameObject FullBird;
        public GameObject Dog;
        public Transform ObjectHolder;
        public Transform HalvesHolder;

        [Header("Curves")]
        public BezierCurve3D CurveFromLeft;
        public BezierCurve3D CurveFromRight;

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        
        public static DogNinja instance;

        public enum ObjectType
        {
            Apple,
            Bone,
            Broccoli,
            Carrot,
            Cucumber,
            Pan,
            Pepper,
            Potato,
            Tire,
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat))
            {
                DogAnim.Play("Bop", 0, 0);
            }
            
            /*
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("bop test -AJ");
                DogAnim.Play("Bop");
            }
            */
        }

        public void Bop(float beat)
        {
            DogAnim.Play("Bop");
        }

        public void ThrowObjectLeft(float beat, int type)
        {
            Jukebox.PlayOneShotGame("dogNinja/fruit1");
        }

        public void ThrowObjectRight(float beat)
        {
            Jukebox.PlayOneShotGame("dogNinja/fruit1");
        }

        public void CutEverything(float beat)
        {
            if (!birdOnScreen) {
                Jukebox.PlayOneShotGame("dogNinja/bird_flap");
                BirdAnim.Play("FlyIn", 0, 0);
                birdOnScreen = true;
            } else {
                BirdAnim.Play("FlyOut", 0, 0);
                birdOnScreen = false;
            }
        }

        public void HereWeGo(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f),
                }, forcePlay: true);
        }
    }

}