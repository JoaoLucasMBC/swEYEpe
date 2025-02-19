using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils.Datums;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.Rendering.DebugUI;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.Networking;

public class KeyboardTextSystem : MonoBehaviour
{
    public string currTextInput;
    public TextMeshPro textInputRef;
    public string targetText;
    public int targetTextLength;
    public TextMeshPro targetTextRef;
    public bool completed = false;
    public KeyboardExperimentManager manager;
    public FlashingScript blinker;
    public TextMeshPro TargetCopy;
    public TextMeshPro CurrCopy;
    public bool beganWord = false;


    private bool started = false;
    private bool inputtingName = false;
    private int currWord = 0;
    private List<String> topWords = new List<string>();
    private bool usingSpecial = false;
    private string lastInput = "";
    private NgramModel probGenerator = new NgramModel("Assets/probs.json");
    private LevenshteinModel wordGenerator = new LevenshteinModel("Assets/vocab.json", .5,1,1);
    private string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    private string[] randomWords =
    {
        "Enter your name",
        "DESPITE",
        "MINUTE",
        "REASON",
        "FOREIGN",
        "GREATER",
        "ANYBODY",
        "TEACHER",
        "FUTURE",
        "PERFECT",
        "TROUBLE",
        "BALANCE",
        "CAPABLE",
        "EXACTLY",
        "ANOTHER",
        "BELIEVE",
        "CONTROL",
        "CONNECT",
        "MESSAGE",
        "MISTAKE",
        "NETWORK",
        "PROCESS",
        "PROJECT",
        "SUPPORT",
        "THOUGHT",
        "HISTORY",
        "IMPROVE",
        "MEETING",
        "SECTION",
        "STATION",
        "THEREFORE",
        "REMEMBER",
        "Finished! Stop typing"
    };
    private string[] randomSentences =
    {
        "Enter your name",
        "THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG ",
        "A FASTER TRADER CAN SELL AT A HIGHER PRICE ",
        "WHATEVER YOU ARE BE A GOOD ONE ",
        "THE CHANGE YOU WISH TO SEE IN THE WORLD",
        "YOU ONLY FAIL IF YOU NEVER TRY ",
        "WHY DO YOU PLAY VIDEO GAMES EVERY DAY  ",
        "THE GLASS BREAKING WAS A MISTAKE TODAY ",
        "BUILD A NETWORK FOR A GREATER MEETING ",
        "DESPITE READING THE MESSAGE HE MADE HISTORY ",
        "JOHNNY SUPPORTS THEREFORE HE IS AMAZING ",
        "Finished! Stop typing"
    };
    private List<int> numberResults = new List<int>();


    private Dictionary<string, double> probabilities = new Dictionary<string, double>();
    public Dictionary<string, Vector3> RegularKeyboard = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> SpecialKeyboard = new Dictionary<string, Vector3>();
    private List<Vector3> ValueList = new List<Vector3>();
    private Dictionary<string, int> PosDict = new Dictionary<string, int>();

    public EyeTracker EyePos;
    private List<Vector3> gazePoints;
    private float currWordTime;
    private string fileId;

    void Awake()
    {
        gazePoints = new List<Vector3>();
        fileId = Guid.NewGuid().ToString().Substring(0, 8);
        //randomWords = randomSentences;
    }

    // Start is called before the first frame update
    void Start()
    {
        currTextInput = "";
        currWordTime = 0.0f;
        textInputRef.text = currTextInput;
        GenerateText();
        initRegular();
        initSpecial();
        initValueList();
        FillListWithRandomNumbers(numberResults, 10, 1, 30);
        //FillListWithRandomNumbers(numberResults, 5, 1, 10);
        manager.LoadCharacters(randomWords, numberResults, 10);
        updatePosDict();
    }

    private void Update()
    {
        CurrCopy.text = textInputRef.text;
        TargetCopy.text = targetTextRef.text;

        if (started || inputtingName)
        {
            var pos = manager.GetGazePoint();
            currWordTime += Time.deltaTime;
            gazePoints.Add(new Vector3(pos.x, pos.y, currWordTime));
        }

        if (currTextInput == targetText)
        {
            GenerateText();
            currTextInput = "";
            started = false;
            lastInput = "";
            manager.nextWord();
            gazePoints.Clear();
            currWordTime = 0.0f;
        }
    }

