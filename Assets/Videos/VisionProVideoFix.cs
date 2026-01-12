using UnityEngine;
using UnityEngine.Video;
#if UNITY_VISIONOS
using Unity.PolySpatial; // 비전 프로 빌드 때만 포함
#endif

[RequireComponent(typeof(VideoPlayer))]
public class VisionProVideoFix : MonoBehaviour
{
    private VideoPlayer _videoPlayer;

    void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        // 비디오가 재생 중이고, 타겟 렌더 텍스처가 있을 때만 갱신 요청
        if (_videoPlayer != null && _videoPlayer.isPlaying && _videoPlayer.targetTexture != null)
        {
#if UNITY_VISIONOS
            // PolySpatial에게 이 텍스처가 변경되었음을 알림 (매 프레임 호출 필수)
            PolySpatialObjectUtils.MarkDirty(_videoPlayer.targetTexture);
#endif
        }
    }
}