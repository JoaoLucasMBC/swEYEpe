using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OutputData : MonoBehaviour
{
    public static void Write(List<Vector2> gazePoints, string word, string path)
    {
        using (StreamWriter stream = new FileInfo(path).AppendText())
        {
            stream.WriteLine(word);

            for (int i = 0; i < gazePoints.Count; i++)
            {
                var point = gazePoints[i];
                stream.WriteLine(point.x + "," + point.y);
            }

            stream.WriteLine("");
        }
    }

}
