using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.AI;

public class CollisionChecker : MonoBehaviour
{
    //SerializeField
    [SerializeField]
    bool _isDrawCollisionBox   = false;
    [Header("Ground")]
    [SerializeField]
    float _groundCheckDistance = 0.1f;
    [SerializeField]
    float _groundCheckScale    = 2.1f;

    [SerializeField]
    CollisionBound[] _collisionBounds;

    //private
    private bool _isCheck  = false;
    private bool _isGround = false;
    //private bool _isHit    = false;
    private LayerMask  _noneMoveCollisionlayer;
    private LayerMask  _ignoreGroundlayer;
    private RaycastHit _rayHit;
    private Vector3    _limitPosition;

    //public Method
    public bool IsCheck  { set { _isCheck = value; } }
    public bool IsGround { get { return _isGround; } }
    public CollisionBound[] CollisionBounds { get { return _collisionBounds; } }
    public Vector3 LimitPosition { get { return _limitPosition; } }
    public bool IsAllTrigger
    {
        set
        {
            if (CollisionBounds != null)
            {
                for (int i = 0; i < CollisionBounds.Length; i++)
                {
                    CollisionBounds[i].Collider.isTrigger = value;
                }
            }
        }
    }

    //Event
    Action<Collider, Collider> _onTriggerBounds;

    public void Initialize(LayerMask noneMoveCollisionlayer, Action<Collider, Collider> onTriggerBounds)
    {
        
        _noneMoveCollisionlayer = noneMoveCollisionlayer;
        _isCheck = true;

        _onTriggerBounds = onTriggerBounds;

        InitCollisionBound();
    }
    

    void InitCollisionBound()
    {
        if (_collisionBounds != null)
        {
            for (int i = 0; i < _collisionBounds.Length; i++)
            {
                _collisionBounds[i].Initialize(OnTriggerBounds);
            }
        }
    }

    void Update()
    {
        if (_isCheck)
            GroundCheck();
    }
    
    void GroundCheck()
    {
        _isGround = false;
        
        RaycastHit hit;

        for (int i = 0; i < 3; i++)
        {
            Vector3 position = transform.position;
            if (i==1)
                position = new Vector3(transform.position.x - (_groundCheckScale/2f), transform.position.y, transform.position.z);
            else if (i ==2)
                position = new Vector3(transform.position.x + (_groundCheckScale/2f), transform.position.y, transform.position.z);
            else
                position = transform.position;
            
            Ray ray = new Ray(position, Vector3.down);
            if (Physics.Raycast(ray, out hit, 5f, ~_ignoreGroundlayer))
            {
                if (hit.distance < _groundCheckDistance)
                {
                    if (hit.transform.tag == "Ground")
                    {
                        _isGround = true;
                        _limitPosition = new Vector3(transform.position.x, hit.point.y + _groundCheckDistance, transform.position.z);
                    }
                }
            }

            Debug.DrawRay(position,Vector3.down * _groundCheckDistance);
        }
    }

    public CollisionBound GetCollisionBound(string name)
    {
        for (int i = 0; i < _collisionBounds.Length; i++)
        {
            if (_collisionBounds[i].BoundName == name)
            {
                return _collisionBounds[i];
            }
        }
        return null;
    }
    
    void OnTriggerBounds(Collider collider, Collider other)
    {
        _onTriggerBounds(collider, other);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_collisionBounds == null)
            return;

        for (int i = 0; i < _collisionBounds.Length; i++)
        {
            if (_collisionBounds[i] == null)
                continue;

            if (_collisionBounds[i].gameObject.activeSelf)
            {
                var worldPosition = _collisionBounds[i].transform.position;
                Handles.Label(worldPosition, _collisionBounds[i].BoundName);
            }
        }

        if (_isDrawCollisionBox)
        {
            CollisionBound collisionBound = GetCollisionBound("Main");
            if (collisionBound != null)
            {
                float checkDistance = _groundCheckDistance + (_groundCheckScale / 2f);
                Vector3 size = new Vector3(_groundCheckScale, _groundCheckScale, _groundCheckScale);
                Vector3 to = _limitPosition + (Vector3.down * _groundCheckDistance);
                if (!_isGround)
                    to = transform.position + (Vector3.down * _groundCheckDistance);

                Handles.Label(to, "GroundCheck");
                Gizmos.DrawLine(collisionBound.transform.position, to);
                DrawWireCube(to, Quaternion.identity, size);
            }
        }

    }
    public void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }
#endif
}
