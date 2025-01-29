using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    // Placement
    Transform topHandle;
    Transform topExtreme;

    Transform bottomHandle;
    Transform bottomExtreme;

    Transform bottomScoreThreshold;

    // Movement
    public float direction;
    public float speed;

    // State
    public bool isAlive = true;
    bool isScored = false;

    float delayTimer;
    float startDelay;    

    public Collider2D enemyCollider;

    // Visuals
    public List<SpriteRenderer> spriteRenderers;

    public Color topColor = Color.gray;
    public Color bottomColor = Color.black;

    public float topSpeedFactor = 2f;
    public float topScale = 0.5f;
    public Vector3 bottomScale = Vector3.one;
    Vector3 originalScale = Vector3.one;    

    public void Setup(float delay,
        Transform top, Transform topMax,
        Transform bottom, Transform bottomMax,
        Transform bottomThreshold)
    {
        // Remember these values
        startDelay = delay;

        topHandle = top;
        topExtreme = topMax;

        bottomHandle = bottom;
        bottomExtreme = bottomMax;

        bottomScoreThreshold = bottomThreshold;

        // Set ourselves under the top handle, move right
        transform.SetParent(topHandle, false);
        transform.localPosition = Vector3.zero;
        direction = 1;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = topColor;
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale * topScale;

        enemyCollider.enabled = false;

        // Start delay is modified by the top speed
        delayTimer = startDelay / topSpeedFactor;
    }

    void Update()
    {
        // Don't move until our start delay is gone
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;   
        }
        else if (isAlive)
        {
            // The top = moving right
            bool isTop = direction > 0;

            // Move the character
            float movement = direction * speed * Time.deltaTime;
            if (isTop)
            {
                movement *= topSpeedFactor;
            }
            transform.position += Vector3.right * movement;

            // Is on the top?
            if (isTop)
            {
                // See if we've passed the extreme position
                Vector3 goal = topExtreme.position;
                if (transform.position.x > goal.x)
                {
                    // Move to the bottom, switch our direction
                    float extremeDelta = transform.position.x - goal.x;
                    transform.SetParent(bottomHandle, false);

                    direction *= -1;
                    transform.localPosition = Vector3.right * extremeDelta * direction;

                    // Change color too
                    foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                    {
                        spriteRenderer.color = bottomColor;
                    }

                    transform.localScale = Vector3.Scale(originalScale, bottomScale);

                    enemyCollider.enabled = true;

                    // Delay again
                    delayTimer = startDelay - (startDelay / topSpeedFactor);
                }
            }
            // On the bottom
            else
            {
                // Scoree?
                if (!isScored)
                {
                    Vector3 threshold = bottomScoreThreshold.position;
                    if (transform.position.x < threshold.x)
                    {
                        isScored = true;

                        UiManager uiManager = FindObjectOfType<UiManager>();
                        if (uiManager != null)
                        {
                            uiManager.OnScore();
                        }
                    }
                }

                // Look for the extreme and delete ourselves
                Vector3 goal = bottomExtreme.position;
                if (transform.position.x < goal.x)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void OnGameLose()
    {
        // Change color, stop from updating
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = Color.white;
        }

        isAlive = false;
    }
}
