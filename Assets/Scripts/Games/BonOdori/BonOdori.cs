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
                            new Param("toggle", true, "Bop", "Toggle if the Donpans and Yagura-chan should bop for the duration of this event."),
                            new Param("auto", false, "Bop (Auto)", "Toggle if the Donpans and Yagura-chan should automatically bop until another Bop event is reached."),
                        },

                    },
                
                new GameAction("pan", "Pan")
                {
                    
                   function = delegate {
  var e = eventCaller.currentEntity;
  string variation = "variation" + (new string[] { "Pan", "Pa", "Pa_n" })[e["type"]];
  BonOdori.instance.Clap(e.beat, e[variation], e["type"], e["mute"],e["clapType"]);
},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Sound", "Mute the voice line."),
                        new Param("type",BonOdori.typePan.Pan, "Type", "Set the type of voice line.", new(){
                        new((x, _) => (int)x == 0, new string[] { "variationPan"}),
                        new((x, _) => (int)x == 1, new string[] { "variationPa"}),
                        new((x, _) => (int)x == 2, new string[] { "variationPa_n"}),
                     }),
                        new Param("variationPan", BonOdori.variationPan.PanC, "Pan Type", "Set the variation of the voice line."),
                        new Param("variationPa", BonOdori.variationPa.PaG, "Pa Type", "Set the variation of the voice line."),
                        new Param("variationPa_n", BonOdori.variationPa_n.Pa_nA , "Pa-n Type", "Set the variation of the voice line."),
                        new Param("clapType", BonOdori.typeClap.SideClap, "Clap Type", "Set the type of clap.")
                    }
                },
               
                 new GameAction("don", "Don")
                {
                    
                   function = delegate {
  var e = eventCaller.currentEntity;
  string variation = "variation" + (new string[] { "Don", "Do", "Do_n" })[e["type"]];
  BonOdori.instance.Sound(e.beat, e[variation], e["type"]);
},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type",BonOdori.typeDon.Don, "Type", "Set the type of voiceline", new(){
                        new((x, _) => (int)x == 0, new string[] { "variationDon"}),
                        new((x, _) => (int)x == 1, new string[] { "variationDo"}),
                        new((x, _) => (int)x == 2, new string[] { "variationDo_n"}),
                     }),
                        new Param("variationDon", BonOdori.variationDon.DonA, "Don Type", "Set the variation of the voice line."),
                        new Param("variationDo", BonOdori.variationDo.DoC, "Do Type", "Set the variation of the voice line."),
                        new Param("variationDo_n", BonOdori.variationDo_n.Do_nA, "Do-n Type", "Set the variation of the voice line."),
                    }
                },
                
                new GameAction("show text", "Show Text")
                {
                    function = delegate {BonOdori.instance.ShowText(eventCaller.currentEntity["line 1"], eventCaller.currentEntity["line 2"], eventCaller.currentEntity["line 3"], eventCaller.currentEntity["line 4"], eventCaller.currentEntity["line 5"]);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {   new Param("whichLine", new EntityTypes.Integer(1,5,1), "Line", "Which line to modify.", new(){
                        new((x, _) => (int)x == 1, new string[] { "line 1"}),
                        new((x, _) => (int)x == 2, new string[] { "line 2"}),
                        new((x, _) => (int)x == 3, new string[] { "line 3"}),
                        new((x, _) => (int)x == 4, new string[] { "line 4"}),
                        new((x, _) => (int)x == 5, new string[] { "line 5"}),
                    }),
                        new Param("line 1", "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line.", "Line 1", "Set the text for line 1."),
                        new Param("line 2", "", "Line 2", "Set the text for line 2."),
                        new Param("line 3", "", "Line 3", "Set the text for line 3.y"),
                        new Param("line 4", "", "Line 4", "Set the text for line 4."),
                        new Param("line 5", "", "Line 5", "Set the text for line 5."),



                        
                    },
                    priority = 1
                },
                    new GameAction("delete text", "Delete Text")
                {
                    function = delegate {BonOdori.instance.DeleteText(eventCaller.currentEntity["line 1"],eventCaller.currentEntity["line 2"],eventCaller.currentEntity["line 3"],eventCaller.currentEntity["line 4"],eventCaller.currentEntity["line 5"]);},
                    defaultLength = 1f,
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
                    function = delegate {BonOdori.instance.ScrollText(eventCaller.currentEntity["line 1"],eventCaller.currentEntity["line 2"],eventCaller.currentEntity["line 3"],eventCaller.currentEntity["line 4"],eventCaller.currentEntity["line 5"], eventCaller.currentEntity.length, eventCaller.currentEntity.beat);},
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

                },
                    new GameAction("bow", "Bow")
                {
                    function = delegate {BonOdori.instance.Bow(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);},
                    defaultLength = 2f,
                    resizable = true,

                },
                // new GameAction("spin", "Spin")
                // {
                //     function = delegate {BonOdori.instance.Spin(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);},
                //     defaultLength = 1f,

                // },
                    new GameAction("toggle bg", "Toggle Darker Background")
                {
                    function = delegate {BonOdori.instance.DarkBG(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity.length);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Darken Background", "Darkens the background"),
                    }

                },
 
                    
          

        });
        
    }
    };
};
namespace HeavenStudio.Games
{