    public void RecieveInput(string text)
    {
        if (!started) return;

        if (text == "Clear")
        {
            currTextInput = "";
            blinker.inputted();
            // reset the eye positions list
            gazePoints.Clear();
            currWordTime = 0.0f;
            started = false;
            inputtingName = false;
        }
    }

    public void RecieveSuggestion(string input)
    {
        input = input.ToUpper();

        if (string.IsNullOrEmpty(input)) return;

        currTextInput = input;

        // update the last word
        //string[] words = currTextInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //words[words.Length - 1] = input;
        //currTextInput = string.Join(" ", words);
        //currTextInput += " ";

        blinker.inputted();

        lastInput = input[input.Length - 1].ToString();

        if (currTextInput == targetText || currWord == 1)
        {
            Debug.Log("entrei");
            GenerateText();
            currTextInput = "";
            started = false;
            lastInput = "";
            manager.nextWord();
            gazePoints.Clear();
            currWordTime = 0.0f;
        }

        textInputRef.text = currTextInput;

    }

    public void RecieveDelete()
    {
        blinker.deleted();

        currTextInput = "";

        //string[] words = currTextInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //Array.Resize(ref words, words.Length - 1);
        //currTextInput = string.Join(" ", words);
        //currTextInput += " ";

        textInputRef.text = currTextInput;

        gazePoints.Clear();
        currWordTime = 0.0f;
        started = false;
        inputtingName = false;
    }


    public void GenerateText()
    {

        if (currWord == 11)
        {
            targetText = randomWords.Last();
            manager.disable_timer();
        }
        else if (currWord > 0)
        {
            targetText = randomWords[numberResults[currWord-1]];
            currWord++;
        } else
        {
            targetText = randomWords[currWord];
            currWord++;
        }
      
        
        targetTextRef.text = targetText;
    }

    private void initRegular()
    {
        RegularKeyboard.Add("U", new Vector3(0.68f, 0.75f, 5.50f));
        RegularKeyboard.Add("W", new Vector3(-0.72f, 0.75f, 5.50f));
        RegularKeyboard.Add("C", new Vector3(-0.26f, 0.19f, 5.50f));
        RegularKeyboard.Add("R", new Vector3(-0.16f, 0.75f, 5.50f));
        RegularKeyboard.Add("X", new Vector3(-0.54f, 0.19f, 5.50f));
        RegularKeyboard.Add("T", new Vector3(0.12f, 0.75f, 5.50f));
        RegularKeyboard.Add("Z", new Vector3(-0.82f, 0.19f, 5.50f));
        RegularKeyboard.Add("E", new Vector3(-0.44f, 0.75f, 5.50f));
        RegularKeyboard.Add("K", new Vector3(1.05f, 0.47f, 5.50f));
        RegularKeyboard.Add("H", new Vector3(0.49f, 0.47f, 5.50f));
        RegularKeyboard.Add("N", new Vector3(0.58f, 0.19f, 5.50f));
        RegularKeyboard.Add("O", new Vector3(1.24f, 0.75f, 5.50f));
        RegularKeyboard.Add("G", new Vector3(0.21f, 0.47f, 5.50f));
        RegularKeyboard.Add("P", new Vector3(1.52f, 0.75f, 5.50f));
        RegularKeyboard.Add("Q", new Vector3(-1.00f, 0.75f, 5.50f));
        RegularKeyboard.Add("D", new Vector3(-0.35f, 0.47f, 5.50f));
        RegularKeyboard.Add("J", new Vector3(0.77f, 0.47f, 5.50f));
        RegularKeyboard.Add("Y", new Vector3(0.40f, 0.75f, 5.50f));
        RegularKeyboard.Add("S", new Vector3(-0.63f, 0.47f, 5.50f));
        RegularKeyboard.Add("A", new Vector3(-0.91f, 0.47f, 5.50f));
        RegularKeyboard.Add("V", new Vector3(0.02f, 0.19f, 5.50f));
        RegularKeyboard.Add("I", new Vector3(0.96f, 0.75f, 5.50f));
        RegularKeyboard.Add("B", new Vector3(0.30f, 0.19f, 5.50f));
        RegularKeyboard.Add("F", new Vector3(-0.07f, 0.47f, 5.50f));
        RegularKeyboard.Add("M", new Vector3(0.86f, 0.19f, 5.50f));
        RegularKeyboard.Add("L", new Vector3(1.33f, 0.47f, 5.50f));
        RegularKeyboard.Add("Clear", new Vector3(1.42f, .19f, 5.50f));
        RegularKeyboard.Add("Delete", new Vector3(1.14f, .19f, 5.50f));
    }

