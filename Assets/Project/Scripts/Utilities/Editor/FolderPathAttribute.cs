using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a string field as a project folder path with a button that opens a folder picker.
/// </summary>
public class FolderPathAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(FolderPathAttribute))]
public class FolderPathDrawer : PropertyDrawer
{
    private const float ButtonWidth = 24;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        Rect fieldRect = new(position.x, position.y, position.width - ButtonWidth - 2, position.height);
        Rect buttonRect = new(fieldRect.xMax + 2, position.y, ButtonWidth, position.height);

        EditorGUI.PropertyField(fieldRect, property, label);

        if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Folder Icon")))
        {
            string folder = EditorUtility.OpenFolderPanel("Select folder", property.stringValue, string.Empty);
            if (!string.IsNullOrEmpty(folder))
            {
                property.stringValue = ToProjectPath(folder);
                property.serializedObject.ApplyModifiedProperties();
            }

            GUIUtility.ExitGUI();
        }
    }

    // Odin FolderPath stored paths relative to the project folder, e.g. Assets/Images
    private static string ToProjectPath(string folder)
    {
        string projectPath = Application.dataPath[..^"Assets".Length];
        return folder.StartsWith(projectPath) ? folder[projectPath.Length..] : folder;
    }
}
