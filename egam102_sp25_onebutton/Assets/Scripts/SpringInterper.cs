using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringInterper : MonoBehaviour
{
    public float value;
    public float goal;

    public float frequency;
    public float damping;

    public float velocity;

    void Update()
    {
        // Update the spring
        UtilsMath.CalcDampedSimpleHarmonicMotion(
            ref value, ref velocity, goal, Time.deltaTime, frequency, damping
        );
    }

    public void SetGoal(float newGoal, bool isInstant)
    {
        goal = newGoal;

        if (isInstant)
        {
            value = goal;
            velocity = 0;
        }
    }
}
