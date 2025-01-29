using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Enemy info
    public EnemyScript enemyPrefab;
    List<EnemyScript> activeEnemies = new List<EnemyScript>();

    // Placement info
    public Transform topHandle;
    public Transform topExtreme;

    public Transform bottomHandle;
    public Transform bottomExtreme;

    public Transform bottomScoreThreshold;

    // Level information
    public SpriteRenderer[] bgRenderers;
    public Camera gameCamera;

    // Wave information
    public List<EnemyWave> easyWaves;
    public List<EnemyWave> mediumWaves;
    public List<EnemyWave> hardWaves;

    public int maxWaves = 10;
    public int easyWaveCount = 3;
    public int mediumWaveCount = 9;
    int waveCount = 0;

    bool isAlive = true;

    void Update()
    {
        if (isAlive)
        {
            // Are all enemies gone?  Make a new wave of enemies
            bool allGone = true;
            foreach (EnemyScript enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    allGone = false;
                    break;
                }
            }

            if (allGone)
            {
                activeEnemies.Clear();
                NewWave();
            }
        }
    }

    void NewWave()
    {
        // Did we complete enough waves? Win the game!
        if (waveCount >= maxWaves)
        {
            UiManager uiManager = FindObjectOfType<UiManager>();
            if (uiManager != null)
            {
                uiManager.OnGameWin();
            }
        }
        // Make a wave of enemies
        else
        {
            // Increment the number of waves
            waveCount++;

            // Tell the UI
            UiManager uiManager = FindObjectOfType<UiManager>();
            if (uiManager != null)
            {
                uiManager.OnWave(waveCount, maxWaves);
            }

            // Which wave type?
            List<EnemyWave> waves = easyWaves;
            if (waveCount >= mediumWaveCount)
            {
                waves = hardWaves;
            }
            if (waveCount >= easyWaveCount)
            {
                waves = mediumWaves;
            }

            // Pick a random wave
            int randomIndex = Random.Range(0, waves.Count);
            if (randomIndex < waves.Count)
            {
                // Create all of the enemies in this wave
                EnemyWave wave = waves[randomIndex];
                for (int i = 0; i < wave.prefabs.Count; i++)
                {
                    CreateEnemy(wave.prefabs[i], wave.delays[i]);
                }
            }
        }
    }

    void CreateEnemy(EnemyScript prefab, float delay)
    {
        // Make a new enemy, asssign the needed values
        EnemyScript newEnemy = Instantiate(prefab);
        newEnemy.Setup(delay, topHandle, topExtreme, bottomHandle, bottomExtreme, bottomScoreThreshold);

        // Add to list of enemies
        activeEnemies.Add(newEnemy);
    }

    public void OnGameLose()
    {
        // Pause all enemies
        EnemyScript[] enemies = FindObjectsOfType<EnemyScript>();
        foreach (EnemyScript enemy in enemies)
        {
            enemy.OnGameLose();
        }

        // Change level elements
        foreach (SpriteRenderer spriteRenderer in bgRenderers)
        {
            Color newColor = Color.white;
            newColor.a = spriteRenderer.color.a;
            spriteRenderer.color = newColor;
        }

        gameCamera.backgroundColor = Color.black;
    }
}
