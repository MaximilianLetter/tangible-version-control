using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToPosition : MonoBehaviour
{
    // Public variables
    public float transitionTime;

    /// <summary>
    /// Starts the transition to the specified goal. The goal can be overriden by a position.
    /// </summary>
    /// <param name="goal">Optional position to transition to.</param>
    public void StartTransition(Vector3 goal, bool local = false)
    {
        if (local)
        {
            StartCoroutine(MoveTowardsLocal(goal));
        }
        else
        {
            StartCoroutine(MoveTowards(goal));
        }
    }

    /// <summary>
    /// Coroutine animating the object to move to the specified position.
    /// </summary>
    /// <param name="goal">The position to transition to.</param>
    /// <returns></returns>
    IEnumerator MoveTowards(Vector3 goal)
    {
        float passedTime = 0f;
        Vector3 startPos = transform.position;

        while (passedTime < transitionTime)
        {
            passedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, goal, passedTime / transitionTime);

            yield return null;
        }

        transform.position = goal;
    }

    /// <summary>
    /// Coroutine animating the object to move to the specified position.
    /// </summary>
    /// <param name="goal">The position to transition to.</param>
    /// <returns></returns>
    IEnumerator MoveTowardsLocal(Vector3 goal)
    {
        float passedTime = 0f;
        Vector3 startPos = transform.localPosition;

        while (passedTime < transitionTime)
        {
            passedTime += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(startPos, goal, passedTime / transitionTime);

            yield return null;
        }

        transform.localPosition = goal;
    }
}
