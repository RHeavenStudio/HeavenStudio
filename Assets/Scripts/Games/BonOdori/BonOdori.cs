using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbBonOdoriLoader
    {

        public static Minigame AddGame(EventCaller eventCaller)
        { 
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)</color>", "ffffff", false, false, new List<GameAction>()
            {
                
                new GameAction("pan", "Pan")
                {
                    
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 1,  "pan", eventCaller.currentEntity["type"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationPan.regular, "Type", "Set the variation of the voice line."),
                    }
                },
                new GameAction("pa-n", "Pa-n")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 1.5, "pa-n",  eventCaller.currentEntity["type"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationPa_n.regular, "Type", "Set the variation of the voice line."),
                    },
                },
                    new GameAction("pa", "Pa")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 0.5,  "pa", eventCaller.currentEntity["type"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationPa.regular, "Type", "Set the variation of the voice line."),
                    },
                },
                new GameAction("don", "Don")
                {
                    function = delegate { BonOdori.instance.Sound(eventCaller.currentEntity.beat, 1,  "don",  eventCaller.currentEntity["type"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationDon.regular, "Type", "Set the variation of the voice line."),
                    },
                },
                new GameAction("do-n", "Do-n")
                {
                    function = delegate { BonOdori.instance.Sound(eventCaller.currentEntity.beat, 1.5, "do-n",  eventCaller.currentEntity["type"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationDo_n.regular, "Type", "Set the variation of the voice line."),
                    },
                },
                new GameAction("do", "Do")
                {
                    function = delegate { BonOdori.instance.Sound(eventCaller.currentEntity.beat, 0.5,  "do",  eventCaller.currentEntity["type"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.variationDo.regular, "Type", "Set the variation of the voice line."),
                    },
                }



            
            
            });

        }  

    }
        
};


namespace HeavenStudio.Games
{




    public class BonOdori : Minigame
    {
        
        public enum variationPan
        {
            regular = 0,
            variation1 = 1,
            variation2 = 2
        }
        public enum variationPa_n
        {
            regular = 0,
            variation1 = 1
        }
        public enum variationPa
        {   
            regular = 0

        }
        public enum variationDon
        {
            regular = 0,
            variation1 = 1,
            variation2 = 2, 
            variation3 = 3
        }
        public enum variationDo_n
        {
            regular = 0,
            variation1 = 1
        }
        public enum variationDo
        {
            regular = 0,
            variation1 = 1
        }
        public static BonOdori instance { get; set; }
        Animator player;
        Animator donpan1;
        Animator donpan2;
        Animator donpan3;

        public GameObject Player;
        public GameObject Donpan1;
        public GameObject Donpan2;
        public GameObject Donpan3;
        public void Awake()

        {
            player = Player.GetComponent<Animator>();
            donpan1 = Donpan1.GetComponent<Animator>();
            donpan2 = Donpan2.GetComponent<Animator>();
            donpan3 = Donpan3.GetComponent<Animator>();
            instance = this;

        }
        public void Clap(double beat, double type, string typeSpeak, int variation)
        {   
            
            
                        switch (typeSpeak){
                            case "pan":
                            ScheduleInput(beat, 0.5f, InputAction_BasicPress, Success, Miss, Empty);
                            switch (variation){
                                case (int) variationPan.regular:
                                MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pan1", beat + 0.5, offset: 0.01f)});
                            
                                break;
                                case (int) variationPan.variation1:
                                MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pan2", beat + 0.5f, offset: 0.01f)});
                                break;
                                case (int) variationPan.variation2:
                                MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pan3", beat + 0.5f, offset: 0.01f)});
                                break;}
                                break;
                            case "pa-n":
                            ScheduleInput(beat, 1f, InputAction_BasicPress, Success, Miss, Empty);
                            switch (variation){
                                case (int) variationPa_n.regular:
                                MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pa_n1", beat + 1f, offset: 0.01f)});
                            
                                break;
                                case (int) variationPa_n.variation1:
                                MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pa_n2", beat + 1f, offset: 0.01f)});
                                break;}
                                break;
                            case "pa":
                            ScheduleInput(beat, 0.5f, InputAction_BasicPress, Success, Miss, Empty);
                            MultiSound.Play(new MultiSound.Sound[]{
                                    new MultiSound.Sound("bonOdori/pa1", beat + 0.5f, offset: 0.01f)});
                                break;
                            
                            
            }
        }
        public void Sound(double beat, double type, string typeSpeak, int variation)
        {  switch (typeSpeak){
                            case "don":
                            switch (variation){
                case (int)variationDon.regular:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/don1", beat + 0.5, offset: 0.01f)});
                break;
                case (int)variationDon.variation1:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/don2", beat + 0.5, offset: 0.01f)});
                break;
                case (int)variationDon.variation2:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/don3", beat + 0.5, offset: 0.01f)});
                break;   
                case (int)variationDon.variation3:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/don4", beat + 0.5, offset: 0.01f)});
                break;                                  
                            }
                            
                            
                            break;
                            case "do-n":

            switch (variation) {
                case (int)variationDo_n.regular:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/do_n1", beat + 1, offset: 0.01f)});
                break;
                case (int)variationDo_n.variation1:
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/do_n2", beat + 1, offset: 0.01f)});
                break;
            }
            break;
                            
                            case "do":
                            switch (variation){
                                case (int) variationDo.regular:
                                MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/do1", beat + 0.5, offset: 0.01f)});
                break;
                            case (int) variationDo.variation1:
                                MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/do2", beat + 0.5, offset: 0.01f)});
                break;
                            }
                            
              
                            break;
            }
        }  

        public void Success(PlayerActionEvent caller, float state)
        {
            player.Play("ClapAll");

            Debug.Log("SUCCESS");
            MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/clap", 0.5f, offset: 0.01f),
                        });
        }
        
        public void Miss(PlayerActionEvent caller)
        {

            Debug.Log("Teste1");
        }
        
        
        public void Empty(PlayerActionEvent caller)
        {

            Debug.Log("Teste2");
        }}}

    

