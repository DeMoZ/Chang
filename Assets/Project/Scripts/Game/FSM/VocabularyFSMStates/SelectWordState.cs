using System;
using Cysharp.Threading.Tasks;
using DMZ.FSM;
using System.Collections.Generic;
using Debug = DMZ.DebugSystem.DMZLogger;
using Zenject;

namespace Chang.FSM
{
    public class SelectWordResult : IQuestionResult
    {
        public QuestionType Type => QuestionType.SelectWord;
        public bool IsCorrect { get; }
        public List<string> Info { get; }

        public SelectWordResult(bool isCorrect, List<string> info)
        {
            IsCorrect = isCorrect;
            Info = info;
        }
    }

    public class SelectWordState : ResultStateBase<QuestionType, VocabularyBus>
    {
        [Inject] private readonly SelectWordController _selectWordController;
        [Inject] private readonly GameOverlayController _gameOverlayController;

        private List<PhraseConfig> _mixWords;
        private PhraseConfig _correctWord;

        public override QuestionType Type => QuestionType.SelectWord;

        public SelectWordState(VocabularyBus bus, Action<QuestionType> onStateResult) : base(bus, onStateResult)
        {
        }

        public override void Enter()
        {
            base.Enter();
            StateBodyAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();
            _selectWordController.SetViewActive(false);
        }

        public async UniTaskVoid StateBodyAsync()
        {
            // 1 instantiate screen and initialise with data.
            if (Bus.CurrentLesson.CurrentQuestion.QuestionType != Type)
                throw new ArgumentException("Question type doesnt match with state type");

            var questionData = (QuestSelectWord)Bus.CurrentLesson.CurrentQuestion;
            _correctWord = questionData.CorrectWord;
            _mixWords ??= new List<PhraseConfig>();
            _mixWords.Clear();
            _mixWords.Add(_correctWord);
            _mixWords.AddRange(questionData.MixWords); // TempPopulateMixWords(); // todo roman this is very temp solution
            Shuffle(_mixWords);

            var questInStudiedLanguage = false; // todo roman implement switch from thai to eng or from eng to thai
            _selectWordController.Init(questInStudiedLanguage, _correctWord, _mixWords, OnToggleValueChanged);

            _selectWordController.SetViewActive(true);
        }

        private void OnToggleValueChanged(int index, bool isOn)
        {
            Debug.Log($"toggle: {index}; isOn: {isOn}");
            var isCorrect = _mixWords[index] == _correctWord;
            var info = new List<string> { _correctWord.Word.Phonetic, _mixWords[index].Word.Phonetic };
            var result = new SelectWordResult(isCorrect, info);
            Bus.QuestionResult = result;
            _gameOverlayController.EnableCheckButton(isOn);
        }

        private void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// todo roman this is very temp solution
        /// </summary>
        private List<PhraseConfig> TempPopulateMixWords()
        {
            ///
            /*
                      var rezult = new List<PhraseConfig>();
                      rezult.Add(((QuestSelectWord)Bus.CurrentQuestion.QuestionData).CorrectWord);

                      // todo roman the flow needs to be from current word|lesson to previous
                     foreach (Question q in Bus.Questions)
                      {
                          /// if (q == Bus.CurrentQuestion) continue;

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

            return rezult;*/
            return null;
        }
    }
}