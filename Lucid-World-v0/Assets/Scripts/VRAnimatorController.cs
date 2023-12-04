using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    public float speedThreshold = 0.1f;
    [Range(0, 1)]
    public float smoothing = 1;
    private Animator animator;
    private Vector3 previousPos;
    private VRRig vrRig;
    private PlayerBehavior playerBehavior;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        vrRig = GetComponent<VRRig>();
        previousPos = vrRig.head.vrTarget.position;
        playerBehavior = GetComponentInParent<PlayerBehavior>();
    }

    void groundedAnimatorOptions()
    {
        //Compute the speed
        Vector3 headsetSpeed = (vrRig.head.vrTarget.position - previousPos) / Time.deltaTime;
        headsetSpeed.y = 0;

        //Local speed
        Vector3 headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);
        previousPos = vrRig.head.vrTarget.position;

        //Set Animator values
        float previousDirectionX = animator.GetFloat("directionX");
        float previousDirectionY = animator.GetFloat("directionY");

        animator.SetBool("isMoving", headsetLocalSpeed.magnitude > speedThreshold);
        animator.SetFloat("directionX", Mathf.Lerp(previousDirectionX, Mathf.Clamp(headsetLocalSpeed.x, -1, 1), smoothing));
        animator.SetFloat("directionY", Mathf.Lerp(previousDirectionY, Mathf.Clamp(headsetLocalSpeed.z, -1, 1), smoothing));
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerBehavior.isFlying)
        {
            groundedAnimatorOptions();
        }
        else
        {
            groundedAnimatorOptions();
        }
        

    }
}
