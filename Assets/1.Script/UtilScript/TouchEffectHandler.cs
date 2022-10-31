using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchEffectHandler : MonoBehaviour, IPointerDownHandler
{
    #region 변수
    [Header("클릭 파티클 목록")]
    [SerializeField]
    GameObject[] touchEffect;
    Vector3 _mousePos;
    #endregion

    private void Start()
    {
        if (transform.GetComponentInParent<Canvas>())
        {
            gameObject.AddComponent<UnityEngine.UI.Image>();
            GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);            
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }        
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(transform.root.GetComponent<Canvas>())
        {
            int num = Random.Range(0, touchEffect.Length);
            var te = Instantiate(touchEffect[num]);
            Vector3 vec = new Vector3(
                eventData.position.x,
                eventData.position.y,
                Camera.main.nearClipPlane
                );
            te.transform.position = Camera.main.ScreenToWorldPoint(vec);
            te.transform.localScale = Vector3.one * 0.025f;
        }
    }

    private void Update()
    {   
        if (Input.GetMouseButtonDown(0))
        {
            if (!transform.root.GetComponent<Canvas>())
            {
                _mousePos = Input.mousePosition;
                _mousePos.z = -Camera.main.transform.position.z;
                int num = Random.Range(0, touchEffect.Length);
                var te = Instantiate(touchEffect[num], transform);
                te.transform.position = Camera.main.ScreenToWorldPoint(_mousePos);                
            }
        }
    }
}
