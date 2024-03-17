using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;



namespace HeavenStudio.Games.Scripts_Airboarder
{

    public class Scrollng_Floor : MonoBehaviour
    {
        public float scrollSpeedX;
        public float scrollSpeedY;
        private SkinnedMeshRenderer meshRenderer;

        public Airboarder game;


    // Start is called before the first frame update
        private void Awake()
        {
            game = Airboarder.instance;
            meshRenderer = GetComponent<SkinnedMeshRenderer>();
            ScrollFloorTexture(Conductor.instance.songBpm);

        }

        

        // Update is called once per frame
        void Update()
        {

          
            
        }

        public void ScrollFloorTexture(float bpm)
        {
            float modifier = bpm/120;
            meshRenderer.material.mainTextureOffset = new Vector2(modifier * scrollSpeedX, modifier * scrollSpeedY);
        }



    }
}