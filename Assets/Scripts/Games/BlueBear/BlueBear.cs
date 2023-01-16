using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrBearLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("blueBear", "Blue Bear", "B4E6F6", false, false, new List<GameAction>()
            {
                new GameAction("donut", "Donut")
                {
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, false); }, 
                    defaultLength = 3,
                },
                new GameAction("cake", "Cake")
                {
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, true); }, 
                    defaultLength = 4,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BlueBear;
    public class BlueBear : Minigame
    {
        [Header("Animators")]
        public Animator headAndBodyAnim; // Head and body
        public Animator bagsAnim; // Both bags sprite
        public Animator donutBagAnim; // Individual donut bag
        public Animator cakeBagAnim; // Individual cake bag

        [Header("References")]
        public GameObject donutBase;
        public GameObject cakeBase;
        public GameObject crumbsBase;
        public Transform foodHolder;
        public Transform crumbsHolder;
        public GameObject individualBagHolder;

        [Header("Curves")]
        public BezierCurve3D donutCurve;
        public BezierCurve3D cakeCurve;

        [Header("Gradients")]
        public Gradient donutGradient;
        public Gradient cakeGradient;

        private bool squashing;

        public static BlueBear instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            headAndBodyAnim.SetBool("ShouldOpenMouth", foodHolder.childCount != 0);

            if (PlayerInput.GetAnyDirectionDown() && !IsExpectingInputNow(InputType.DIRECTION_DOWN))
            {
                headAndBodyAnim.Play("BiteL", 0, 0);
            }
            else if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                headAndBodyAnim.Play("BiteR", 0, 0);
            }
        }

        private void LateUpdate()
        {
            if (squashing)
            {
                var dState = donutBagAnim.GetCurrentAnimatorStateInfo(0);
                var cState = cakeBagAnim.GetCurrentAnimatorStateInfo(0);

                bool noDonutSquash = dState.IsName("DonutIdle");
                bool noCakeSquash = cState.IsName("CakeIdle");

                if (noDonutSquash && noCakeSquash)
                {
                    squashing = false;
                    bagsAnim.Play("Idle", 0, 0);
                }
            }
        }

        public void SpawnTreat(float beat, bool isCake)
        {
            var objectToSpawn = isCake ? cakeBase : donutBase;
            var newTreat = GameObject.Instantiate(objectToSpawn, foodHolder);
            
            var treatComp = newTreat.GetComponent<Treat>();
            treatComp.startBeat = beat;
            treatComp.curve = isCake ? cakeCurve : donutCurve;

            newTreat.SetActive(true);

            Jukebox.PlayOneShotGame(isCake ? "blueBear/cake" : "blueBear/donut");

            SquashBag(isCake);
        }

        public void SquashBag(bool isCake)
        {
            squashing = true;
            bagsAnim.Play("Squashing", 0, 0);

            individualBagHolder.SetActive(true);

            if (isCake)
            {
                cakeBagAnim.Play("CakeSquash", 0, 0);
            }
            else
            {
                donutBagAnim.Play("DonutSquash", 0, 0);
            }
        }
    }
}
