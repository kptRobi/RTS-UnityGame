using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour {

    public enum Task
    {
        idle, move, follow, chase, attack
    }

    const string ANIMATOR_SPEED = "Speed",
        ANIMATOR_ALIVE = "Alive",
        ANIMATOR_ATTACK = "Attack";
    public static List<ISelectable>SelectableUnits { get { return seletableUnits; } }
    static List<ISelectable> seletableUnits = new List<ISelectable>();

    [Header("Unit")]
    [SerializeField]
    GameObject hpBarPrefab;
    [SerializeField]
    float hp, hpMax = 100;
    [SerializeField]
    protected float attackDistance = 1, 
         attackCooldown = 1,
         attackDamage = 0,
         stoppingDistance = 1;

    protected Transform target;
    protected HealthBar healthBar;
    protected Task task = Task.idle;
    protected NavMeshAgent nav;

    public bool IsAlive { get { return hp > 0; } }
    public float HealthPercent { get { return hp / hpMax; } }

    float attackTimer;
    protected Animator animator;

    protected virtual void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hp = hpMax;
        healthBar = Instantiate(hpBarPrefab, transform).GetComponent<HealthBar>();
    }

    protected virtual void Start()
    {
        if (this is ISelectable)
        {
            SelectableUnits.Add(this as ISelectable);
            (this as ISelectable).setSelected(false);
        }
    }

    private void OnDestroy()
    {
        if (this is ISelectable) SelectableUnits.Remove(this as ISelectable);
    }

    // Update is called once per frame
    void Update () {
        if (IsAlive)
        {
            switch (task)
            {
                case Task.idle:
                    Idling();
                    break;
                case Task.move:
                    Moving();
                    break;
                case Task.follow:
                    Following();
                    break;
                case Task.chase:
                    Chasing();
                    break;
                case Task.attack:
                    Attacking();
                    break;
                default:
                    break;
            }
        }
        Animate();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        
    }


    protected virtual void Idling() {
        nav.velocity = Vector3.zero;
    }
    protected virtual void Moving()
    {
        float distance = Vector3.Magnitude(nav.destination - transform.position);
        if (distance <= stoppingDistance)
        {
            this.task = Task.idle;
        }
    }
    protected virtual void Following()
    {
        if (target)
        {
            nav.SetDestination(target.position);
        }
        else
        {
            task = Task.idle;
        }
    }
    protected virtual void Chasing()
    {
        if (target)
        {
            nav.SetDestination(target.position);
            float distance = Vector3.Magnitude(nav.destination - transform.position);
            if (distance <= attackDistance)
            {
                this.task = Task.attack;
            }

        }
        else
        {
            task = Task.idle;
        }

    }
    protected virtual void Attacking()
    {
        if (target)
        {
        nav.velocity = Vector3.zero;
            transform.LookAt(target);
            float distance = Vector3.Magnitude(target.position - transform.position);
            if (distance <= attackDistance)
            {
                if ((attackTimer -= Time.deltaTime) <= 0)
                {
                    Attack();
                }
            }
            else
            {
                this.task = Task.chase;

            }
        }
        else
        {
            task = Task.idle;
        }
    }

    //animacja
    protected virtual void Animate()
    {
        var speedVector = nav.velocity;
        speedVector.y = 0;
        float speed = speedVector.magnitude;
        animator.SetFloat(ANIMATOR_SPEED, speed);
        animator.SetBool(ANIMATOR_ALIVE, IsAlive);
    }
    public virtual void Attack() {
        Unit unit = target.GetComponent<Unit>();
        if (unit && unit.IsAlive)
        {
            animator.SetTrigger(ANIMATOR_ATTACK);
            attackTimer = attackCooldown;
        }
        else
        {
            target = null;
        }
    }
    public virtual void DealDamage() {
        if (target)
        {
            Unit unit = target.GetComponent<Unit>();
            if(unit)
            {
                unit.ReciveDamage(attackDamage, transform.position);
            }
        }
    }

    public virtual void ReciveDamage(float damage, Vector3 damageDealerPosition)
    {
        if (IsAlive)
        {
            hp -= damage;
        }
        if (!IsAlive)
        {
            healthBar.gameObject.SetActive(false);
            enabled = false;
            nav.enabled = false;
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            if (this is ISelectable) seletableUnits.Remove(this as ISelectable);
            Animate();
        }
    }



    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

}
