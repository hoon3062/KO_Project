using UnityEngine;

public class KeyboardVisibilityAlert : MonoBehaviour
{
    [Header("설정")]
    public Transform targetKeyboard;      // 감지할 키보드 객체
    public GameObject recenterGuideUI;    // 안내 UI 패널

    [Header("사용자 머리 설정")]
    // [수정] Camera 대신 Transform(GameObject의 위치정보)을 직접 받음
    // Vision Pro의 'CenterEyeAnchor' 또는 'Main Camera' 오브젝트를 넣으세요.
    public Transform userHead;        

    [Header("민감도")]
    [Tooltip("시야각(FOV) 설정. 이 각도 안에 있으면 '보인다'고 판단합니다. (기본 45도)")]
    [Range(20f, 90f)]
    public float fieldOfView = 45f; 

    private void Start()
    {
        // 만약 인스펙터에서 머리를 연결하지 않았다면, 메인 카메라의 Transform을 찾음
        if (userHead == null)
        {
            if (Camera.main != null)
            {
                userHead = Camera.main.transform;
                Debug.Log("[KeyboardVisibilityAlert] userHead가 없어 Main Camera를 참조합니다.");
            }
            else
            {
                Debug.LogError("[KeyboardVisibilityAlert] userHead를 찾을 수 없습니다! 인스펙터에서 할당해주세요.");
            }
        }

        // 시작할 때는 안내 문구 숨김
        if (recenterGuideUI != null) 
            recenterGuideUI.SetActive(false);
    }

    private void LateUpdate()
    {
        // 필수 요소가 없거나 키보드가 꺼져있으면 작동 중지
        if (userHead == null || targetKeyboard == null || !targetKeyboard.gameObject.activeInHierarchy)
        {
            if (recenterGuideUI != null && recenterGuideUI.activeSelf) 
                recenterGuideUI.SetActive(false);
            return;
        }

        CheckVisibilityByAngle();
        Debug.Log("[KeyboardVisibilityAlert] 머리 위치: " + userHead.position);
    }

    private void CheckVisibilityByAngle()
    {
        // 1. 머리에서 키보드로 향하는 방향 벡터 계산
        Vector3 directionToTarget = targetKeyboard.position - userHead.position;

        // 2. 머리의 정면(forward)과 키보드 방향 사이의 각도 계산 (0 ~ 180도)
        float angle = Vector3.Angle(userHead.forward, directionToTarget);

        // 3. 거리 계산 (너무 뒤에 있는 경우 방지용, 필요 시 사용)
        // float distance = directionToTarget.magnitude;

        // 4. 시야 판별
        // 각도가 설정한 FOV보다 작으면 "시야 안에 있음"
        bool isOnScreen = angle < fieldOfView;

        if (isOnScreen)
        {
            // 시야 안에 있음 -> 안내 끄기
            if (recenterGuideUI.activeSelf) recenterGuideUI.SetActive(false);
            Debug.Log("[KeyboardVisibilityAlert] 키보드가 시야 안에 있습니다.");
        }
        else
        {
            // 시야 밖에 있음 -> 안내 켜기
            if (!recenterGuideUI.activeSelf) recenterGuideUI.SetActive(true);
            Debug.Log("[KeyboardVisibilityAlert] 키보드가 시야 밖에 있습니다. 재조정이 필요합니다.");
        }
    }
}