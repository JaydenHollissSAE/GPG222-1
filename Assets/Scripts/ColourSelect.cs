using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourSelect : MonoBehaviour
{
    private List<Image> colourButtons = new List<Image>();
    private bool start = true;
    void Start()
    {
        int children = transform.childCount;
        for (int i = 0; i < children; i++)
        {
            Transform child = transform.GetChild(i);
            int children2 = child.childCount;
            for (int j = 0; j < children2; j++)
            {
                colourButtons.Add(child.GetChild(j).GetComponent<Image>());
            }
        }
        if (GameManager.instance == null) GameManager.instance = FindFirstObjectByType<GameManager>();
        for (int i = 0; i < colourButtons.Count; i++)
        {
            colourButtons[i].color = GameManager.instance.drawingColours[i];
        }
        gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        if (!start) Cursor.visible = false;
        else start = true;
    }


    public void SetNewColour(int index)
    {
        if (GameManager.instance == null) GameManager.instance = FindFirstObjectByType<GameManager>();
        GameManager.instance.localDraw.SetColourSprites(GameManager.instance.drawingColours[index]);
    }



}
