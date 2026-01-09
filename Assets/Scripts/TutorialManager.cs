using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    [Header("1. 참조 연결 (Scripts & Objects)")]
    public TypingCount typingCountScript;   // 카운트 감지용
    public GameObject virtualKeyboardObject; // 씬에 있는 키보드 부모 오브젝트 (처음엔 숨김)

    [Header("2. 비디오 단계 UI")]
    public RawImage videoScreen;            // 비디오가 나오는 화면 (RawImage)
    public VideoPlayer handGuidePlayer;     // 비디오 플레이어 컴포넌트
    public GameObject videoControlButtons;  // '다시보기' & '연습하기' 버튼을 묶은 부모 오브젝트
    public Button replayButton;             // 버튼1: 다시 보기
    public Button startPracticeButton;      // 버튼2: 연습 시작

    [Header("3. 연습 단계 UI")]
    public GameObject practiceUI;           // 진행률 텍스트 등이 포함된 UI 부모 (처음엔 숨김)
    public TextMeshProUGUI progressText;    // "0 / 6" 표시
    public GameObject mainSessionButton;    // 6회 달성 시 나타날 '본 세션 시작' 버튼

    [Header("설정")]
    public int goalCount = 6;               // 목표 횟수

    private void Start()
    {
        // --- 초기화 ---
        
        // 1. 키보드와 연습용 UI 숨기기
        if (virtualKeyboardObject != null) virtualKeyboardObject.SetActive(false);
        if (practiceUI != null) practiceUI.SetActive(false);
        if (mainSessionButton != null) mainSessionButton.SetActive(false);

        // 2. 비디오 관련 설정
        if (videoControlButtons != null) videoControlButtons.SetActive(false); // 선택 버튼 숨김
        if (videoScreen != null) videoScreen.gameObject.SetActive(true);       // 화면 켜기
        
        // 3. 버튼 이벤트 연결
        if (replayButton != null) replayButton.onClick.AddListener(OnClickReplay);
        if (startPracticeButton != null) startPracticeButton.onClick.AddListener(OnClickStartPractice);

        // 4. 비디오 종료 이벤트 연결 및 재생 시작
        if (handGuidePlayer != null)
        {
            handGuidePlayer.loopPointReached += OnVideoFinished; // 비디오가 끝나면 호출될 함수 연결
            PlayVideo();
        }

        // 5. 타이핑 카운트 이벤트 연결
        if (typingCountScript != null)
        {
            typingCountScript.OnCountUpdated += OnTypeCountUpdated;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 연결 해제 (메모리 관리)
        if (handGuidePlayer != null) handGuidePlayer.loopPointReached -= OnVideoFinished;
        if (typingCountScript != null) typingCountScript.OnCountUpdated -= OnTypeCountUpdated;
    }

    // --- 비디오 로직 ---

    private void PlayVideo()
    {
        if (videoControlButtons != null) videoControlButtons.SetActive(false); // 버튼 숨김
        if (videoScreen != null) videoScreen.gameObject.SetActive(true);       // 화면 보이기
        handGuidePlayer.Play();
    }

    // 비디오 재생이 끝났을 때 자동 호출
    private void OnVideoFinished(VideoPlayer vp)
    {
        // 버튼 2개(다시보기, 연습하기) 표시
        if (videoControlButtons != null) videoControlButtons.SetActive(true);
    }

    // 버튼1: 동영상 다시 보기
    private void OnClickReplay()
    {
        PlayVideo();
    }

    // 버튼2: 타이핑 연습 시작
    private void OnClickStartPractice()
    {
        // 1. 비디오 및 관련 버튼 숨기기
        if (videoScreen != null) videoScreen.gameObject.SetActive(false);
        if (videoControlButtons != null) videoControlButtons.SetActive(false);

        // 2. 키보드 나타나게 하기
        if (virtualKeyboardObject != null) virtualKeyboardObject.SetActive(true);

        // 3. 연습 UI(진행률) 표시 및 초기화
        if (practiceUI != null) practiceUI.SetActive(true);
        UpdateProgressUI(typingCountScript.finishedCount);
    }

    // --- 연습(타이핑) 로직 ---

    private void OnTypeCountUpdated(int currentCount)
    {
        // 연습 모드가 시작되지 않았는데 카운트가 올라가는 것 방지 (혹시 모를 예외 처리)
        if (practiceUI != null && !practiceUI.activeSelf) return;

        UpdateProgressUI(currentCount);

        // 목표 달성 시 본 세션 버튼 표시
        if (currentCount >= goalCount)
        {
            if (mainSessionButton != null && !mainSessionButton.activeSelf)
            {
                mainSessionButton.SetActive(true);
            }
        }
    }

    private void UpdateProgressUI(int current)
    {
        if (progressText != null)
        {
            // [핵심] 실제 카운트가 7, 8이 되어도 화면에는 6으로 고정
            int visualCount = Mathf.Clamp(current, 0, goalCount);
            
            // 색상 효과 (완료 시 초록색)
            string countColor = (visualCount >= goalCount) ? "#00FF00" : "#FFFFFF";

            progressText.text = $"진행률: <color={countColor}>{visualCount}</color> / {goalCount}";
        }
    }
}