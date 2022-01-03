using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class GitHubAPIManager : MonoBehaviour
{
    private readonly string baseURL = "https://api.github.com/repos/";
    private readonly string baseRepo = "MaximilianLetter/model-comparison-by-github/";

    void Start()
    {
        StartCoroutine(GetListOfBranches());
    }

    // test get branches
    IEnumerator GetListOfBranches()
    {
        string branchesURL = baseURL + baseRepo + "branches";
        // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/branches

        UnityWebRequest repoBranchesRequest = UnityWebRequest.Get(branchesURL);

        yield return repoBranchesRequest.SendWebRequest();

        if (repoBranchesRequest.isNetworkError || repoBranchesRequest.isHttpError)
        {
            Debug.LogError(repoBranchesRequest.error);
            yield break;
        }

        // Alternative, Unity default approach with serializable objects
        //JsonUtility.FromJson<TODO_serializable_class>(repoBranchesRequest.downloadHandler.text);

        JSONNode branchesInfo = JSON.Parse(repoBranchesRequest.downloadHandler.text);

        Debug.Log("Total amount of branches: " + branchesInfo.Count);
        for (int i = 0; i < branchesInfo.Count; i++)
        {
            Debug.Log(branchesInfo[i]["name"]);
        }
    }
}
