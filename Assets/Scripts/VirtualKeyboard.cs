using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // [추가] 이 스크립트를 넣으면 AudioSource가 자동으로 추가됨
public class VirtualKeyboard : MonoBehaviour
{
    [Header("사용자 입력을 보여줄 TextMeshPro")]
    public TextMeshProUGUI sourceText;

    [Header("목표 문장을 관리하는 RandomPhrases")]
    public RandomPhrases randomPhrases;

    [Header("타이핑 횟수 표시 스크립트 연결")]
    public TypingCount typingCount;

    [Header("오디오 설정")] // [추가] 인스펙터에서 설정할 항목
    public AudioSource audioSource;
    public AudioClip clickSound;

    private string currentInput = "";
    private bool typingStarted = false;

    // 커서 표시용
    private bool cursorVisible = true;
    private Coroutine cursorBlinkCoroutine;

    // 타이핑 로그용
    private float typingStartTime = 0f; 
    private int backspaceCount = 0;     

    private void Start()
    {
        // [추가] AudioSource가 연결되지 않았다면 자동으로 가져오기
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (randomPhrases != null)
        {
            randomPhrases.ShowRandomSentence();
        }

        cursorBlinkCoroutine = StartCoroutine(CursorBlink());
        UpdateSourceText();
    }

    private void OnDestory()
    {
        if (cursorBlinkCoroutine != null)
        {
            StopCoroutine(cursorBlinkCoroutine);
        }
    }

    // 소리 재생 전용 함수 [추가]
    private void PlayFeedbackSound()
    {
        if (audioSource != null && clickSound != null)
        {
            // PlayOneShot은 소리가 겹쳐도 끊기지 않고 자연스럽게 재생됩니다.
            audioSource.PlayOneShot(clickSound); 
        }
    }

    public void OnKeyPress(string key)
    {
        // [추가] 키가 눌리면 소리 재생
        PlayFeedbackSound();

        if (DataManager.Instance != null)
        {
            // 1. 전체 세션 타이머 시작 (아직 안 켜졌다면)
            DataManager.Instance.StartSessionTimer();

            // 2. Raw 데이터 기록 (키 값 + 현재 시간 저장)
            DataManager.Instance.LogRawKey(key);
        }

        switch (key)
        {
            case "backspace":
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    backspaceCount++;
                }
                break;

            case "space":
                if (!typingStarted)
                {
                    typingStarted = true;
                    typingStartTime = Time.time;
                    backspaceCount = 0;
                }
                currentInput += " ";
                break;

            case "enter":
                CheckResult();
                return;

            default:
                // 일반 문자 입력
                if (!typingStarted)
                {
                    typingStarted = true;
                    typingStartTime = Time.time; 
                    backspaceCount = 0; 
                }
                currentInput += key;
                break;
        }

        UpdateSourceText();
    }

    private void UpdateSourceText()
    {
        if (sourceText == null) return;
        string baseText = currentInput;
        string cursor = cursorVisible ? "<color=#000000>|</color>" : "<color=#00000000>|</color>";

        sourceText.text = baseText + cursor;
    }

    private IEnumerator CursorBlink() {
        while (true)
        {
            cursorVisible = !cursorVisible;
            UpdateSourceText();
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Levenshtein 거리
    private int ComputeLevenshtein(string refText, string hypText)
    {
        if (string.IsNullOrEmpty(refText)) return string.IsNullOrEmpty(hypText) ? 0 : hypText.Length;
        if (string.IsNullOrEmpty(hypText)) return refText.Length;

        int n = refText.Length;
        int m = hypText.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i; 
        for (int j = 0; j <= m; j++) d[0, j] = j; 

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (refText[i - 1] == hypText[j - 1]) ? 0 : 1; 
                d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m]; 
    }

    private float ComputeCER(string target, string input)
    {
        int distance = ComputeLevenshtein(target, input);
        int N = Mathf.Max(target.Length, 1); 
        return (float)distance / N * 100f;
    }

    private void CheckResult()
    {
        string target = (randomPhrases != null && randomPhrases.targetText != null) ? randomPhrases.targetText.text : "";
        string finalInput = currentInput;

        float durationSeconds = 0f;
        if (typingStarted)
        {
            durationSeconds = Mathf.Max(Time.time - typingStartTime, 0.0001f);
        }

        int finalLength = finalInput.Length;
        int charsForWpm = Mathf.Max(finalLength - 1, 0); 
        float wpm = 0f;
        if (durationSeconds > 0f && charsForWpm > 0)
        {
            float words = charsForWpm / 5f;
            wpm = words * (60f / durationSeconds);
        }

        float cer = ComputeCER(target, finalInput);

        float correctionRate = 0f;
        if (finalLength > 0)
        {
            correctionRate = (float)backspaceCount / finalLength;
        }

        int correctCount = 0;
        int minLength = Mathf.Min(finalInput.Length, target.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (finalInput[i] == target[i]) correctCount++;
        }
        float simpleAccuracy = (float)correctCount / Mathf.Max(target.Length, 1) * 100f;

        Debug.Log(
            $"[타이핑 결과] 단순정확도: {simpleAccuracy:F2}% | " +
            $"WPM: {wpm:F2} | CER: {cer:F2}% | 수정률: {correctionRate:F3} | " +
            $"백스페이스: {backspaceCount} | 소요시간: {durationSeconds:F2}s");

        if (DataManager.Instance != null)
        {
            DataManager.Instance.LogTrial(
                target: target,
                source: finalInput,
                durationSeconds: durationSeconds,
                wpm: wpm,
                cer: cer,
                correctionRate: correctionRate,
                backspaceCount: backspaceCount
            );
        }

        if (typingCount != null)
        {
            typingCount.IncreaseCount();
        }

        // 다음 trial 준비
        currentInput = "";
        typingStarted = false;
        backspaceCount = 0;
        typingStartTime = 0f;

        UpdateSourceText();

        if (randomPhrases != null)
        {
            randomPhrases.ShowRandomSentence();
        }
    }
}