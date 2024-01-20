using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpritesheetScaler : EditorWindow
{

    Object source;
    float multiplier = 1;
    int inflateX = 0;
    int inflateY = 0;

    // Creates a new option in "Windows"
    [MenuItem("Window/Scale spritesheet pivots and slices")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SpritesheetScaler window = (SpritesheetScaler)EditorWindow.GetWindow(typeof(SpritesheetScaler));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source texture:", EditorStyles.boldLabel);
        source = EditorGUILayout.ObjectField(source, typeof(Texture2D), false, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Multiplier:", EditorStyles.boldLabel);
        multiplier = EditorGUILayout.FloatField(multiplier, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Inflate Quads:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("X", EditorStyles.label);
        inflateX = EditorGUILayout.IntField(inflateX, GUILayout.Width(220/2));
        GUILayout.Label("Y", EditorStyles.label);
        inflateY = EditorGUILayout.IntField(inflateY, GUILayout.Width(220/2));
        GUILayout.EndHorizontal();

        GUILayout.Space(25f);
        if (GUILayout.Button("Scale pivots and slices"))
        {
            ScalePivotsAndSlices();
        }
    }

    void ScalePivotsAndSlices()
    {
        if (!source || (multiplier <= 0))
        {
            Debug.Log("Missing one object");
            return;
        }

        if (source.GetType() != typeof(Texture2D))
        {
            Debug.Log(source + "needs to be Texture2D!");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(source);
        TextureImporter ti1 = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        bool wasReadable = ti1.isReadable;
        ti1.isReadable = true;

        ti1.spritePixelsPerUnit = (ti1.spritePixelsPerUnit * multiplier);

        List<SpriteMetaData> newData = new List<SpriteMetaData>();

        Debug.Log("Amount of slices found: " + ti1.spritesheet.Length);

        for (int i = 0; i < ti1.spritesheet.Length; i++)
        {
            SpriteMetaData d = ti1.spritesheet[i];
            // pivot relative to the origin of the rect in pixels
            Vector2 oldPivot = new Vector2(d.pivot.x * d.rect.width, d.pivot.y * d.rect.height);
            d.rect = ScaleRect(d.rect, multiplier, inflateX, inflateY);

            d.border.x += d.border.x > 0 ? inflateX : 0;
            d.border.y += d.border.y > 0 ? inflateY : 0;
            d.border.z += d.border.z > 0 ? inflateX : 0;
            d.border.w += d.border.w > 0 ? inflateY : 0;

            if (d.alignment != (int)SpriteAlignment.Center && (inflateX > 0 || inflateY > 0))
            {
                d.alignment = (int)SpriteAlignment.Custom;
                oldPivot += new Vector2(inflateX, inflateY);
                Vector2 newPivot = oldPivot * multiplier;
                d.pivot = new Vector2(newPivot.x / d.rect.width, newPivot.y / d.rect.height);
            }

            d.border *= multiplier;
            newData.Add(d);
        }

        ti1.spritesheet = newData.ToArray();

        ti1.isReadable = wasReadable;

        AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceUpdate);
    }

    Rect ScaleRect(Rect source, float mult, int inflateX, int inflateY)
    {
        var newRect = new Rect();
        newRect.Set((source.x - inflateX) * mult, (source.y - inflateY) * mult, (source.width + inflateX*2) * mult, (source.height + inflateY*2) * mult);
        return newRect;
    }
}