    public class BonOdori : Minigame
    {
       string prefix;
       double beatUniversal;
        string suffix;
        SpriteRenderer darkPlane;
        bool goBopDonpans;
        bool goBopJudge;
        bool bopDonpans;
        int clapTypeGlobal = 0;
        string clapTypeString = "ClapSide";
        string originalText1;
        string originalText2;
        string originalText3;
        string originalText4;
        string originalText5;
        Coroutine Scroll1;
        Coroutine Scroll2;
        Coroutine Scroll3;
        Coroutine Scroll4;
        Coroutine Scroll5;
        Coroutine DarkerBG;
        bool darkBgIsOn = false;
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
        [SerializeField] Animator Player;
        [SerializeField] Animator Judge;
        [SerializeField] GameObject DarkPlane;

        [SerializeField] Animator CPU1;
        [SerializeField] Animator CPU2;
        [SerializeField] Animator CPU3;
        [SerializeField] Animator Face;
        public enum typeClap
        {
            SideClap = 0,
            FrontClap = 1
        }
        public enum typePan
        {
            Pan = 0,
            Pa = 1,
            Pa_n = 2
        }
        public enum typeDon
        {
            Don = 0,
            Do = 1,
            Do_n = 2
        }

        public enum variationPan
        {
            PanC = 0,
            PanE = 1,
            PanA = 2
        }
        public enum variationPa_n
        {
            Pa_nA = 0,
            Pa_nC = 1
        }
        public enum variationPa
        {   
            PaG = 0

        }
        public enum variationDon
        {
            DonA = 0,
            DonD = 1,
            DonC = 2, 
            DonG = 3
        }
        public enum variationDo_n
        {
            Do_nA = 0,
            Do_nG = 1
        }
        public enum variationDo
        {
            DoC = 0,
            DoG = 1
        }
        public static BonOdori instance { get; set; }
        public void Awake()

