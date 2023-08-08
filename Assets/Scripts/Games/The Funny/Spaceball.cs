using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbFunnyLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("the_funny", "The Funny", "000073", false, false, new List<GameAction>()
            {
                new GameAction("camera", "Zoom Camera")
                {
                    defaultLength = 4, 
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(1, 320, 10), "Zoom", "The camera's zoom level (Lower value = Zoomed in)"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "The easing function to use while zooming") 
                    } 
                },
                new GameAction("camera", "Move Camera")
                {
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("valX", new EntityTypes.Integer(-256, 256, 0), "X", "The window's X value. Can go off screen."),
                        new Param("valY", new EntityTypes.Integer(-256, 256, 0), "Y", "The window's Y value. Can go off screen."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "The easing function to use while moving.")
                    }
                },
            }
            //new List<string>() {"agb", "normal"},
            //"agbbatter", "en",
            //new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Spaceball;

    public class The_Funny : Minigame
    {
        /*
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

        [SerializeField] GameObject Ball;
        [SerializeField] GameObject BallsHolder;

        [SerializeField] GameObject Dispenser;
        public GameObject Dust;
        */
        private float lastCamDistance;
        private float currentZoomCamBeat;
        private float currentZoomCamLength;
        private float currentZoomCamDistance;

        private int currentZoomIndex;
        /*
        [SerializeField] Sprite[] BallSprites;
        [SerializeField] Material[] CostumeColors;
        */
        private List<RiqEntity> _allCameraEvents = new List<RiqEntity>();

        //public Alien alien;

        private Util.EasingFunction.Ease lastEase;

        public static The_Funny instance { get; set; }
        /*
        public override void OnGameSwitch(double beat)
        {
            for (int i = 1; i < BallsHolder.transform.childCount; i++)
                Destroy(BallsHolder.transform.GetChild(i).gameObject);
            GameCamera.instance.camera.orthographic = false;

            if (EligibleHits.Count > 0)
                EligibleHits.RemoveRange(0, EligibleHits.Count);
        }
        */
        
        //Updates zoom on chage time or smth
        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        private void Awake()
        {
            //this is this
            instance = this;
            //idk
            var camEvents = EventCaller.GetAllInGameManagerList("spaceball", new string[] { "camera" });
            //uhh
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            //for each camera event do a thing
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }
            
            //Theyre all temporary now
            _allCameraEvents = tempEvents;
            //Zoom distance is -10 ig
            currentZoomCamDistance = -10;
        }

        private void Update()
        {
            //if theres a camera event happening
            if (_allCameraEvents.Count > 0)
            {
                //if zoom index is not at the end of the cam events list and is >=0
                if (currentZoomIndex < _allCameraEvents.Count && currentZoomIndex >= 0)
                {
                    //if the conductor is >= the camera event at zoom index .beat
                    if (Conductor.instance.songPositionInBeatsAsDouble >= _allCameraEvents[currentZoomIndex].beat)
                    {
                        //update zoom
                        UpdateCameraZoom();
                        //zoom index +1
                        currentZoomIndex++;
                    }
                }
                //find what value the zoom should be at i think
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, currentZoomCamLength);
                //if normalized beat is >= 0
                if (normalizedBeat >= 0)
                {
                    //if normalized beat is >1
                    if (normalizedBeat > 1)
                    {
                        //uh idk it zooms ig
                        GameCamera.additionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                    }
                    else
                    {
                        //if zoom length is < 0
                        if (currentZoomCamLength < 0)
                        {
                            // bring it above 0 or smth
                            GameCamera.additionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                        }
                        else
                        {
                            //uh that thing is set to the last ease or something idk
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                            // it sets the new cam z to the last cam distance or smth
                            float newPosZ = func(lastCamDistance + 10, currentZoomCamDistance + 10, normalizedBeat);
                            GameCamera.additionalPosition = new Vector3(0, 0, newPosZ);
                        }
                    }
                }
                else
                {
                    // ?
                    // ye idk either ig it resets it?
                    GameCamera.additionalPosition = new Vector3(0, 0, 0);
                }
            }
        }
        //well what do you think it does? it makes a grilled cheese of course.
        private void UpdateCameraZoom()
        {
            //if theres no events, zoom is less or smth
            if (_allCameraEvents.Count == 0)
                currentZoomCamDistance = -10;
            //if zoom index isnt at the end and is greater than 0
            if (currentZoomIndex < _allCameraEvents.Count && currentZoomIndex >= 0)
            {
                //if zoomindex -1 is >= 0
                if (currentZoomIndex - 1 >= 0)
                    //last camera distance is last camera distance val a *-1 for some reason
                    lastCamDistance = _allCameraEvents[currentZoomIndex - 1]["valA"] * -1;
                else
                {
                    //else if index is 0 then distance is -10
                    if (currentZoomIndex == 0)
                        lastCamDistance = -10;
                    else
                        //otherwise its just the same thing but not index and just 0
                        lastCamDistance = _allCameraEvents[0]["valA"] * -1;
                }
                //zoom cam beat is the beat that the zoom cam is on
                currentZoomCamBeat = (float)_allCameraEvents[currentZoomIndex].beat;
                //length of block currently
                currentZoomCamLength = _allCameraEvents[currentZoomIndex].length;
                //distance?
                float dist = _allCameraEvents[currentZoomIndex]["valA"] * -1;
                //if distance is >= 0 zoom distance = 0 else its distance
                if (dist > 0)
                    currentZoomCamDistance = 0;
                else
                    currentZoomCamDistance = dist;
                //final part of easeing or smth
                lastEase = (Util.EasingFunction.Ease) _allCameraEvents[currentZoomIndex]["ease"];
            }
        }
        /*
        public void Shoot(double beat, bool high, int type)
        {
            GameObject ball = Instantiate(Ball);
            ball.transform.parent = Ball.transform.parent;
            ball.SetActive(true);
            ball.GetComponent<SpaceballBall>().startBeat = beat;

            if (high)
            {
                ball.GetComponent<SpaceballBall>().high = true;
                SoundByte.PlayOneShotGame("spaceball/longShoot");
            }
            else
            {
                SoundByte.PlayOneShotGame("spaceball/shoot");
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
                    ball.transform.localScale = new Vector3(2f, 2f, 1);
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
            SpaceballPlayer.instance.SetCostume(CostumeColors[type], type);
        }
        */
    }
}