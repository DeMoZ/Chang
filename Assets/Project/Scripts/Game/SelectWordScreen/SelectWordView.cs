using System;
using System.Collections.Generic;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class SelectWordView : CScreen
    {
        public override ScreenType ScreenType { get; } = ScreenType.SelectWord;

        // todo roman here should be a classes, not a configx
        // todo roman refactoring is required
        public void Init(PhraseConfig correctWord, List<PhraseConfig> mixWords)
        {
            Debug.Log("Init SelectWordView");
        }
    }
}