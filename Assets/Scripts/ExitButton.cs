using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // 버튼의 OnClick 이벤트에 연결할 함수
    public void QuitApplication()
    {
        Debug.Log("[ExitButton] 프로그램 종료를 요청했습니다.");

#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중일 때는 플레이 모드를 중지
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 앱에서는 어플리케이션 종료
        Application.Quit();
#endif
    }
}