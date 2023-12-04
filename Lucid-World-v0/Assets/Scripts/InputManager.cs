using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using DilmerGamesLogger = DilmerGames.Core.Logger;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private XRNode xrNode = XRNode.LeftHand;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices.FirstOrDefault();
    }

    private void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (!device.isValid)
        {
            GetDevice();
        }

        //First scenario: Get all the device's features
        //List<InputFeatureUsage> features = new List<InputFeatureUsage>();
        //device.TryGetFeatureUsages(features);

        //foreach(var feature in features)
        //{
        //    if(feature.type == typeof(bool))
        //    {
        //        DilmerGamesLogger.Instance.LogInfo($"Feature {feature.name} type {feature.type}");
        //    }

        //}


        //Capture trigger button
        bool triggerButtonAction = false;

        if(device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonAction) && triggerButtonAction)
        {
            DilmerGamesLogger.Instance.LogInfo($"Trigger button activated {triggerButtonAction}");
        }

        

        //Capture primary button
        bool primaryButton = false;

        InputFeatureUsage<bool> usage = CommonUsages.primaryButton;

        if (device.TryGetFeatureValue(usage, out primaryButton) && primaryButton)
        {
            DilmerGamesLogger.Instance.LogInfo($"Primary button activated {primaryButton}");
        }

        //Capture primary 2D Axis
        Vector2 primary2DAxisValue = Vector2.zero;

        InputFeatureUsage<Vector2> primary2DAxis = CommonUsages.primary2DAxis;

        if (device.TryGetFeatureValue(primary2DAxis, out primary2DAxisValue) && (primary2DAxisValue.y > 0.1F))
        {
            DilmerGamesLogger.Instance.LogInfo($"Primary2DAxis activated {primary2DAxisValue}");
        }

        float gripValue = 0.0F;

        InputFeatureUsage<float> gripUsage = CommonUsages.grip;

        if(device.TryGetFeatureValue(gripUsage, out gripValue) && (gripValue > 0.0F))
        {
            DilmerGamesLogger.Instance.LogInfo($"Grip activated {gripValue}");
        }

        

    }
}
