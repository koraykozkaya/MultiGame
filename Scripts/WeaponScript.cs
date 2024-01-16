using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WeaponScript : MonoBehaviourPunCallbacks
{
    private const int V = 9;
    public GunScript[] loadout;
    public Transform weaponParent;
    private int currentIndex;
    public GameObject bulletholePrefab;
    public LayerMask canBeShot;

    private float currenCoolDown;

    private GameObject currenWeapon;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) { photonView.RPC("Equip", RpcTarget.All, 0); }
        if (currenWeapon != null)
        {
            Aim(Input.GetMouseButton(1));

            if (Input.GetMouseButtonDown(0)&&currenCoolDown<=0)
            {
                photonView.RPC("Shoot",RpcTarget.All);
            }

            currenWeapon.transform.localPosition = Vector3.Lerp(currenWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            if (currenCoolDown > 0) currenCoolDown -= Time.deltaTime;
        }
    }
    [PunRPC]
    void Equip(int p_ind)
    {

        if (currenWeapon != null) Destroy(currenWeapon);

        currentIndex = p_ind;
        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        t_newWeapon.GetComponent<Sway>().enabled = photonView.IsMine;

        currenWeapon = t_newWeapon;

    }

    void Aim(bool p_isAiming)
    {
        Transform t_anchor = currenWeapon.transform.Find("Anchor");
        Transform t_state_ads = currenWeapon.transform.Find("States/ADS");
        Transform t_state_hip = currenWeapon.transform.Find("States/Hip");

        if (p_isAiming)
        {
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);

        }
        else
        {
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
        }
    }
    [PunRPC]
    void Shoot()
    {
        Transform t_spawn = transform.Find("Cameras/Normal Camera");

        Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
       
            

           
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();





            
            currenCoolDown = loadout[currentIndex].firerate;
        

        RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 5f);
                
                if(photonView.IsMine)
                {
                if (t_hit.collider.gameObject.layer == 9) ;
                }


            }


        currenWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
        currenWeapon.transform.position -= currenWeapon.transform.forward * loadout[currentIndex].kickback;

        
    }
}
