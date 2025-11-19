using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chang.Vocabulary
{
    public class VocabularyBookConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var questionType = (QuestionType)jsonObject["QuestionType"].Value<int>();

            IQuestion question;
            switch (questionType)
            {
                case QuestionType.SelectWord:
                    question = new QuestSelectWord();
                    break;
                case QuestionType.MatchWords:
                    question = new QuestMatchWords();
                    break;
                default:
                    throw new NotImplementedException($"Converter for {questionType} is not implemented");
            }

            serializer.Populate(jsonObject.CreateReader(), question);
            return question;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IQuestion).IsAssignableFrom(objectType);
        }
    }
}