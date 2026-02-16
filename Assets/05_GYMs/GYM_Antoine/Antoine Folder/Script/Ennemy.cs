using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Ennemy : MonoBehaviour
{
    Animator animator;
    NavMeshAgent navMesh;

    [Header("Data")]
    [SerializeField]private EnemyData data;
    [Header("Basic")]
    [SerializeField] Transform Player;
    [SerializeField] Transform GoTo;
    [SerializeField] Transform RotationLookAt;

    private int HP = 20;

    [SerializeField] Transform AttackTrigger;
    [SerializeField] Transform Neck;
    [SerializeField] string move = "0";

    [Header("Raycast")]
    [SerializeField] Transform LockOn;
    [SerializeField] int LookRange = 7;
    [SerializeField] int RadiusLook = 45;

    [Header("Layer")]
    [SerializeField] LayerMask ignoreLayer;

    [Header("GoTo")]
    [SerializeField] Vector3 WhereToGoPos;
    [SerializeField] Vector3 OgOffsetLookAt;

    [SerializeField] float LoseFocusDist;
    [SerializeField] float OffsetFollowPlayer = 0.5f;
    [SerializeField] Vector3 HeadRoatationOffset;

    [Header("Movement")]
    private float SpeedRotate = 5;
    private float speed = 7;

    [Header("Eyes")]
    [SerializeField] List<MeshRenderer> Eyes;
    [SerializeField] Color colorNormal;
    [SerializeField] Color colorChase;
    [SerializeField] Vector2 eyeColorIntensity;
    bool PlayerInFieldOfView;

    [Header("Patrol Route")]
    [SerializeField] List<Vector3> PatrolPosition;
    
    [Header("Damage Display")]
    [SerializeField] private TMP_Text hitValueDisplay;
    [SerializeField] private float durationDelay;
    [SerializeField] private float durationDotween;
    private TweenerCore<Vector3, Vector3, VectorOptions> dotween;
    int currentPatrolPose;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navMesh = GetComponent<NavMeshAgent>();

        HP = data.health;
        speed = data.speed;
        SpeedRotate = data.speedRotate;
        GoTo.position += transform.forward * OffsetFollowPlayer;
        OgOffsetLookAt = GoTo.localPosition;

        RotationLookAt.SetParent(null);
        RotationLookAt.position = GoTo.position;

        WhereToGoPos = GoTo.position;

        colorNormal *= eyeColorIntensity.x; colorChase *= eyeColorIntensity.y;
        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
        EyesSetColorTo(colorNormal, colorChase, 0);
    }

    private void FixedUpdate()
    {
        isPlayerInFieldOfView();

        if (PlayerInFieldOfView)
        {
            RaycastHit hit;
            Vector3 directionTarget = (Player.position - LockOn.position).normalized;

            if (Physics.Raycast(LockOn.position, directionTarget, out hit, LookRange, ignoreLayer))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    if (move != "chase" && move != "attack") move = "chase";

                    EyesSetColorTo(colorNormal, colorChase, 1);
                    WhereToGoPos = Player.position;

                    LookAtPosition(WhereToGoPos);
                }
                else
                {
                    if (move == "chase") move = "lose chase";

                    FaceForward();
                }
            }
            else
            {
                if (move == "chase")
                {
                    move = "lose chase";

                    FaceForward();
                }
            }
        }
        else
        {
            if (move == "chase") move = "lose chase";

            FaceForward();
        }

        if (move == "chase")
        {
            navMesh.destination = WhereToGoPos;

            if (Vector3.Distance(AttackTrigger.position, Player.position) <= 1.5f)
            {
                move = "attack";
                navMesh.isStopped = true;
                animator.SetInteger("Attack", 1);
            }
        }
        else if (move == "lose chase")
        {
            navMesh.destination = WhereToGoPos;

            if (Vector3.Distance(transform.position + (transform.forward * OffsetFollowPlayer), WhereToGoPos) <= LoseFocusDist + OffsetFollowPlayer)
            {
                EyesSetColorTo(colorNormal, colorChase, 0);
                WhereToGoPos = SelectPatrolPosition();
                move = "patrol";
            }
        }
        else if (move == "patrol")
        {
            navMesh.destination = WhereToGoPos;

            if (Vector3.Distance(transform.position + (transform.forward * OffsetFollowPlayer), WhereToGoPos) <= LoseFocusDist + OffsetFollowPlayer)
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

        if ((Mathf.Abs(navMesh.velocity.x) + Mathf.Abs(navMesh.velocity.z)) / 2 > 0) animator.SetBool("Move", true);
        else animator.SetBool("Move", false);
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

    void EyesSetColorTo(Color colorStart, Color colorEnd, float gradient)
    {
        if (Eyes.Count > 0)
        {
            for (int i = 0; i < Eyes.Count; i++)
            {
                Eyes[i].material.color = Color.Lerp(colorStart, colorEnd, gradient);
            }
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

    protected void AttackAnimEnd()
    {
        move = "patrol";
        navMesh.isStopped = false;
        animator.SetInteger("Attack", 0);
    }

    void isPlayerInFieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(LockOn.position, LookRange, ignoreLayer);
        if (rangeChecks.Length > 0)
        {
            foreach (Collider hitCollider in rangeChecks)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    if (Player == null) Player = hitCollider.transform;

                    Vector3 anglePose1 = Player.position - LockOn.position;
                    Vector3 anglePose2 = LockOn.position + (LockOn.forward * 0.5f) - LockOn.position;

                    if (Vector3.Angle(anglePose1, anglePose2) < RadiusLook)
                    {
                        PlayerInFieldOfView = true;
                        return;
                    }
                }
            }
        }

        PlayerInFieldOfView = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            Debug.Log("attack");
            Attack attack = other.GetComponent<Attack>();
            
            TakeDamage((int)attack.damage);
        }
    }

    private void TakeDamage(int damage)
    {
        if (dotween != null)
        {
            dotween.Kill();
            hitValueDisplay.transform.localScale = Vector3.zero;
        }

        dotween = null;
        hitValueDisplay.text = damage.ToString();
        ShowHitDisplay();
        HP -= damage;
        if (HP <= 0)
        {
            if (dotween != null)
            {
                dotween.Kill();
                hitValueDisplay.transform.localScale = Vector3.zero;
            }
            Destroy(gameObject);
        }
      
    }
    
    private void ShowHitDisplay()
    {
        dotween = hitValueDisplay.transform.DOScale(1f, durationDotween).SetEase(Ease.OutBounce).OnComplete(()=>
        {
            hitValueDisplay.transform.DOScale(0f, durationDotween).SetEase(Ease.OutBounce).SetDelay(durationDelay);
        });
    }
}
