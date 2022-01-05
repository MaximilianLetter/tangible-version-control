using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System.Threading.Tasks;

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
            // TODO what about node id, is that a link to branches?

            // Get single commit information

            string singleCommitURL = baseURL + baseRepo + "commits/" + commitID;
            UnityWebRequest singleCommitRequest = UnityWebRequest.Get(singleCommitURL);

            yield return singleCommitRequest.SendWebRequest();

            if (singleCommitRequest.isNetworkError || singleCommitRequest.isHttpError)
            {
                Debug.LogError(singleCommitRequest.error);
                yield break;
            }

            JSONNode singleCommitInfo = JSON.Parse(singleCommitRequest.downloadHandler.text);
            var fileName = (string)singleCommitInfo["files"][0]["filename"];

            if (System.IO.Path.GetExtension(fileName) != ".glb")
            {
                // If 
                Debug.Log("Not a glTF file, continue.");
                continue;
            }

            string modelURL = singleCommitInfo["files"][0]["raw_url"];

            // Fallback for testing
            //modelURL = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb";
            Debug.Log(modelURL);

            UnityWebRequest modelDataRequest = UnityWebRequest.Get(modelURL);

            yield return modelDataRequest.SendWebRequest();

            if (modelDataRequest.isNetworkError || modelDataRequest.isHttpError)
            {
                Debug.LogError(modelDataRequest.error);
                yield break;
            }

            var modelInfoRaw = modelDataRequest.downloadHandler.data;

            LoadObject(modelInfoRaw);
        }
    }

    async void LoadObject(byte[] data)
    {
        Debug.Log("Building glTF ...");
        var t = Task<GameObject>.Run(() => ConstructGltf.ConstructAsync(GltfUtility.GetGltfObjectFromGlb(data)));
        await t;
        Debug.Log("Building glTF done.");

        GameObject result = t.Result;
        if (result != null)
        {
            // The glTF result is the complete scene, including light and camera. Only keep the real mesh, destroy other or dont even create other.
            Debug.Log(result);
        }
    }
}
