using UnityEngine;

public class WindObjectController : MonoBehaviour
{
    #region
    [SerializeField]
    Camera cam;
    [SerializeField]
    int Count;
    [SerializeField]
    GameObject pre;
    #endregion

    private void Start()
    {   
        for(int i = 0; i < Count; i++)
        {   
            var obj = Instantiate(pre, transform);
            MoveToCamera(obj.transform);
            obj.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(RandomValue(), RandomValue(), RandomValue(), RandomValue());            
        }
    }

    public void MoveToCamera(Transform obj)
    {
        Vector3 dir = cam.transform.localRotation * Vector3.forward;                
        obj.transform.localPosition = new Vector3(Random.Range(-300, 300)/100f, Random.Range(-300, 300)/100f, transform.position.z + 200);                
        obj.transform.position += (dir * obj.GetSiblingIndex());
    }

    public float RandomValue()
    {
        float random = Random.Range(0, 100) / 100f;
        return random;
    }
}
