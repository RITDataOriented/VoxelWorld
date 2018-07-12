using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// text conversion code obtained 
// from: https://forum.unity.com/threads/using-the-job-system-to-save-images.522837/#post-3434261
// used to log text to a file
// assumes it writes the whole file at once
public struct LogToFile : IJob
{
    public NativeArray<byte> logStr;

    public void Execute ()
    {
        string resultString = Encoding.ASCII.GetString(logStr.ToArray());
        // not the best way to do this - way the tutorial did it
        System.IO.File.WriteAllText("../Log.txt", resultString);

    }
}
