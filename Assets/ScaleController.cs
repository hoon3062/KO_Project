using UnityEngine;
using TMPro;

public class ScaleController : MonoBehaviour
{
    
    public float offset = 1.0f;
    // public GameObject cube;
    public TextMeshProUGUI offsetText;
    public void ScalePositive() => AddOffset(0.1f);
    public void ScaleNegative() => AddOffset(-0.1f);

    public void ResetPosition()
    {
        offset = 1.0f;
        // if (cube != null)
        //     cube.transform.position = Vector3.zero;

        UpdateOffsetText();
    }

    private void AddOffset(float delta)
    {
        offset += delta;

        // if (cube != null)
        //     cube.transform.position = offset;

        UpdateOffsetText();
    }

    private void UpdateOffsetText()
    {
        if (offsetText != null)
            offsetText.text = $"offset = {{ {offset}}}";
    }

    void Start()
    {
        UpdateOffsetText();
    }

}