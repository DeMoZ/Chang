using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class SelectWordView : CScreen
    {
        [SerializeField] private Image _questionImage;
        [SerializeField] private ChangText _questionWord;
        [SerializeField] private ChangText _mixWordPrefab;
        [SerializeField] private Transform _mixWordContent;

        [field: SerializeField] public override QuestionType ScreenType { get; } = QuestionType.SelectWord;

        // todo roman here should be a classes, not a configx
        // todo roman refactoring is required
        public void Init(PhraseConfig correctWord, List<PhraseConfig> mixWords)
        {
            Debug.Log("Init SelectWordView");

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            // init thai word
            _questionWord.Set(correctWord.Word.Word, correctWord.Word.Phonetic);
            // init mix words
            foreach (PhraseConfig conf in mixWords)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                mix.Set(conf.Word.Word, conf.Word.Phonetic);
            }
        }
    }
}