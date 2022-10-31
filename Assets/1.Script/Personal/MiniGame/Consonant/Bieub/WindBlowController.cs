using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBlowController : MonoBehaviour
{
    #region
    bool gameStart = true;
    bool isMove = false;
    
    Vector3 originRoation;
    
    private Vector3 screenSpace;
    private Vector3 offset;
    #endregion

    #region ÀÌº¥Æ®
    public event System.Action StageClear;
    #endregion

    private void Start()
    {
        originRoation = transform.rotation.eulerAngles;

    }
    private void OnMouseDown()
    {   
        if (gameStart)
        {
            isMove = true;
            screenSpace = Camera.main.WorldToScreenPoint(transform.position);
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
        }
    }

    private void OnMouseDrag()
    {
        if (isMove)
        {
            var curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
            transform.position = curPosition;
        }
    }
    private void OnMouseUp()
    {
        if (isMove)
            isMove = false; 
    }


    #region
    public bool GetIsMove { get => isMove; }
    #endregion
}
