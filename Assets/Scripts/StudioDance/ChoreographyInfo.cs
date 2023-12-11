using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.StudioDance
{
    [CreateAssetMenu(fileName = "New Choreography", menuName = "StudioDance/Choreography")]
    public class ChoreographyInfo : ScriptableObject
    {
        [Serializable]
        public struct ChoreographyStep
        {
            public string stateName;
            public double beatLength;
        }

        public string choreographyName;
        public List<ChoreographyStep> choreographySteps;
        public string poseStateOdd;
        public string poseStateEven;
    }
}