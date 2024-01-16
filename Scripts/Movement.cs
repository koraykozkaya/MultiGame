using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Movement : MonoBehaviourPunCallbacks
{

    public float speed;
    public float sprintModifer;
    public float jumpForce;
    public LayerMask ground;
    public Transform groundDetector;
    public GameObject cameraParent;
    public Transform weaponParent;
    private float movementCounter;
    private float idleCounter;
    public Camera normalCam;
    private Rigidbody rg;
    private Vector3 weaponParentOrigin;
    private Vector3 targetWeaponBobPosition;
    
    private float baseFOV;
    private float sprintFOVModifer = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        cameraParent.SetActive(photonView.IsMine);

        if(!photonView.IsMine)
        {
            gameObject.layer = 9;
        }

        
        baseFOV = normalCam.fieldOfView;
        if (Camera.main) Camera.main.enabled = false;

        rg = GetComponent<Rigidbody>();

        
        weaponParentOrigin = weaponParent.localPosition;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        bool isGrounding = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounding;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounding;

        if(isJumping)
        {
            rg.AddForce(Vector3.up * jumpForce);
        }

        if (t_hmove == 0 && t_vmove == 0) 
        { 
            HeadBob(idleCounter, 0.025f, 0.025f);
            idleCounter += Time.deltaTime; 
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }
        else if(!isSprinting)
        { 
            HeadBob(movementCounter, 0.035f, 0.035f); 
            movementCounter += Time.deltaTime * 3f; 
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }
        else
        {
            HeadBob(movementCounter, 0.15f, 0.075f); 
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }

    }



    // Update is called once per frame
    void FixedUpdate()
    {

        if (!photonView.IsMine) return;

        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        bool isGrounding = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounding;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounding;

        

        Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
        t_direction.Normalize();

        float t_adjustedspeed = speed;
        if (isSprinting) t_adjustedspeed *= sprintModifer;
        
        Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * speed * Time.deltaTime;
        t_targetVelocity.y = rg.velocity.y;
        rg.velocity = t_targetVelocity;

        if(isSprinting){ normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifer, Time.deltaTime * 8f); }
        else { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV , Time.deltaTime * 8f); }
    }

    void HeadBob (float p_z, float p_x_intensity, float p_y_intensity)
        {
        targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z*2) * p_y_intensity, 0);
        }   

    
}
