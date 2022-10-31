using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class Balloon_Drag : MonoBehaviour
{
    [SerializeField]
    GameObject Balloon;//공중에 떠다니는 풍선 오브젝트
    private void Start()
    {

    }
    private void OnMouseUp()
    {
        /*Random_Move.isMove = true;*/
    }
    private void OnMouseDown()
    {

    }
    private void OnMouseDrag()
    {
        //마우스 좌표를 받아온다.
        Vector3 mousePosition
            = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 40);
        //마우스 좌표를 스크린 투 월드로 바꾸고 이 객체의 위치로 설정해 준다.
        this.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
