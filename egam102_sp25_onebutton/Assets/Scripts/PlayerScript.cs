using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Visuals
    public Transform moveHandle = null;
    public Transform scaleHandle = null;
    public Transform colliderScaleHandle = null;

    Vector3 originalPosition = Vector3.zero;
    Vector3 originalScale = Vector3.one;
    Vector3 originalColliderScale = Vector3.one;

    public SpringInterper scaleSpring;
    public float jumpNudge = 1f;
    public float landNudge = -1f;

    public SpriteRenderer spriteRenderer;

    // Jumping
    float jumpTimer = 0;
    float currentJumpHeight;
    float currentJumpDuration;

    public float jumpMinHeight = 1f;
    public float jumpMaxHeight = 2f;

    public float jumpMinDuration = 0.33f;
    public float jumpMaxDuration = 1f;

    public AnimationCurve jumpCurve = null;

    public float gravity = -1f;
    public float gravityFactor = 2f;
    float gravityVelocity = 0;

    // Scale
    float squashTimer = 0;
    public float maxSquashDuration = 1f;

    public Vector2 squashScale = Vector2.one;
    public Vector2 stretchScale = Vector2.one;

    public Vector2 colliderSquashScale = Vector2.one;
    public Vector2 colliderStretchScale = Vector2.one;

    // State
    bool isAlive = true;
    bool isGrounded = true;
    bool isJumping = false;

    void Start()
    {
        // Remember the original sizes
        originalPosition = moveHandle.localPosition;
        originalScale = scaleHandle.localScale;
        originalColliderScale = colliderScaleHandle.localScale;

        scaleSpring.SetGoal(0, true);
    }

    void Update()
    {
        if (isAlive)
        {
            // On the ground?
            if (isGrounded)
            {
                // Hold the button? Squash down
                if (IsPressed())
                {
                    squashTimer += Time.deltaTime;

                    float squashInterp = squashTimer / maxSquashDuration;
                    float squashScale = Mathf.Lerp(0, -1, squashInterp);
                    scaleSpring.SetGoal(squashScale, false);
                }
                // Release? Jump into the air
                else if (IsUp())
                {
                    isJumping = true;
                    isGrounded = false;
                    jumpTimer = 0;

                    // Jump height is based on how long we squashed
                    float squashInterp = squashTimer / maxSquashDuration;
                    currentJumpHeight = Mathf.Lerp(jumpMinHeight, jumpMaxHeight, squashInterp);
                    currentJumpDuration = Mathf.Lerp(jumpMinDuration, jumpMaxDuration, squashInterp);

                    // Return scale to 0, nudge the spring
                    scaleSpring.SetGoal(0, true);
                    scaleSpring.value = 1f;
                    scaleSpring.velocity += jumpNudge;
                }
            }
            // Jumping?
            else if (isJumping)
            {
                jumpTimer += Time.deltaTime;

                // Move up based on the curve
                float jumpInterp = jumpTimer / currentJumpDuration;
                float yPos = jumpCurve.Evaluate(jumpInterp) * currentJumpHeight;
                moveHandle.localPosition = originalPosition + Vector3.up * yPos;

                // Did we max out on how long we can hold jump?
                if (jumpTimer > currentJumpDuration ||
                    IsDown())
                {
                    isJumping = false;
                }
            }
            // In air?
            else
            {
                // Move downwards
                float downwardMovement = gravity * Time.deltaTime;

                // Twice as fast if we're holding jump
                if (IsPressed())
                {
                    downwardMovement *= gravityFactor;
                    scaleSpring.SetGoal(1, true);
                }
                else
                {
                    scaleSpring.SetGoal(0, false);
                }

                gravityVelocity += downwardMovement;
                moveHandle.localPosition += Vector3.up * gravityVelocity;

                float heightValue = moveHandle.localPosition.y - originalPosition.y;
                float heightInterp = heightValue / jumpMaxHeight;
                SetScale(heightInterp);

                // Hit the floor? Switch to grounded
                if (moveHandle.localPosition.y < originalPosition.y)
                {
                    // Return scale to 0, nudge
                    scaleSpring.SetGoal(0, true);
                    scaleSpring.value = -1f;
                    scaleSpring.velocity += landNudge + gravityVelocity;

                    squashTimer = 0;
                    gravityVelocity = 0;

                    isGrounded = true;
                    moveHandle.localPosition = originalPosition;
                }
            }

            // Match the scale
            SetScale(scaleSpring.value);
        }
    }

    // Controls the size of the player
    void SetScale(float scaleInterp)
    {
        // Squash / stretch based on the offset
        Vector2 scaleFactor = Vector2.one;
        Vector2 colliderScaleFactor = Vector2.one;

        // Negative = squash
        if (scaleInterp < 0)
        {
            scaleFactor = Vector2.LerpUnclamped(Vector2.one, squashScale, Mathf.Abs(scaleInterp));
            colliderScaleFactor = colliderSquashScale;
        }
        // Positive = stretch
        else if (scaleInterp > 0)
        {
            scaleFactor = Vector2.LerpUnclamped(Vector2.one, stretchScale, scaleInterp);
            colliderScaleFactor = colliderStretchScale;
        }        

        scaleHandle.localScale = Vector3.Scale(originalScale, scaleFactor);
        colliderScaleHandle.localScale = Vector3.Scale(originalColliderScale, colliderScaleFactor);
    }

    // Make our own functions to detect input
    // This allows for multiple input types (space bar and mouse click)

    bool IsPressed()
    {
        // Return true if any of the inputs are pressed
        bool isPressed =
            Input.GetKey(KeyCode.Space) ||
            Input.GetMouseButton(0);

        return isPressed;
    }

    bool IsDown()
    {
        // Return true if any of the inputs are down
        bool isPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0);

        return isPressed;
    }

    bool IsUp()
    {
        // Return true if any of the inputs are down
        bool isPressed =
            Input.GetKeyUp(KeyCode.Space) ||
            Input.GetMouseButtonUp(0);

        return isPressed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyScript enemy = collision.transform.GetComponentInParent<EnemyScript>();
        if (enemy != null)
        {
            // End the game
            OnGameLose();
        }
    }

    void OnGameLose()
    {
        // End the game
        isAlive = false;

        // Pause enemies
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.OnGameLose();
        }

        // Update the UI
        UiManager uiManager = FindObjectOfType<UiManager>();
        if (uiManager != null)
        {
            uiManager.OnGameLose();
        }

        // Swap all of the coloring
        spriteRenderer.color = Color.red;
    }
}