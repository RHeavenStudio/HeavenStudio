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
                new GameAction("ThrowObjectLeft", "Throw Left Object")
                {
                    function = delegate { DogNinja.instance.ThrowObjectLeft(eventCaller.currentEntity.beat); }, 
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
                    function = delegate { DogNinja.instance.HWG(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
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
        public Animator Bird; // Bird flying in and out
        // public Animator ;
        
        [Header("References")]
        public GameObject ObjectBase;
        public GameObject HalvesBase;
        public Transform ObjectHolder;
        public Transform HalvesHolder;

        [Header("Curves")]
        public BezierCurve3D CurveFromLeft;
        public BezierCurve3D CurveFromRight;

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

        private void CutSound(bool applause)
        {
            Jukebox.PlayOneShotGame("dogNinja/cut");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        public void ThrowObjectLeft(float beat)
        {
            Jukebox.PlayOneShotGame("dogNinja/ThrowObject");
        }

        public void ThrowObjectRight(float beat)
        {
            Jukebox.PlayOneShotGame("dogNinja/ThrowObject");
        }

        public void CutEverything(float beat)
        {
            Bird.Play("FlyIn");
            Jukebox.PlayOneShotGame("dogNinja/bird_flap");
        }

        public void HWG(float beat)
        {

        }
    }

}