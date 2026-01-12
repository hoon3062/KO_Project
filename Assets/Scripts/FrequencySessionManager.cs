using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System; // [필수] Action 이벤트를 사용하기 위해 추가

public class FrequencySessionManager : MonoBehaviour
{
    [Header("실험 주사율 설정")]
    public List<float> targetFrequencies = new List<float> { 90f, 45f, 30f, 22.5f, 18f };
    public int targetCount = 10; // 목표 문장 수 (기본 10)

    [Header("UI 컴포넌트")]
    // [참고] StartButton, Slider는 ScenarioManager가 주도권을 가지면 사용되지 않을 수 있습니다.
    public TextMeshProUGUI statusText;   // ScenarioManager와 공유되는 텍스트

    [Header("외부 스크립트 연결")]
    public TypingCount typingCountScript; 
    public FrequencyController freqController; 

    // [추가] ScenarioManager가 구독할 이벤트
    public event Action OnSessionEnded;  // 한 주사율 세션 끝 (설문 타이밍)
    public event Action OnAllFinished;   // 모든 실험 끝

    private bool isSessionActive = false;
    private List<float> randomizedSessions = new List<float>(); 
    private int currentSessionIndex = 0; 

    private void Start()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentCsvAndReset();
        }
        // TypingCount 이벤트 연결
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated += CheckSessionProgress;
        }

        // 초기 상태 텍스트
        UpdateStatusText("Frequency Experiment Ready");
    }

    private void OnDestroy()
    {
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated -= CheckSessionProgress;
        }
    }

    // [추가] ScenarioManager에서 호출하여 실험 준비
    public void InitializeExperiment()
    {
        // 1. 세션 리스트 준비 (원본 복사)
        randomizedSessions = new List<float>(targetFrequencies);
        
        // 2. 랜덤 셔플
        ShuffleSessionList();
        
        // 3. 인덱스 초기화
        currentSessionIndex = 0;

        Debug.Log($"[FrequencyManager] 실험 초기화 완료. 총 {randomizedSessions.Count}개 세션.");
    }

    // [추가] ScenarioManager에서 호출하여 다음 세션 시작
    public void StartNextSession()
    {
        // 모든 세션이 끝났는지 확인
        if (currentSessionIndex >= randomizedSessions.Count)
        {
            AllExperimentsOver();
            return;
        }

        isSessionActive = true;
        float nextHz = randomizedSessions[currentSessionIndex];

        // [핵심] 주사율 변경
        if (freqController != null)
        {
            freqController.SetHz(nextHz);
        }

        // 진행률 초기화 및 UI 표시
        if (typingCountScript != null)
        {
            typingCountScript.ResetCount();
        }
        
        UpdateUIProgress(0);
        
        Debug.Log($"[FrequencyManager] 세션 {currentSessionIndex + 1} 시작. Target Hz: {nextHz}");
    }

    private void ShuffleSessionList()
    {
        for (int i = 0; i < randomizedSessions.Count; i++)
        {
            float temp = randomizedSessions[i];
            int randomIndex = UnityEngine.Random.Range(i, randomizedSessions.Count);
            randomizedSessions[i] = randomizedSessions[randomIndex];
            randomizedSessions[randomIndex] = temp;
        }
    }

    private void CheckSessionProgress(int currentCount)
    {
        if (!isSessionActive) return;

        UpdateUIProgress(currentCount);

        // 목표 달성 시
        if (currentCount >= targetCount)
        {
            CompleteSession();
        }
    }

    private void UpdateUIProgress(int current)
    {
        if (statusText != null)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{randomizedSessions.Count}\nProgress: <color=yellow>{current}</color> / {targetCount}";
        }
    }

    private void UpdateStatusText(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void CompleteSession()
    {
        Debug.Log($"[FrequencyManager] 세션 {currentSessionIndex + 1} 완료!");
        
        isSessionActive = false; // 입력 중지

        // 데이터 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentCsvAndReset();
        }

        currentSessionIndex++;

        // [중요] 바로 다음 세션을 시작하지 않고, 이벤트를 발생시켜 ScenarioManager에게 알림 (설문 타임)
        OnSessionEnded?.Invoke();
    }

    private void AllExperimentsOver()
    {
        isSessionActive = false;

        // 실험 종료 후 90Hz(기본값)로 복구
        if (freqController != null)
        {
            freqController.ResetFreq(); 
        }

        OnAllFinished?.Invoke(); // 종료 이벤트 발생
        Debug.Log("[FrequencyManager] 모든 실험 종료됨.");
    }
}