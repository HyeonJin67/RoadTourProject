using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DRunDotoriHandler : MonoBehaviour
{
    #region 변수
    [SerializeField]
    Transform target;
    float firingAngle = 45.0f;
    float gravity = 9.8f;

    Transform bullet;
    Transform myTransform;

    Vector3 rotate;
    #endregion

    public void Shotting()
    {
        StartCoroutine(SimulateProjectile());
    }

    private void OnEnable()
    {
        bullet = transform;
        myTransform = FindObjectOfType<DRunPlayerController>().transform;
        target = FindObjectOfType<DRunNpcMoveController>().transform;
        rotate = transform.eulerAngles;
    }

    IEnumerator SimulateProjectile()
    {
        transform.localScale = Vector3.one * 15;
        bullet.position = myTransform.position + new Vector3(0, 0, 0);

        float target_Distance = Vector3.Distance(bullet.position, target.position);
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        // 속도 값
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // 체공 시간
        float flightDuration = target_Distance / Vx;
        bullet.rotation = Quaternion.LookRotation(target.position - bullet.position);

        float elapse_time = 0;

        while (elapse_time < flightDuration)
        {
            bullet.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            yield return null;
        }
    }

    public void Pop()
    {
        Vector3 nes = new Vector3(transform.parent.position.x, transform.parent.position.y + 2f, transform.parent.position.z);
        transform.DORotate(new Vector3(36, 0, 0), 0.2f).SetLoops(10, LoopType.Incremental).SetEase(Ease.Linear).OnComplete(() => transform.rotation = Quaternion.Euler(rotate));
        transform.DOMove(nes, 1).SetLoops(2, LoopType.Yoyo).OnComplete(() => { transform.position = transform.parent.position; transform.localScale = Vector3.one * 0.7f; });
    }

}
