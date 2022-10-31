using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//공의 움직임
public class WASDMoveHandle : MonoBehaviour
{
    private Rigidbody mRigidBody;
    private AudioSource mAudioSource;
    public AudioClip JumpSound;
    public AudioClip HitSound;
    public Camera ViewCamera;

    void Start()
    {
        mRigidBody = GetComponent<Rigidbody>();
        mAudioSource = GetComponent<AudioSource>();
    }
    void FixedUpdate()
    {
        if (mRigidBody != null)
        {
            //키보드의 WASD방향키 조작
            if (Input.GetButton("Horizontal"))
            {
                //mRigidBody.AddTorque(Vector3.back * Input.GetAxis("Horizontal")*10);
                mRigidBody.AddForce(Vector3.right *Input.GetAxis("Horizontal")*10); //Rigidbody의 Use Gravity사용해줘야 제대로 적용
            }
            if (Input.GetButton("Vertical"))
            {
                //mRigidBody.AddTorque(Vector3.right * Input.GetAxis("Vertical")*10);
                mRigidBody.AddForce(Vector3.forward *Input.GetAxis("Vertical")*10); //Rigidbody의 Use Gravity사용해줘야 제대로 적용
            }
            if (Input.GetButtonDown("Jump"))
            {
                if (mAudioSource != null && JumpSound != null)
                {
                    mAudioSource.PlayOneShot(JumpSound);
                }
                mRigidBody.AddForce(Vector3.up*200);
            }
        }
        //공을 따라다니는 카메라 시점
        if (ViewCamera != null)
        {
            Vector3 direction = (Vector3.up+Vector3.back);
            RaycastHit hit;
            Debug.DrawLine(transform.position, transform.position, Color.red);
            if (Physics.Linecast(transform.position, transform.position, out hit))
            {
                ViewCamera.transform.position = hit.point;
            }
            else
            {
                ViewCamera.transform.position = transform.position;
            }
            ViewCamera.transform.LookAt(transform.position + transform.forward);
        }
    }
}
   
