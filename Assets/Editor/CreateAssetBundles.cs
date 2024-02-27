using System.IO;
using UnityEditor;
using UnityEngine;

using SatorImaging.UnitySourceGenerator.Editor;
using HeavenStudio;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles/Current Platform")]
    static void BuildAllAssetBundlesCurrPlatform()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Assets/Build AssetBundles/Windows")]
    static void BuildAllAssetBundlesWindows()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Windows";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Build AssetBundles/Linux")]
    static void BuildAllAssetBundlesLinux()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Linux";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);
    }

    [MenuItem("Assets/Build AssetBundles/Mac")]
    static void BuildAllAssetBundlesMacOS()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Mac";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX);
    }
}