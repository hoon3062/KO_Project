using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("UI 연결")]
    public TypingCount typingCountScript;

    [System.Serializable]
    public class TypingRecord
    {
        public string targetText;
        public string sourceText;
        public float durationSeconds;
        public float wpm;
        public float cer;
        public float correctionRate;
        public int backspaceCount;
    }

    public List<TypingRecord> records = new List<TypingRecord>();

    [Header("파일로 저장할지 여부")]
    public bool saveToCsv = true;

    private string csvPath;     // 요약본 경로
    private string rawCsvPath;  // Raw 데이터 경로

    private float sessionStartTime = -1f;

    // [추가] Raw 데이터 인덱스 카운터
    private int rawKeyIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        csvPath = Path.Combine(Application.persistentDataPath, "typing_log.csv");
        rawCsvPath = Path.Combine(Application.persistentDataPath, "typing_log_raw.csv");

        Debug.Log($"[DataManager] Summary path: {csvPath}");
        Debug.Log($"[DataManager] Raw path: {rawCsvPath}");

        EnsureCsvHeader();
    }

    public void StartSessionTimer()
    {
        if (sessionStartTime < 0f)
        {
            sessionStartTime = Time.time;
            Debug.Log($"[DataManager] 세션 타이머 시작: {sessionStartTime}");
        }
    }

    // [수정됨] 키보드 입력 로그 저장 함수
    public void LogRawKey(string key)
    {
        if (!saveToCsv) return;

        // Raw 파일 헤더 체크 (index, key, time)
        if (!File.Exists(rawCsvPath))
        {
            try 
            {
                File.WriteAllText(rawCsvPath, "index,key,time\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Raw CSV init failed: {e}");
            }
        }

        // 1. 인덱스 증가
        rawKeyIndex++;

        // 2. 키 값 안전 처리
        string safeKey = key.Replace(",", "<COMMA>").Replace("\n", "<ENTER>");
        
        // 3. 시간 (밀리초 포함)
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // 요청하신 포맷: index, key, time
        string line = $"{rawKeyIndex},{safeKey},{time}\n";
        
        try
        {
            File.AppendAllText(rawCsvPath, line);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Raw logging failed: {e}");
        }
    }

    private void EnsureCsvHeader()
    {
        if (saveToCsv)
        {
            // 요약본 헤더
            if (!File.Exists(csvPath))
            {
                try
                {
                    File.WriteAllText(csvPath, "target,source,duration,wpm,cer,correctionRate,backspaceCount\n");
                }
                catch (Exception e) { Debug.LogError($"CSV init failed: {e}"); }
            }

            // [수정됨] Raw 데이터 헤더 (index, key, time)
            if (!File.Exists(rawCsvPath))
            {
                try
                {
                    File.WriteAllText(rawCsvPath, "index,key,time\n");
                }
                catch (Exception e) { Debug.LogError($"Raw CSV init failed: {e}"); }
            }
        }
    }

    public void SaveCurrentCsvAndReset()
    {
        if (!File.Exists(csvPath) && !File.Exists(rawCsvPath))
        {
            Debug.LogWarning("[DataManager] 저장할 파일이 없습니다.");
            return;
        }

        float totalSessionDuration = 0f;
        if (sessionStartTime >= 0f)
        {
            totalSessionDuration = Time.time - sessionStartTime;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string durationStr = totalSessionDuration.ToString("F0");

        // 1. 요약본 파일 변경
        if (File.Exists(csvPath))
        {
            string newFileName = $"typing_log_{timestamp}_{durationStr}s.csv";
            string newPath = Path.Combine(Application.persistentDataPath, newFileName);
            try
            {
                File.Move(csvPath, newPath);
            }
            catch (Exception e) { Debug.LogError($"파일 이동 실패(Summary): {e}"); }
        }

        // 2. Raw 파일 변경
        if (File.Exists(rawCsvPath))
        {
            string newRawFileName = $"typing_log_{timestamp}_{durationStr}s_raw.csv";
            string newRawPath = Path.Combine(Application.persistentDataPath, newRawFileName);
            try
            {
                File.Move(rawCsvPath, newRawPath);
                Debug.Log($"[DataManager] 저장 완료: {newRawFileName}");
            }
            catch (Exception e) { Debug.LogError($"파일 이동 실패(Raw): {e}"); }
        }

        // [중요] 초기화
        records.Clear();
        sessionStartTime = -1f;
        rawKeyIndex = 0; // 인덱스도 0으로 초기화 (다음 파일은 1부터 시작)

        if (typingCountScript != null) typingCountScript.ResetCount();
        else
        {
            var foundScript = FindAnyObjectByType<TypingCount>();
            if (foundScript != null) foundScript.ResetCount();
        }
        
        EnsureCsvHeader();
    }

    public void LogTrial(string target, string source, float durationSeconds, float wpm, float cer, float correctionRate, int backspaceCount)
    {
        var rec = new TypingRecord
        {
            targetText = target,
            sourceText = source,
            durationSeconds = durationSeconds,
            wpm = wpm,
            cer = cer,
            correctionRate = correctionRate,
            backspaceCount = backspaceCount
        };

        records.Add(rec);

        if (!saveToCsv) return;

        EnsureCsvHeader();

        string safeTarget = (target ?? "").Replace(",", " ");
        string safeSource = (source ?? "").Replace(",", " ");
        string line = $"{safeTarget},{safeSource},{durationSeconds:F3},{wpm:F3},{cer:F3},{correctionRate:F3},{backspaceCount}\n";
        
        File.AppendAllText(csvPath, line);
    }
}