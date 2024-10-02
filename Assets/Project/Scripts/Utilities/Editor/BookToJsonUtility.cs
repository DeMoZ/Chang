using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BookToJsonUtility", menuName = "Chang/GameBook/BookToJsonUtility", order = 0)]
public class BookToJsonUtility : ScriptableObject
{
    public GameBookConfig GameBookConfig;
    public TextAsset GameBookJson;

    [Button]
    void MakeJson()
    {
        Debug.Log($"{nameof(MakeJson)} Start");
        var result = new List<LessonName>();

        foreach (var lesson in GameBookConfig.Lessons)
        {
            var assetPath = AssetDatabase.GetAssetPath(lesson);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);

            result.Add(new LessonName()
            {
                FileName = fileName,
                AssetPath = assetPath,
            });
        }

        var json = JsonConvert.SerializeObject(result, Formatting.Indented);
        File.WriteAllText(AssetDatabase.GetAssetPath(GameBookJson), json);

        AssetDatabase.Refresh();
        Debug.Log($"{nameof(MakeJson)} Done");
    }
}