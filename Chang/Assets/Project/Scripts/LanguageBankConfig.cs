using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chang/LanguageBank Config", fileName = "WordConfig")]
public class LanguageBankConfig : ScriptableObject
{
    public  List<PhraseConfig> Key;
}