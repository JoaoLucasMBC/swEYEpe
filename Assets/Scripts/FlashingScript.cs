using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingScript : MonoBehaviour
{
    private float timer = 0;
    private bool On = false;
    private float timeToEnable = .2f;
    KeyboardTextSystem keyboard;

    // Start is called before the first frame update
    void Start()
    {
        makeDisappear();
    }

    // Update is called once per frame
    void Update()
    {
        if (On)
        {
            timer += Time.deltaTime;

            if (timer > timeToEnable)
            {
                On = false;
                timer = 0;
                makeDisappear();
            }
        }
    }

    public void inputted()
    {
        On = true;
        makeCyan();
    }

    public void deleted()
    {
        On = true;
        makeRed();
    }


    void makeCyan()
    {
        foreach (Renderer variableName in GetComponentsInChildren<Renderer>())
        {
            variableName.enabled = true;
            variableName.material.color = Color.cyan;
        }
        changeOpacity();

    }

    void makeRed()
    {
        foreach (Renderer variableName in GetComponentsInChildren<Renderer>())
        {
            variableName.enabled = true;
            variableName.material.color = Color.red;
        }
        changeOpacity();

    }

    void makeDisappear()
    {
        foreach (Renderer variableName in GetComponentsInChildren<Renderer>())
        {
            variableName.enabled = false;
        }
    }

    void changeOpacity()
    {
        foreach (Renderer variableName in GetComponentsInChildren<Renderer>())
        {
            var trans = 0.05f;
            var col = variableName.material.color;
            col.a = trans;
            variableName.material.color = col;
        }
    }
}
