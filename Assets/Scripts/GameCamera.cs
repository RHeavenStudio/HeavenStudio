using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class GameCamera : MonoBehaviour
    {
        public static GameCamera instance { get; private set; }
        public new Camera camera;

        [Header("Components")]
        public Color baseColor;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        private void Start()
        {
            camera.backgroundColor = baseColor;
        }
    }
}