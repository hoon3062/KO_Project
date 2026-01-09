using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ScenarioManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TextMeshProUGUI infoDisplayUI;   // 중앙 텍스트 (SessionManager의 statusText와 같은 오브젝트 연결)
    public GameObject nextButton;           // '다음 세션 진행' 버튼
    public GameObject virtualKeyboardRoot;  // 키보드 전체 부모 오브젝트

    [Header("스크립트 연결")]
    public SessionManager sessionManager;   // 수정된 SessionManager 연결

    private void Start()
    {
        // 1. 초기 UI 상태 설정
        if (nextButton != null) nextButton.SetActive(false);
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false); // 시작 전엔 키보드 숨김
        
        // 버튼 리스너
        if (nextButton != null)
        {
            nextButton.GetComponent<Button>().onClick.AddListener(OnClickNextSession);
        }

        // 2. SessionManager 이벤트 구독
        if (sessionManager != null)
        {
            sessionManager.OnSessionEnded += OnSessionEnded;
            sessionManager.OnAllFinished += OnAllFinished;
            
            // SessionManager도 같은 텍스트 UI를 쓰도록 설정
            sessionManager.statusText = infoDisplayUI;
        }

        // 3. 인트로 시나리오 시작
        StartCoroutine(IntroScenario());
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (sessionManager != null)
        {
            sessionManager.OnSessionEnded -= OnSessionEnded;
            sessionManager.OnAllFinished -= OnAllFinished;
        }
    }

    // --- 시나리오 흐름 ---

    // 1. 인트로: 순차적 텍스트 표시
    private IEnumerator IntroScenario()
    {
        ShowText("한 세션당 타이핑 해야 할 문장은\n총 10문장입니다.");
        yield return new WaitForSeconds(3.0f);

        ShowText("각 세션이 끝날 때마다\n설문지를 작성해주시면 됩니다.");
        yield return new WaitForSeconds(3.0f);

        ShowText("표시되는 문장에 맞게\n타이핑 해주십시오.");
        yield return new WaitForSeconds(2.0f);

        // 설명 끝, 실험 초기화 및 첫 세션 시작
        if (sessionManager != null)
        {
            sessionManager.InitializeExperiment();
            StartActiveSession(); // 첫 세션 바로 시작
        }
    }

    // 2. 세션 진행 상태 (키보드 켜기)
    private void StartActiveSession()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(true);
        if (nextButton != null) nextButton.SetActive(false); // 버튼 숨김

        // SessionManager에게 시작 명령 -> 이때부터 텍스트는 SessionManager가 "Progress..."로 업데이트함
        sessionManager.StartNextSession();
    }

    // 3. 한 세션 종료 (SessionManager 이벤트 감지)
    private void OnSessionEnded()
    {
        // 키보드 가리기
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);

        // 설문 안내 텍스트 표시
        ShowText("설문지를 작성해주십시오.");

        // 진행 버튼 표시
        if (nextButton != null) nextButton.SetActive(true);
    }

    // 4. 진행 버튼 클릭 핸들러
    private void OnClickNextSession()
    {
        // 다음 세션 시작
        StartActiveSession();
    }

    // 5. 모든 실험 종료 (SessionManager 이벤트 감지)
    private void OnAllFinished()
    {
        if (virtualKeyboardRoot != null) virtualKeyboardRoot.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        ShowText("모든 실험이 종료되었습니다.\n참여해 주셔서 감사합니다."); // 붉은색 등 강조 가능
    }

    // 텍스트 출력 헬퍼 함수
    private void ShowText(string message)
    {
        if (infoDisplayUI != null)
        {
            infoDisplayUI.text = message;
        }
    }
}