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


    public Transform target;
    protected NavMeshAgent nav;
    Animator animator;
    protected Task task = Task.idle;
    [SerializeField]
    float stoppingDistance = 1;

    //żyćko
    [SerializeField]
    float hp, hpMax = 100;
    public bool IsAlive { get { return hp > 0; } }
    public float HealthPercent { get { return hp / hpMax; } }
    [SerializeField]
    GameObject hpBarPrefab;
    protected HealthBar healthBar;

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hp = hpMax;
        healthBar = Instantiate(hpBarPrefab, transform).GetComponent<HealthBar>();
    }

    private void Start()
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
            Animate();
        }
    }

    protected virtual void Idling() {
        nav.velocity = Vector3.zero;
    }
    protected virtual void Moving() {
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
    protected virtual void Chasing() { }
    protected virtual void Attacking()
    {
        nav.velocity = Vector3.zero;
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

}
