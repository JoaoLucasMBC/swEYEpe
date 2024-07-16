using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterScript : MonoBehaviour
{
    public EyeTracker EyePos;
    private Renderer rend;
    private float timeToInput = 0.4f;
    private float timer = 0;
    public KeyboardTextSystem keyboard;


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        rend = gameObject.GetComponent<Renderer>();

        if (LookingAtBox(EyePos.worldPosition, EyePos.gazeLocation))
        {
            rend.enabled = false;
            timer += Time.deltaTime;

            if (timer > timeToInput)
            {
                timer = 0;
                keyboard.RecieveEnter();
            }
        }
        else
        {
            rend.enabled = true;
            timer = 0;
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
