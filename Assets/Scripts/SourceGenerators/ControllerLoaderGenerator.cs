using static SatorImaging.UnitySourceGenerator.USGFullNameOf;
using SatorImaging.UnitySourceGenerator;
using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

using HeavenStudio;
using HeavenStudio.InputSystem;

// HOW TO USE: Add the following attribute to *target* class.
//             Note that target class must be defined as partial.
//[UnitySourceGenerator(typeof(ControllerLoaderGenerator), OverwriteIfFileExists = false)]
public partial class ControllerLoaderGenerator
{
#if UNITY_EDITOR   // USG: class definition is required to avoid build error but methods are not.
#pragma warning disable IDE0051

    readonly static string MEMBER_ACCESS = "public static";
    readonly static string MAIN_MEMBER_NAME = "InitInputControllers";
    static string OutputFileName() => MAIN_MEMBER_NAME + ".cs";  // -> Name.<TargetClass>.<GeneratorClass>.g.cs

    static bool Emit(USGContext context, StringBuilder sb)
    {
        List<PlayerInput.InputControllerInitializer> loadRunners = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(x => x.Namespace == "HeavenStudio.InputSystem.Loaders" && x.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static) != null)
        .Select(t => (PlayerInput.InputControllerInitializer)Delegate.CreateDelegate(
            typeof(PlayerInput.InputControllerInitializer),
            null,
            t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static),
            false
            ))
        .ToList();
        // sort by load order attribute (lower is first)
        loadRunners.Sort((x, y) => x.Method.GetCustomAttribute<LoadOrder>().Order.CompareTo(y.Method.GetCustomAttribute<LoadOrder>().Order));

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
using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;

using HeavenStudio.InputSystem;
using HeavenStudio.InputSystem.Loaders;
using Debug = UnityEngine.Debug;

namespace {context.TargetClass.Namespace}
{{
    partial class {context.TargetClass.Name}
    {{
");
        // class open ----------------------------------------------------------------------


        #region  // USG: MainMember
        sb.Append($@"
        {MEMBER_ACCESS} int {MAIN_MEMBER_NAME}()
        {{
");
        sb.IndentLevel(3);

        sb.Append($@"
            inputDevices = new List<InputController>();
            InputController[] controllers;
            PlayerInputRefresh = new();
            PlayerInputCleanUp = null;
");

        foreach (var loadRunner in loadRunners)
        {
            MethodInfo methodInfo = RuntimeReflectionExtensions.GetMethodInfo(loadRunner);
            string callingClass = methodInfo.DeclaringType.Name;
            string method = methodInfo.Name;
            string fullMethodLabel = $"{callingClass}.{method}";

            sb.Append($@"
            controllers = {fullMethodLabel}();
            if (controllers != null)
            {{
                inputDevices.AddRange(controllers);
            }}
            else
            {{
                Debug.Log(""{fullMethodLabel} had no controllers to initialize."");
            }}
");
        }

        sb.Append($@"
            return inputDevices.Count;
");

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
        return true;
    }

#pragma warning restore IDE0051
#endif
}
