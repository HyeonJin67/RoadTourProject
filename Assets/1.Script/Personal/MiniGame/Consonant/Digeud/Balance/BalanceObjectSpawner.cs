using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceObjectSpawner : MonoBehaviour
{
    #region º¯¼ö
    public Vector3 center;
    public Vector3 size;
    #endregion



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(center, size);
    }
}
