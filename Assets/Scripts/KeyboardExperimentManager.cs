using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

public class KeyboardExperimentManager : MonoBehaviour
{
    public EyeTracker EyePos;

    private float total_time = 0;
    private bool counting = false;
    private string UserName = "";
    private bool UsingStandard = false;
    private int characters = 0;
    StreamWriter dataOutput;
    private int currWord = -1;

    private List<string> managerWords = new List<string>();
    private int managerLen;

    private string pathToTXT = @"C:\Users\joaolmbc\Desktop\Softkeyboard\ExperimentDocJohnny3.txt";
    private string eyeTrackingPath = @"C:\Users\joaolmbc\Desktop\Softkeyboard\EyeTrackingJohnny3.txt";

    private bool pause = true;


    // Start is called before the first frame update
    void Start()
    {
        dataOutput = new StreamWriter(eyeTrackingPath);
        dataOutput.WriteLine("Begin data from special keyboard");
    }

    public void swapUsing() { UsingStandard = !UsingStandard; }

    // Update is called once per frame
    void Update()
    {
        if (counting)
        {
            total_time += Time.deltaTime;

            var output = FindIntersection(EyePos.worldPosition, EyePos.gazeLocation);
            dataOutput.WriteLine(output.ToString());
        }
    }

    public void enable_timer()
    {
        if (pause == true)
        {
            pause = false;
            Debug.Log("Unpaused");
        }
        counting = true;
    }

    public void pause_timer()
    {
        Debug.Log("Paused Timer");
        pause = true;
        counting = false;
    }

    public void get_name(string name)
    {
        UserName = name;
    }

    public void disable_timer()
    {
        counting = false;
        Debug.Log("Total time taken was: " +  total_time);
        Debug.Log("Total characters: " + characters);
        Debug.Log("WPM is: " + characters / 5 / total_time * 60);
        writeResults();
    }

    public void reset_timer()
    {
        counting = false;
        total_time = 0;
        Debug.Log("Restarted!");

        dataOutput.Close();
        dataOutput = new StreamWriter(eyeTrackingPath, true);
    }


    private void writeResults()
    {
        dataOutput.WriteLine("Total time taken was: " + total_time);
        dataOutput.Close();

        using (StreamWriter writer = new StreamWriter(pathToTXT, true))
        {
            writer.WriteLine("User: " + UserName);
            if (UsingStandard)
            {
                writer.WriteLine("Using Standard Keyboard");
            }
            else
            {
                writer.WriteLine("Using Unique Keyboard");
            }
            writer.WriteLine("Total time taken was: " + total_time);
            writer.WriteLine("Total characters: " + characters);
            writer.WriteLine("WPM is: " + characters / 5 / total_time * 60);
            writer.WriteLine("");
            writer.WriteLine("---------------------------");
            writer.WriteLine("");
        }
    }

    public void LoadCharacters(string[] words, List<int> randomized, int len)
    {
        managerLen = len;

        for (int i = 0; i < len; i++)
        {
            int index = randomized[i];
            characters += words[index].Length;
            managerWords.Add(words[index]);
            characters += 1;   // for the space between words
            Debug.Log(words[index]);
        }

        //Debug.Log("TOTAL LENGTH IS: " + characters);
    }


    public static Vector2 FindIntersection(Vector3 userPos, Vector3 fixationPT)
    {
        Vector3 direction = fixationPT - userPos;

        float zPlane = 5;
        if (direction.z == 0)
        {
            return Vector2.zero;
        }

        float t = (zPlane - userPos.z) / direction.z;

        Vector3 intersectionPoint = userPos + t * direction;

        return new Vector2(intersectionPoint.x, intersectionPoint.y);
    }

    public void nextWord()
    {
        if (currWord < managerWords.Count)
        {
            dataOutput.WriteLine();
            currWord++;
            dataOutput.WriteLine(managerWords[currWord]);
        }
    }
}
