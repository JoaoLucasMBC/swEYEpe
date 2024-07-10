using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextInputSystem : MonoBehaviour
{

    public string currTextInput;
    public TextMeshPro textInputRef;
    public string targetText;
    public int targetTextLength;
    public TextMeshPro targetTextRef;

    public LookThroughExperimentManager ExperimentManagerRef;
    // Start is called before the first frame update
    void Start()
    {
        currTextInput = "";
        textInputRef.text = currTextInput;
        GenerateRandomText(targetTextLength);
    }

    public void ReceiveInput(int inputType, string toInput) {
        if (inputType == 0) {
            //Append to string
            currTextInput += toInput;
            textInputRef.text = currTextInput;
            ExperimentManagerRef.RecordButtonPress(toInput);
        } else if (inputType == 1) {
            //backspace
            Debug.Log("Current Length: " + currTextInput.Length);
            if (currTextInput.Length > 0) {
                currTextInput = currTextInput.Remove(currTextInput.Length - 1);
            }
            // ExperimentManagerRef.RecordButtonPress("<");
            textInputRef.text = currTextInput;
        } else {
            //inputType == 2
            if (currTextInput == targetText) {
                currTextInput = "";
                textInputRef.text = currTextInput;
                ExperimentManagerRef.SubmitSequence();
                GenerateRandomText(targetTextLength);
            }
        }
    }

    public void GenerateRandomText(int length) {
        targetText = "";
        for (int i = 0; i < length; i++) {
            int toAdd = Random.Range(0,9);
            targetText += toAdd.ToString();
        }
        targetTextRef.text = targetText;
        ExperimentManagerRef.StartPhase(targetText);
    }

    //Todo: 
    //1. Implement limits on where change in gaze depth can be read
    //2. Set up experiment procedure that measures how long it takes for users to actuate each button and how long it takes to match the number
    //
}
