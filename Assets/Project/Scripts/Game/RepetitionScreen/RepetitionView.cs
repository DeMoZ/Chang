using UnityEngine;

namespace Chang
{
    public class RepetitionView : MonoBehaviour
    {
        [SerializeField] private OverviewItem questions;
        [SerializeField] private OverviewItem words;
        [SerializeField] private OverviewItem sentences;
        
        [Space]
        [SerializeField] private OverviewLogItem overviewLogItemPrefab;
    }
}