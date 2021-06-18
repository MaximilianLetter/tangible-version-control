using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToPosition : MonoBehaviour
{
    // Public variables
    public float transitionTime;
    public Vector3 goalPos;

    // Internal values
    private Vector3 startLocalPos;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    /// <summary>
    /// Starts the transition to the specified goal. The goal can be overriden by a position.
    /// </summary>
    /// <param name="goal">Optional position to transition to.</param>
    public void StartTransition(Vector3 goal = new Vector3())
    {
        if (goal == Vector3.zero)
        {
            goal = goalPos;
        }
        StartCoroutine(MoveTowards(goal));
    }

    /// <summary>
    /// Coroutine animating the object to move to the specified position.
    /// </summary>
    /// <param name="goal">The position to transition to.</param>
    /// <returns></returns>
    IEnumerator MoveTowards(Vector3 goal)
    {
        float passedTime = 0f;
        Vector3 startPos = startLocalPos;

        while (passedTime < transitionTime)
        {
            passedTime += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(startPos, goal, passedTime / transitionTime);

            yield return null;
        }

        transform.localPosition = goal;
    }

    /// <summary>
    /// Resets the position of the object to the original starting position.
    /// </summary>
    public void ResetToStartPosition()
    {
        transform.localPosition = startLocalPos;
    }
}
