using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterScript : MonoBehaviour
{
    public EyeTracker EyePos;
    private Renderer rend;
    private float timeToInput = 0.15f;
    private float timeout = 0.6f;
    private float timer = 0f;
    private bool looked = false;
    public KeyboardTextSystem keyboard;

    // Update is called once per frame
    void Update()
    {

        rend = gameObject.GetComponent<Renderer>();
        timer += Time.deltaTime;

        if (LookingAtBox(EyePos.worldPosition, EyePos.gazeLocation))
        {
            rend.enabled = false;
            
            if (!looked && timer > timeToInput)
            {
                looked = true;
                timer = 0f;
                keyboard.RecieveEnter();
            }
        }
        else
        {
            rend.enabled = true;

            if (!looked)
            {
                timer = 0;
            }
        }

        if (looked && timer > timeout)
        {
            timer = 0;
            looked = false;
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
}
