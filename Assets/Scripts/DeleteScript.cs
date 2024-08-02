using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteScript : MonoBehaviour
{
    public EyeTracker EyePos;
    private Renderer rend;
    public float timeToInput = .7f;
    private float timer = 0;
    public KeyboardTextSystem keyboard;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //CheckPos();
        rend = gameObject.GetComponent<Renderer>();

        if (LookingAtBox(EyePos.worldPosition, EyePos.gazeLocation))
        {
            rend.enabled = false;
            timer += Time.deltaTime;

            if (timer > timeToInput)
            {
                timer = 0;
                keyboard.RecieveDelete();
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

    void CheckPos()
    {
        var newPos = keyboard.giveUpdatedPositions(gameObject.name);

        if (newPos == gameObject.transform.position)
        {
            return;
        }
        else
        {
            gameObject.transform.position = newPos;
        }
    }
}