    private void initSpecial()
    {
        SpecialKeyboard.Add("T", new Vector3(0.07f, 0.45f, 5.00f));
        SpecialKeyboard.Add("D", new Vector3(-0.85f, 0.75f, 5.00f));
        SpecialKeyboard.Add("X", new Vector3(-0.23f, -0.17f, 5.00f));
        SpecialKeyboard.Add("E", new Vector3(-0.53f, 0.75f, 5.00f));
        SpecialKeyboard.Add("K", new Vector3(0.69f, 0.45f, 5.00f));
        SpecialKeyboard.Add("L", new Vector3(0.37f, 0.15f, 5.00f));
        SpecialKeyboard.Add("Z", new Vector3(-0.53f, -0.17f, 5.00f));
        SpecialKeyboard.Add("C", new Vector3(-0.85f, 0.15f, 5.00f));
        SpecialKeyboard.Add("G", new Vector3(0.07f, 1.07f, 5.00f));
        SpecialKeyboard.Add("N", new Vector3(-0.23f, 0.15f, 5.00f));
        SpecialKeyboard.Add("P", new Vector3(0.37f, 1.07f, 5.00f));
        SpecialKeyboard.Add("S", new Vector3(-0.53f, 0.15f, 5.00f));
        SpecialKeyboard.Add("F", new Vector3(-0.85f, 0.45f, 5.00f));
        SpecialKeyboard.Add("R", new Vector3(-0.23f, 0.45f, 5.00f));
        SpecialKeyboard.Add("Y", new Vector3(0.37f, 0.45f, 5.00f));
        SpecialKeyboard.Add("O", new Vector3(0.07f, 0.75f, 5.00f));
        SpecialKeyboard.Add("U", new Vector3(0.37f, 0.75f, 5.00f));
        SpecialKeyboard.Add("Q", new Vector3(-0.53f, 1.07f, 5.00f));
        SpecialKeyboard.Add("M", new Vector3(0.69f, 0.15f, 5.00f));
        SpecialKeyboard.Add("A", new Vector3(-0.53f, 0.45f, 5.00f));
        SpecialKeyboard.Add("V", new Vector3(0.07f, -0.17f, 5.00f));
        SpecialKeyboard.Add("H", new Vector3(0.07f, 0.15f, 5.00f));
        SpecialKeyboard.Add("B", new Vector3(0.37f, -0.17f, 5.00f));
        SpecialKeyboard.Add("J", new Vector3(0.69f, 0.75f, 5.00f));
        SpecialKeyboard.Add("I", new Vector3(-0.23f, 0.75f, 5.00f));
        SpecialKeyboard.Add("W", new Vector3(-0.23f, 1.07f, 5.00f));
        SpecialKeyboard.Add("Clear", new Vector3(0.69f, -0.17f, 5.00f));
        SpecialKeyboard.Add("Delete", new Vector3(-0.85f, -0.17f, 5.00f));
    }


    public Vector3 giveUpdatedPositions(string name)
    {
        if (usingSpecial)
        {
            return SpecialKeyboard[name];
        } else
        {
            return RegularKeyboard[name];
        }
    }

    public void RecieveSwap()
    {
        usingSpecial = !usingSpecial;
        manager.reset_timer();
        started = false;
        currWord = 0;
        GenerateText();
        currTextInput = "";
        lastInput = "";
        beganWord = false;
        textInputRef.text = currTextInput;

        manager.swapUsing();
    }

    public void RecieveEnter()
    {
        Debug.Log("Recieved enter");

        blinker.inputted();

        if (started)
        {
            StartCoroutine(runPrediction());
            string path = @"C:\Users\joaolmbc\Desktop\Softkeyboard\gaze-collection-topg-{0}.txt";
            if (currWord > 1)
            {
                OutputData.Write(gazePoints, randomWords[numberResults[currWord - 2]], string.Format(path, fileId));
            }
            started = false;
            gazePoints.Clear();
            currWordTime = 0.0f;
        }
        else
        {
            started = true;
            manager.enable_timer();
            manager.nextWord();
        }
    }

