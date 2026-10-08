using System;
using System.Collections.Generic;
using System.Linq;
using Chang.UI;
using Zenject;

namespace Chang
{
    public class PlayResultController : IViewController
    {
        private const string TranslationIndent = "  ";

        private PlayResultView _view;

        [Inject]
        public PlayResultController(PlayResultView view)
        {
            _view = view;
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(List<Chang.FSM.ResultItem> lessonLog, Action onContinueClick)
        {
            _view.Init(onContinueClick);
            SetTotals(lessonLog);
            
            foreach (var item in lessonLog)
            {
                _view.AddItem(
                    $"{item.Presentation}\n<size=75%>{TranslationIndent}{item.Translation}</size>",
                    item.Mark.ToString(),
                    item.IsCorrect);
            }
        }

        /// <summary>
        /// Words: played word keys. Mastery: mean mark of the played keys after the lesson. Accuracy: correct answers of all answers
        /// </summary>
        private void SetTotals(List<Chang.FSM.ResultItem> lessonLog)
        {
            if (lessonLog.Count == 0)
            {
                _view.SetTotals(0, 0, 0);
                return;
            }

            // the last item of a key holds its mark after the lesson
            List<Chang.FSM.ResultItem> lastByKey = lessonLog
                .GroupBy(item => (item.Key, item.IsSentence))
                .Select(group => group.Last())
                .ToList();

            int words = lastByKey.Count(item => !item.IsSentence);
            float mastery = lastByKey.Average(item => (float)item.Mark) / ProjectConstants.MARK_MAX;
            float accuracy = (float)lessonLog.Count(item => item.IsCorrect) / lessonLog.Count;

            _view.SetTotals(words, UnityEngine.Mathf.RoundToInt(mastery * 100), UnityEngine.Mathf.RoundToInt(accuracy * 100));
        }
    }
}