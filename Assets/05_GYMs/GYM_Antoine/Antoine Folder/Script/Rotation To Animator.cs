using UnityEngine;

public class RotationToAnimator : MonoBehaviour
{
    [SerializeField] string RotationX = "PosX";
    [SerializeField] string RotationY = "PosY";
    [SerializeField] Transform GetRotation;
    [SerializeField] float Offset = 45f;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        float theY = GetRotation.rotation.eulerAngles.y + Offset;
        if (theY > 360 || theY < -360)
        {
            if (theY > 0) theY = theY - ((int)(Mathf.Abs(theY) / 360) * 360);
            else theY = theY + ((int)(Mathf.Abs(theY) / 360) * 360);
        }

        if (theY >= 337 || theY <= 22.5f) //0
        {
            animator.SetFloat(RotationX, 0);
            animator.SetFloat(RotationY, -1);
        }
        if (theY >= 157.5 && theY <= 202.5f) //180
        {
            animator.SetFloat(RotationX, 0);
            animator.SetFloat(RotationY, 1);
        }
        if (theY >= 67.5f && theY <= 112.5f) //90
        {
            animator.SetFloat(RotationX, 1);
            animator.SetFloat(RotationY, 0);
        }
        if (theY >= 247.5f && theY <= 292.5f) //-90
        {
            animator.SetFloat(RotationX, -1);
            animator.SetFloat(RotationY, 0);
        }

        if (theY > 22.5f && theY < 67.5f) //45
        {
            animator.SetFloat(RotationX, 0.75f);
            animator.SetFloat(RotationY, -0.75f);
        }
        if (theY > 112.5f && theY < 157.5) //135
        {
            animator.SetFloat(RotationX, 0.75f);
            animator.SetFloat(RotationY, 0.75f);
        }
        if (theY > 292.5f && theY < 337) //-45
        {
            animator.SetFloat(RotationX, -0.75f);
            animator.SetFloat(RotationY, -0.75f);
        }
        if (theY > 202.5f && theY < 247.5f) //-135
        {
            animator.SetFloat(RotationX, -0.75f);
            animator.SetFloat(RotationY, 0.75f);
        }
    }
}
