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
        StartCoroutine(GetListOfCommits());
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

    /// <summary>
    /// Get a list of all commits of the base repository from GitHub
    /// </summary>
    /// <returns></returns>
    IEnumerator GetListOfCommits()
    {
        string commitsURL = baseURL + baseRepo + "commits";
        // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/commits

        UnityWebRequest repoCommitsRequest = UnityWebRequest.Get(commitsURL);

        yield return repoCommitsRequest.SendWebRequest();

        if (repoCommitsRequest.isNetworkError || repoCommitsRequest.isHttpError)
        {
            Debug.LogError(repoCommitsRequest.error);
            yield break;
        }

        // Alternative, Unity default approach with serializable objects
        //JsonUtility.FromJson<TODO_serializable_class>(repoBranchesRequest.downloadHandler.text);

        JSONNode listCommitsInfo = JSON.Parse(repoCommitsRequest.downloadHandler.text);

        Debug.Log("Total amount of commits: " + listCommitsInfo.Count);
        for (int i = 0; i < listCommitsInfo.Count; i++)
        {
            Debug.Log(listCommitsInfo[i]["commit"]["message"]);
            Debug.Log(listCommitsInfo[i]["sha"]);
            string commitID = listCommitsInfo[i]["sha"];
            // what about node id, is that a link to branches?

            string singleCommitURL = baseURL + baseRepo + "commits/" + commitID;
            UnityWebRequest singleCommitRequest = UnityWebRequest.Get(singleCommitURL);

            yield return singleCommitRequest.SendWebRequest();

            if (singleCommitRequest.isNetworkError || singleCommitRequest.isHttpError)
            {
                Debug.LogError(singleCommitRequest.error);
                yield break;
            }

            JSONNode singleCommitInfo = JSON.Parse(singleCommitRequest.downloadHandler.text);
            Debug.Log(singleCommitInfo["files"][0]["filename"]);

            // TODO next: try to load model from file URL of commit
        }
    }
}
