﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Unit, ISelectable {

    [Header("Soldier")]
    [Range(0, 0.3f), SerializeField]
    float shootDuration = 0;
    [SerializeField]
    ParticleSystem muzzleEffect, impactEffect;
    [SerializeField]
    LayerMask shootingLayerMask;

    LineRenderer lineEffect;
    Light lightEffect;

    protected override void Awake()
    {
        base.Awake();
        lineEffect = muzzleEffect.GetComponent<LineRenderer>();
        lightEffect = muzzleEffect.GetComponent<Light>();
        impactEffect.transform.SetParent(null);
        EndShootEffect();
    }

    public void setSelected(bool selected)
    {
        healthBar.gameObject.SetActive(selected);
    }

    void Command(Vector3 destination)
    {
        nav.SetDestination(destination);
        task = Task.move;
        target = null;
    }

    void Command(Soldier SoldierToFollow)
    {
        target = SoldierToFollow.transform;
        task = Task.follow;
    }
    void Command(Dragon dragonToKill)
    {
        target = dragonToKill.transform;
        task = Task.chase;
    }

    public override void DealDamage()
    {
        if (Shoot())
        {
            base.DealDamage();
        }
    }
    bool Shoot()
    {
        Vector3 start = muzzleEffect.transform.position;
        Vector3 direction = transform.forward;



        RaycastHit hit;
        if(Physics.Raycast(start, direction, out hit, attackDistance, shootingLayerMask))
        {
            Debug.Log("DUPA1");
            StartShootEffect(start, hit.point, true);
            Debug.Log("DUPA2");
            var unit = hit.collider.gameObject.GetComponent<Unit>();
            Debug.Log("DUPA3" + unit);
            return unit;
        }
        StartShootEffect(start, start + direction * attackDistance, false);
        return false;
    }

    void StartShootEffect(Vector3 lineStart, Vector3 lineEnd, bool hitSomething)
    {
        if (hitSomething)
        {
            Debug.Log("DUPA2.5");
            impactEffect.transform.position = lineEnd;
            Debug.Log("DUPA2.6");
            impactEffect.Play();
        }
        lineEffect.SetPositions(new Vector3[] { lineStart, lineEnd });

        lightEffect.enabled = true;
        lineEffect.enabled = true;
        muzzleEffect.Play();
        Invoke("EndShootEffect", shootDuration);
    }

    void EndShootEffect() { 
        lightEffect.enabled = false;
        lineEffect.enabled =  false;
    }
}