using System;
using Cysharp.Threading.Tasks;
using Chang;
using Chang.Resources;
using DMZ.FSM;
using Debug = DMZ.DebugSystem.DMZLogger;
using System.Collections.Generic;

namespace Chang.FSM
{
    public class SelectWordState : ResultStateBase<QuestionType, VocabularyBus>
    {
        private SelectWordController _selectWordController;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
            _selectWordController = Bus.ScreenManager.SelectWordController;
        }

        public override void Enter()
        {
            base.Enter();

            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            _selectWordController.SetViewActive(false);
        }

        public async UniTaskVoid StateBodyAsync()
        {
            // 1 instantiate screen and initialise with data.
            if (Bus.CurrentQuestion.QuestionType != Type)
                throw new ArgumentException("Question type doesnt match with state type");

            var questionData = (QuestSelectWord)Bus.CurrentQuestion.QuestionData;
            var correctWord = questionData.CorrectWord;
            var mixWolrds = TempPopulateMixWords(); // todo roman this is very temp solution
            _selectWordController.Init(correctWord, mixWolrds, OnContinue);

            _selectWordController.SetViewActive(true);
            // 2 await for input

            // 3 await for press "Continue"

            // 4 OnStateResult.Invoke(success/not success);

            // i dont need preloader because all the data will be loaded from cash
        }

        private void OnContinue(QuestionTypeStateResult result)
        {

        }

        private async UniTask LoadQuestionContentAsync()
        {
            // Bus.ClickedLessonConfig = await _resourcesManager.LoadAssetAsync<LessonConfig>(Bus.ClickedLesson);
            // await UniTask.Delay(3000); // todo roman temp test
        }

        /// <summary>
        /// todo roman this is very temp solution
        /// </summary>
        private List<PhraseConfig> TempPopulateMixWords()
        {
            var rezult = new List<PhraseConfig>();
            foreach (Question q in Bus.Questions)
            {
                if (q == Bus.CurrentQuestion) continue;

                var qType = q.QuestionType;

                switch (qType)
                {
                    case QuestionType.DemonstrationWord:
                        rezult.Add(((QuestDemonstration)q.QuestionData).PhraseConfig);
                        break;
                    case QuestionType.SelectWord:
                        rezult.Add(((QuestSelectWord)q.QuestionData).CorrectWord);
                        break;
                    case QuestionType.MatchWords:
                        // skip
                        break;
                    case QuestionType.DemonstrationDialogue:
                        // skip
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(qType));
                }

                if (rezult.Count >= 6) break;
            }

            return rezult;
        }
    }
}

// todo roman put this question result into bus
public class QuestionTypeStateResult
{

}