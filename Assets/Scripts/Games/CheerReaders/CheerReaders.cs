using HeavenStudio.Games.Scripts_PajamaParty;
using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBookLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cheerReaders", "Cheer Readers \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class CheerReaders : Minigame
    {
        [Header("Objects")]
        public GameObject PepSquadMember;
        public GameObject faceSprites;

        [Header("Positions")]
        public Transform SpawnRoot;

        public static CheerReaders instance;
        RvlCharacter[,] chars;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            //spawn characters & faces
            // is 4x3 grid
            // c  c  c  c 
            // c  c  c  c  
            // c  c  c  P  
            chars = new RvlCharacter[4, 3];
            float RADIUS = 2.75f;
            float scale = 1.0f;
            int sorting = 10;
            Vector3 spawnPos = SpawnRoot.position + new Vector3(-RADIUS * 3, 0);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    //on x-axis we go left to right
                    spawnPos += new Vector3(RADIUS * scale, 0);
                    if (!(y == 0 && x == 2)) //don't spawn at the player's position
                    {
                        GameObject mobj = Instantiate(PepSquadMember, SpawnRoot.parent);
                        RvlCharacter character = mobj.GetComponent<RvlCharacter>();
                        mobj.GetComponent<SortingGroup>().sortingOrder = sorting;
                        mobj.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                        mobj.transform.localScale = new Vector3(scale, scale);
                        character.row = y;
                        character.col = x;
                        chars[x, y] = character;
                    }
                    scale -= 0.1f;
                    spawnPos = SpawnRoot.position - new Vector3(RADIUS * 3 * scale, -RADIUS / 3.75f * (y + 1), -RADIUS / 5f * (y + 1));
                    sorting--;
                }
            }

            // Update is called once per frame
            void Update()
            {

            }
        }
    }
}
