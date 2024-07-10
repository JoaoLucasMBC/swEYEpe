using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using System.Linq;

public class GazeViewer : MonoBehaviour
{
    public GameObject fixationPoint;

    public int smoothingArraySize; //The max size of the smoothing array
    private List<Vector3> smoothingArray = new List<Vector3>();
    private Vector3 smoothedGazeLocation = Vector3.zero;
    public EyeTracker EyePos;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        smoothingArray.Add(EyePos.gazeLocation);
        //Debug.Log(EyePos.gazeLocation);

        if (smoothingArray.Count <= smoothingArraySize)
        {
            smoothedGazeLocation = EyePos.gazeLocation;
        }
        else
        {
            //Smoothing Array is smoothing array size plus 1
            smoothingArray.RemoveAt(0);
            smoothedGazeLocation = smoothingArray.Aggregate(new Vector3(0, 0, 0), (s, v) => s + v) / (float)smoothingArray.Count;
        }



        if (fixationPoint != null)
        {
            //Debug.Log(smoothedGazeLocation);
            fixationPoint.transform.position = smoothedGazeLocation;
        }

    }
}
