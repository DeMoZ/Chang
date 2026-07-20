using System;
using UnityEngine;

namespace Chang
{
    [Serializable]
    public class Translation
    {
        [field: SerializeField] public Languages Language { get; set; }
        [field: SerializeField] public string Meaning { get; set; }
    }
}