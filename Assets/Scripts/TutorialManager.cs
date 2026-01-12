using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    [Header("1. UI & References")]
    public GameObject virtualKeyboardObject; 
    public TypingCount typingCountScript; // (옵션) 리셋용

    [Header("2. Video Components")]
    public RawImage videoScreen;            
    public VideoPlayer handGuidePlayer;     
    public GameObject videoControlButtons;  // '다시보기' & '연습하기' 부모 패널
    public Button replayButton;             
    public Button startPracticeButton;
    
    [Header("3. Practice Session UI")]
    public Button skipButton;               // [추가] 비디오 스킵 버튼

    public TextMeshProUGUI infoDisplayUI;   
    public GameObject practiceUI;           
    public TextMeshProUGUI progressText;    
    public GameObject mainSessionButton;    

    [Header("4. Practice Settings")]
    public int goalCount = 6;               

    private int localCurrentCount = 0; 
    private bool isPracticeActive = false; 

    private void Start()
    {
        // 초기화
        localCurrentCount = 0;
        isPracticeActive = false;

        // UI 초기 상태 설정
        if (virtualKeyboardObject != null) virtualKeyboardObject.SetActive(false);
        if (practiceUI != null) practiceUI.SetActive(false);
        if (mainSessionButton != null) mainSessionButton.SetActive(false);

        if (videoControlButtons != null) videoControlButtons.SetActive(false); 
        if (videoScreen != null) videoScreen.gameObject.SetActive(true);       
        if (infoDisplayUI != null) infoDisplayUI.gameObject.SetActive(true);     
        
        // 버튼 이벤트 연결
        if (replayButton != null) replayButton.onClick.AddListener(OnClickReplay);
        if (startPracticeButton != null) startPracticeButton.onClick.AddListener(OnClickStartPractice);
        
        // [추가] 스킵 버튼 이벤트 연결
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnClickSkip);
            skipButton.gameObject.SetActive(true); // 시작 시 보이게 설정
        }

        if (handGuidePlayer != null)
        {
            handGuidePlayer.loopPointReached += OnVideoFinished; 
            PlayVideo();
        }
    }

    private void OnDestroy()
    {
        if (handGuidePlayer != null) handGuidePlayer.loopPointReached -= OnVideoFinished;
    }

    // --- [핵심 기능] Enter 키 로직 ---
    public void OnEnterKeyPressed()
    {
        if (!isPracticeActive) return;

        if (localCurrentCount < goalCount)
        {
            localCurrentCount++;
        }

        UpdateProgressUI(localCurrentCount);

        if (localCurrentCount >= goalCount)
        {
            if (mainSessionButton != null)
            {
                mainSessionButton.SetActive(true);
            }
        }
    }

    // --- 비디오 로직 ---
    private void PlayVideo()
    {
        isPracticeActive = false; 
        
        // UI 상태 초기화
        if (videoControlButtons != null) videoControlButtons.SetActive(false); 
        if (videoScreen != null) videoScreen.gameObject.SetActive(true);
        
        // [추가] 비디오 재생 시 스킵 버튼 보이기
        if (skipButton != null) skipButton.gameObject.SetActive(true);

        handGuidePlayer.Play();
    }

    // 비디오가 끝났거나 스킵했을 때 호출됨
    private void OnVideoFinished(VideoPlayer vp)
    {
        // [추가] 비디오가 끝났으니 스킵 버튼 숨기기
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        // 다시보기/연습하기 버튼 표시
        if (videoControlButtons != null) videoControlButtons.SetActive(true);
    }

    // [추가] 스킵 버튼 클릭 시 호출
    private void OnClickSkip()
    {
        // 비디오 정지
        if (handGuidePlayer != null) handGuidePlayer.Stop();
        
        // 비디오 완료 로직 강제 실행 (버튼 표시 등)
        OnVideoFinished(handGuidePlayer);
    }

    private void OnClickReplay()
    {
        PlayVideo();
    }

    private void OnClickStartPractice()
    {
        if (videoScreen != null) videoScreen.gameObject.SetActive(false);
        if (videoControlButtons != null) videoControlButtons.SetActive(false);
        if (infoDisplayUI != null) infoDisplayUI.gameObject.SetActive(false);
        
        // [추가] 혹시 모르니 스킵 버튼 확실히 숨기기
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        if (virtualKeyboardObject != null) virtualKeyboardObject.SetActive(true);
        if (practiceUI != null) practiceUI.SetActive(true);
        
        localCurrentCount = 0;
        isPracticeActive = true; 
        
        if (typingCountScript != null) typingCountScript.ResetCount();

        UpdateProgressUI(0); 
    }

    private void UpdateProgressUI(int current)
    {
        if (progressText != null)
        {
            int visualCount = Mathf.Clamp(current, 0, goalCount);
            string countColor = (visualCount >= goalCount) ? "#00FF00" : "#FFFFFF";
            progressText.text = $"진행률: <color={countColor}>{visualCount}</color> / {goalCount}";
        }
    }
}