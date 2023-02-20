using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SneakySpirits
{
    public class SneakySpiritsGhost : MonoBehaviour
    {
        private SneakySpirits game;
        private Animator anim;

        void Awake()
        {
            anim = GetComponent<Animator>();
            game = SneakySpirits.instance;
        }

        public void Init(float riseDownBeat)
        {
            anim.DoScaledAnimationAsync("Move", 1f);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(riseDownBeat - 0.5f, delegate { anim.DoScaledAnimationAsync("MoveDown", 1f); }),
                new BeatAction.Action(riseDownBeat, delegate { Destroy(gameObject); })
            });
        }
    }
}


