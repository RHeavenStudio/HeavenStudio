using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)</color>", "312B9F", false, false, new List<GameAction>()
            {                   new GameAction("bop", "Bop")
                    {   function = delegate {BonOdori.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["toggle"], eventCaller.currentEntity["auto"]);},
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", true, "Bop", "Toggle if the Donpans should bop for the duration of this event."),
                            new Param("auto", false, "Bop (Auto)", "Toggle if the Donpans+ should automatically bop until another Bop event is reached."),
                        },

                    },
                
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
                },
                new GameAction("show text", "Show Text")
                {
                    function = delegate {BonOdori.instance.ShowText(eventCaller.currentEntity["line 1"], eventCaller.currentEntity["line 2"], eventCaller.currentEntity["line 3"], eventCaller.currentEntity["line 4"], eventCaller.currentEntity["line 5"],eventCaller.currentEntity["font"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {   new Param("whichLine", new EntityTypes.Integer(1,5,1), "Line", "Which line to modify.", new(){
                        new((x, _) => (int)x == 1, new string[] { "line 1"}),
                        new((x, _) => (int)x == 2, new string[] { "line 2"}),
                        new((x, _) => (int)x == 3, new string[] { "line 3"}),
                        new((x, _) => (int)x == 4, new string[] { "line 4"}),
                        new((x, _) => (int)x == 5, new string[] { "line 5"}),
                    }),
                        new Param("line 1", "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line", "Line 1", "Set the text for line 1."),
                        new Param("line 2", "", "Line 2", "Set the text for line 2."),
                        new Param("line 3", "", "Line 3", "Set the text for line 3.y"),
                        new Param("line 4", "", "Line 4", "Set the text for line 4."),
                        new Param("line 5", "", "Line 5", "Set the text for line 5."),
                        new Param("font", BonOdori.font.RodinProDb, "font", "Set the font for the text")


                        
                    },
                    priority = 1
                },
                    new GameAction("delete text", "Delete Text")
                {
                    function = delegate {BonOdori.instance.DeleteText(eventCaller.currentEntity["line 1"],eventCaller.currentEntity["line 2"],eventCaller.currentEntity["line 3"],eventCaller.currentEntity["line 4"],eventCaller.currentEntity["line 5"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("line 1", false, "Line 1", "Delete the contents of line 1."),
                        new Param("line 2", false, "Line 2", "Delete the contents of line 2."),
                        new Param("line 3", false, "Line 3", "Delete the contents of line 3."),
                        new Param("line 4", false, "Line 4", "Delete the contents of line 4."),
                        new Param("line 5", false, "Line 5", "Delete the contents of line 5."),
                    },

                },
                    new GameAction("scroll text", "Scroll Text")
                {
                    function = delegate {BonOdori.instance.ScrollText(eventCaller.currentEntity["line 1"],eventCaller.currentEntity["line 2"],eventCaller.currentEntity["line 3"],eventCaller.currentEntity["line 4"],eventCaller.currentEntity["line 5"], eventCaller.currentEntity.length);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("line 1", false, "Line 1", "Scroll the contents of line 1."),
                        new Param("line 2", false, "Line 2", "Scroll the contents of line 2."),
                        new Param("line 3", false, "Line 3", "Scroll the contents of line 3."),
                        new Param("line 4", false, "Line 4", "Scroll the contents of line 4."),
                        new Param("line 5", false, "Line 5", "Scroll the contents of line 5."),
                    },

                }
 
                    
          

        });
        
    }
    };
};
namespace HeavenStudio.Games
{




    public class BonOdori : Minigame
    {
       string prefix;
        string suffix;
        TextMeshProUGUI Text1_GUI;
        TextMeshProUGUI Text2_GUI;
        TextMeshProUGUI Text3_GUI;
        TextMeshProUGUI Text4_GUI;
        TextMeshProUGUI Text5_GUI;
        TextMeshProUGUI Text6_GUI;
        TextMeshProUGUI Text7_GUI;
        TextMeshProUGUI Text8_GUI;
        TextMeshProUGUI Text9_GUI;
        TextMeshProUGUI Text10_GUI;


        [SerializeField] TMPro.TMP_FontAsset WW;
        [SerializeField] TMPro.TMP_FontAsset RodinDB;
        [SerializeField] TMP_Text Text1;
        [SerializeField] TMP_Text Text2;
        [SerializeField] TMP_Text Text3;
        [SerializeField] TMP_Text Text4;
        [SerializeField] TMP_Text Text5;
        
        [SerializeField] TMP_Text Text6;
        [SerializeField] TMP_Text Text7;
        [SerializeField] TMP_Text Text8;
        [SerializeField] TMP_Text Text9;
        [SerializeField] TMP_Text Text10;
        [SerializeField] Animator DarkPlane;
        [SerializeField] Animator Player;
        [SerializeField] Animator Judge;
        [SerializeField] Animator CPU;
        [SerializeField] Animator Face;

        public enum font
        {
            RodinProDb = 0,
            WarioWareV2 = 1
        }
        public enum color
        {
            red = 0,
            yellow = 1,
            green = 2
        }
        public enum Tags
        {
            Tag1 = 1,
            Tag2 = 2,
            Tag3 = 3,
            Tag4 = 4,
            Tag5 = 5,
            Tag6 = 6,
            Tag7 = 7,
            Tag8 = 8,
            Tag9 = 9,
            Tag10 = 10
        }
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
        public void Awake()

