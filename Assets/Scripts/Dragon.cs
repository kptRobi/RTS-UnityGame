using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : Unit {

    List<Soldier> seenSoldiers = new List<Soldier>();
    Soldier ClosestSoldier
    {
        get
        {
            if (seenSoldiers == null || seenSoldiers.Count <= 0)
            {
                return null;
            }
            float minDistance = float.MaxValue;
            Soldier closestSoldier = null;
            foreach (Soldier soldeir in seenSoldiers)
            {
                if (!soldeir || !soldeir.IsAlive) continue;
                float distance = Vector3.Magnitude(soldeir.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSoldier = soldeir;
                }
            }
            return closestSoldier;
        }
    }

    [SerializeField]
    float idlingCooldown = 2;
    [SerializeField]
    float patrolRadius = 5;
    [SerializeField]
    float chasingSpeed = 5;

    int sinceLastHitAnim = 2;

    float normalSpeed;
    Vector3 startPoint;

    float idlingTimer;

    protected override void Awake()
    {
        base.Awake();
        normalSpeed = nav.speed;
        startPoint = transform.position;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var soldier = other.gameObject.GetComponent<Soldier>();
        if (soldier && !seenSoldiers.Contains(soldier))
        {
            seenSoldiers.Add(soldier); 
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        var soldier = other.gameObject.GetComponent<Soldier>();
        if (soldier)
        {
            seenSoldiers.Remove(soldier);
        }
    }


    protected override void Idling()
    {
        base.Idling();
        UpdateSight();
        if((idlingTimer -= Time.deltaTime) <= 0)
        {
            idlingTimer = idlingCooldown;
            task = Task.move;
            SetRandomRoamingPosition();
        }
    }

    protected override void Moving()
    {
        base.Moving();
        UpdateSight();
        //dodane wszystko poza nav.speed = normalSpeed;
        var animatorinfo = animator.GetCurrentAnimatorClipInfo(0);
        var current_animation = animatorinfo[0].clip.name;
        if (current_animation == "Get Hit")
        {
            nav.velocity = Vector3.zero;
        }
        else
        {
            nav.speed = normalSpeed;

        }
    }

    protected override void Chasing()
    {
        base.Chasing();
        //dodane wszystko poza nav.speed = chasingSpeed;
        var animatorinfo = animator.GetCurrentAnimatorClipInfo(0);
        var current_animation = animatorinfo[0].clip.name;
        if (current_animation == "Get Hit")
        {
            nav.velocity = Vector3.zero;
        }
        else
        {
            nav.speed = chasingSpeed;

        }
    }

    void UpdateSight()
    {
        var soldier = ClosestSoldier;
        if (soldier)
        {
            target = soldier.transform;
            task = Task.chase;
        }
    }

    void SetRandomRoamingPosition()
    {
        Vector3 delta = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        delta.Normalize();
        delta *= patrolRadius;

        nav.SetDestination(startPoint + delta);
    }

    public override void ReciveDamage(float damage, Vector3 damageDealerPosition)
    {
        base.ReciveDamage(damage, damageDealerPosition);
        if (!target)
        {
            task = Task.move;
            nav.SetDestination(damageDealerPosition);
        }
        //dodane sinceLastHitAnim
        if(HealthPercent > 0.5f && sinceLastHitAnim >= 2)
        {
            animator.SetTrigger("Get Hit");
            nav.velocity = Vector3.zero;
            sinceLastHitAnim = 0;
        }
        else
        {
            sinceLastHitAnim++;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.blue;
        startPoint = transform.position;
        Gizmos.DrawWireSphere(startPoint, patrolRadius);
    }
}
