using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_Splashdown
{
    public class NtrSynchrette : MonoBehaviour
    {
        [SerializeField] private NtrSplash splashPrefab;
        [SerializeField] private Animator anim;
        private Transform synchretteTransform;

        private Splashdown game;

        private enum MovementState
        {
            None,
            Dive
        }

        private MovementState currentMovementState;

        private void Awake()
        {
            synchretteTransform = anim.transform;
            game = Splashdown.instance;
        }

        private void Update()
        {
            switch (currentMovementState)
            {
                case MovementState.None:
                    synchretteTransform.transform.localPosition = Vector3.zero;
                    break;
                case MovementState.Dive:
                    synchretteTransform.transform.localPosition = new Vector3(0f, -6f, 0f);
                    break;
            }
        }

        public void Appear(bool miss = false)
        {
            SetState(MovementState.None);
            if (!miss) anim.DoScaledAnimationAsync("Appear" + game.currentAppearType, 0.5f);
            else anim.DoScaledAnimationAsync("MissAppear", 0.5f);
            Instantiate(splashPrefab, transform).Init("Appearsplash");
        }

        public void GoDown()
        {
            SetState(MovementState.Dive);
            Instantiate(splashPrefab, transform).Init("GodownSplash");
        }

        private void SetState(MovementState state)
        {
            currentMovementState = state;
        }
    }
}

