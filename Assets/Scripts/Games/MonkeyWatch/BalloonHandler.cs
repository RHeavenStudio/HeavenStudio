using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class BalloonHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform target;
        [SerializeField] private Transform balloonTrans;
        [Header("Properties")]
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;

        private float additionalXOffset;
        private float additionalYOffset;

        public void Init(double beat)
        {
            Update();
        }

        private void Update()
        {
            balloonTrans.position = new Vector3(target.position.x + xOffset + additionalXOffset, target.position.y + yOffset + additionalYOffset, target.position.z);
        }
    }
}