        {
            darkPlane = DarkPlane.GetComponent<SpriteRenderer>();





            
            clapTypeGlobal = 0;
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
        public void OnStop()
        {
            DarkPlane.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f); 
        
        }
        public void Update()
        {
            Conductor con = new Conductor();
            if (!con.NotStopped())
            {
                Text1.text = "";
                Text2.text = "";
                Text3.text = "";
                Text4.text = "";
                Text6.text = "";
                Text7.text = "";
                Text8.text = "";
                Text9.text = "";
                Text10.text = "";



                StopCoroutine(Scroll1);
                StopCoroutine(Scroll2);
                StopCoroutine(Scroll3);
                StopCoroutine(Scroll4);
                StopCoroutine(Scroll5);
                StopCoroutine(DarkerBG);
                Text6.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                Text7.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                Text8.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                Text9.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                Text10.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));



            }
    

            
            if (PlayerInput.GetIsAction(BonOdori.InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)){
                ScoreMiss();
                SoundByte.PlayOneShotGame("bonOdori/clap");
                if (clapTypeGlobal == 0)
                { 
                    clapTypeString = "ClapSide";
                }
                else
                {
                    clapTypeString = "ClapFront";
                }
                    
                    Player.Play(clapTypeString);
                    if (!goBopDonpans)
                    {

                    
                    BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beatUniversal + 1d, delegate { Player.Play("NeutralClapped"); CPU1.Play("NeutralClapped"); CPU2.Play("NeutralClapped"); CPU3.Play("NeutralClapped"); goBopDonpans = true;})
                });
                }
                
                
  
            
            
        }

        }

            
        


        public void Clap(double beat, int variation, int typeSpeak, bool muted, int clapType)

        {  
            if (clapType == 1)
            {
                clapTypeGlobal = 1;
            }
            else
            {
                clapTypeGlobal = 0;
            }
            if (muted)
            {
                ScheduleInput(beat, 0f, InputAction_BasicPress, Success, Miss, Empty);
            } 
            else
            {

            
            
            
                switch (typeSpeak){
                            case 0:
                           
                            switch (variation){
                                case 0:
                                SoundByte.PlayOneShotGame("bonOdori/pan1");
                                break;
                                case 1:
                                SoundByte.PlayOneShotGame("bonOdori/pan2");                               break;
                                case 2:
                                SoundByte.PlayOneShotGame("bonOdori/pan3");
                                break;}
                                break;
                            case 2:

                            switch (variation){
                                case 0:
                              SoundByte.PlayOneShotGame("bonOdori/pa_n1");
                                break;
                                case 1:
                              SoundByte.PlayOneShotGame("bonOdori/pa_n2");
                                break;}
                                break;
                            case 1:
                              SoundByte.PlayOneShotGame("bonOdori/pa1");
                                break;

                            
 
            
        }
                        beatUniversal = beat;
                ScheduleInput(beat, 0f, InputAction_BasicPress, Success, Miss, Empty);}
            }
            public void Sound(double beat, int variation, int typeSpeak )
        {  switch (typeSpeak){
                            case 0:
                            switch (variation){
                case 0:
                              SoundByte.PlayOneShotGame("bonOdori/don1");
                break;
                case 1:
                              SoundByte.PlayOneShotGame("bonOdori/don2");
                break;
                case 2:
                              SoundByte.PlayOneShotGame("bonOdori/don3");
                break;   
                case 3:
                              SoundByte.PlayOneShotGame("bonOdori/don4");
                break;                                  
                            }
                            
                            
                            break;
                            case 2:

            switch (variation) {
                case 0:
                              SoundByte.PlayOneShotGame("bonOdori/do_n1");
                break;
                case 1:
                              SoundByte.PlayOneShotGame("bonOdori/do_n2");
                break;
            }
            break;
                            
                            case 1:
                            switch (variation){
                                case 0:
                              SoundByte.PlayOneShotGame("bonOdori/do1");
                break;
                            case 1:
                              SoundByte.PlayOneShotGame("bonOdori/do2");
                break;
                            }
                            
              
                            break;
            }
        }  

        public void Success(PlayerActionEvent caller, float state)
        {
            if (clapTypeGlobal == 0)
                { 
                        clapTypeString = "ClapSide";
                }
                else
                {
                        clapTypeString = "ClapFront";
                }
                    
                    Player.Play(clapTypeString);
                    CPU1.Play(clapTypeString);
                    CPU2.Play(clapTypeString);
                    CPU3.Play(clapTypeString);
                    if (!goBopDonpans)
                    {

                    
                    BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beatUniversal + 1d, delegate { Player.Play("NeutralClapped"); CPU1.Play("NeutralClapped"); CPU2.Play("NeutralClapped"); CPU3.Play("NeutralClapped");}),
                });
                }
