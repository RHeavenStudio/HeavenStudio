using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<< Updated upstream
using HeavenStudio.Util;
namespace HeavenStudio.Games.Scripts_BuiltToScaleDS
=======
using HeavenStudio.Util;
namespace HeavenStudio.Games.BuiltToScaleDS
>>>>>>> Stashed changes
{
    public class BTSPiece : MonoBehaviour
    {
        public Animator anim;

        void LateUpdate()
        {
            if (anim.IsAnimationNotPlaying())
                Destroy(gameObject);
        }
    }
}
