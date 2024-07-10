using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils.Datums;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.Rendering.DebugUI;
using Vector3 = UnityEngine.Vector3;

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


    private bool everStarted = false;
    private int currWord = 0;
    private List<String> topWords = new List<string>();
    private bool usingSpecial = false;
    private string lastInput = "";
    private NgramModel probGenerator = new NgramModel("Assets/probs.json");
    private LevenshteinModel wordGenerator = new LevenshteinModel("Assets/vocab.json", .5,1,1);
    private string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    private string[] randomWords =
{
    "Enter your name, then press the = key",
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
    private List<int> numberResults = new List<int>();


    private Dictionary<string, double> probabilities = new Dictionary<string, double>();
    public Dictionary<string, Vector3> RegularKeyboard = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> SpecialKeyboard = new Dictionary<string, Vector3>();
    private List<Vector3> ValueList = new List<Vector3>();
    private Dictionary<string, int> PosDict = new Dictionary<string, int>();


    // Start is called before the first frame update
    void Start()
    {
        currTextInput = "";
        textInputRef.text = currTextInput;
        GenerateText();
        initRegular();
        initSpecial();
        initValueList();
        FillListWithRandomNumbers(numberResults, 10, 1, 30);
        manager.LoadCharacters(randomWords, numberResults, 10);

        probabilities = probGenerator.GetTopWeights(4);

        updatePosDict();
    }

    private void Update()
    {
        CurrCopy.text = textInputRef.text;
        TargetCopy.text = targetTextRef.text;
    }

    public void RecieveInput(string text)
    {
        if (!everStarted) return;

        if (text == "Clear")
        {
            currTextInput = "";
            blinker.inputted();
        }
        else
        {
            // call the function that updates the trie

            
        }

        if (currTextInput == targetText)
        {
            GenerateText();
            currTextInput = "";
            completed = true;
            manager.nextWord();
            manager.pause_timer();
        }

    }

    public void RecieveSuggestion(string input)
    {
        if (!everStarted) return;

        input = input.ToUpper();

        currTextInput = input;
        blinker.inputted();
        beganWord = true;

        lastInput = input[input.Length - 1].ToString();

        if (currTextInput == targetText)
        {
            GenerateText();
            currTextInput = "";
            completed = true;
            beganWord = false;
            lastInput = "";
            manager.nextWord();
        }
        GenerateWords();

        textInputRef.text = currTextInput;

    }

    public void RecieveDelete()
    {

        if (currTextInput == "")
        {
            return;
        }

        blinker.deleted();

        currTextInput = "";
        textInputRef.text = currTextInput;

        int len = currTextInput.Length;

        if (len > 0)
        {
            lastInput = currTextInput[currTextInput.Length - 1].ToString();
        } else
        {
            lastInput = "";
            beganWord= false;
        }
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
        everStarted = false;
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

        if (everStarted)
        {
            GenerateText();
        }
        else
        {
            everStarted = true;
        }

        manager.enable_timer();
        manager.nextWord();
    }

    private void GenerateWords()
    {
        topWords = wordGenerator.FindTopKNearestWords(currTextInput, 3);
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