SoundByte.PlayOneShotGame("bonOdori/clap")
        }
        
        public void Miss(PlayerActionEvent caller)
        {
                    CPU1.Play(clapTypeString);
                    CPU2.Play(clapTypeString);
                    CPU3.Play(clapTypeString);
                        SoundByte.PlayOneShot("miss");
                         BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beatUniversal + 1d, delegate { Face.Play("Sad");}),
                    new BeatAction.Action(beatUniversal + 3d, delegate {Face.Play("Neutral");})
                });
                        
        }
        
        
        public void Empty(PlayerActionEvent caller)
        {
            if (clapTypeGlobal == 0)
                { 
                    clapTypeString = "ClapSide";
                }
                else
                {
                    clapTypeString = "ClapFront";
                }
                    
                    Player.Play(clapTypeString);
                    CPU1.Play(clapTypeString);
                    CPU2.Play(clapTypeString);
                    CPU3.Play(clapTypeString);
                    if (!goBopDonpans)
                    {

                    
                    BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beatUniversal + 1d, delegate { Player.Play("NeutralClapped"); CPU1.Play("NeutralClapped"); CPU2.Play("NeutralClapped"); CPU3.Play("NeutralClapped");}),
                });
                }
                 
                        SoundByte.PlayOneShot("nearMiss");


        }
    string ChangeColor(string text, bool isScroll)
    {
            if (text.Contains("r|") | text.Contains("y|") | text.Contains("g|")){
            if (!isScroll){

            
            return text.Replace("r|", "<color=#ff0000>")
                   .Replace("g|", "<color=#00ff00>")
                   .Replace("y|", "<color=#ffff00>")
                   + "</color>";
            }
            else
            {
                            return text.Replace("r|", "<color=#ff00ff>")
                   .Replace("g|", "<color=#00ffff>")
                   .Replace("y|", "<color=#ffffff>")
                   + "</color>";

            }}
            return text;

    }

        public void ShowText(string text1, string text2, string text3, string text4, string text5)
        {
            
            if (text1 is not "" && text1 is not "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line."){
                if (Scroll1 is not null)
                {
                    StopCoroutine(Scroll1);
                    Scroll1 = null;
                }
                Text6.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                originalText1 = text1;
                text1 = ChangeColor(text1, false);

                Text1.text = text1;
                
 
                Text6.text = ChangeColor(originalText1, true);
  
                }
            if (text2 is not ""){
                if (Scroll2 is not null)
                {
                    StopCoroutine(Scroll2);
                    Scroll2 = null;
                }
                Text7.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                originalText2 = text2;
                text2 = ChangeColor(text2, false);
                Text2.text = text2;
                Text7.text = ChangeColor(originalText2, true);
   
                }
            if (text3 is not ""){
                if (Scroll3 is not null)
                {
                    StopCoroutine(Scroll3);
                    Scroll3 = null;
                }
                originalText3 = text3;
                Text8.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                
                text3 = ChangeColor(text3, false);
                
                Text3.text = text3;
                Text8.text = ChangeColor(originalText3, true);
 
                }
            if (text4 is not ""){
                if (Scroll4 is not null)
                {
                    StopCoroutine(Scroll4);
                    Scroll4 = null;
                }
                Text9.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                originalText4 = text4;
                text4 = ChangeColor(text4, false);
                Text4.text = text4;

                Text9.text = text4; 
                Text9.text = ChangeColor(originalText4, true);

                }
            if (text5 is not ""){
                if (Scroll5 is not null)
                {
                    StopCoroutine(Scroll5);
                    Scroll5 = null;
                }
                Text10.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                originalText5 = text5;
                text5 = ChangeColor(text5, false);
                Text5.text = text5;
                Text10.text = ChangeColor(originalText5, true);

                }
            

        }
        public void DeleteText(bool text1, bool text2, bool text3, bool text4, bool text5){
            if (text1 == true){
                if (Scroll1 is not null)
                {
                    StopCoroutine(Scroll1);
                    Scroll1 = null;
                }
                Text6.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                Text1.text = "";
                Text6.text = "";
            }
            if (text2 == true){
                if (Scroll2 is not null)
                {
                    StopCoroutine(Scroll2);
                    Scroll2 = null;
                }
                Text7.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                Text2.text = "";
                Text7.text = "";
            }
            if (text3 == true){
                if (Scroll3 is not null)
                {
                    StopCoroutine(Scroll3);
                    Scroll3 = null;
                }
                Text8.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                Text3.text = "";
                Text8.text = "";
            }
            if (text4 == true){
                if (Scroll4 is not null)
                {
                    StopCoroutine(Scroll4);
                    Scroll4 = null;
                }
                Text9.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                Text4.text = "";
                Text9.text = "";
            }
            if (text5 == true){
                if (Scroll5 is not null)
                {
                    StopCoroutine(Scroll5);
                    Scroll5 = null;
                }
                Text10.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                Text5.text = "";
                Text10.text = "";
            }


        }
 IEnumerator SmoothText(TMP_Text text, float length, double beat)
    {
        Conductor conductor = new Conductor();
        float startTime = Time.time;
        float endTime = startTime + length;
        float duration = ((length / conductor.GetBpmAtBeat(beat)) * 60);

        while (Time.time < endTime)
        {
            float t = ((Time.time - startTime) / duration);
        
            float maskValue = Mathf.Lerp(-10f, -7f, t);

            text.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, maskValue, 10));

            yield return null;
            
        }



    }
    public void ScrollText(bool text1, bool text2, bool text3, bool text4, bool text5, float length, double beat)
    {
        if (text1){
            Scroll1 = StartCoroutine(SmoothText(Text6, length, beat));}
        if (text2){
            Scroll2 = StartCoroutine(SmoothText(Text7, length, beat));}
        if (text3){
            Scroll3 = StartCoroutine(SmoothText(Text8, length, beat));}
        if (text4){
            Scroll4 = StartCoroutine(SmoothText(Text9, length, beat));}
        if (text5){
            Scroll5 = StartCoroutine(SmoothText(Text10, length, beat));}
            
    }

    public void Bop(double beat, float length, bool shouldBop, bool autoBop)
        {
            if (!shouldBop) { goBopDonpans = false; goBopJudge = false; return; }
            goBopDonpans = autoBop;
            goBopJudge = autoBop;
            if (autoBop) { return;}
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            Player.Play("Bop");
                            CPU1.Play("Bop");
                            CPU2.Play("Bop");
                            CPU3.Play("Bop");
                            Judge.Play("Bop");

                        }),
                        new BeatAction.Action(beat + length, delegate
                        {
                            Player.Play("NeutralBopped");
                            CPU1.Play("NeutralBopped");
                            CPU2.Play("NeutralBopped");
                            CPU3.Play("NeutralBopped");


                        })
                    });
                }
            

        }

        }
    public void Bow(double beat, float length)
    {
        if (goBopDonpans == true)
        {
            bopDonpans = true;
        }
        else
        {
            bopDonpans = false;
            
        }
        goBopDonpans = false;
        Player.Play("Bow");
        CPU1.Play("Bow");
        CPU2.Play("Bow");
        CPU3.Play("Bow");
                        BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate { Player.Play("NeutralBopped"); CPU1.Play("NeutralBopped");CPU2.Play("NeutralBopped"); CPU3.Play("NeutralBopped"); if (bopDonpans) {goBopDonpans = true;}})
                });
    }
    // public void Spin(double beat, float length)
    // {

    // }
            
        public override void OnBeatPulse(double beat)
        {
            if (goBopDonpans)
            {
                Player.Play("Bop");
                CPU1.Play("Bop");
                CPU2.Play("Bop");
                CPU3.Play("Bop");

            }
            if (goBopJudge)
            {
                Judge.Play("Bop");
            }
        }
public void DarkBG(double beat, bool toggle, float length)
{
    DarkerBG = StartCoroutine(DarkBGCoroutine(beat, toggle, length));

}
IEnumerator DarkBGCoroutine(double beat, bool toggle, float length)
{
    if (toggle)
    {   
        if (darkBgIsOn)
        {
            yield return null;
        }
        else
        {

        
        float startTime = Time.time;
        Conductor con = new Conductor();
        float realLength = length / con.GetBpmAtBeat(beat) * 60;
        while (Time.time < realLength + startTime)
        {


    
    
        darkPlane.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.4666f, (Time.time - startTime) / realLength)); 
        darkBgIsOn = true;
        yield return null;



    }}}
    else
    {
        if (!darkBgIsOn)
        {
            yield return null;
        }
        else
        {

        
        float startTime = Time.time;
        Conductor con = new Conductor();
        float realLength = length / con.GetBpmAtBeat(beat) * 60;
        while (Time.time < realLength + startTime)
        {


    
    
        darkPlane.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.4666f,0f, (Time.time - startTime) / realLength)); 

        darkBgIsOn = true;
        yield return null;


    }

}
        


    }
}}}