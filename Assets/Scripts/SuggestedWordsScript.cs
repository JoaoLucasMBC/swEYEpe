using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SuggestedWordsScript : MonoBehaviour
{
    public TextMeshPro firstWord;
    public TextMeshPro secondWord;
    public TextMeshPro thirdWord;
    private List<TextMeshPro> suggestedWords = new List<TextMeshPro>();

    public KeyboardTextSystem keyboard;


    // Start is called before the first frame update
    void Start()
    {
        suggestedWords.Add(firstWord);
        suggestedWords.Add(secondWord);
        suggestedWords.Add(thirdWord);
    }

    // Update is called once per frame
    void Update()
    {
        ClearText();

        var topWords = keyboard.GiveTopWords();

        
        for (int i = 0; i < topWords.Count; i++)
        {
            if (i >= 3)
            {
                break;
            }

            suggestedWords[i].text = topWords[i].ToUpper();
        }
    }

    void ClearText()
    {
        for (int i = 0; i < suggestedWords.Count;i++)
        {
            suggestedWords[i].text = "";
        }
    }
}
