using UnityEngine;
using Unity.PolySpatial.InputDevices; //PolySpatial 입력장치 관련 클래스
using UnityEngine.InputSystem.EnhancedTouch; //향상된 터치 시스템 클래스
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch; //Touch로 간략화하여 참조
using TouchPhase = UnityEngine.InputSystem.TouchPhase; //TouchPhase로 간략화하여 참조
using UnityEngine.InputSystem.LowLevel;

public class MoveInteractor : MonoBehaviour
{
    private GameObject selectedObject; //선택된 객체를 저장할 변수
    private Vector3 lastPosition; //터치한 마지막 위치를 저장할 변수

    void OnEnable(){
        EnhancedTouchSupport.Enable();
    }

    void Update(){
        if (Touch.activeTouches.Count > 0){ //활성화된 터치가 있을 경우
            foreach (var touch in Touch.activeTouches){ //활성화 된 모든 터치 이벤트 순회
                SpatialPointerState touchData = EnhancedSpatialPointerSupport.GetPointerState(touch); //현재 터치 이벤트 상태를 가져와 touchData 객체 생성

                if (touchData.targetObject != null && touchData.Kind != SpatialPointerKind.Touch){ //Direct Touch 제외
                    GameObject rootObject = GetRootParent(touchData.targetObject);
                    if (touch.phase == TouchPhase.Began && rootObject.CompareTag("Movable")){ //터치가 시작되었을 때 초기 위치를 기록
                        selectedObject = rootObject; //선택한 오브젝트 햘댱
                        lastPosition = touchData.interactionPosition;
                    }
                    else if (touch.phase == TouchPhase.Moved && selectedObject != null){ //터치한 객체가 이동 중일 때 마지막 위치로부터 이동거리 계산 후 업데이트
                        Vector3 deltaPosition = touchData.interactionPosition - lastPosition; //움직인 위치 - 초기 위치로 이동거리 계산
                        selectedObject.transform.position += deltaPosition; //선택했던 오브젝트의 위치에 이동거리만큼 증가
                        lastPosition = touchData.interactionPosition;
                    }
                }
            }
        }
        if (Touch.activeTouches.Count == 0){ //선택 오브젝트 재설정
            selectedObject = null;
        }
    }
    private GameObject GetRootParent(GameObject obj)
    {
        Transform parent = obj.transform;
        while (parent.parent != null)
        {
            parent = parent.parent;
        }
        return parent.gameObject;
    }
}