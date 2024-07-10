using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BypassDisabling : MonoBehaviour
{
    public EyeTracker EyePos;
    private Renderer rend;
    public float timeToInput = .7f;
    private float timer = 0;

    private bool Override = false;

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
                Override = !Override;
            }
        }
        else
        {
            rend.enabled = true;
            timer = 0;
        }

        if (Override)
        {
            rend.material.color = Color.red;
        } else
        {
            rend.material.color = Color.green;
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

    public bool Get_Override()
    {
        return Override; 
    }
}
