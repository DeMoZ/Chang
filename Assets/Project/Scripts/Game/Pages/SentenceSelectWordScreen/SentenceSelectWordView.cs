using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chang.UI
{
    public class SentenceSelectWordView : CScreen
    {
        public void ShowHint()
        {
            throw new System.NotImplementedException();
        }

        public void Init(bool isQuestInTranslation,
            List<PhraseData> sequence,
            Sprite sprite,
            List<PhraseData> mixWords,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
           
        }
    }
}