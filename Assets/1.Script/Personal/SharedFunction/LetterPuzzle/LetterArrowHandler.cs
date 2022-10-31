using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// 화살표 제어자
public class LetterArrowHandler : MonoBehaviour
{
    [SerializeField]
    Transform ArrowView;
    [SerializeField]
    Camera UICamera;

    bool onDraw = false;

    Vector3 originPos;

    LetterDrawHandler letter;

    private void Start()
    {
        originPos = transform.position;
        letter = FindObjectOfType<LetterDrawHandler>();

        letter.Draw += OnDraw;        
    }

    public void OnDraw(PointerEventData eventData)
    {
        Vector3 vec = new Vector3(
              eventData.position.x,
              eventData.position.y,
              originPos.z
              ) ;
        transform.position = UICamera.ScreenToWorldPoint(vec);
        ArrowView.position = transform.position;
    }

    public void ArrowSpriteSetting()
    {

    }


    // 화살표 감추기
    public void HiddenView()
    {
        ArrowView.gameObject.SetActive(false);
    }

    public void SetPostion(Vector3 Pos)
    {
        transform.position = Pos;
        ArrowView.position = transform.position;
    }

    public void SetDraw(bool set)
    {
        onDraw = set;
    }

    public bool GetDraw()
    {
        return onDraw;
    }
}
