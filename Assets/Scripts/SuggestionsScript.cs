using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SuggestionsScript : MonoBehaviour
{
    public EyeTracker EyePos;
    private Renderer rend;
    private float timeToInput = .8f;
    private float timer = 0;
    public KeyboardTextSystem keyboard;
    public TextMeshPro curr;

    private bool justEntered = false;

    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {


        if (LookingAtSuggestion(EyePos.worldPosition, EyePos.gazeLocation))
        {
            rend.enabled = false;
            timer += Time.deltaTime;

            if (timer > timeToInput && !justEntered)
            {
                timer = 0;
                keyboard.RecieveSuggestion(curr.text);
                justEntered = true;
            }
        }
        else
        {
            timer = 0;
            rend.enabled = true;
            justEntered = false;
        }
    }

    bool LookingAtSuggestion(Vector3 userPosition, Vector3 fixationPoint)
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
