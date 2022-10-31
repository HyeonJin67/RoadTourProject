using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float dragDistance = 50.0f;     //드래그 거리
    private Vector3 touchStart;             //터치 시작 위치
    private Vector3 touchEnd;               //터치 종료 위치

    private Movement movement;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }
    private void Update()
    {
        if(Application.isMobilePlatform)//현재 모바일 플랫폼이면 
        {
            OnMobilePlatform();
        }
        else //현재 PC 플랫폼이면
        {
            OnPCPlatform();
        }
    }
    private void OnMobilePlatform()
    {
        //현재 화면을 터치하고 있지 않으면 메소드 종료
        if (Input.touchCount == 0) return;

        //첫 번째 터치 정보 불러오기 
        Touch touch = Input.GetTouch(0);

        //터치 시작
        if(touch.phase == TouchPhase.Began)
        {
            touchStart = touch.position;
        }
        //터치 & 드래그 
        else if (touch.phase == TouchPhase.Moved)
        {
            touchEnd = touch.position;

            OnDragXY();
        }
    }
    private void OnPCPlatform()
    {
        //터치 시작 
        if(Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
          
        }
        //터치 & 드래그
        else if (Input.GetMouseButton(0))
        {
            touchEnd = Input.mousePosition;

            OnDragXY();
        }
    }
    private void OnDragXY()
    {
        //터치 상태로 x축 드래그 범위가 dragDistance보다 클때 
        if(Mathf.Abs(touchEnd.x - touchStart.x) >= dragDistance)
        {
            movement.MoveToX((int)Mathf.Sign(touchEnd.x - touchStart.x));
            return;
        }

        //터치 상태로 y축 양의 방향으로 드래그 범위가 dragDistance 보다 클 때 
/*        if( touchEnd.y - touchStart.y >= dragDistance)
        {
            movement.MoveToY();
            return;
        }*/
    }
}
