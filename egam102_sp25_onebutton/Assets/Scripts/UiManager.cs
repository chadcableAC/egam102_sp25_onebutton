using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    // Enable handles
    public GameObject winHandle;
    public GameObject loseHandle;
    public GameObject gameHandle;

    // Score UI
    public TextMeshProUGUI scoreText;
    int score = 0;

    public TextMeshProUGUI waveText;
    public float waveTextDuration = 1f;
    float waveTimer = 0;

    void Start()
    {
        // Show game screen
        winHandle.SetActive(false);
        loseHandle.SetActive(false);

        // Reset the score
        scoreText.text = "Score: " + score;
    }

    private void Update()
    {
        if (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer < 0)
            {
                waveText.text = string.Empty;
            }
        }
    }

    public void OnGameLose()
    {
        loseHandle.SetActive(true);
        scoreText.color = Color.black;

        waveText.text = string.Empty;
    }

    public void OnGameWin()
    {
        winHandle.SetActive(true);

        waveText.text = string.Empty;
    }

    public void OnWave(int currentWave, int totalWaves)
    {
        // Increment
        waveText.text = "Wave " + currentWave + " of " + totalWaves;
        waveTimer = waveTextDuration;
    }

    public void OnScore()
    {
        // Increment
        score++;
        scoreText.text = "Score: " + score;
    }
}
