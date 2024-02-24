using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_CropStomp
{
    public class Farmer : MonoBehaviour
    {
        public double nextStompBeat;

        private CropStomp game;

        PlayerActionEvent stomp;

        [SerializeField] private Transform collectedHolder;

        [SerializeField] private GameObject plantLeftRef;
        [SerializeField] private GameObject plantRightRef;
        [SerializeField] private GameObject plantLastRef;
        [SerializeField] private Sprite[] veggieSprites;
        [SerializeField] private GameObject startPlant;
        private List<GameObject> spawnedPlants = new List<GameObject>();
        private int lastVeggieType;

        [SerializeField] private float plantDistance = 0.5f;
        [SerializeField] private float plantStartDistance = 0.1f;

        [NonSerialized] public int plantThreshold = 8;

        [NonSerialized] public int plantLimit = 80;

        public static int collectedPlants = 0;

        private void OnDestroy()
        {
            if (!Conductor.instance.isPlaying)
            {
                collectedPlants = 0;
                UpdatePlants();
            }
        }

        public void Init()
        {
            game = CropStomp.instance;
            if (!Conductor.instance.isPlaying)
            {
                collectedPlants = 0;
            }
            UpdatePlants();
        }

        private void Update()
        {
            if (!game.isMarching)
                return;
            Conductor cond = Conductor.instance;

            if (stomp == null && cond.isPlaying)
            {
                if (GameManager.instance.currentGame == "cropStomp")
                {
                    stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, CropStomp.InputAction_BasicPress, Just, Miss, Out);
                    stomp.countsForAccuracy = false;
                }
            }

            if (PlayerInput.GetIsAction(CropStomp.InputAction_BasicPress) && !game.IsExpectingInputNow(CropStomp.InputAction_BasicPress))
            {
                game.bodyAnim.DoScaledAnimationAsync("Crouch", 0.5f);
            }
        }

        public void CollectPlant(int veggieType)
        {
            if (collectedPlants > plantLimit) return;
            if (collectedPlants <= plantLimit - plantThreshold) lastVeggieType = veggieType;
            collectedPlants++;
            UpdatePlants();
        }

        public void UpdatePlants()
        {
            startPlant.SetActive(collectedPlants >= plantThreshold);
            if (spawnedPlants.Count > 0)
            {
                foreach (var plant in spawnedPlants) {
                    Destroy(plant);
                }
                spawnedPlants.Clear();
            }
            for (int i = 0; i <= collectedPlants - (plantThreshold * 2) && i <= plantLimit - (plantThreshold * 2); i += plantThreshold)
            {
                bool isLast = i == plantLimit - (plantThreshold * 2);
                int realIndex = i / plantThreshold;
                GameObject spawnedPlant;
                if (isLast)
                {
                    spawnedPlant = Instantiate(plantLastRef, collectedHolder);
                    spawnedPlant.GetComponent<SpriteRenderer>().sprite = veggieSprites[lastVeggieType];
                } else {
                    spawnedPlant = Instantiate((realIndex % 2 == 0) ? plantRightRef : plantLeftRef, collectedHolder);
                }
                spawnedPlant.transform.localPosition = new Vector3(0, (realIndex * plantDistance) + plantStartDistance, 0);
                spawnedPlant.GetComponent<SpriteRenderer>().sortingOrder = -realIndex - 2;
                spawnedPlant.SetActive(true);
                spawnedPlants.Add(spawnedPlant);
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            // REMARK: does not count for performance
            Stomp(state >= 1f || state <= -1f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (GameManager.instance.currentGame != "cropStomp" || !game.isMarching) return;
            
            // REMARK: does not count for performance
            nextStompBeat += 2f;
            stomp?.Disable();
            stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, CropStomp.InputAction_BasicPress, Just, Miss, Out);
            stomp.countsForAccuracy = false;
        }

        private void Out(PlayerActionEvent caller) {}

        void Stomp(bool ng)
        {
            if (GameManager.instance.currentGame != "cropStomp" || !game.isMarching) return;
            if (ng) {
                game.bodyAnim.DoScaledAnimationAsync("Crouch", 0.5f);
            } else {
                game.Stomp();
                game.bodyAnim.DoScaledAnimationAsync("Stomp", 0.5f);
            }
            nextStompBeat += 2f;
            stomp?.Disable();
            stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, CropStomp.InputAction_BasicPress, Just, Miss, Out);
            stomp.countsForAccuracy = false;
        }
    }
}
