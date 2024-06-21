using DeathMatch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SharkScript : MonoBehaviour
{
    private enum SharkType { Patrolling, Attacking }
    [SerializeField] SharkType sharkType;
    [SerializeField] private WaypointsParent[] waypointsParents;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float nextWaypointDistance;
    [SerializeField] private float nextRotationDistance;
    [SerializeField] private GameObject bloodParticle;
    [SerializeField] private GameObject splashParticle;
    private Waypoint currentWaypoint;
    private Animator anim;
    private Coroutine attackCoroutine;

    private bool attcking;
    private bool particleCoolDown;
    private List<Target> targets = new List<Target>();

    public int TargetsLeft { get => targets.Count; }

    [Serializable]
    public class Target
    {
        public CharacterID ID;
        public Action OnEat;

        public Target(CharacterID iD, Action onEat)
        {
            ID = iD;
            OnEat = onEat;
        }
    }

    private void Awake() 
    {
        targets.Clear();
    }

    private void Start() 
    {
        anim = GetComponent<Animator>();
        if(sharkType == SharkType.Patrolling)
        {
            currentWaypoint = waypointsParents[0].waypoints[0];
            transform.LookAt(currentWaypoint.nextWaypoint.transform.position);
            transform.position = currentWaypoint.transform.position;
            anim.SetTrigger("move");
        }
    }

    public void AddTarget(CharacterID character, Action onEat)
    {
        targets.Add(new Target(character, onEat));
    }

    private void Attack(int currentWaypointParrent)
    {
        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackCor(currentWaypointParrent));
    }

    private IEnumerator AttackCor(int wp)
    {
        currentWaypoint = waypointsParents[wp].waypoints[0];
        transform.position = currentWaypoint.transform.position;
        anim.SetTrigger("attack");
        while(currentWaypoint != null && currentWaypoint.nextWaypoint != null && currentWaypoint.nextWaypoint.nextWaypoint != null)
        {
            if((Vector3.Distance(transform.position, currentWaypoint.transform.position) <= nextWaypointDistance))
            {
               currentWaypoint = currentWaypoint.nextWaypoint;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentWaypoint.transform.position - transform.position), rotationSpeed * Time.deltaTime);
            if((Vector3.Distance(transform.position, currentWaypoint.transform.position) <= nextRotationDistance))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentWaypoint.nextWaypoint.nextWaypoint.transform.position - transform.position), rotationSpeed * Time.deltaTime);
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.transform.position, moveSpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position, targets[0].ID.transform.position) <= 2)
            {
                targets[0].ID.transform.SetParent(transform);
                if(!particleCoolDown)
                {
                    particleCoolDown = true;
                    Instantiate(bloodParticle, transform.position, transform.rotation);
                    splashParticle.transform.position = targets[0].ID.transform.position;
                    splashParticle.SetActive(true);

                    targets[0].ID.GetComponent<Character>().Scream();
                    //AudioManager.Instance.SharkAttakSFX();
                }
            }

            yield return null;
        }

        anim.SetTrigger("base");
        currentWaypoint = waypointsParents[0].waypoints[0];
        transform.position = currentWaypoint.transform.position;
        transform.LookAt(currentWaypoint.nextWaypoint.transform.position);
        targets[0].ID.transform.SetParent(null);
        targets[0].ID.gameObject.SetActive(false);

        var target = targets[0];

        targets.Remove(targets[0]);
        target.OnEat?.Invoke();
        attcking = false;
        particleCoolDown = false;
    }

    private void Update() 
    {
        if(sharkType == SharkType.Patrolling)
        {
            if(currentWaypoint != null)
            {
                if((Vector3.Distance(transform.position, currentWaypoint.transform.position) <= nextWaypointDistance))
                {
                    currentWaypoint = currentWaypoint.nextWaypoint;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentWaypoint.transform.position - transform.position), rotationSpeed * Time.deltaTime);
                if((Vector3.Distance(transform.position, currentWaypoint.transform.position) <= nextRotationDistance))
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentWaypoint.nextWaypoint.nextWaypoint.transform.position - transform.position), rotationSpeed * Time.deltaTime);
                }
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.transform.position, moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (targets.Count > 0 && !attcking)
            {
                attcking = true;
                Attack(targets[0].ID.ID);
            }
        }
    }
}
