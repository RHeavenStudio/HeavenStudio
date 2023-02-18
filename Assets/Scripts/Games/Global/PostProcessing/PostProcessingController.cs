using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Global.PostProcessing
{
    public class PostProcessingController : MonoBehaviour
    {
        public List<DynamicBeatmap.DynamicEntity> AllEvents = new List<DynamicBeatmap.DynamicEntity>();
        public int LastIndexCount = 0;
    }
}
