using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;

    public int maxAmmo = 30;
    private int currentAmmo;
    public float ReloadTime = 2f;
    private bool isReloading = false;
    private bool isShooting = false;

    public TextMeshProUGUI ammoText;

    /*[SerializeField] Transform barrelTransform;*/

    /*bool isAutomatic = false;
    bool isSingleShot = true;*/

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
        /*if (isSingleShot)
        {
            ShootSingle();
        }
        else
        {
            StartCoroutine(ShootAutomatic());
        }*/
        if (!isShooting && !isReloading)
        {
            StartCoroutine(ShootAutomatic());
        }


    }

    /*void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isAutomatic = !isAutomatic;
            isSingleShot = !isSingleShot;
        }

    }*/


    /*void ShootSingle()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject bulletObj = Instantiate(bulletImpactPrefab, hit.point, Quaternion.identity);
            Destroy(bulletObj, 10f);

            if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<IDamageable>().TakeDamage(((GunInfo)itemInfo).damage);
            }
            Debug.Log("Single");
        }
    }*/

    IEnumerator ShootAutomatic()
    {
        while (Input.GetButton("Fire1"))
        {
            if (currentAmmo > 0)
            {
                muzzleFlash.Play();

                currentAmmo--;
                UpdateAmmoText();

                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    /*GameObject hitEffectObj = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    ParticleSystem hitEffectPB = hitEffectObj.GetComponent<ParticleSystem>();
                    hitEffectPB.Play();*/

                    /*GameObject bulletObj = Instantiate(bulletImpactPrefab, hit.point, Quaternion.identity);
                    Destroy(bulletObj, 10f);*/

                    PV.RPC("GunEffect", RpcTarget.All, hit.point, hit.normal);

                    if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
                    {
                        muzzleFlash.Play();
                        hit.collider.gameObject.GetComponent<IDamageable>().TakeDamage(((GunInfo)itemInfo).damage);
                    }
                    Debug.Log("Auto");
                    yield return new WaitForSeconds(0.1f);
                }
                isShooting = false;
            }
            
        }
    }

    [PunRPC]
    void GunEffect(Vector3 hitPoint, Vector3 hitNormal)
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
        currentAmmo =maxAmmo;
        isReloading = false;
        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        ammoText.text = "Ammo: " + currentAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    /*[PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }*/
}
