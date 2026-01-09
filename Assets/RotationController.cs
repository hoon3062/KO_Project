using UnityEngine;
using TMPro;

public class RotationController : MonoBehaviour
{
    

    // public GameObject cube;              // 회전시킬 Cube
    public TextMeshProUGUI offsetText;   // UI 텍스트 표시용

    public Vector3 offset = Vector3.zero; // 회전 누적 오프셋 (Euler)

    void Start()
    {
        UpdateOffsetText();
    }

    // X축 회전
    public void RotateXPositive() => AddOffset(new Vector3(5, 0, 0));
    public void RotateXNegative() => AddOffset(new Vector3(-5, 0, 0));

    // Y축 회전
    public void RotateYPositive() => AddOffset(new Vector3(0, 5, 0));
    public void RotateYNegative() => AddOffset(new Vector3(0, -5, 0));

    // Z축 회전
    public void RotateZPositive() => AddOffset(new Vector3(0, 0, 5));
    public void RotateZNegative() => AddOffset(new Vector3(0, 0, -5));

    // Reset 버튼
    public void ResetRotation()
    {
        offset = Vector3.zero;
        // if (cube != null)
        //     cube.transform.rotation = Quaternion.identity;

        UpdateOffsetText();
    }

    // 공통 회전 처리
    private void AddOffset(Vector3 delta)
    {
        offset += delta;

        // if (cube != null)
        //     cube.transform.rotation = Quaternion.Euler(offset);

        UpdateOffsetText();
    }

    // UI 텍스트 갱신
    private void UpdateOffsetText()
    {
        if (offsetText != null)
            offsetText.text = $"offset = {{ {offset.x}, {offset.y}, {offset.z} }}";
    }
}
