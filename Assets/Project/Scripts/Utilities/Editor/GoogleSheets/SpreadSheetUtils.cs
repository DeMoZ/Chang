using System;
using System.Collections.Generic;
using DMZ.DebugSystem;

namespace Chang.Utilities.GoogleSheets
{
    public class SpreadSheetUtils
    {
        public static string SafeGetValue(IList<object> collection, int index, bool notify = false)
        {
            if (collection == null || collection.Count <= index)
            {
                if (notify)
                {
                    DMZLogger.LogWarning($"Index [{index}] is out of range for the collection. Collection size is {collection.Count}.");
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
                    DMZLogger.LogWarning($"Indexes [{index1},{index2}] is not valid for collection.\n{e}");
                }

                return string.Empty;
            }
        }
    }
}