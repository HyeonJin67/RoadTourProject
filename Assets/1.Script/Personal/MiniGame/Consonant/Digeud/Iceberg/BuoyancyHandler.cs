using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 부력 생성자
[RequireComponent(typeof(Rigidbody))]
public class BuoyancyHandler : MonoBehaviour
{
    #region 변수
    public Transform[] floaters;
    public Transform StarPaint;

    public float underWaterDrag = 3;
    public float underWaterAngularDrag = 1;
    public float airDrag = 0;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 15f;

    public float waterHeight = 0f;

    Rigidbody m_rigidbody;
    ScoreHandler score;

    bool underwater;

    int floatersUnderwater;
    int count = 0;
    float scale = 4;
    #endregion

    #region 함수
    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        score = FindObjectOfType<ScoreHandler>();
    }

    private void FixedUpdate()
    {
        floatersUnderwater = 0; 
        for(int i = 0; i < floaters.Length; i++)
        {
            float difference = floaters[i].position.y - waterHeight;
            if (difference < 0)
            {
                m_rigidbody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floaters[i].position, ForceMode.Force);
                floatersUnderwater += 1;

                if (!underwater)
                {
                    underwater = true;
                    SwitchState(true);
                }
            }
        }
       if (underwater && floatersUnderwater == 0)
        {
            underwater = false;
            SwitchState(false );
        }
    }

    void SwitchState(bool isUnderWater)
    {
        m_rigidbody.drag = isUnderWater ? underWaterDrag : airDrag;
        m_rigidbody.angularDrag = isUnderWater ? underWaterAngularDrag : airAngularDrag;
        if (isUnderWater)
        {
            m_rigidbody.drag = underWaterDrag;
            m_rigidbody.angularDrag = underWaterAngularDrag;
        }
        else
        {
            m_rigidbody.drag = airDrag;
            m_rigidbody.angularDrag = airAngularDrag;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<FishHandler>() && collision.transform.GetComponent<FishHandler>().State == FishState.Hook)
        {
            collision.transform.GetComponent<FishHandler>().State = FishState.OnIce;
            collision.transform.localScale = Vector3.one *scale;
            score.SetScore();
            count++;
        }
    }

    #endregion
}
