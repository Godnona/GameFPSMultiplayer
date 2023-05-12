using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;
    PhotonView PV;

    public int maxAmmo = 30;
    private int currentAmmo;
    public float ReloadTime = 2f;
    private bool isReloading = false;

    public ParticleSystem muzzleFlashPrefab;
    public GameObject hitEffect;

    public TextMeshProUGUI ammoText;
    
    private bool isShooting = false;
   
    RaycastHit hit;
    Ray ray;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
       currentAmmo = maxAmmo;
       UpdateAmmoText();
    }

    void Update()
    {
        if (isReloading)
            return;
        if (currentAmmo <= 0 || Input.GetKey(KeyCode.E))
        {
            StartCoroutine(Reload());
            return;
        }
    }

    public override void Use()
    {
        if (!isShooting && !isReloading)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if(currentAmmo > 0)
        {
            //PV.RPC("ShowMuzzleFlash", RpcTarget.All,hit.point, hit.normal);
            muzzleFlashPrefab.Play();

            currentAmmo--;
            UpdateAmmoText();

            ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;

            if (Physics.Raycast(ray, out hit))
            {
                PV.RPC("ShowGunEffect", RpcTarget.All, hit.point, hit.normal);

                /*GameObject bulletObj = Instantiate(bulletImpactPrefab, hit.point, Quaternion.identity);
                Destroy(bulletObj, 10f);*/

                if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
                {
                    hit.collider.gameObject.GetComponent<IDamageable>().TakeDamage(((GunInfo)itemInfo).damage);
                }
                Debug.Log("Single !=");
            }
            isShooting = false;
        }
        
    }

    /*[PunRPC]
    void ShowMuzzleFlash()
    {
        ParticleSystem muzzleFlash = Instantiate(muzzleFlashPrefab, cam.transform.position, cam.transform.rotation);
        muzzleFlash.Play();
        Destroy(muzzleFlash.gameObject, 0.5f);
    }*/

    [PunRPC]
    void ShowGunEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        GameObject hitEffectObj = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        ParticleSystem hitEffectPB = hitEffectObj.GetComponent<ParticleSystem>();
        hitEffectPB.Play();
        Destroy(hitEffectObj, 1f);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(ReloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        ammoText.text = "Ammo: " + currentAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    /*void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray,out RaycastHit hit))
        {
            hit.collider.gameObject.GetComponent<IDamageable>() ?.TakeDamage(((GunInfo)itemInfo).damage);
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }*/

    /*[PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if(colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up)* bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }*/


}
