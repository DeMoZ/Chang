using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Utilities.GoogleSheets
{
    public class RootSheetsToConfig : ScriptableObject
    {
        public Languages Language = Languages.Thai;

        /// <summary>
        /// Reads Google book from Google Sheet and creates Config files for each sheet.
        ///</summary>
        [Button, Tooltip("Reads Google book from Google Sheet and creates Config files for each sheet.")]
        public void CreateVocabularyConfig()
        {
            ProcessAll(ProcessSheetsToVocabulary, true);
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates Config files for each sheet.
        ///</summary>
        [Button, Tooltip("Reads Google book from Google Sheet and creates Config files for each sheet.")]
        public void CreateVocabularyBookConfig()
        {
            ProcessAll(ProcessSheetsToVocabularyBook, true);
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates Config files for each sheet.
        ///</summary>
        [Button, Tooltip("Reads Google book from Google Sheet and creates Config files for each sheet.")]
        public void CreateSentencesConfig()
        {
            ProcessAll(ProcessSheetsToSentences, true);
        }

        /// <summary>
        /// Reads Google book from Google Sheet and creates Config files for each sheet.
        ///</summary>
        [Button, Tooltip("Reads Google book from Google Sheet and creates Config files for each sheet.")]
        public void CreateSentencesBookConfig()
        {
            ProcessAll(ProcessSheetsToSentencesBook, true);
        }

        private void ProcessAll(Action<Action<(string title, string info, float progress)>> action,
            bool withProgressBar)
        {
            try
            {
                action.Invoke(step =>
                {
                    if (withProgressBar)
                    {
                        EditorUtility.DisplayProgressBar(step.title, step.info, step.progress);
                    }
                    else
                    {
                        Debug.Log($"Processing. title: {step.title}, info: {step.info}, progress: {step.progress}...");
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            if (withProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ProcessSheetsToVocabulary(Action<(string title, string info, float progress)> onStep)
        {
            string process = $"{nameof(SheetsToVocabulary)}";
            Debug.Log($"Started {process}");

            onStep?.Invoke((process, process, 1));
            SheetsToVocabulary.ReadAsync(Language).GetAwaiter().GetResult();

            AssetDatabase.SaveAssets();
            Debug.Log($"Finished {process}.");
        }
        
        private void ProcessSheetsToVocabularyBook(Action<(string title, string info, float progress)> onStep)
        {
            string process = $"{nameof(ProcessSheetsToVocabularyBook)}";
            Debug.Log($"Started {process}");

            onStep?.Invoke((process, process, 1));
            SheetsToVocabularyBook.ReadAsync(Language).GetAwaiter().GetResult();

            AssetDatabase.SaveAssets();
            Debug.Log($"Finished {process}.");
        }

        private void ProcessSheetsToSentences(Action<(string title, string info, float progress)> onStep)
        {
            string process = $"{nameof(ProcessSheetsToSentences)}";
            Debug.Log($"Started {process}");

            onStep?.Invoke((process, process, 1));
            SheetsToSentences.ReadAsync(Language).GetAwaiter().GetResult();

            AssetDatabase.SaveAssets();
            Debug.Log($"Finished {process}.");
        }
        private void ProcessSheetsToSentencesBook(Action<(string title, string info, float progress)> onStep)
        {
            string process = $"{nameof(ProcessSheetsToSentencesBook)}";
            Debug.Log($"Started {process}");

            onStep?.Invoke((process, process, 1));
            SheetsToSentencesBook.ReadAsync(Language).GetAwaiter().GetResult();

            AssetDatabase.SaveAssets();
            Debug.Log($"Finished {process}.");
        }
    }
}