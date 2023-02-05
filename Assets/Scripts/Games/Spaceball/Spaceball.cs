using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbBatterLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceball", "Spaceball", "00A518", false, false, new List<GameAction>()
            {
                new GameAction("shoot", "Pitch Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, false, eventCaller.currentEntity["type"]); }, 
                    defaultLength = 2, 
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "The type of ball/object to shoot") 
                    } 
                },
				new GameAction("shootHigh", "Pitch High Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, true, eventCaller.currentEntity["type"]); }, 
                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "The type of ball/object to shoot") 
                    } 
                },
				new GameAction("costume", "Change Batter Costume")
                {
                    function = delegate { Spaceball.instance.Costume(eventCaller.currentEntity["type"]); },
                    parameters = new List<Param>() 
                    {
                        new Param("type", Spaceball.CostumeType.Standard, "Type", "The costume to change to") 
                    } 
                },
                new GameAction("alien", "Show Alien")
                {
                    function = delegate { Spaceball.instance.alien.Show(eventCaller.currentEntity.beat); } 
                },
                new GameAction("camera", "Zoom Camera")
                {
                    function = delegate { Spaceball.instance.OverrideCurrentZoom(); }, 
                    defaultLength = 4, 
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(1, 320, 10), "Zoom", "The camera's zoom level (Lower value = Zoomed in)"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The easing function to use while zooming") 
                    } 
                },
                new GameAction("prepare dispenser", "Dispenser Prepare")
                {
                    function = delegate { Spaceball.instance.PrepareDispenser(); }, 
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Spaceball;

    public class Spaceball : Minigame
    {
		public enum BallType {
            Baseball = 0,
            Onigiri = 1,
            Alien = 2,
            Tacobell = 3,
        }
		
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

        public Sprite[] BallSprites;

        private List<DynamicBeatmap.DynamicEntity> allCameraEvents = new List<DynamicBeatmap.DynamicEntity>();

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
            var camEvents = EventCaller.GetAllInGameManagerList("spaceball", new string[] { "camera" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom();
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
                        GameCamera.additionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                    }
                    else
                    {
                        if (currentZoomCamLength < 0)
                        {
                            GameCamera.additionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);

                            float newPosZ = func(lastCamDistance + 10, currentZoomCamDistance + 10, normalizedBeat);
                            GameCamera.additionalPosition = new Vector3(0, 0, newPosZ);
                        }
                    }
                }
                else
                {
                    // ?
                    GameCamera.additionalPosition = new Vector3(0, 0, 0);
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
                    lastCamDistance = allCameraEvents[currentZoomIndex - 1]["valA"] * -1;
                else
                {
                    if (currentZoomIndex == 0)
                        lastCamDistance = -10;
                    else
                        lastCamDistance = allCameraEvents[0]["valA"] * -1;
                }

                currentZoomCamBeat = allCameraEvents[currentZoomIndex].beat;
                currentZoomCamLength = allCameraEvents[currentZoomIndex].length;

                float dist = allCameraEvents[currentZoomIndex]["valA"] * -1;

                if (dist > 0)
                    currentZoomCamDistance = 0;
                else
                    currentZoomCamDistance = dist;

                lastEase = (EasingFunction.Ease) allCameraEvents[currentZoomIndex]["ease"];
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

            ball.GetComponent<SpaceballBall>().Sprite.sprite = BallSprites[type];
            switch(type)
            {
                case (int)BallType.Baseball:
                    break;
                case (int)BallType.Onigiri:
                    ball.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case (int)BallType.Alien:
                    break;
                case (int)BallType.Tacobell:
                    ball.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    ball.GetComponent<SpaceballBall>().isTacobell = true;
                    break;
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