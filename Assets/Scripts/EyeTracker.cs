using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

public class EyeTracker : MonoBehaviour
{

    public Vector3 gazeLocation = Vector3.zero;
    public Vector3 worldPosition = Vector3.zero;

    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.EyesActions eyesActions;

    // Start is called before the first frame update
    void Start()
    {
        InputSubsystem.Extensions.MLEyes.StartTracking();
        mlInputs = new MagicLeapInputs();
        mlInputs.Enable();
        eyesActions = new MagicLeapInputs.EyesActions(mlInputs);
    }

    // Update is called once per frame
    void Update()
    {
        var eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();
        gazeLocation = eyes.fixationPoint;
        worldPosition = Camera.main.transform.position;
    }
}
