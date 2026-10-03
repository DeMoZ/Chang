using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization.Scripts;

namespace Chang.Utilities.Localization
{
    /// <summary>
    /// Sheet csv parsed the same way as LocalizationManager reads it at runtime.
    /// </summary>
    public class LocalizationSheetData
    {
        public readonly List<string> Languages = new();
        public readonly List<LocalizationEntry> Entries = new();
        public readonly List<string> Errors = new();
        public readonly LocalizationKeyNode Root;

        private LocalizationSheetData(char separator)
        {
            Root = new LocalizationKeyNode(string.Empty, string.Empty, separator);
        }

        public static LocalizationSheetData Parse(string csv, char separator)
        {
            var data = new LocalizationSheetData(separator);
            List<string> lines = LocalizationManager.GetLines(csv);

            if (lines.Count == 0)
            {
                data.Errors.Add("Csv is empty");
                return data;
            }

            data.Languages.AddRange(lines[0].Split(',').Select(i => i.Trim()).Skip(1));

            foreach (string duplicate in data.Languages.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key))
            {
                data.Errors.Add($"Duplicated language `{duplicate}`, the sheet is not loaded at runtime");
            }

            var keys = new HashSet<string>();

            for (var i = 1; i < lines.Count; i++)
            {
                List<string> columns = LocalizationManager.GetColumns(lines[i]);
                string key = columns[0];

                if (key == string.Empty)
                {
                    continue;
                }

                if (!keys.Add(key))
                {
                    data.Errors.Add($"Line {i + 1}: duplicated key `{key}`, it is not loaded at runtime");
                    continue;
                }

                if (columns.Count - 1 < data.Languages.Count)
                {
                    data.Errors.Add($"Line {i + 1}: key `{key}` has {columns.Count - 1} values for {data.Languages.Count} languages");
                }

                var entry = new LocalizationEntry(key, columns.Skip(1).ToArray());
                data.Entries.Add(entry);
                data.Root.Add(entry);
            }

            return data;
        }
    }

    public class LocalizationEntry
    {
        public readonly string Key;
        public readonly string[] Values;

        public LocalizationEntry(string key, string[] values)
        {
            Key = key;
            Values = values;
        }

        public string GetValue(int languageIndex)
        {
            return languageIndex < Values.Length ? Values[languageIndex] : null;
        }
    }

    /// <summary>
    /// A part of the key between separators. A node can be a key and a group at the same time ("A.B" and "A.B.C").
    /// </summary>
    public class LocalizationKeyNode
    {
        public readonly string Name;
        public readonly string Path;
        public readonly List<LocalizationKeyNode> Children = new();
        public LocalizationEntry Entry;
        public int KeysCount;

        private readonly char _separator;

        public LocalizationKeyNode(string name, string path, char separator)
        {
            Name = name;
            Path = path;
            _separator = separator;
        }

        public void Add(LocalizationEntry entry)
        {
            LocalizationKeyNode node = this;
            node.KeysCount++;

            foreach (string part in entry.Key.Split(_separator))
            {
                LocalizationKeyNode child = node.Children.Find(c => c.Name == part);

                if (child == null)
                {
                    string path = node.Path == string.Empty ? part : node.Path + _separator + part;
                    child = new LocalizationKeyNode(part, path, _separator);
                    node.Children.Add(child);
                }

                node = child;
                node.KeysCount++;
            }

            node.Entry = entry;
        }

        public IEnumerable<LocalizationEntry> GetEntries()
        {
            if (Entry != null)
            {
                yield return Entry;
            }

            foreach (LocalizationEntry entry in Children.SelectMany(child => child.GetEntries()))
            {
                yield return entry;
            }
        }
    }
}
