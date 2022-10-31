using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField]
    string _name ="";
    [SerializeField]
    float _destroyTime =1f;
    
    public string Name { get { return _name; } }
	void Start ()
    {
        Destroy(gameObject, _destroyTime);
	}
}
