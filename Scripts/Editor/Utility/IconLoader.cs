using UnityEditor;
using UnityEngine;

public static class IconLoader
{
    public static Texture2D LoadIcon(string iconName)
    {
        Texture2D icon = LoadIconFromPackage(iconName);

        if (icon == null)
            icon = LoadIconFromAssets(iconName);

        return icon;
    }

    private static Texture2D LoadIconFromPackage(string iconName)
    {
        string packagePath = $"Packages/com.brunomikoski.animationsequencer/Scripts/Editor/Icons/{iconName}.png";
        Texture2D icon = (Texture2D)EditorGUIUtility.Load(packagePath);

        return icon;
    }

    private static Texture2D LoadIconFromAssets(string iconName)
    {
        string[] assetGuids = AssetDatabase.FindAssets(iconName);

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (fileName == iconName)
            {
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                if (icon != null)
                    return icon;
            }
        }

        return null;
    }
}
