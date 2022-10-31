using UnityEngine;


//입력된 벡터를 이용하여 방향성만을 체크
public class CheckDirectionScript : MonoBehaviour
{
    #region 이벤트
    public System.Func<Vector2, Vector2, bool> CheckDir;
    #endregion

    private void Awake()
    {
        CheckDir += (Vector2 a, Vector2 b) =>
        {
            return Vector2.Dot(a.normalized, b.normalized) > 0;
        };
    }
}
