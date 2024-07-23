using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OutputData : MonoBehaviour
{
    public static void Write(List<Vector3> gazePoints, string word, string path)
    {
        using (StreamWriter stream = new FileInfo(path).AppendText())
        {
            for (int i = 0; i < gazePoints.Count; i++)
            {
                var point = gazePoints[i];
                stream.WriteLine(word + "," + point.x + "," + point.y + "," + point.z);
            }

            stream.WriteLine("");
        }
    }

}
