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

        public void Init(double spawnBeat, float length)
        {
            if (length == 0) length = 1;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(spawnBeat, delegate { anim.DoScaledAnimationAsync("Move", (1 / length) * Conductor.instance.SongPitch); }),
                new BeatAction.Action(spawnBeat + (length * 0.5f), delegate { anim.DoScaledAnimationAsync("MoveDown", (1 / length) * Conductor.instance.SongPitch); }),
                new BeatAction.Action(spawnBeat + length, delegate { Destroy(gameObject); }),
            });
        }
    }
}