        {
            instance = this;
            Text1_GUI = Text1.GetComponent<TextMeshProUGUI>();
            Text2_GUI = Text2.GetComponent<TextMeshProUGUI>();
            Text3_GUI = Text3.GetComponent<TextMeshProUGUI>();
            Text4_GUI = Text4.GetComponent<TextMeshProUGUI>();
            Text5_GUI = Text5.GetComponent<TextMeshProUGUI>();
            Text6_GUI = Text6.GetComponent<TextMeshProUGUI>();
            Text7_GUI = Text7.GetComponent<TextMeshProUGUI>();
            Text8_GUI = Text8.GetComponent<TextMeshProUGUI>();
            Text9_GUI = Text9.GetComponent<TextMeshProUGUI>();
            Text10_GUI = Text10.GetComponent<TextMeshProUGUI>();

    


        }

        public void Update()
        {
            if (PlayerInput.GetIsAction(BonOdori.InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)){
                ScoreMiss();
                SoundByte.PlayOneShot("miss");
  
            
            
        }

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
            MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/clap", 0f, offset: 0.01f),
                        });
        }
        
        public void Miss(PlayerActionEvent caller)
        {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("miss", 0f, offset: 0.01f),
                        });
        }
        
        
        public void Empty(PlayerActionEvent caller)
        {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("bonOdori/nearMiss", 0f, offset: 0.01f),
                        });



        }
    string ChangeColor(string text)
    {
            if (text.Contains("r|") | text.Contains("y|") | text.Contains("g|")){
            return text.Replace("r|", "<color=#ff0000>")
                   .Replace("g|", "<color=#00ff00>")
                   .Replace("y|", "<color=#ffff00>")
                   + "</color>";
            }
            return text;

    }

        public void ShowText(string text1, string text2, string text3, string text4, string text5, int font)
        {
            if (font == 0)
            {
                Text1.GetComponent<TextMeshPro>().font = RodinDB;
                Text2.GetComponent<TextMeshPro>().font = RodinDB;
                Text3.GetComponent<TextMeshPro>().font = RodinDB;
                Text4.GetComponent<TextMeshPro>().font = RodinDB;
                Text5.GetComponent<TextMeshPro>().font = RodinDB;
                Text6.GetComponent<TextMeshPro>().font = RodinDB;
                Text7.GetComponent<TextMeshPro>().font = RodinDB;
                Text8.GetComponent<TextMeshPro>().font = RodinDB;
                Text9.GetComponent<TextMeshPro>().font = RodinDB;
                Text10.GetComponent<TextMeshPro>().font = RodinDB;
;
            }
            if (font == 1)
            {
                Text1.GetComponent<TextMeshPro>().font = WW;
                Text2.GetComponent<TextMeshPro>().font = WW;
                Text3.GetComponent<TextMeshPro>().font = WW;
                Text4.GetComponent<TextMeshPro>().font = WW;
                Text5.GetComponent<TextMeshPro>().font = WW;
                Text6.GetComponent<TextMeshPro>().font = WW;
                Text7.GetComponent<TextMeshPro>().font = WW;
                Text8.GetComponent<TextMeshPro>().font = WW;
                Text9.GetComponent<TextMeshPro>().font = WW;
                Text10.GetComponent<TextMeshPro>().font = WW;
            }



        


            
            

    

                
            if (text1 is not "" || text1 is not "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line"){
                text1 = ChangeColor(text1);
                Text1.text = text1;
                Text6.text = text1;
  
                }
            if (text2 is not ""){
                text2 = ChangeColor(text2);
                Text2.text = text2;
   
                }
            if (text3 != ""){
                text3 = ChangeColor(text3);
                Text3.text = text3; 
 
                }
            if (text4 != ""){
                text4 = ChangeColor(text4);
                Text4.text = text4; 

                }
            if (text5 != ""){
                text5 = ChangeColor(text5);
                Text5.text = text5;

                }
            
            if (Text1.text != "" | Text2.text != "" | Text3.text != "" | Text4.text != "" | Text5.text != ""){
                DarkPlane.Play("Appear");
            }

        }
        public void DeleteText(bool text1, bool text2, bool text3, bool text4, bool text5){
            if (text1 == true){
                Text1.text = "";
            }
            if (text2 == true){
                Text2.text = "";
            }
            if (text3 == true){
                Text3.text = "";
            }
            if (text4 == true){
                Text4.text = "";
            }
            if (text5 == true){
                Text5.text = "";
            }
            if (Text1.text == "" && Text2.text == "" && Text3.text == "" && Text4.text == "" && Text5.text == ""){
                DarkPlane.Play("GoAway");
            }

        }
public void ScrollText(bool text1, bool text2, bool text3, bool text4, bool text5, float length)
{ RectTransform rectTransform = Text6.GetComponent<RectTransform>();

if (rectTransform != null)
{
    // Set Left and Top using anchoredPosition
    rectTransform.anchoredPosition = new Vector2(-10, -10);

    // Calculate the Right and Bottom based on Left and Top
    float width = -10f - 9.5f;  // Right - Left
    float height = 10 - (-10);      // Top - Bottom

    // Set sizeDelta based on calculated values
    rectTransform.sizeDelta = new Vector2(width, height);
} 
}
        public void Bop(double beat, double length, bool bop, bool auto)
        {  

        }
            


        


    }
}