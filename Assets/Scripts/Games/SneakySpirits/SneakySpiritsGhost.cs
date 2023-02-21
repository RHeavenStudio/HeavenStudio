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

        public void Init(float spawnBeat, float length)
        {
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(spawnBeat - 0.2f, delegate { anim.DoScaledAnimationAsync("Move", 1f); }),
                new BeatAction.Action(spawnBeat + length - 0.5f, delegate { anim.DoScaledAnimationAsync("MoveDown", 1f); }),
                new BeatAction.Action(spawnBeat + length, delegate { Destroy(gameObject); }),
            });
        }
    }
}


