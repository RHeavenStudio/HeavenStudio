using UnityEngine;
using System;

namespace HeavenStudio.Common
{
    public class SingleSuperScroll : MonoBehaviour
    {
        #region Private

        [SerializeField] SpriteRenderer sr;

        #endregion

        #region Public

        public bool AutoScroll;
        public float AutoScrollX;
        public float AutoScrollY;

        public float X = 1.0f;
        public float Y = 1.0f;
        public float XBounds = 1.0f;
        public float YBounds = 1.0f;

        #endregion

        #region MonoBehaviour

        private void Start()
        {

        }

        public void LateUpdate()
        {
            if (AutoScroll) {
                X += AutoScrollX;
                Y += AutoScrollY;
            }

            gameObject.transform.position = new Vector2(X, Y);
        }

        #endregion
    }
}