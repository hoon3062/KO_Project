using UnityEngine;
using UnityEngine.XR.Hands;

public class HandTrackingWithOffset : XRHandSkeletonDriver
{
    [SerializeField] private RotationController rotationController;
    [SerializeField] private PositionController positionController;
    [SerializeField] private ScaleController scaleController;
    private Vector3 positionOffset;
    private Vector3 rotationOffset;
    private float scaleOffset;

    void Update()
    {
        if (rotationController != null)
        {
            rotationOffset = rotationController.offset;
            Debug.Log("offset has updated");
        }
        else Debug.Log("cannot find rotationController");
            
        if (positionController != null)
            positionOffset = positionController.offset;
        if (scaleController != null)
            scaleOffset = scaleController.offset;
        
    }
    protected override void OnRootPoseUpdated(Pose rootPose)
    {
        base.OnRootPoseUpdated(rootPose);

        // Apply position offset
        rootTransform.localPosition += positionOffset;

        // Apply rotation offset
        rootTransform.localRotation *= Quaternion.Euler(rotationOffset);
        rootTransform.localScale = Vector3.one * scaleOffset;
    }
}
