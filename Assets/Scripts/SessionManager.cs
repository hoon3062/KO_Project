using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    [Header("실험 설정")]
    public float maxOffsetLimit = 0.1f; // 최대 오프셋 범위

    [Header("UI 컴포넌트")]
    public Slider targetSlider;          
    public TextMeshProUGUI sliderValueText; 
    public Button startButton;           
    public TextMeshProUGUI statusText;   

    [Header("외부 스크립트 연결")]
    public TypingCount typingCountScript; 
    public DelayController delayController; 

    private int targetCount = 10; 
    private bool isSessionActive = false;

    private List<float> sessionOffsets = new List<float>(); 
    private int currentSessionIndex = 0; 

    private void Start()
    {
        if (targetSlider != null)
        {
            targetSlider.minValue = 1;
            // [수정] 슬라이더 최대값을 10으로 변경
            targetSlider.maxValue = 10; 
            targetSlider.wholeNumbers = true;
            targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
            targetCount = (int)targetSlider.value;
            UpdateSliderText();
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartExperiment);
        }

        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated += CheckSessionProgress;
        }

        if (statusText != null) statusText.text = "Ready to Start";
    }

    private void OnDestroy()
    {
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated -= CheckSessionProgress;
        }
    }

    private void OnSliderValueChanged(float value)
    {
        targetCount = (int)value;
        UpdateSliderText();
    }

    private void UpdateSliderText()
    {
        if (sliderValueText != null)
            sliderValueText.text = $"Target Sentences: {targetCount}";
    }

    private void OnStartExperiment()
    {
        GenerateSessionList();
        ShuffleSessionList();
        currentSessionIndex = 0;
        StartNextSession();
    }

    private void GenerateSessionList()
    {
        sessionOffsets.Clear();
        float step = 0.02f; 

        if (delayController != null)
        {
            step = delayController.offsetStep; 
        }

        for (float val = 0.0f; val <= maxOffsetLimit + 0.0001f; val += step)
        {
            sessionOffsets.Add(val);
        }

        Debug.Log($"[SessionManager] 세션 생성 완료: 0 ~ {maxOffsetLimit} (Step: {step}), 총 {sessionOffsets.Count}개");
    }

    private void ShuffleSessionList()
    {
        for (int i = 0; i < sessionOffsets.Count; i++)
        {
            float temp = sessionOffsets[i];
            int randomIndex = Random.Range(i, sessionOffsets.Count);
            sessionOffsets[i] = sessionOffsets[randomIndex];
            sessionOffsets[randomIndex] = temp;
        }

        string log = "세션 순서: ";
        foreach (var offset in sessionOffsets) log += $"{offset:F3} -> "; 
        Debug.Log(log);
    }

    private void StartNextSession()
    {
        if (currentSessionIndex >= sessionOffsets.Count)
        {
            AllExperimentsOver();
            return;
        }

        isSessionActive = true;
        float nextOffset = sessionOffsets[currentSessionIndex];

        if (delayController != null)
        {
            delayController.SetOffset(nextOffset);
        }

        // [수정] 오프셋 정보 제거, 진행 상황 초기값(0) 표시
        if (statusText != null)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{sessionOffsets.Count}\nProgress: 0 / {targetCount}";
        }

        if (typingCountScript != null)
        {
            typingCountScript.ResetCount();
        }
        
        Debug.Log($"[SessionManager] 세션 {currentSessionIndex + 1} 시작. Offset: {nextOffset}");
    }

    private void CheckSessionProgress(int currentCount)
    {
        if (!isSessionActive) return;

        // [수정] 오프셋 정보 제거, 세션 번호와 문장 갯수만 표시
        if (statusText != null && currentCount < targetCount)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{sessionOffsets.Count}\nProgress: {currentCount} / {targetCount}";
        }

        if (currentCount >= targetCount)
        {
            CompleteSession();
        }
    }

    private void CompleteSession()
    {
        Debug.Log($"[SessionManager] 세션 {currentSessionIndex + 1} 완료!");
        
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentCsvAndReset();
        }

        currentSessionIndex++;
        StartNextSession();
    }

    private void AllExperimentsOver()
    {
        isSessionActive = false;

        if (delayController != null)
        {
            delayController.ResetDelay(); 
        }

        if (statusText != null)
        {
            statusText.text = "<color=red>All over</color>";
        }
        Debug.Log("[SessionManager] 모든 실험 세션 종료 (Delay 0으로 초기화됨)");
    }
}