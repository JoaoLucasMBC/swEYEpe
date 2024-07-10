using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class NgramModel
{
    private Dictionary<string, Dictionary<string, double>> probs;
    private string text;

    public NgramModel(string jsonFilePath)
    {
        ReadProbabilities(jsonFilePath);
        text = "%%%";
    }

    public string Text
    {
        get { return text; }
        set { text = value; }
    }

    public Dictionary<string, Dictionary<string, double>> Probs
    {
        get { return probs; }
        set { probs = value; }
    }

    private void ReadProbabilities(string jsonFilePath)
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonText = File.ReadAllText(jsonFilePath);
            probs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(jsonText);    
            //Debug.Log("Successfully loaded JSON file: " + jsonFilePath);
        }
        else
        {
            Debug.Log("Failed to load JSON file: " + jsonFilePath);
        }
    }

    public Dictionary<string, double> GetTopWeights(int n)
    {
        //Debug.Log(text);
        string suffix = text.Substring(text.Length - n);
        if (!probs.ContainsKey(suffix))
        {
            return new Dictionary<string, double>();
        }

        var weights = probs[suffix];
        var top = GetTopNIndices(weights, Math.Min(weights.Count, 26)).ToList();

        var topWords = top.Select(i => weights.ElementAt(i).Key).ToList();
        var topFreqs = top.Select(i => weights.ElementAt(i).Value).ToList();

        var result = new Dictionary<string, double>();
        for (int i = 0; i < topWords.Count; i++)
        {
            //Debug.Log(topWords[i]);
            result[topWords[i]] = topFreqs[i];
        }

        return result;
    }

    private IEnumerable<int> GetTopNIndices(Dictionary<string, double> dictionary, int n)
    {
        int count = dictionary.Count;
        int[] indices = Enumerable.Range(0, count).ToArray();

        Array.Sort(indices, (i, j) => dictionary.Values.ElementAt(j).CompareTo(dictionary.Values.ElementAt(i)));

        return indices.Take(n);
    }
}