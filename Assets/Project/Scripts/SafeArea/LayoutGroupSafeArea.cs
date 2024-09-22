using UnityEngine;
using UnityEngine.UI;

namespace SafeArea
{
	public abstract class LayoutGroupSafeArea : MonoBehaviour
	{
		[Header("Choose what modify")]
		[SerializeField] private bool hasModifyTopPadding;
		[SerializeField] private bool hasModifyBottomPadding;
		
		private void Awake()
		{
			var safeArea = Screen.safeArea;
			if (safeArea.size.y < Screen.height)
			{
				var gpoup = GetComponent<LayoutGroup>();

				if (hasModifyTopPadding)
					gpoup.padding.top += GetOffset(Screen.height, safeArea.yMax);

				if (hasModifyBottomPadding)
					gpoup.padding.bottom += GetOffset(0, safeArea.yMin);
			}
		}
		
		private int GetOffset(float screenY, float safeAreaY)
		{
			return Mathf.FloorToInt(Mathf.Abs(screenY - safeAreaY));
		}
	}
}