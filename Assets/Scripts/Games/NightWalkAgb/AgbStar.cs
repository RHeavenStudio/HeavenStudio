using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbStar : MonoBehaviour
    {
        private float originX;
        private float originY;
        private AgbStarHandler handler;
        private Animator anim;
        private int evoStage = 1;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public void Init(float x, float y, AgbStarHandler handlerToPut)
        {
            originX = x;
            originY = y;
            handler = handlerToPut;
            Update();
        }

        private void Update()
        {
            transform.localPosition = handler.GetRelativePosition(ref originX, ref originY);
        }

        public void Blink()
        {
            anim.Play("Blink" + evoStage, 0, 0);
        }

        public void Evolve()
        {
            if (evoStage >= 5) return;
            anim.Play("Evolve" + evoStage, 0, 0);
            evoStage++;
        }

        public void Devolve()
        {
            anim.Play("Devolve" + evoStage, 0, 0);
        }
    }
}

