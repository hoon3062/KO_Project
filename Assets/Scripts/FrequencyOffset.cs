using UnityEngine;
using TMPro;

public class FrequencyController : MonoBehaviour
{
    [Header("현재 주사율 값 (Hz)")]
    public float currentHz = 90.0f; // 기본값 90Hz

    [Header("버튼 클릭 시 증감 단위")]
    public float hzStep = 15.0f; // [설정] 15단위로 변경 (90->75->60...)

    [Header("최소/최대 주사율 제한")]
    public float minHz = 1.0f;   // 0Hz가 되면 멈추므로 최소 1로 설정
    public float maxHz = 120.0f; // 기기 최대 스펙에 맞춰 설정

    public TextMeshProUGUI hzText;

    // 버튼 연결용 함수
    public void FreqUp() => AddHz(hzStep);
    public void FreqDown() => AddHz(-hzStep);

    // 외부에서 특정 값으로 강제 설정할 때
    public void SetHz(float value)
    {
        currentHz = Mathf.Clamp(value, minHz, maxHz);
        UpdateHzText();
    }

    // 초기화 (보통 90Hz 또는 60Hz)
    public void ResetFreq()
    {
        currentHz = 90.0f;
        UpdateHzText();
    }

    private void AddHz(float delta)
    {
        currentHz += delta;
        // 범위 제한 (Clamp)
        currentHz = Mathf.Clamp(currentHz, minHz, maxHz);
        UpdateHzText();
    }

    private void UpdateHzText()
    {
        if (hzText != null)
            // 소수점 없이 정수로 표시 (예: Freq = 60 Hz)
            hzText.text = $"Freq = {currentHz:0} Hz";
    }

    void Start()
    {
        UpdateHzText();
    }
}