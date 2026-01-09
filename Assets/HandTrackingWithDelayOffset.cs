using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;

/// <summary>
/// 핸드 트래킹 데이터(루트 및 관절)를 저장하는 패킷
/// </summary>
public struct HandDataPacket
{
    public float Timestamp;
    public Pose RootPose;
    public Pose[] JointLocalPoses;
}

public class HandTrackingWithDelayOffset : XRHandSkeletonDriver
{
    [Header("Delay Control")]
    [SerializeField] private DelayController delayController;

    // 딜레이 값을 저장할 변수
    private float m_CurrentDelayOffset = 0.0f;

    // 데이터 큐 및 배열 풀
    private readonly Queue<HandDataPacket> m_DataQueue = new Queue<HandDataPacket>();
    private readonly Queue<Pose[]> m_PoseArrayPool = new Queue<Pose[]>();
    private const int k_PoolSize = 100;
    private int m_JointCount;

    // 최신 트래킹 데이터 임시 저장
    private Pose m_LatestRootPose = Pose.identity;
    private bool m_HasNewRootData = false;
    private bool m_HasNewJointData = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_JointCount = XRHandJointID.EndMarker.ToIndex();

        // 배열 풀 초기화
        for (int i = 0; i < k_PoolSize; i++)
        {
            m_PoseArrayPool.Enqueue(new Pose[m_JointCount]);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // 큐 정리
        while (m_DataQueue.Count > 0)
        {
            var packet = m_DataQueue.Dequeue();
            if (packet.JointLocalPoses != null)
                m_PoseArrayPool.Enqueue(packet.JointLocalPoses);
        }

        m_PoseArrayPool.Clear();
    }

    // 다른 스크립트와의 실행 순서 충돌 방지를 위해 LateUpdate 사용
    private void LateUpdate()
    {
        // 1. 현재 딜레이 값 가져오기
        m_CurrentDelayOffset = (delayController != null) ? delayController.offset : 0.0f;

        // 2. 새 데이터 캡처 및 큐 저장
        if (m_HasNewRootData && m_HasNewJointData)
        {
            // 풀에서 배열 가져오기 (없으면 생성)
            Pose[] jointPoseBuffer = (m_PoseArrayPool.Count > 0)
                ? m_PoseArrayPool.Dequeue()
                : new Pose[m_JointCount];

            // 데이터 복사
            m_JointLocalPoses.CopyTo(jointPoseBuffer);

            // 큐에 추가
            m_DataQueue.Enqueue(new HandDataPacket
            {
                Timestamp = Time.time,
                RootPose = m_LatestRootPose,
                JointLocalPoses = jointPoseBuffer
            });

            m_HasNewRootData = false;
            m_HasNewJointData = false;
        }

        // 3. 딜레이된 데이터 적용
        float targetTime = Time.time - m_CurrentDelayOffset;
        HandDataPacket packetToApply = default;
        bool hasPacketToApply = false;

        // 딜레이 시간이 지난 패킷 중 가장 최신 것 찾기
        while (m_DataQueue.Count > 0 && m_DataQueue.Peek().Timestamp <= targetTime)
        {
            var dequeuedPacket = m_DataQueue.Dequeue();

            if (hasPacketToApply)
            {
                m_PoseArrayPool.Enqueue(packetToApply.JointLocalPoses);
            }

            packetToApply = dequeuedPacket;
            hasPacketToApply = true;
        }

        if (hasPacketToApply)
        {
            ApplyDelayedPoses(packetToApply);
            m_PoseArrayPool.Enqueue(packetToApply.JointLocalPoses);
        }

        if (delayController != null)
        {
            // 이 로그가 콘솔에 0으로 찍히는지, 설정한 값(예: 0.1)으로 찍히는지 확인
            Debug.Log($"Current Offset in Script: {delayController.offset}"); 
        }
        else
        {
            Debug.LogError("DelayController is NULL!");
        }
    }

    // 실시간 업데이트 방지 및 데이터 캡처용 오버라이드
    protected override void OnRootPoseUpdated(Pose rootPose)
    {
        m_LatestRootPose = rootPose;
        m_HasNewRootData = true;
    }

    protected override void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
    {
        base.UpdateJointLocalPoses(args);
        m_HasNewJointData = true;
    }

    private void ApplyDelayedPoses(HandDataPacket packet)
    {
        // A. 루트 트랜스폼 적용 (오프셋 없음)
        if (m_HasRootTransform)
        {
            rootTransform.localPosition = packet.RootPose.position;
            rootTransform.localRotation = packet.RootPose.rotation;
        }

        // B. 관절 트랜스폼 적용
        for (var i = 0; i < m_JointCount; i++)
        {
            if (m_HasJointTransformMask[i] && m_JointTransforms[i] != null)
            {
                m_JointTransforms[i].SetLocalPose(packet.JointLocalPoses[i]);
            }
        }
    }
}
