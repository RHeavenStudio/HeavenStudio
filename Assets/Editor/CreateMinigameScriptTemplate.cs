using UnityEngine;
using UnityEditor;
using SatorImaging.UnitySourceGenerator.Editor;

public class CreateMinigameScriptTemplate
{
    [MenuItem("Assets/Heaven Studio/Create Minigame Script From Template", priority = 0)]
    public static void CreateMinigameScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Editor/ScriptTemplates/MinigameScriptTemplate.txt", "NewMinigame.cs");
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(HeavenStudio.Minigames));
    }
}
