using UnityEngine;
public class WindBreakerHandler : MonoBehaviour
{
    #region 변수
    [SerializeField]
    bool isBlowable = false;

    WindBlowController wind;
    #endregion
    private void OnEnable()
    {
        isBlowable = false;
        wind = FindObjectOfType<WindBlowController>();
        transform.name = transform.GetSiblingIndex().ToString();
        wind.StageClear += Blow;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<WindBreakerHandler>())
        {
            if (other.transform.GetSiblingIndex() > transform.GetSiblingIndex())
            {
                isBlowable = true;
            }
            else
            {
                isBlowable = false;
            }
        }
        else
        {
            isBlowable = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WindBlowController>())
        {
            if (isBlowable)
            {   
                Blow();
            }
        }
    }

    public void Blow()
    {
        transform.localPosition = new Vector3(-100, 100, 0);
    }

    private void OnDestroy()
    {
        wind.StageClear -= Blow;
    }

    #region 파라미터 변수
    public bool Blowable { get => isBlowable; }
    #endregion
}
