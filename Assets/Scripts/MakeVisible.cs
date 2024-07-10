using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeVisible : MonoBehaviour
{
    // Start is called before the first frame update
    public EyeTracker EyePos;
    private Renderer rend;
    private bool toEnable = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(EyePos.gazeLocation, gameObject.transform.position);

        if (dist < 1)
        {
            Debug.Log("We are enabled");
            toEnable = true;
        } else
        {
            toEnable = false;
        }

        foreach (MeshRenderer variableName in GetComponentsInChildren<MeshRenderer>())
        {
            variableName.enabled = toEnable;
        }
    }
}
