using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform CameraTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (CameraTransform == null)
        {
            CameraTransform = Camera.main.transform;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraTransform != null)
        {
            transform.position = CameraTransform.position;
            transform.LookAt(CameraTransform.position + CameraTransform.forward);
        }
    }
}
