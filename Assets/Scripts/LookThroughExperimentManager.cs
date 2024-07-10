using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LookThroughExperimentManager : MonoBehaviour
{
    //PURPOSE: This is the manager script for the numberpad "look-through" experiment. It's purpose is to
    // gauge the user's ability to use the new "look-through" interaction to completely an over-arching task.
    // This task is to copy the numbers displayed at the top of by actuating the corresponding buttons on the numberpad.
    // During this experiment, we will measure the time from the last button press, the specific number pressed, and the overall time
    // it takes for the user to complete and submit a sequence. We will track how many errors the user makes as well. The user
    // terminates the experiment by completing 3 sequences successfully. A DNF is also useful information...
    //Text output:
    //Keylog: <List of keys pressed>
    //TimeLog: <List of times from last key press for each key pressed (the first one is and starts the experiment process)>
    //NumMistakes: <int # of times the user inputs erroneous submissions>
    
    //Potential future work that I haven't implemented:
    //- Line-of-sight to actuation time recording
    //- Implement a way to track mistakes/corrected mistakes along with not penalizing

    private bool userIsOnTrack; //keeps track of if the last input matched the right button.
    private int currStringLength; //For checking if the user has input the right number of characters.
    private string targetSequence; //Provided by TextInputSystem.cs
    private string currTargetChar; //Used to check for erroneous input
    private int stringIdx; //Idx of character in string.
    public int numSequences; //Number of sequences to run in experiment
    private int currSequence; //Current sequence # to keep track of when to end the experiment
    public List<GameObject> toDisable; //Set all gameobjects to inactive when experiment is complete to avoid input of extra data.

    //NOTE THAT YOU WILL HAVE TO CHANGE THIS IF YOU ARE NOT ME:
    private string pathToTXT = @"C:\Users\shurh2\Desktop\MagicLeap_repo\MagicLeap2 Experimental\ExperimentalData\LookThroughExperiment_results.txt";
    
    //Timer related vars
    private bool experimentActive;
    private float currTime;
    private bool experimentComplete;

    //Data related vars
    private List<float> timesList = new List<float>();
    private List<string> buttonsList = new List<string>();
    private int numMistakes;


    public void StartPhase(string generatedTargetSequence) {
        //called by TextInputSystem on startup and between sequences
        targetSequence = generatedTargetSequence;
        stringIdx = 0;
        currTargetChar = char.ToString(targetSequence[0]);
    }

    public void RecordButtonPress(string tooLazyToCastIt) {
        Debug.Log("Target char: " + currTargetChar + " provided char: " + tooLazyToCastIt);
        
        if (tooLazyToCastIt == currTargetChar) {
            //Record current button press and time from last button press
            buttonsList.Add(currTargetChar);
            timesList.Add(currTime);
            if (stringIdx < 3) {
                //I've locked sequence lengths to 4. Change this if you change sequence length in TextInputSystem.cs
                stringIdx++;
                currTargetChar = char.ToString(targetSequence[stringIdx]);
            }
            //StartPhase will handle stringIdx and currTargetChar reassignment when starting a new subsequence

            //Restart timer
            currTime = 0;

            //I start the experiment with the first button press:
            if (!experimentActive && !experimentComplete) {
                experimentActive = true;
            }
            
        } else {
            //Increment numMistakes
            //For now, don't record erroneous presses in the output list (errors will be seen by long time-to-press values)
            numMistakes++;
        }
    }

    public void SubmitSequence() {
        //Called when enter is input in TextInputSystem and the sequence is correct
        if (currSequence >= numSequences - 1) {
            experimentComplete = true;
            DeactivateAll();
            writeResults();
        } else {
            currSequence++;
        }
    }

    public void writeResults() {
        Debug.Log("Got to writeResults");
        using (StreamWriter writer = new StreamWriter(pathToTXT)) {
            writer.WriteLine("KeyLog: ");
            for (int j = 0; j < buttonsList.Count; j++) {
                if (j < buttonsList.Count - 1) {
                    writer.Write(buttonsList[j] + ",");
                } else {
                    writer.Write(buttonsList[j]);
                }
                
            }
            writer.WriteLine("");
            writer.WriteLine("TimeLog: ");
            for (int j = 0; j < timesList.Count; j++) {
                if (j < timesList.Count - 1) {
                    writer.Write(timesList[j] + ",");
                } else {
                    writer.Write(timesList[j]);
                }
            }
            writer.WriteLine("");
            writer.WriteLine("Number of Misinputs: " + numMistakes);
           
            
        }
        Debug.Log("Finished Writing");
        
    }

    public void DeactivateAll() {
        for (int i = 0; i < toDisable.Count; i++) {
            toDisable[i].SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        numMistakes = 0;
        currTime = 0;
        experimentActive = false;
        experimentComplete = false;
        currSequence = 0;
        userIsOnTrack = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (experimentActive) {
            currTime += Time.deltaTime;
        }
    }
}
