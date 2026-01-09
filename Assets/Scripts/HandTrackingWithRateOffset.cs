using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using System.IO;
using System;

public class HandTrackingWithRateOffset : XRHandSkeletonDriver
{
    [Header("Rate Control")]
    [Tooltip("주사율 제어 컨트롤러")]
    [SerializeField] private FrequencyController freqController;
    
    [Tooltip("기본 주사율 (컨트롤러 없을 시)")]
    [Range(1, 120)] public float defaultHz = 90.0f;

    [Header("Debug Info")]
    [SerializeField] private bool showDebugLog = false;

    [Header("CSV Logging")]
    [SerializeField] private bool saveLogToCsv = true;
    [SerializeField] private string csvFileNamePrefix = "Rate_Stable_Log"; 

    // ================= 측정 변수들 =================
    public float DisplayedHandHz { get; private set; } = 0.0f;
    public float CurrentTargetHz { get; private set; } = 0.0f;

    // 카운터 및 타이머
    private int m_RenderCount = 0;
    private float m_RateTimer = 0.0f;
    
    // [핵심] Stable Mode를 위한 단순 타이머 변수
    private float m_LastCollectionTime = 0.0f; 

    // [추가] dt 계산을 위한 마지막 로그 시간 저장 (정밀도 위해 double 사용)
    private double m_LastLoggedTime = double.NaN;

    // [핵심] 허용 오차 (약 2ms)
    private const float k_Tolerance = 0.002f;

    private string m_FullFilePath;

    // 데이터 저장용
    private Pose m_LatestRootPose = Pose.identity;
    private Pose[] m_LatestJointPoses;
    
    private int m_JointCount;
    private bool m_HasNewRootData = false;
    private bool m_HasNewJointData = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_JointCount = XRHandJointID.EndMarker.ToIndex();
        m_LatestJointPoses = new Pose[m_JointCount];
        
        m_RenderCount = 0;
        m_RateTimer = 0.0f;
        m_LastCollectionTime = Time.time; 
        
        // [초기화] 첫 프레임 계산 방지용 NaN 설정
        m_LastLoggedTime = double.NaN;

        if (saveLogToCsv) CreateCsvFile();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    void LateUpdate()
    {
        UpdateDebugStatistics();

        if (m_HasNewJointData)
        {
            ApplyPoses();
            m_HasNewJointData = false; 
            m_RenderCount++; 
        }
    }

    // --- [핵심] 데이터 수집 단계 (Stable Logic) ---
// --- [핵심] 데이터 수집 단계 (Stable Logic + Bypass) ---
    protected override void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
    {
        // 1. 현재 목표 Hz 확인
        CurrentTargetHz = (freqController != null) ? freqController.currentHz : defaultHz;
        if (CurrentTargetHz <= 0.1f) CurrentTargetHz = 0.1f;

        // [수정됨] 80Hz 이상(사실상 Max Rate)을 원하면 타이머 체크 없이 무조건 통과!
        // 이유: 90Hz 환경에서 90Hz를 제한하려다 미세한 프레임 떨림으로 데이터를 버리는 사고를 방지함.
        if (CurrentTargetHz < 80.0f)
        {
            float updateInterval = 1.0f / CurrentTargetHz;

            // 시간 체크 (오차 허용 범위 적용)
            if (Time.time - m_LastCollectionTime < updateInterval - k_Tolerance)
            {
                return; // 아직 시간 안 됨 -> Skip
            }
        }

        // 3. 통과! -> 기준 시간을 '현재'로 리셋
        m_LastCollectionTime = Time.time;

        // 4. 데이터 복사
        base.UpdateJointLocalPoses(args);
        m_JointLocalPoses.CopyTo(m_LatestJointPoses);
        m_HasNewJointData = true;

        // 5. 로그 기록
        if (saveLogToCsv)
        {
            LogEventToCsv();
        }
    }

    protected override void OnRootPoseUpdated(Pose rootPose)
    {
        m_LatestRootPose = rootPose;
        m_HasNewRootData = true;
    }

    private void ApplyPoses()
    {
        if (m_HasRootTransform && m_HasNewRootData)
        {
            rootTransform.localPosition = m_LatestRootPose.position;
            rootTransform.localRotation = m_LatestRootPose.rotation;
        }

        for (var i = 0; i < m_JointCount; i++)
        {
            if (m_HasJointTransformMask[i] && m_JointTransforms[i] != null)
            {
                m_JointTransforms[i].SetLocalPose(m_LatestJointPoses[i]);
            }
        }
    }

    private void UpdateDebugStatistics()
    {
        if (!showDebugLog) return;

        m_RateTimer += Time.deltaTime;
        if (m_RateTimer >= 1.0f)
        {
            DisplayedHandHz = m_RenderCount / m_RateTimer;
            Debug.Log($"[RateControl] Target: {CurrentTargetHz:F0} | Actual: {DisplayedHandHz:F1}");
            m_RenderCount = 0;
            m_RateTimer = 0.0f;
        }
    }

    // --- CSV 관련 (Event Log) ---
    private void CreateCsvFile()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{csvFileNamePrefix}_{timestamp}.csv";
        m_FullFilePath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            // [수정] 헤더에 dt, ActualHz, ErrHz 추가
            File.WriteAllText(m_FullFilePath, "Time,Event,TargetHz,dt,ActualHz,ErrHz\n");
            Debug.Log($"Event Log File Created: {m_FullFilePath}");
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    private void LogEventToCsv()
    {
        try
        {
            // double now = Time.timeAsDouble;
            double now = Time.realtimeSinceStartupAsDouble;
            // [요청하신 계산 로직 적용]
            double dt = double.IsNaN(m_LastLoggedTime) ? double.NaN : (now - m_LastLoggedTime);
            double actualHz = (!double.IsNaN(dt) && dt > 0.0) ? (1.0 / dt) : double.NaN;
            double errHz = (!double.IsNaN(actualHz)) ? (actualHz - (double)CurrentTargetHz) : double.NaN;

            // CSV 저장 (F6: 소수점 6자리까지 정밀 기록)
            // NaN인 경우 CSV에는 "NaN"으로 기록되어 파이썬 등에서 읽기 용이함
            string line = $"{now:F4},1,{CurrentTargetHz:F1},{dt:F6},{actualHz:F3},{errHz:F3}\n";
            
            File.AppendAllText(m_FullFilePath, line);

            // 마지막 기록 시간 갱신
            m_LastLoggedTime = now;
        }
        catch (Exception e) { Debug.LogError($"CSV Write Error: {e.Message}"); }
    }
}