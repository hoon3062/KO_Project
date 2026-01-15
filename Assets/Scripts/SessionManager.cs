using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System; // Action 이벤트를 위해 추가

public class SessionManager : MonoBehaviour
{
    [Header("실험 설정")]
    public float maxOffsetLimit = 0.1f;
    public int targetCount = 10; // 고정값 10

    [Header("외부 스크립트 연결")]
    public TypingCount typingCountScript; 
    public DelayController delayController; 
    
    // [추가] 텍스트 UI는 ScenarioManager와 공유해서 사용
    public TextMeshProUGUI statusText;   

    // [추가] 외부로 상태를 알리는 이벤트
    public event Action OnSessionEnded;  // 한 세션 끝 (설문 타이밍)
    public event Action OnAllFinished;   // 모든 실험 끝

    private bool isSessionActive = false;
    private List<float> sessionOffsets = new List<float>(); 
    private int currentSessionIndex = 0; 
// 현재 진행 순서(인덱스)를 반환하는 프로퍼티 (외부 읽기 전용)
    public int CurrentIndex => currentSessionIndex;
    public int TotalSessions => sessionOffsets.Count;
    public int CurrentSessionNum => currentSessionIndex + 1;

    private void Start()
    {
        // 슬라이더 및 버튼 로직 제거 (ScenarioManager가 제어함)
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentCsvAndReset();
        }
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated += CheckSessionProgress;
        }
    }

    private void OnDestroy()
    {
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated -= CheckSessionProgress;
        }
    }

    // [변경] 외부(ScenarioManager)에서 호출하여 초기화
    public void InitializeExperiment()
    {
        GenerateSessionList();
        ShuffleSessionList();
        currentSessionIndex = 0;
    }

    // [변경] 외부에서 호출하여 다음 세션 시작
    public void StartNextSession()
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

        // 세션 시작 시 진행률 표시
        UpdateStatusText(0);

        if (typingCountScript != null)
        {
            typingCountScript.ResetCount();
        }
        
        Debug.Log($"[SessionManager] 세션 {currentSessionIndex + 1} 시작. Offset: {nextOffset}");
    }

    private void CheckSessionProgress(int currentCount)
    {
        if (!isSessionActive) return;

        UpdateStatusText(currentCount);

        if (currentCount >= targetCount)
        {
            CompleteSession();
        }
    }

    private void UpdateStatusText(int current)
    {
        if (statusText != null)
        {
            statusText.text = $"Session {currentSessionIndex + 1}/{sessionOffsets.Count}\nProgress: <color=yellow>{current}</color> / {targetCount}";
        }
    }

    private void CompleteSession()
    {
        Debug.Log($"[SessionManager] 세션 {currentSessionIndex + 1} 완료!");
        isSessionActive = false; // 입력 중지 상태

        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentCsvAndReset();
        }

        currentSessionIndex++;

        // [핵심 변경] 바로 StartNextSession을 부르지 않고 이벤트만 발생시킴
        OnSessionEnded?.Invoke();
    }

    private void AllExperimentsOver()
    {
        isSessionActive = false;
        if (delayController != null) delayController.ResetDelay(); 

        OnAllFinished?.Invoke(); // 종료 이벤트 발생
    }

    // 오프셋 리스트 생성 로직 (기존 동일)
    private void GenerateSessionList()
    {
        sessionOffsets.Clear();
        float step = 0.02f; 
        if (delayController != null) step = delayController.offsetStep; 

        for (float val = 0.0f; val <= maxOffsetLimit + 0.0001f; val += step)
        {
            sessionOffsets.Add(val);
        }
    }

    private void ShuffleSessionList()
    {
        for (int i = 0; i < sessionOffsets.Count; i++)
        {
            float temp = sessionOffsets[i];
            int randomIndex = UnityEngine.Random.Range(i, sessionOffsets.Count);
            sessionOffsets[i] = sessionOffsets[randomIndex];
            sessionOffsets[randomIndex] = temp;
        }
    }
}