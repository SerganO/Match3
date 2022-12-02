using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LevelHandler : MonoBehaviour
{
    public GridController GridController;

    public TMP_Text Scores;
    public TMP_Text WidthValue;
    public TMP_Text HeightValue;

    public void IncrementWidth()
    {
        GridController.GridData.Width += 1;
        GridController.Rebuild();
        ResetScores();
        WidthValue.text = $"{GridController.GridData.Width}";
    }
    public void DecrementWidth()
    {
        if(GridController.GridData.Width <= 3)
        {
            return;
        }
        GridController.GridData.Width -= 1;
        GridController.Rebuild();
        ResetScores();
        WidthValue.text = $"{GridController.GridData.Width}";
    }
    
    public void IncrementHeight()
    {
        GridController.GridData.Height += 1;
        GridController.Rebuild();
        ResetScores();
        HeightValue.text = $"{GridController.GridData.Height}";
    }
    
    public void DecrementHeight()
    {

        if (GridController.GridData.Height <= 3)
        {
            return;
        }
        GridController.GridData.Height -= 1;
        GridController.Rebuild();
        ResetScores();
        HeightValue.text = $"{GridController.GridData.Height}";
    }

    public void AddScores(int scores)
    {
        var current = Convert.ToInt32(Scores.text);
        Scores.text = $"{current + scores}";
    }

    public void ResetScores()
    {
        Scores.text = "0";
    }
}
