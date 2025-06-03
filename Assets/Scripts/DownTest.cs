using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DownTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetText("DownTest2"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GetText(string file_name)
    {


        string url = "https://docs.google.com/spreadsheets/d/10ThOUsiKIAiXvGgI9vjkKpo07yGdn1yVbsf4xB1a93Q/gviz/tq?tqx=out:csv&sheet=Sheet1";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string savePath = string.Format("{0}.csv", file_name);
                System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
            }
        }
    }


}
