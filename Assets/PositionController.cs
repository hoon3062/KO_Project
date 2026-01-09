using UnityEngine;
using TMPro;

public class PositionController : MonoBehaviour
{
    

    public Vector3 offset = Vector3.zero;
    // public GameObject cube;
    public TextMeshProUGUI offsetText;
    public void MoveXPositive() => AddOffset(new Vector3(0.1f, 0, 0));
    public void MoveXNegative() => AddOffset(new Vector3(-0.1f, 0, 0));

    public void MoveYPositive() => AddOffset(new Vector3(0, 0.1f, 0));
    public void MoveYNegative() => AddOffset(new Vector3(0, -0.1f, 0));

    public void MoveZPositive() => AddOffset(new Vector3(0, 0, 0.1f));
    public void MoveZNegative() => AddOffset(new Vector3(0, 0, -0.1f));

    public void ResetPosition()
    {
        offset = Vector3.zero;
        // if (cube != null)
        //     cube.transform.position = Vector3.zero;

        UpdateOffsetText();
    }

    private void AddOffset(Vector3 delta)
    {
        offset += delta;

        // if (cube != null)
        //     cube.transform.position = offset;

        UpdateOffsetText();
    }

    private void UpdateOffsetText()
    {
        if (offsetText != null)
            offsetText.text = $"offset = {{ {offset.x}, {offset.y}, {offset.z} }}";
    }

    void Start()
    {
        UpdateOffsetText();
    }

}