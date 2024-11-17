using TMPro;
using UnityEngine;

namespace Chang
{
    public class OverviewLogItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Text mark;

        [Tooltip("How many times the word was shown")]
        [SerializeField] private TMP_Text totalShown;

        [Tooltip("Last time the word was shown")]
        [SerializeField] private TMP_Text date;

        // todo roman implement this logic
        [Tooltip("Repetition iteration number. The bigger the number, the less show priority. Will reset to 0 after each wront answer")]
        [SerializeField] private TMP_Text timeStep;

        public void Set(string text,
        string mark,
        string totalShown,
        string date,
        string timeStep)
        {
            this.text.text = text;
            this.mark.text = mark;
            this.totalShown.text = totalShown;
            this.date.text = date;
            this.timeStep.text = timeStep;
        }
    }
}