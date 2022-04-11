using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    public void ChangeToScene(string sceneName)
    {
        if (sceneName == SceneManager.GetActiveScene().name || sceneName == string.Empty)
        {
            Debug.Log("Scene name to load is equal to active scene or not set.");
            return;
        }

        if (SceneManager.GetSceneByName(sceneName) == null)
        {
            Debug.Log("Scene not found.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
