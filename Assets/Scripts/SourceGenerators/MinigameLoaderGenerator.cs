using static SatorImaging.UnitySourceGenerator.USGFullNameOf;
using SatorImaging.UnitySourceGenerator;
using System.Text;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HeavenStudio;

// HOW TO USE: Add the following attribute to *target* class.
//             Note that target class must be defined as partial.
//[UnitySourceGenerator(typeof(MinigameLoaderGenerator), OverwriteIfFileExists = false)]
public partial class MinigameLoaderGenerator
{
#if UNITY_EDITOR   // USG: class definition is required to avoid build error but methods are not.
#pragma warning disable IDE0051

    readonly static string MEMBER_ACCESS = "public static";
    readonly static string MAIN_MEMBER_NAME = "LoadMinigames";
    static string OutputFileName() => MAIN_MEMBER_NAME + ".cs";  // -> Name.<TargetClass>.<GeneratorClass>.g.cs

    static bool Emit(USGContext context, StringBuilder sb)
    {
        List<Func<EventCaller, Minigames.Minigame>> loadRunners = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(x => x.Namespace == "HeavenStudio.Games.Loaders" && x.GetMethod("AddGame", BindingFlags.Public | BindingFlags.Static) != null)
        .Select(t => (Func<EventCaller, Minigames.Minigame>)Delegate.CreateDelegate(
            typeof(Func<EventCaller, Minigames.Minigame>),
            null,
            t.GetMethod("AddGame", BindingFlags.Public | BindingFlags.Static),
            false
            ))
        .ToList();

        // USG: static classes are IsAbstract is set.
        if (!context.TargetClass.IsClass)
            return false;  // return false to tell USG doesn't write file.

        // USG: you can modify output path. default file name is that USG generated.
        //      note that USG doesn't care the modified path is valid or not.
        //context.OutputPath += "_MyFirstTest.txt";

        // USG: EditorUtility.DisplayDialog() or others don't work in batch mode.
        //      throw if method depending on GUI based functions.
        //if (UnityEngine.Application.isBatchMode)
        //    throw new System.NotSupportedException("GUI based functions do nothing in batch mode.");

        // USG: write content into passed StringBuilder.
        sb.Append($@"
using Debug = UnityEngine.Debug;
using HeavenStudio.Games.Loaders;

namespace {context.TargetClass.Namespace}
{{
    static partial class {context.TargetClass.Name}
    {{
");
        // class open ----------------------------------------------------------------------


        #region  // USG: MainMember
        sb.Append($@"
        {MEMBER_ACCESS} void {MAIN_MEMBER_NAME}(EventCaller eventCaller)
        {{
");
        sb.IndentLevel(3);

        sb.Append($@"
            Minigames.Minigame game;
");

        foreach (var loadRunner in loadRunners)
        {
            MethodInfo methodInfo = RuntimeReflectionExtensions.GetMethodInfo(loadRunner);
            string callingClass = methodInfo.DeclaringType.Name;
            string method = methodInfo.Name;
            string fullMethodLabel = $"{callingClass}.{method}";

            sb.Append($@"
            game = {fullMethodLabel}(eventCaller);
            if (game != null)
            {{
                eventCaller.minigames.Add(game.name, game);
            }}
            else
            {{
                Debug.LogWarning(""Game loader {callingClass} failed!"");
            }}
");
        }

        // USG: semicolon?
        sb.Append($@"
        }}
");
        #endregion


        // class close ----------------------------------------------------------------------
        sb.Append($@"
    }}
}}
");

        // USG: return true to tell USG to write content into OutputPath. false to do nothing.
        // only write if changes are made.
        if (!System.IO.File.Exists(context.OutputPath))
            return true;

        string oldContent = System.IO.File.ReadAllText(context.OutputPath);
        return sb.ToString() != oldContent;
    }

#pragma warning restore IDE0051
#endif
}
