using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    using Scripts_Spaceball;

    public class Spaceball : Minigame
    {
        public enum CostumeType {
            Standard,
            Bunny,
            SphereHead
        }

        public GameObject Ball;
        public GameObject BallsHolder;

        public GameObject Dispenser;
        public GameObject Dust;

        private float lastCamDistance;
        private float currentZoomCamBeat;
        private float currentZoomCamLength;
        private float currentZoomCamDistance;

        private int currentZoomIndex;

        public Sprite[] Balls;

        private List<Beatmap.Entity> allCameraEvents = new List<Beatmap.Entity>();

        public Alien alien;

        private EasingFunction.Ease lastEase;

        public static Spaceball instance { get; set; }

        public override void OnGameSwitch(float beat)
        {
            for (int i = 1; i < BallsHolder.transform.childCount; i++)
                Destroy(BallsHolder.transform.GetChild(i).gameObject);
            GameCamera.instance.camera.orthographic = false;

            if (EligibleHits.Count > 0)
                EligibleHits.RemoveRange(0, EligibleHits.Count);
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            var camEvents = EventCaller.GetAllInGameManagerList("spaceball", new string[] { "camera" });
            List<Beatmap.Entity> tempEvents = new List<Beatmap.Entity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom(); // can't believe this shit actually works
        }

        private void Update()
        {
            if (allCameraEvents.Count > 0)
            {
                if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
                {
                    if (Conductor.instance.songPositionInBeats >= allCameraEvents[currentZoomIndex].beat)
                    {
                        UpdateCameraZoom();
                        currentZoomIndex++;
                    }
                }

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, currentZoomCamLength);

                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1)
                    {
                        GameCamera.instance.camera.transform.localPosition = new Vector3(0, 0, currentZoomCamDistance);
                    }
                    else
                    {
                        if (currentZoomCamLength < 0)
                        {
                            GameCamera.instance.camera.transform.localPosition = new Vector3(0, 0, currentZoomCamDistance);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);

                            float newPosZ = func(lastCamDistance, currentZoomCamDistance, normalizedBeat);
                            GameCamera.instance.camera.transform.localPosition = new Vector3(0, 0, newPosZ);
                        }
                    }
                }
                else
                {
                    // ?
                    GameCamera.instance.camera.transform.localPosition = new Vector3(0, 0, -10f);
                }
            }
        }

        private void UpdateCameraZoom()
        {
            if (allCameraEvents.Count == 0)
                currentZoomCamDistance = -10;

            if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
            {
                if (currentZoomIndex - 1 >= 0)
                    lastCamDistance = allCameraEvents[currentZoomIndex - 1].valA * -1;
                else
                {
                    if (currentZoomIndex == 0)
                        lastCamDistance = -10;
                    else
                        lastCamDistance = allCameraEvents[0].valA * -1;
                }

                currentZoomCamBeat = allCameraEvents[currentZoomIndex].beat;
                currentZoomCamLength = allCameraEvents[currentZoomIndex].length;

                float dist = allCameraEvents[currentZoomIndex].valA * -1;

                if (dist > 0)
                    currentZoomCamDistance = 0;
                else
                    currentZoomCamDistance = dist;

                lastEase = allCameraEvents[currentZoomIndex].ease;
            }
        }

        public void OverrideCurrentZoom()
        {
            // lastCamDistance = GameCamera.instance.camera.transform.localPosition.z;
        }

        public void Shoot(float beat, bool high, int type)
        {
            GameObject ball = Instantiate(Ball);
            ball.transform.parent = Ball.transform.parent;
            ball.SetActive(true);
            ball.GetComponent<SpaceballBall>().startBeat = beat;

            if (high)
            {
                ball.GetComponent<SpaceballBall>().high = true;
                Jukebox.PlayOneShotGame("spaceball/longShoot");
            }
            else
            {
                Jukebox.PlayOneShotGame("spaceball/shoot");
            }

            if (type == 1)
            {
                ball.GetComponent<SpaceballBall>().Sprite.sprite = Balls[1];
            }

            Dispenser.GetComponent<Animator>().Play("DispenserShoot", 0, 0);
        }

        public void PrepareDispenser()
        {
            Dispenser.GetComponent<Animator>().Play("DispenserPrepare", 0, 0);
        }

        public void Costume(int type)
        {
            SpaceballPlayer.instance.SetCostume(type);
        }
    }
}