    private IEnumerator runPrediction()
    {
        GazeData gazeData = new GazeData { gaze_points = gazePoints };

        string json = JsonUtility.ToJson(gazeData);
        Debug.Log("Gaze Points: " + gazePoints);
        Debug.Log("Serialized JSON: " + json);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:5000/cluster", json, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                topWords.Clear();
                topWords.AddRange(response.top_words);
                Debug.Log("Top words updated.");

                currTextInput = topWords[0].ToUpper();
               // currTextInput += topWords[0].ToUpper() + " ";
                textInputRef.text = currTextInput;
            }
        }
    }

    [System.Serializable]
    public class GazeData
    {
        public List<Vector3> gaze_points;
    }


    [System.Serializable]
    public class Response
    {
        public List<string> top_words;
    }

    public List<string> GiveTopWords()
    {

        return topWords;
    }

    public float givePercentage(string input)
    {

        if (lastInput == "" || input == "Clear" || input == "Delete")
        {
            return 1;
        }
        /*
        else
        {
            return totalDict[lastInput][input] + .85f;
        }
        */
        float totalVal = 0;

        if (probabilities.ContainsKey(input.ToLower()))
        {
            totalVal += (float)probabilities[input.ToLower()];
        }

        

        return totalVal;
    }

    public int GetCurrWord()
    {
        return currWord;
    }

    private void initValueList()
    {
        ValueList.Add(new Vector3(-0.23f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(0.07f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(-0.23f, 0.15f, 5.00f));
        ValueList.Add(new Vector3(0.07f, 0.15f, 5.00f));
        ValueList.Add(new Vector3(-0.53f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(-0.23f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(-0.53f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(0.07f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(-0.53f, 0.15f, 5.00f));
        ValueList.Add(new Vector3(0.37f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(-0.53f, -0.17f, 5.00f));
        ValueList.Add(new Vector3(0.37f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(-0.23f, -0.17f, 5.00f));
        ValueList.Add(new Vector3(0.37f, 0.15f, 5.00f));
        ValueList.Add(new Vector3(0.07f, -0.17f, 5.00f));
        ValueList.Add(new Vector3(0.37f, -0.17f, 5.00f));
        ValueList.Add(new Vector3(-0.53f, 1.07f, 5.00f));
        ValueList.Add(new Vector3(-0.85f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(-0.23f, 1.07f, 5.00f));
        ValueList.Add(new Vector3(-0.85f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(0.07f, 1.07f, 5.00f));
        ValueList.Add(new Vector3(-0.85f, 0.15f, 5.00f));
        ValueList.Add(new Vector3(0.37f, 1.07f, 5.00f));
        ValueList.Add(new Vector3(0.69f, 0.75f, 5.00f));
        ValueList.Add(new Vector3(0.69f, 0.45f, 5.00f));
        ValueList.Add(new Vector3(0.69f, 0.15f, 5.00f));
    }

    public Vector3 PriorityPosition(string input)
    {
        if (input == "Clear")
        {
            return SpecialKeyboard["Clear"];
        }

        var index = PosDict[input];


        return ValueList[index];
    }

    private void updatePosDict()
    {
        PosDict.Clear();

        var counter = 0;

        var sortedKeyValuePairs = new List<KeyValuePair<String, double>>();
        sortedKeyValuePairs = probabilities.OrderByDescending(kvp => kvp.Value).ToList();

        var len = sortedKeyValuePairs.Count;

        for (int i = 0; i < alphabet.Length; i++)
        {

            if (!probabilities.ContainsKey(alphabet[i].ToLower()))
            {
                PosDict.Add(alphabet[i], len + counter);
                counter += 1;
            } else
            {
                int index = sortedKeyValuePairs.FindIndex(kvp => kvp.Key == alphabet[i].ToLower());
                PosDict.Add(alphabet[i], index);
            }
        }
    }

    public static void FillListWithRandomNumbers(List<int> list, int count, int minValue, int maxValue)
    {
        if (count > (maxValue - minValue + 1))
        {
            throw new ArgumentException("Count is larger than the range of unique numbers available.");
        }

        List<int> availableNumbers = Enumerable.Range(minValue, maxValue - minValue + 1).ToList();
        System.Random random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            int index = random.Next(availableNumbers.Count);
            list.Add(availableNumbers[index]);
            availableNumbers.RemoveAt(index);
        }
    }
}
