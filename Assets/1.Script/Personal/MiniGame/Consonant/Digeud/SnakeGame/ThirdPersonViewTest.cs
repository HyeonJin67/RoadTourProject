using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//3ÀÎÄª ºä Å×½ºÆ®
public class ThirdPersonViewTest : MonoBehaviour
{
    [SerializeField]
    Transform TargetObject;
    public Vector3 CameraDistance;
    public Vector3 CameraAngle;

    private void Update()
    {
        transform.position = TargetObject.position + CameraDistance;
        transform.localRotation = Quaternion.Euler( CameraAngle);
    }
}
