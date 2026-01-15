using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;

public class ScenarioManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TextMeshProUGUI infoDisplayUI;   
    public GameObject nextButton;           
    public GameObject virtualKeyboardRoot;  

    [Header("매니저 스크립트 연결")]
    public SessionManager sessionManager;           
    public FrequencySessionManager frequencyManager; 
    public GameObject exitButton; 

    private bool isRateMode = false; 

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "RateScene") isRateMode = true;
        else if (sceneName == "DelayScene") isRateMode = false;
        else Debug.LogWarning($"[ScenarioManager] 알 수 없는 씬: {sceneName}");

        if (nextButton != null) nextButton.SetActive(false);
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false); // 시작 시 종료버튼 숨김
        
        if (nextButton != null)
            nextButton.GetComponent<Button>().onClick.AddListener(OnClickNextSession);

        if (isRateMode && frequencyManager != null)
        {
            frequencyManager.OnSessionEnded += OnSessionEnded;
            frequencyManager.OnAllFinished += OnAllFinished;
            frequencyManager.statusText = infoDisplayUI; 
        }
        else if (sessionManager != null)
        {
            sessionManager.OnSessionEnded += OnSessionEnded;
            sessionManager.OnAllFinished += OnAllFinished;
            sessionManager.statusText = infoDisplayUI; 
        }

        StartCoroutine(IntroScenario());
    }

    private void OnDestroy()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionEnded -= OnSessionEnded;
            sessionManager.OnAllFinished -= OnAllFinished;
        }

        if (frequencyManager != null)
        {
            frequencyManager.OnSessionEnded -= OnSessionEnded;
            frequencyManager.OnAllFinished -= OnAllFinished;
        }
    }

    private IEnumerator IntroScenario()
    {
        ShowText("한 세션당 타이핑 해야 할\n문장은 총 10문장입니다.");
        yield return new WaitForSeconds(5.0f);

        ShowText("각 세션이 끝날 때마다\n설문지를 작성해주시면 됩니다.");
        yield return new WaitForSeconds(5.0f);

        ShowText("표시되는 문장을 가능한\n<color=yellow>정확하고 빠르게</color> 타이핑 해주세요.");
        yield return new WaitForSeconds(5.0f);
        
        ShowText("휴식은 자유롭게 취하셔도 됩니다.\n단, <color=yellow>타이핑 중</color>이던 문장은\n<color=yellow>끝까지</color> 타이핑 해주십시오.");
        yield return new WaitForSeconds(6.0f);

        ShowText("키보드가 시야 밖으로 나갈 경우 HMD기기의 <color=yellow>우측 상단 버튼</color>을\n약 2초간 눌러 재조정 해주세요.");
        yield return new WaitForSeconds(8.0f);

        InitializeSelectedManager(); 
        StartActiveSession(); 
    }

    private void InitializeSelectedManager()
    {
        if (isRateMode) frequencyManager?.InitializeExperiment();
        else sessionManager?.InitializeExperiment();
    }

    private void StartActiveSession()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(true);
        if (nextButton != null) nextButton.SetActive(false);

        if (isRateMode) frequencyManager?.StartNextSession();
        else sessionManager?.StartNextSession();
    }

    // [수정된 부분] 세션 종료 시 처리
    private void OnSessionEnded()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);

        int finishedSession = 0;
        int totalSessions = 0; // 전체 세션 수를 저장할 변수

        // 1. 현재 완료된 세션 번호와 전체 세션 수 가져오기
        if (isRateMode && frequencyManager != null)
        {
            finishedSession = frequencyManager.CurrentIndex;
            // FrequencyManager의 리스트 개수가 전체 세션 수
            totalSessions = frequencyManager.targetFrequencies.Count; 
        }
        else if (sessionManager != null)
        {
            finishedSession = sessionManager.CurrentIndex;
            // SessionManager의 TotalSessions 프로퍼티 사용
            totalSessions = sessionManager.TotalSessions; 
        }

        // 2. [핵심 수정] 마지막 세션인지 확인
        if (finishedSession >= totalSessions)
        {
            // 마지막 세션이라면 '다음' 버튼 단계(설문 안내)를 건너뛰고 바로 종료 처리
            OnAllFinished(); 
            return;
        }

        // 3. 마지막 세션이 아니라면 다음 세션 안내 및 버튼 표시
        int nextSession = finishedSession + 1;
        string message = $"기기를 내려놓은 뒤,\n설문지를 작성해주십시오.\n" +
                         $"<size=80%>완료 세션 : {finishedSession}, 다음 세션 : {nextSession}</size>";

        ShowText(message);

        if (nextButton != null) nextButton.SetActive(true);
    }

    private void OnClickNextSession()
    {
        StartActiveSession();
    }

    private void OnAllFinished()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
        
        ShowText("모든 실험이 종료되었습니다.\n참여해 주셔서 감사합니다.");
        
        if (exitButton != null)
        {
            exitButton.SetActive(true);
        }
    }

    private void ShowText(string message)
    {
        if (infoDisplayUI != null)
        {
            infoDisplayUI.text = message;
        }
    }
}