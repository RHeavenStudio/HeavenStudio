using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class TooltipGiver : MonoBehaviour
    {
        [SerializeField] private string TooltipText;

        void Start()
        {
            Tooltip.AddTooltip(gameObject, TooltipText);

        }
    }
}