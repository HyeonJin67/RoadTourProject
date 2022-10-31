using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//이정표 회전용
public class PoleRotationHandle : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * 500);
    }
}
