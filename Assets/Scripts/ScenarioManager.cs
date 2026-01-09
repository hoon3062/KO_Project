using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // [필수] 씬 이름을 가져오기 위해 추가
using System.Collections;

public class ScenarioManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TextMeshProUGUI infoDisplayUI;   
    public GameObject nextButton;           
    public GameObject virtualKeyboardRoot;  

    [Header("매니저 스크립트 연결")]
    // 두 매니저를 모두 연결해두고, 씬에 따라 하나만 사용합니다.
    public SessionManager sessionManager;           // DelayScene용
    public FrequencySessionManager frequencyManager; // RateScene용

    // 현재 모드를 구분하기 위한 플래그
    private bool isRateMode = false; 

    private void Start()
    {
        // 1. 현재 씬 이름 확인
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "RateScene")
        {
            isRateMode = true;
            Debug.Log("[ScenarioManager] RateScene 감지: FrequencySessionManager를 사용합니다.");
        }
        else if (sceneName == "DelayScene")
        {
            isRateMode = false;
            Debug.Log("[ScenarioManager] DelayScene 감지: SessionManager를 사용합니다.");
        }
        else
        {
            Debug.LogWarning($"[ScenarioManager] 알 수 없는 씬 이름({sceneName})입니다. 기본값(Delay)으로 설정합니다.");
        }

        // 2. 초기 UI 상태 설정
        if (nextButton != null) nextButton.SetActive(false);
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);
        
        if (nextButton != null)
        {
            nextButton.GetComponent<Button>().onClick.AddListener(OnClickNextSession);
        }

        // 3. 선택된 매니저의 이벤트 구독 및 텍스트 UI 공유
        if (isRateMode)
        {
            if (frequencyManager != null)
            {
                frequencyManager.OnSessionEnded += OnSessionEnded;
                frequencyManager.OnAllFinished += OnAllFinished;
                frequencyManager.statusText = infoDisplayUI; // UI 공유
            }
        }
        else
        {
            if (sessionManager != null)
            {
                sessionManager.OnSessionEnded += OnSessionEnded;
                sessionManager.OnAllFinished += OnAllFinished;
                sessionManager.statusText = infoDisplayUI; // UI 공유
            }
        }

        // 4. 인트로 시나리오 시작
        StartCoroutine(IntroScenario());
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (모드에 상관없이 둘 다 해제 시도 - 안전함)
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

    // --- 시나리오 흐름 ---

    private IEnumerator IntroScenario()
    {
        ShowText("한 세션당 타이핑 해야 할 문장은\n총 10문장입니다.");
        yield return new WaitForSeconds(3.0f);

        ShowText("각 세션이 끝날 때마다\n설문지를 작성해주시면 됩니다.");
        yield return new WaitForSeconds(3.0f);

        ShowText("표시되는 문장에 맞게\n타이핑 해주십시오.");
        yield return new WaitForSeconds(2.0f);

        // 설명 끝, 실험 초기화 및 첫 세션 시작
        InitializeSelectedManager(); // [변경] 통합 함수 호출
        StartActiveSession(); 
    }

    // [핵심] 현재 모드에 따라 적절한 매니저 초기화
    private void InitializeSelectedManager()
    {
        if (isRateMode)
        {
            if (frequencyManager != null) frequencyManager.InitializeExperiment();
        }
        else
        {
            if (sessionManager != null) sessionManager.InitializeExperiment();
        }
    }

    // [핵심] 현재 모드에 따라 적절한 매니저 실행
    private void StartActiveSession()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(true);
        if (nextButton != null) nextButton.SetActive(false);

        // 선택된 매니저에게 시작 명령
        if (isRateMode)
        {
            if (frequencyManager != null) frequencyManager.StartNextSession();
        }
        else
        {
            if (sessionManager != null) sessionManager.StartNextSession();
        }
    }

    private void OnSessionEnded()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);
        ShowText("설문지를 작성해주십시오.");
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
    }

    private void ShowText(string message)
    {
        if (infoDisplayUI != null)
        {
            infoDisplayUI.text = message;
        }
    }
}