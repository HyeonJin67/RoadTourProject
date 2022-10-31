using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceController : MonoBehaviour
{
    #region 변수
    [SerializeField]
    Transform spring, dish;
    
    [SerializeField]
    GameObject[] Spot;

    private Vector3 DishPos;
    private float BF;
    #endregion

    #region 이벤트
    public event System.Action Fallen;
    public event System.Action Reset;    
    #endregion

    #region 함수

    private void Start()
    {
        DishPos = dish.position;        
        
        BF = spring.GetComponent<SpringJoint>().spring;

        Fallen += OverWeight;
        Reset += ResetBalance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Fallen?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reset?.Invoke();
        }
    }

    public void ResetBalance()
    {
        dish.position = DishPos;
        dish.rotation = Quaternion.Euler(Vector3.zero);

        spring.GetComponent<SpringJoint>().spring = BF;

        foreach (var p in Spot)
        {
            p.GetComponent<LineRenderer>().enabled = true;
        }
    }

    public void OverWeight()
    {
        
        spring.GetComponent<SpringJoint>().spring = 1;
        dish.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.Impulse);

        foreach (var p in Spot)
        {
            p.GetComponent<LineRenderer>().enabled = false;
        }
    }
    #endregion
}
