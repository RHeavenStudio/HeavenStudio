using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeZoomOut : MonoBehaviour
    {
        float zoomOffset;
        float yOffset;

        void Start ()
        {
            zoomOffset = transform.localPosition.z;
            yOffset = transform.localPosition.y;
        }

        void Update()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, GameCamera.additionalPosition.y + yOffset, GameCamera.additionalPosition.z + zoomOffset);
        }
    }
}


