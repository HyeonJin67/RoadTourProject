using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;


public enum ECollider 
{
    Box,
    Capsule,
    Sphere,
}

public class CollisionBound : MonoBehaviour {

    [SerializeField]
    string _name;
    [SerializeField]
    ECollider _type;

    Collider _collider;

    Vector3 _center = Vector3.zero;
    Vector3 _size   = Vector3.zero;
    float _radius   = 0;
    float _height   = 0;
    
    
    public string BoundName { get { return _name; } }
    public Collider Collider{get{return _collider !=null ? _collider : GetComponent<Collider>();}}

    Action<Collider, Collider> _onTriggerEnter;

    Vector3 _originLocalPosition;
    Vector3 _originScale;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider is BoxCollider)
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            _center = boxCollider.center;
            _size   = boxCollider.size;
        }
        else if(_collider is CapsuleCollider)
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            _center = capsuleCollider.center;
            _radius = capsuleCollider.radius;
            _height = capsuleCollider.height;
        }
        else if (_collider is SphereCollider)
        {
            SphereCollider SphereCollider = GetComponent<SphereCollider>();
            _center = SphereCollider.center;
            _radius = SphereCollider.radius;
        }

        _originLocalPosition = transform.localPosition;
        _originScale         =  transform.lossyScale;

    }
    public void Reset()
    {
        transform.localPosition = _originLocalPosition;
        transform.localScale    = _originScale;
        SetActive(true,0f);
    }
    public void Initialize(Action<Collider, Collider> onTriggerEnter)
    {
        if(onTriggerEnter!=null)
            _onTriggerEnter = onTriggerEnter;
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (_onTriggerEnter != null)
            _onTriggerEnter(_collider, other);
    }
    
    public void SetActive(bool isActive, float time=0)
    {
        if (isActive)
            this.MonoInvoke(OnActive, time);
        else
            this.MonoInvoke(OffActive, time);
    }

    void OnActive()
    {
        gameObject.SetActive(true);
    }
    void OffActive()
    {
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {

    }
}
