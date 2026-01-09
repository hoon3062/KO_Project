using UnityEngine;
using TMPro;

public class DelayController : MonoBehaviour
{
    [Header("현재 오프셋 값")]
    public float offset = 0.0f;

    [Header("버튼 클릭 시 증감 단위")]
    public float offsetStep = 0.02f; // [수정] 기본값 0.02로 변경

    public TextMeshProUGUI offsetText;

    public void DelayUp() => AddOffset(offsetStep);
    public void DelayDown() => AddOffset(-offsetStep);

    public void SetOffset(float value)
    {
        offset = value;
        if (offset < 0f) offset = 0f;
        UpdateOffsetText();
    }

    public void ResetDelay()
    {
        offset = 0.0f;
        UpdateOffsetText();
    }

    private void AddOffset(float delta)
    {
        offset += delta;
        if (offset < 0f) offset = 0f;
        UpdateOffsetText();
    }

    private void UpdateOffsetText()
    {
        if (offsetText != null)
            // 소수점 셋째 자리까지 표시해야 0.02 단위 확인 가능
            offsetText.text = $"offset = {offset * 1000f:0} ms";
    }

    void Start()
    {
        UpdateOffsetText();
    }
}