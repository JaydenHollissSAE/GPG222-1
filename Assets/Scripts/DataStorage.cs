using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System;

public class DataStorage : MonoBehaviour
{
    public string path;
    public string[] list;
    public List<string> players = new List<string>();
    public List<float> scores = new List<float>();
    private float highScore;
    private string highPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetFloat("HighScore");
        }
        if (PlayerPrefs.HasKey("HighPlayer"))
        {
            highPlayer = PlayerPrefs.GetString("HighPlayer");
        }


        list = File.ReadAllText(path).Replace("\n", ",").Replace("\\n", ",").Replace("\"", "").Split(",");


        int a = 0;
        //Debug.Log(list.Length);
        while (a < list.Length)
        {
            players.Add(list[a + 1]);
            scores.Add(float.Parse(list[a + 2]));
            a += 3;
        }

        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] > highScore) 
            { 
                highScore = scores[i];
                highPlayer = players[i];
            }

        }

        PlayerPrefs.SetFloat("HighScore", highScore);
        PlayerPrefs.SetString("HighPlayer", highPlayer);



    }
}
