using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreBoardManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        PuckDibleScript.puckDibleAddToScore += UpdateScore;
        GhostScript.ghostAddToScore += UpdateScore;
        text.text = "Score: " + score.ToString();
    }

    private void OnDisable()
    {
        PuckDibleScript.puckDibleAddToScore -= UpdateScore;
        GhostScript.ghostAddToScore -= UpdateScore;
    }

    void UpdateScore(int scoreIncr)
    {
        score += scoreIncr;
        text.text = "Score: " + score.ToString();
    }
}
