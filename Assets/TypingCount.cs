using UnityEngine;
using TMPro;
using System; // Action 이벤트를 사용하기 위해 추가

public class TypingCount : MonoBehaviour
{
    [Header("횟수를 표시할 TextMeshPro UI")]
    public TextMeshProUGUI countText;

    // [추가] 외부(SessionManager)에서 횟수 변경을 감지할 수 있게 하는 이벤트
    public event Action<int> OnCountUpdated;

    public int finishedCount { get; private set; } = 0; // 읽기 전용으로 변경

    private void Start()
    {
        UpdateUI();
    }

    public void IncreaseCount()
    {
        finishedCount++;
        UpdateUI();
        
        // [추가] 횟수가 변경되었음을 알림
        OnCountUpdated?.Invoke(finishedCount);
    }

    public void ResetCount()
    {
        finishedCount = 0;
        UpdateUI();
        
        // [추가] 리셋되었음도 알림
        OnCountUpdated?.Invoke(finishedCount);
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = $"Typed: {finishedCount}";
        }
    }
}