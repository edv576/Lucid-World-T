using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
//VR Logger
using DilmerGamesLogger = DilmerGames.Core.Logger;

public class PlayerBehavior : MonoBehaviour
{
    public float speed;
    Vector3 moveDir = Vector3.zero;
    CharacterController controller;
    public Camera cam;
    //Initialize gravity on Earth terms -> 9.81
    public float gravity = 45.81f;
    const float earthGravity = 9.81f;
    const float noGravity = 0.0f;
    Animator animator;
    public GameObject handStand;

    //The player starts grounded
    public bool isFlying = false;

    [SerializeField]
    private XRNode xrNode = XRNode.RightHand;

    [SerializeField]
    private XRNode xrNode2 = XRNode.LeftHand;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private InputDevice device2;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices.FirstOrDefault();

        InputDevices.GetDevicesAtXRNode(xrNode2, devices);
        device2 = devices.FirstOrDefault();
    }

    private void OnEnable()
    {
        if (!device.isValid || !device2.isValid)
        {
            GetDevice();
        }
    }

    //Changes if the user is flying or walking in the ground 
    public void ChangeFlyingStatus()
    {
        if (isFlying)
        {
            isFlying = false;
            animator.SetBool("isFlying", false);
        }
        else
        {
            isFlying = true;
            animator.SetBool("isFlying", true);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        isFlying = false;
        animator.SetBool("isFlying", false);
    }

    //When player is grounded
    void MoveWalking()
    {
        Vector2 secondary2DAxisValue = Vector2.zero;

        InputFeatureUsage<Vector2> secondary2DAxis = CommonUsages.primary2DAxis;

        if (device.TryGetFeatureValue(secondary2DAxis, out secondary2DAxisValue) && (secondary2DAxisValue.y > 0.25F || secondary2DAxisValue.y < -0.25F))
        {
            //DilmerGamesLogger.Instance.LogInfo($"Primary2DAxis activated {secondary2DAxisValue}");
            if (secondary2DAxisValue.y > 0.25f)
            {
                //moveDir = new Vector3(cam.gameObject.transform.forward.x, 0, cam.gameObject.transform.forward.z);
                moveDir = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);

            }
            else
            {
                //moveDir = new Vector3(-cam.gameObject.transform.forward.x, 0, -cam.gameObject.transform.forward.z);
                moveDir = new Vector3(-cam.transform.forward.x, 0, -cam.transform.forward.z);
            }

            //moveDir = transform.TransformDirection(moveDir);
            moveDir = moveDir.normalized;
            moveDir *= speed;


        }
        else if (moveDir != Vector3.zero)
        {
            moveDir = Vector3.zero;
        }

        moveDir.y -= gravity * Time.deltaTime;
        controller.Move(moveDir * Time.deltaTime);

    }

    //When player is flying
    void MoveFlying()
    {
        InputFeatureUsage<bool> gripButton = CommonUsages.gripButton;

        bool gripped = false;

        if(device.TryGetFeatureValue(gripButton, out gripped) && gripped)
        {

            controller.Move(handStand.transform.forward * Time.deltaTime * 10.0f);
            //transform.position += handStand.transform.forward * Time.deltaTime * 10.0f;
        }

        
    }

    public void Rotate1()
    {


        this.transform.Rotate(0, 90, 0);
            

    }

    public void Rotate2()
    {


        this.transform.Rotate(0, -90, 0);


    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!device.isValid)
        {
            GetDevice();
        }

        if (!isFlying)
        {

            MoveWalking();


        }
        else
        {
            MoveFlying();
        }

        //RotateAll();


    }


}
