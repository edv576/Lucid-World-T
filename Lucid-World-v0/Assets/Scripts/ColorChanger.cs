using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ColorChanger : MonoBehaviour
{

    public Material greyMaterial = null;
    public Material pinkMaterial = null;

    private MeshRenderer meshRenderer = null;
    private XRGrabInteractable grabInteractable = null;
    // Start is called before the first frame update
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.onActivate.AddListener(SetPink);
        grabInteractable.onDeactivate.AddListener(SetGrey);
    }

    private void OnDestroy()
    {
        grabInteractable.onActivate.RemoveListener(SetPink);
        grabInteractable.onDeactivate.RemoveListener(SetGrey);
    }

    private void SetGrey(XRBaseInteractor interactor)
    {
        meshRenderer.material = greyMaterial;
    }

    private void SetPink(XRBaseInteractor interactor)
    {
        meshRenderer.material = pinkMaterial;
    }
}
