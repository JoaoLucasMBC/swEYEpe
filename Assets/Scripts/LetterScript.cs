using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class LetterScript : MonoBehaviour
{
    // Start is called before the first frame update
    public EyeTracker EyePos;
    private bool MovingKeyboard = false;
    public float timeToInput = 1f;
    public KeyboardTextSystem keyboard;
    public BypassDisabling disabling;

    private Renderer rend;
    private float timer = 0;
    private bool PartiallyOn = true;
    private bool TotallyOn = true;
    private Vector3 reset = new Vector3(.2f,.2f,.2f);
    private bool justEntered = false;

    private bool neverEnter = false;

    void Start()
    {
        //keyboard.CollectPositions(gameObject.name, gameObject.transform.position);
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateOn();
        PickPos();
        
        gameObject.transform.localScale = reset;
        //gameObject.transform.localScale *= keyboard.giveSize(gameObject.name);

        SetVars();

        

        //if (LookingAtBox(EyePos.worldPosition, EyePos.gazeLocation) && ((rend.enabled == true) || (PartiallyOn)))


        if(neverEnter)
        {
            rend.enabled = false;
            timer += Time.deltaTime;

            if (timer > timeToInput && !justEntered)
            {
                timer = 0;
                keyboard.RecieveInput(gameObject.name);
                justEntered = true;
            }
        } else 
        { 
            timer = 0;
            justEntered = false;
        }
    }

    bool LookingAtBox(Vector3 userPosition, Vector3 fixationPoint)
    {

        Vector3 direction = (fixationPoint - userPosition).normalized;
        float distance = Vector3.Distance(userPosition, fixationPoint);
        Ray ray = new Ray(userPosition, direction);

        if (rend.bounds.IntersectRay(ray))
        {
            return true;
        }

        return false;
    }

    void CheckPos()
    {
        var newPos = keyboard.giveUpdatedPositions(gameObject.name);

        if (newPos == gameObject.transform.position) {
            return;
        } else
        {
            gameObject.transform.position = newPos;
        }
    }

    void UpdateOn()
    {
        var percentage = keyboard.givePercentage(gameObject.name);
       
        if (percentage < .05) { TotallyOn = false;  } else { TotallyOn = true; }
        if (percentage > 0)   { PartiallyOn = true; } else { PartiallyOn = false; }
    }

    void MakeOpaque()
    {
        var trans = 0.00f;
        var col = rend.material.color;
        col.a = trans;
        rend.material.color = col;
    }

    void PriorityPos()
    {
        var newPos = keyboard.PriorityPosition(gameObject.name);

        gameObject.transform.position = newPos;
    }

    void PickPos()
    {
        if (MovingKeyboard) { PriorityPos();} else { CheckPos();}        
    }

    void SetVars()
    {
        if (keyboard.GetCurrWord() == 1) { rend.enabled = true; } else { rend.enabled = TotallyOn; }

        if (MovingKeyboard) 
        { 
            timeToInput = .8f; 
        } else {
            if (keyboard.beganWord && TotallyOn)
            {
                timeToInput = .35f;
            }
            else if (keyboard.beganWord && PartiallyOn)
            {
                timeToInput = .8f;
            }
            else
            {
                timeToInput = .8f;
            }
        }

        if (disabling.Get_Override())
        {
            rend.enabled = true;
        }
    }
}
