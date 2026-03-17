using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Ennemy : MonoBehaviour
{
    protected Animator animator;
    protected NavMeshAgent navMesh;

    [Header("Data")]
    [SerializeField]private EnemyData data;

    protected int HP = 5;
    protected Vector2 speed;
    protected Vector2 acceleration;
    protected Vector2 SpeedRotate;

    [Header("Basic")]
    [SerializeField] protected Transform Player;
    [SerializeField] protected Transform Leure;

    [SerializeField] Transform GoTo;
    [SerializeField] Transform RotationLookAt;
    [SerializeField] protected Transform AttackTrigger;
    [SerializeField] Transform Neck;
    [SerializeField] protected string move = "0";

    [SerializeField] float DistanceAlwaysSeeEnnemy = 2f;

    [SerializeField] protected float timerGeneral = 0;
    Transform CurrentTarget;

    [Header("Raycast")]
    [SerializeField] Transform LockOn;
    [SerializeField] int LookRange = 7;
    [SerializeField] int RadiusLook = 45;

    [Header("Layer")]
    [SerializeField] LayerMask LayerBlockRay;

    [Header("GoTo")]
    [SerializeField] Vector3 WhereToGoPos;
    [SerializeField] Vector3 OgOffsetLookAt;

    [SerializeField] float LoseFocusDist;
    [SerializeField] float OffsetFollowPlayer = 0.5f;
    [SerializeField] Vector3 HeadRoatationOffset;

    [Header("Eyes")]
    [SerializeField] List<MeshRenderer> Eyes;
    [SerializeField] Color colorNormal;
    [SerializeField] Color colorChase;
    [SerializeField] Color colorMotionless;
    [SerializeField] Vector2 eyeColorIntensity;

    bool TargetInFieldOfView;


    [Header("Patrol Route")]
    [SerializeField] List<Vector3> PatrolPosition;
    int currentPatrolPose;

    [Header("Damage Display")]
    [SerializeField] protected TMP_Text hitValueDisplay;
    [SerializeField] private float durationDelay;
    [SerializeField] private float durationDotween;
    protected TweenerCore<Vector3, Vector3, VectorOptions> dotween;

    [Header("Deal Damage")]
    [SerializeField] protected EnnemyHit MainHitBox;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        navMesh = GetComponent<NavMeshAgent>();

        GoTo.position += transform.forward * OffsetFollowPlayer;
        OgOffsetLookAt = GoTo.localPosition;

        HP = data.health;

        speed = new Vector2(data.speed, data.chasespeed);
        SpeedRotate = new Vector2(data.speedRotate, data.chasespeedRotate);
        acceleration = new Vector2(data.acceleration, data.chaseacceleration);

        RotationLookAt.SetParent(null);
        RotationLookAt.position = GoTo.position;

        WhereToGoPos = GoTo.position;

        colorNormal *= eyeColorIntensity.x; colorChase *= eyeColorIntensity.y;
        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
        EyesSetColorTo(colorNormal);
    }

    protected virtual void FixedUpdate()
    {
        isPlayerInFieldOfView();

        if (TargetInFieldOfView && move != "sleep")
        {
            if (move != "chase" && move != "attack")
            {
                move = "chase";
                navMesh.speed = speed.y;
                navMesh.acceleration = acceleration.y;
                navMesh.angularSpeed = SpeedRotate.y;
            }

            EyesSetColorTo(colorChase);
            WhereToGoPos = CurrentTarget.position;

            LookAtPosition(WhereToGoPos);
        }
        else if (move != "sleep")
        {
            if (move == "chase") move = "lose chase";

            FaceForward();
        }

        if (move == "chase")
        {
            navMesh.destination = WhereToGoPos;

            AttackPatern();
        }
        else if (move == "lose chase")
        {
            navMesh.destination = WhereToGoPos;

            if (Vector3.Distance(transform.position, WhereToGoPos) <= LoseFocusDist + OffsetFollowPlayer)
            {
                PatrolStart();
            }
        }
        else if (move == "patrol")
        {
            navMesh.destination = WhereToGoPos;

            if (Vector3.Distance(transform.position, WhereToGoPos) <= LoseFocusDist + OffsetFollowPlayer)
            {
                currentPatrolPose += 1;

                if (currentPatrolPose < PatrolPosition.Count) WhereToGoPos = PatrolPosition[currentPatrolPose];
                else
                {
                    currentPatrolPose = 0;
                    WhereToGoPos = PatrolPosition[0];
                }
            }
        }
        else if (move == "attack")
        {
            if (CurrentTarget != null)
            {
                WhereToGoPos = CurrentTarget.position;
                navMesh.destination = WhereToGoPos;
            }
            else
            {
                PatrolStart();
            }
        }
        else if (move == "sleep")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                animator.SetBool("Sleep", false);
            }
        }

        if ((Mathf.Abs(navMesh.velocity.x) + Mathf.Abs(navMesh.velocity.z)) / 2 > 0) animator.SetBool("Move", true);
        else animator.SetBool("Move", false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSleep(5);
        }
    }

    void LookAtPosition(Vector3 pos)
    {
        Vector3 offset = Neck.transform.up * OffsetFollowPlayer;

        GoTo.position = pos - offset;

        RotationLookAt.position = GoTo.position;
        RotationLookAt.transform.LookAt(pos);

        GoTo.rotation = RotationLookAt.rotation;
        GoTo.Rotate(HeadRoatationOffset);
    }

    void FaceForward()
    {
        Vector3 RestPose = GoTo.localPosition;
        RestPose.x = 0;

        GoTo.localPosition = RestPose;
    }

    void EyesSetColorTo(Color color)
    {
        if (Eyes.Count > 0)
        {
            foreach (MeshRenderer eye in Eyes) eye.material.color = color;
        }
    }

    Vector3 SelectPatrolPosition()
    {
        Vector3 whereTo;

        if (PatrolPosition.Count > 0)
        {
            currentPatrolPose = 0;
            whereTo = PatrolPosition[0];
            float min = Mathf.Infinity;

            for (int i = 0; i < PatrolPosition.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, PatrolPosition[i]);

                if (dist < min)
                {
                    whereTo = PatrolPosition[i];
                    currentPatrolPose = i;
                    min = dist;
                }
            }
        }
        else whereTo = transform.position;

        return whereTo;
    }

    protected virtual void AttackStart(int attackID)
    {
        EyesSetColorTo(colorChase);

        move = "attack";
        navMesh.isStopped = true;
        animator.SetInteger("Attack", attackID);
    }

    protected virtual void AttackAnimEnd()
    {
        if (CurrentTarget != null)
        {
            move = "chase";
            WhereToGoPos = CurrentTarget.position;

            navMesh.isStopped = false;
            animator.SetInteger("Attack", 0);

            GoTo.localPosition = OgOffsetLookAt;
            GoTo.localRotation = Quaternion.Euler(HeadRoatationOffset);

            FaceForward();
        }
        else
        {
            navMesh.isStopped = false;
            animator.SetInteger("Attack", 0);

            PatrolStart();
        }
    }

    void isPlayerInFieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(LockOn.position, LookRange, LayerBlockRay);
        if (rangeChecks.Length > 0)
        {
            bool leureDetected = false;

            for (int i = 0; i < rangeChecks.Length; i++)
            {
                if (rangeChecks[i].CompareTag("Leure"))
                {
                    if (Leure != rangeChecks[i].transform) Leure = rangeChecks[i].transform;
                    leureDetected = true;
                }
                if (rangeChecks[i].CompareTag("Player"))
                {
                    if (Player == null) Player = rangeChecks[i].transform;
                }
            }

            if (leureDetected)
            {
                if (CanSeeObject(Leure))
                {
                    TargetInFieldOfView = true;
                    CurrentTarget = Leure;
                }

                else if (Player != null)
                {
                    if (CanSeeObject(Player))
                    {
                        TargetInFieldOfView = true;
                        CurrentTarget = Player;
                    }
                    else TargetInFieldOfView = false;
                }

                return;
            }
            else if (Player != null)
            {
                if (CanSeeObject(Player))
                {
                    TargetInFieldOfView = true;
                    CurrentTarget = Player;
                    return;
                }
                else
                {
                    TargetInFieldOfView = false;
                    return;
                }
            }
        }

        TargetInFieldOfView = false;
    }

    public virtual void TakeDamage(int damage)
    {
        if (dotween != null)
        {
            dotween.Kill();
            if (hitValueDisplay) hitValueDisplay.transform.localScale = Vector3.zero;
        }

        dotween = null;
        if (hitValueDisplay)
        {
            hitValueDisplay.text = damage.ToString();
            ShowHitDisplay();
        }

        HP -= damage;

        if (HP <= 0)
        {
            Death();
        }
        else
        {
            if (move == "sleep")
            {
                WhereToGoPos = CurrentTarget.position;
                navMesh.destination = WhereToGoPos;

                if (animator == null) EndSleep();
                else animator.SetBool("Sleep", false);
            }
            else
            {
                if (move != "attack") move = "chase";
                Vector3 directionTarget = (CurrentTarget.position - transform.position).normalized;
                WhereToGoPos = CurrentTarget.position + (-directionTarget * 5);
                navMesh.destination = WhereToGoPos;
            }
        }
      
    }
    
    protected void ShowHitDisplay()
    {
        if (hitValueDisplay)
        {
            dotween = hitValueDisplay.transform.DOScale(1f, durationDotween).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                hitValueDisplay.transform.DOScale(0f, durationDotween).SetEase(Ease.OutBounce).SetDelay(durationDelay);
            });
        }
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }

    protected virtual void AttackPatern()
    {
        if (Vector3.Distance(AttackTrigger.position, CurrentTarget.position) <= 2f && CurrentTarget != null)
        {
            AttackStart(1);
        }
    }

    protected void PatrolStart()
    {
        EyesSetColorTo(colorNormal);

        WhereToGoPos = SelectPatrolPosition();
        GoTo.localPosition = OgOffsetLookAt;
        RotationLookAt.position = GoTo.position;
        move = "patrol";

        navMesh.speed = speed.x;
        navMesh.acceleration = acceleration.x;
        navMesh.angularSpeed = SpeedRotate.x;
    }

    public void StartSleep(float timer)
    {
        timerGeneral = timer;
        move = "sleep";
        navMesh.isStopped = true;

        if (animator != null)
        {
            EyesSetColorTo(Color.black);
            animator.SetBool("Sleep", true);
        }
    }

    protected void EndSleep()
    {
        if (navMesh != null) navMesh.isStopped = false;
        move = "chase";
        EyesSetColorTo(colorNormal);
    }

    bool CanSeeObject(Transform Target)
    {
        if (Vector3.Distance(Target.position, transform.position) <= DistanceAlwaysSeeEnnemy)
        {
            return true;
        }
        else
        {
            Vector3 anglePose1 = Target.position - LockOn.position;
            Vector3 anglePose2 = LockOn.position + (LockOn.forward * 0.5f) - LockOn.position;

            if (Vector3.Angle(anglePose1, anglePose2) < RadiusLook)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    protected void ToogleMainAttack(int toogle)
    {
        if (toogle == 1) MainHitBox.ToggleHitBox(true);
        else MainHitBox.ToggleHitBox(false);
    }
}