using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private ParticleSystem pistolFire, pistolShell, rifleFire, rifleShell;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AudioClip knifeStabFlesh, knifeCutthroat, knifeSlash, knifeOut ,pistolClipIn, pistolClipOut, pistolSlide, pistolShot, rifleClipIn, rifleClipOut, rifleSlide, rifleShot, dryPistol, dryRifle, flashOn, flashOff;
    private bool autoFire;

    public bool weaponsActive;

    private int weaponType;
    [SerializeField]
    private int pistolAmmo;
    [SerializeField]
    private int rifleAmmo;
    private int inMagazine;

    [SerializeField]
    private LayerMask hitMask;
    [SerializeField]
    private float range;
    [SerializeField]
    private float damageAmount;

    private bool inAction;

    private int pistolMagazine = 7;
    private int rifleMagazine = 30;

    public Camera FPVCamera;

    private int pistolMaxMagazineCount = 7, rifleMaxMagazineCount = 30;

    [SerializeField]
    private GameObject fleshHitEffect, dirtHitEffect, metalHitEffect;

    private bool hasRifle = false;
    private void Start()
    {
        weaponType = 1;
        ChangeWeapon();
    }
    public void ChangeWeapon()
    {
        if(weaponType == 0)
        {
            animator.SetBool("holdingPistol", false);
            animator.SetBool("holdingRifle", false);
            animator.SetBool("holdingKnife", true);
            range = 1.5f;
            damageAmount = 50;
        }
        else if (weaponType == 1)
        {
            animator.SetBool("holdingPistol", true);
            animator.SetBool("holdingRifle", false);
            animator.SetBool("holdingKnife", false);
            LoadMagazineCount();
            range = 50;
            damageAmount = 50;
        }
        else if (weaponType == 2)
        {
            animator.SetBool("holdingPistol", false);
            animator.SetBool("holdingRifle", true);
            animator.SetBool("holdingKnife", false);
            LoadMagazineCount();
            range = 100;
            damageAmount = 35;
        }
    }
    public void AcquireRifle()
    {
        hasRifle = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SaveMagazineCount();
            weaponType = 0;
            ChangeWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SaveMagazineCount();
            weaponType = 1;
            ChangeWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasRifle)
        {
            SaveMagazineCount();
            weaponType = 2;
            ChangeWeapon();
        }
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            FlashLight();
        }*/
        if (weaponsActive)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Fire();
            }

            if (Input.GetButtonUp("Fire1") || inMagazine < 1)
            {
                autoFire = false;
            }
            if (Input.GetButtonDown("Reload"))
            {
                MagazineReload();
            }
            if (autoFire)
            {
                animator.SetBool("autoFire", true);
            }
            else animator.SetBool("autoFire", false);
            Debug.DrawRay(rifleFire.transform.position, rifleFire.transform.forward * 100, Color.red);
            transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, FPVCamera.transform.rotation, 0.3f);
        }
    }
    private RaycastHit cutthroatTarget;
    private void Fire()
    {
        if (weaponType == 0)
        {
            if (Physics.Raycast(FPVCamera.transform.position, FPVCamera.transform.forward, out cutthroatTarget, range, hitMask))
            {
                if (cutthroatTarget.transform.tag == "BodyPart" && (cutthroatTarget.transform.GetComponentInParent<Enemy>().transform.forward + FPVCamera.transform.forward).magnitude > 1.75f && !inAction)
                {
                    cutthroatTarget.transform.GetComponentInParent<Enemy>().Stop();
                    if (GetComponentInParent<Controller>() && cutthroatTarget.transform.GetComponentInParent<Enemy>().transform.Find("BackstabPos"))
                        StartCoroutine(GetComponentInParent<Controller>().Backstab(cutthroatTarget.transform.GetComponentInParent<Enemy>().transform.Find("BackstabPos")));
                    else Debug.Log("Couldn't find Controller script.");
                    animator.SetTrigger("backstab");
                    inAction = true;
                    Debug.Log("Cutting throat of " + cutthroatTarget.transform.GetComponentInParent<Enemy>().transform.gameObject.name + " and the time is " + Time.time);
                }
                else animator.SetTrigger("slash");
            }
            else animator.SetTrigger("slash");
        }
        else if (weaponType == 1)
        {
            if (inMagazine > 0)
            {
                animator.SetTrigger("fire");
            }
            else
            {
                source.PlayOneShot(dryPistol);
                Debug.Log("No ammo in magazine.");
            }
        }
        else
        {
            if (inMagazine > 0)
            {
                animator.SetTrigger("fire");
                autoFire = true;
            }
            else
            {
                source.PlayOneShot(dryRifle);
                Debug.Log("No ammo in magazine.");
                autoFire = false;
            }
        }
    }

    private void Shot()
    {
        RaycastHit hit;
        if (Physics.Raycast(FPVCamera.transform.position, FPVCamera.transform.forward, out hit, range, hitMask))
        {
            if (hit.transform.tag == "BodyPart")
            {
                GameObject blood = Instantiate(fleshHitEffect, hit.point, Quaternion.identity, hit.transform);
                blood.transform.forward = hit.normal;
                hit.transform.gameObject.GetComponent<BodyPart>().Damage(damageAmount);
                Rigidbody bodyPartRB = hit.transform.GetComponent<Rigidbody>();
                bodyPartRB.AddForce(-hit.normal *(bodyPartRB.mass * 1000));
                if (weaponType == 0)
                    source.PlayOneShot(knifeStabFlesh);
            }
            else if (hit.transform.tag == "MetalSurface")
            {
                GameObject dirtSplat = Instantiate(metalHitEffect, hit.point, Quaternion.identity, hit.transform);
                dirtSplat.transform.forward = hit.normal;
            }
            else if (weaponType == 0)
            {
                KnifeHitGround();
                GameObject dirtSplat = Instantiate(dirtHitEffect, hit.point, Quaternion.identity, hit.transform);
                dirtSplat.transform.forward = hit.normal;
            }
            else
            {
                GameObject dirtSplat = Instantiate(dirtHitEffect, hit.point, Quaternion.identity, hit.transform);
                dirtSplat.transform.forward = hit.normal;
            }
            Debug.Log("Hit " + hit.transform.gameObject.name + " and the time is " + Time.time);
        }
        else if (weaponType == 0)
        {
            KnifeMiss();
        }
        ResetAllTriggers();
        FindObjectOfType<HUDManager>().UpdateHUD(inMagazine, pistolAmmo);
    }
    private void ResetAllTriggers()
    {
        animator.ResetTrigger("fire");
        animator.ResetTrigger("slash");
        animator.ResetTrigger("backstab");
        animator.ResetTrigger("reload");
        inAction = false;
    }
    void KnifeCutthroat()
    {
        cutthroatTarget.transform.GetComponentInParent<Enemy>().Die();
        source.PlayOneShot(knifeCutthroat);
        ResetAllTriggers();
    }
    private void MagazineReload()
    {
        Debug.Log("Reload button pressed.");
        if (weaponType == 0)
        {
            Debug.Log("You can't reload a knife. Or can you?");
        }
        else if (weaponType == 1)
        {
            if (pistolAmmo < 1) { Debug.Log("You have no ammo left."); return; }
            if (inMagazine != pistolMaxMagazineCount) 
            {
                pistolAmmo += inMagazine;
                inMagazine = 0;
                if (pistolAmmo < pistolMaxMagazineCount)
                {
                    inMagazine = pistolAmmo;
                    pistolAmmo = 0;
                }
                if (pistolAmmo >= pistolMaxMagazineCount)
                {
                    pistolAmmo -= pistolMaxMagazineCount;
                    inMagazine += pistolMaxMagazineCount;
                }
                //Reload Animation
                animator.SetTrigger("reload");
                animator.ResetTrigger("fire");
            }
            else
                Debug.Log("Magazine is already full.");
        }
        else
        {
            if (rifleAmmo < 1) { Debug.Log("You have no ammo left."); return; }
            if (inMagazine != rifleMaxMagazineCount)
            {
                rifleAmmo += inMagazine;
                inMagazine = 0;
                if (rifleAmmo < rifleMaxMagazineCount)
                {
                    inMagazine = rifleAmmo;
                    rifleAmmo = 0;
                }
                if (rifleAmmo >= rifleMaxMagazineCount)
                {
                    rifleAmmo -= rifleMaxMagazineCount;
                    inMagazine += rifleMaxMagazineCount;
                }
                //Reload Animation
                animator.SetTrigger("reload");
            }
            else
                Debug.Log("Magazine is already full.");
        }
        FindObjectOfType<HUDManager>().UpdateHUD(inMagazine, pistolAmmo);
    }
    private void SaveMagazineCount()
    {
        if (weaponType == 1)
        {
            pistolMagazine = inMagazine;
            inMagazine = 0;
        }
        else if(weaponType == 2)
        {
            rifleMagazine = inMagazine;
            inMagazine = 0;
        }
    }
    private void LoadMagazineCount()
    {
        if (weaponType == 1)
        {
            inMagazine = pistolMagazine;
        }
        else if (weaponType == 2)
        {
            inMagazine = rifleMagazine;
        }
        FindObjectOfType<HUDManager>().UpdateHUD(inMagazine, pistolAmmo);
    }
    void KnifeStab()
    {
        Shot();
        ResetAllTriggers();
    }
    void KnifeMiss()
    {
        source.PlayOneShot(knifeSlash);
    }
    void KnifeHitGround()
    {
        source.PlayOneShot(knifeOut);
    }
    void PistolClipIn()
    {
        source.PlayOneShot(pistolClipIn);
    }
    void PistolClipOut()
    {
        source.PlayOneShot(pistolClipOut);
    }
    void PistolSlide()
    {
        source.PlayOneShot(pistolSlide);
        ResetAllTriggers();
    }
    void PistolFire()
    {
        if (inMagazine < 1) return;
        inMagazine--;
        source.PlayOneShot(pistolShot);
        pistolFire.Play();
        pistolShell.Play();
        GetComponentInParent<Controller>().RecoilCamera();
        ResetAllTriggers();
        Shot();
    }
    void RifleClipIn()
    {
        source.PlayOneShot(rifleClipIn);
    }
    void RifleClipOut()
    {
        source.PlayOneShot(rifleClipOut);
    }
    void RifleSlide()
    {
        source.PlayOneShot(rifleSlide);
        ResetAllTriggers();
    }
    void RifleFire()
    {
        if (inMagazine < 1) return;
        source.PlayOneShot(rifleShot);
        rifleFire.Play();
        rifleShell.Play();
        inMagazine--;
        ResetAllTriggers();
        GetComponentInParent<Controller>().RecoilCamera();
        Shot();
    }
    private bool lightOn = false;
    [SerializeField]
    private GameObject flashLight;
    private void FlashLight()
    {
        if (lightOn)
        {
            lightOn = false;
            flashLight.SetActive(false);
            source.PlayOneShot(flashOff);
        }
        else
        {
            lightOn = true;
            flashLight.SetActive(true);
            source.PlayOneShot(flashOn);
        }
    }
}
