using System.Collections.Generic;
using UnityEngine;

namespace Chang.Core
{
    public class Vocabulary : ScriptableObject
    {
        public Languages Language;
        public List<Word> Words;
    }
}