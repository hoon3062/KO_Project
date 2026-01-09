using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FrequencySessionManager : MonoBehaviour
{
    [Header("실험 주사율 설정")]
    // [설정] 요청하신 4가지 주사율을 리스트에 기본값으로 넣어둠
    public List<float> targetFrequencies = new List<float> { 90f, 45f, 30f, 22.5f, 18f };

    [Header("UI 컴포넌트")]
    public Slider targetSlider;          
    public TextMeshProUGUI sliderValueText; 
    public Button startButton;           
    public TextMeshProUGUI statusText;   

    [Header("외부 스크립트 연결")]
    public TypingCount typingCountScript; 
    public FrequencyController freqController; // [변경] DelayController -> FrequencyController

    private int targetCount = 10; 
    private bool isSessionActive = false;

    // 실험 진행을 위한 큐 (섞인 주사율 저장)
    private List<float> randomizedSessions = new List<float>(); 
    private int currentSessionIndex = 0; 

    private void Start()
    {
        if (targetSlider != null)
        {
            targetSlider.minValue = 1;
            targetSlider.maxValue = 10; // 최대 문장 수
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

        if (statusText != null) statusText.text = "Frequency Exp Ready";
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
        // 1. 세션 리스트 준비 (복사)
        randomizedSessions = new List<float>(targetFrequencies);
        
        // 2. 랜덤 셔플
        ShuffleSessionList();
        
        // 3. 시작
        currentSessionIndex = 0;
        StartNextSession();
    }

    private void ShuffleSessionList()
    {
        for (int i = 0; i < randomizedSessions.Count; i++)
        {
            float temp = randomizedSessions[i];
            int randomIndex = Random.Range(i, randomizedSessions.Count);
            randomizedSessions[i] = randomizedSessions[randomIndex];
            randomizedSessions[randomIndex] = temp;
        }

        string log = "[FrequencyManager] 세션 순서: ";
        foreach (var hz in randomizedSessions) log += $"{hz}Hz -> "; 
        Debug.Log(log);
    }

    private void StartNextSession()
    {
        if (currentSessionIndex >= randomizedSessions.Count)
        {
            AllExperimentsOver();
            return;
        }

        isSessionActive = true;
        float nextHz = randomizedSessions[currentSessionIndex];

        // [핵심] 주사율 컨트롤러 값 변경
        if (freqController != null)
        {
            freqController.SetHz(nextHz);
        }

        // 현재 세션 상태 표시 (Hz 정보는 숨김, 진행도만 표시)
        if (statusText != null)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{randomizedSessions.Count}\nProgress: 0 / {targetCount}";
        }

        // 문장 카운트 초기화
        if (typingCountScript != null)
        {
            typingCountScript.ResetCount();
        }
        
        Debug.Log($"[FrequencyManager] 세션 {currentSessionIndex + 1} 시작. Target Hz: {nextHz}");
    }

    private void CheckSessionProgress(int currentCount)
    {
        if (!isSessionActive) return;

        // 진행 상황 업데이트
        if (statusText != null && currentCount < targetCount)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{randomizedSessions.Count}\nProgress: {currentCount} / {targetCount}";
        }

        // 목표 달성 시
        if (currentCount >= targetCount)
        {
            CompleteSession();
        }
    }

    private void CompleteSession()
    {
        Debug.Log($"[FrequencyManager] 세션 {currentSessionIndex + 1} 완료!");
        
        // 데이터 저장 (DataManager가 있다면)
        if (DataManager.Instance != null)
        {
            // 현재까지의 기록을 파일로 저장하고 다음 파일을 준비함
            DataManager.Instance.SaveCurrentCsvAndReset();
        }

        currentSessionIndex++;
        StartNextSession();
    }

    private void AllExperimentsOver()
    {
        isSessionActive = false;

        // 실험 종료 후 90Hz(기본값)로 복구
        if (freqController != null)
        {
            freqController.ResetFreq(); 
        }

        if (statusText != null)
        {
            statusText.text = "<color=red>Frequency Exp Finished</color>";
        }
        Debug.Log("[FrequencyManager] 모든 주사율 실험 세션 종료 (90Hz로 초기화됨)");
    }
}