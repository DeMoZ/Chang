namespace Chang
{
    public static class WordHelper
    {
        public static bool GetQuestInTranslation(int mark)
        {
            return mark switch
            {
                < 3 => false,
                < 5 => true,
                _ => RandomUtils.GetRandomBool()
            };
        }

        public static bool GetShowPhonetics(int mark)
        {
            return mark < 7;
        }
    }
}