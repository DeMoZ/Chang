using System;
using System.Collections.Generic;
using UnityEditor;
using Debug =  DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    public class SpreadSheetUtilities
    {
        public static string SafeGetValue(IList<object> collection, int index, bool notify = false)
        {
            if (collection == null || collection.Count <= index)
            {
                if (notify)
                {
                    Debug.LogWarning($"Index [{index}] is out of range for the collection. Collection size is {collection.Count}.");
                }

                return string.Empty;
            }

            var value = collection[index];
            return value?.ToString() ?? string.Empty;
        }

        public static string SafeGetValue(IList<IList<object>> collection, int index1, int index2, bool notify = false)
        {
            try
            {
                return collection[index1][index2].ToString();
            }
            catch (Exception e)
            {
                if (notify)
                {
                    Debug.LogWarning($"Indexes [{index1},{index2}] is not valid for collection.\n{e}");
                }

                return string.Empty;
            }
        }
        
        public static void CreateFoldersRecursively(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parentPath = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                CreateFoldersRecursively(parentPath);
            }

            string folderName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}