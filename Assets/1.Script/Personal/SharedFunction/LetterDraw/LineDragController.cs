using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 마우스 드래그 커서
public class LineDragController : MonoBehaviour
{
    #region 변수
    [SerializeField]
    Camera _camera;

    [SerializeField]
    Transform startPoint;
    Vector3 pos;

    [SerializeField]
    GameObject Particle;
    #endregion
    
    #region 함수
    private void Start()
    {
        pos = transform.position;
        SetStartPoint(0);
    }

    public void ShowMouse(PointerEventData eventData)
    {                
        Vector3 vec = new Vector3(
            eventData.position.x,
            eventData.position.y,
            pos.z
           );
        transform.position = _camera.ScreenToWorldPoint(vec);        
    }

    public void SetStartPoint(int num)
    {
        transform.position = startPoint.GetChild(num).position;
    }

    public void ParticleMake()
    {
        var ex = Instantiate(Particle, transform.GetChild(0));
        ex.transform.position = transform.position;
        ex.transform.localScale = Vector3.one * 75;
    }
    #endregion
}
