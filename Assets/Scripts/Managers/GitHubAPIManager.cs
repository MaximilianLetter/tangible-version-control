﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System.Threading.Tasks;

public class GitHubAPIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject branchPrefab;
    [SerializeField]
    private GameObject versionPrefab;

    private readonly string baseURL = "https://api.github.com/repos/";
    private readonly string baseRepo = "MaximilianLetter/model-comparison-by-github/";
    private readonly string virtualTwinId = "32072b374b46026e19f824e6704bba50eb306a6e";

    private TimelineManager timelineManager;
    private Transform branchesContainer;

    private bool ready = false;

    void Start()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();
        branchesContainer = AppManager.Instance.GetTimelineContainer().transform.GetChild(0);

        StartCoroutine(CollectDataFromAPI());
    }

    IEnumerator CollectDataFromAPI()
    {
        Debug.Log("Start to collect data from GitHub API ...");

        yield return StartCoroutine(GetListOfBranches());

        yield return StartCoroutine(GetListOfCommits());

        Debug.Log("Data collection done.");

        ready = true;
    }

    /// <summary>
    /// Get a list of all branches from the base repository from GitHub and create a branch GameObject for each entry.
    /// </summary>
    /// <returns></returns>
    IEnumerator GetListOfBranches()
    {
        // API call and provided information
        // https://docs.github.com/en/rest/reference/branches#list-branches

        Debug.Log("Collect information about branches ...");

        string branchesURL = baseURL + baseRepo + "branches";
        // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/branches

        UnityWebRequest repoBranchesRequest = UnityWebRequest.Get(branchesURL);

        yield return repoBranchesRequest.SendWebRequest();

        if (repoBranchesRequest.isNetworkError || repoBranchesRequest.isHttpError)
        {
            Debug.LogError(repoBranchesRequest.error);
            yield break;
        }

        JSONNode branchesInfo = JSON.Parse(repoBranchesRequest.downloadHandler.text);

        Debug.Log("Total amount of branches: " + branchesInfo.Count);
        for (int i = 0; i < branchesInfo.Count; i++)
        {
            var newBranch = Instantiate(branchPrefab, branchesContainer);

            var branchLogic = newBranch.GetComponent<Branch>();
            branchLogic.index = i;
            branchLogic.branchName = branchesInfo[i]["name"];
            newBranch.name = branchLogic.branchName;
            newBranch.transform.SetAsFirstSibling();

            Debug.Log("New branch created: " + branchLogic.branchName);
        }

        Debug.Log("Processing branches done.");
    }

    /// <summary>
    /// Get a list of all commits from the base repository from GitHub and create a version GameObject for each entry.
    /// </summary>
    /// <returns></returns>
    IEnumerator GetListOfCommits()
    {
        // API call and provided information
        //https://docs.github.com/en/rest/reference/commits#list-commits
        //https://docs.github.com/en/rest/reference/commits#get-a-commit

        Debug.Log("Collect information about commits ...");

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
                // If file is invalid, check the next commit
                Debug.Log("Not a glTF file, continue.");
                continue;
            }

            string modelURL = singleCommitInfo["files"][0]["raw_url"];

            // Fallback for testing
            //modelURL = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb";

            UnityWebRequest modelDataRequest = UnityWebRequest.Get(modelURL);

            yield return modelDataRequest.SendWebRequest();

            if (modelDataRequest.isNetworkError || modelDataRequest.isHttpError)
            {
                Debug.LogError(modelDataRequest.error);
                yield break;
            }

            var modelInfoRaw = modelDataRequest.downloadHandler.data;

            Debug.Log("File found and downloaded.");
            LoadObject(modelInfoRaw, singleCommitInfo);
        }

        Debug.Log("Processing commits done.");
    }

    /// <summary>
    /// Builds a GameObject out of glTF data and populates fields with commit information.
    /// </summary>
    /// <param name="data">glTF raw data.</param>
    /// <param name="info">Info about commit that is provided by API call.</param>
    async void LoadObject(byte[] data, JSONNode info)
    {
        Debug.Log("Building GameObject from glTF ...");
        var t = Task<GameObject>.Run(() => ConstructGltf.ConstructAsync(GltfUtility.GetGltfObjectFromGlb(data)));
        await t;

        GameObject result = t.Result;
        if (result != null)
        {
            var newVersion = Instantiate(versionPrefab);
            var versionLogic = newVersion.GetComponent<VersionObject>();

            versionLogic.id = info["sha"];
            versionLogic.description = info["commit"]["message"];
            versionLogic.createdBy = info["commit"]["author"]["name"];
            versionLogic.createdAt = info["commit"]["author"]["date"];

            if (versionLogic.id == virtualTwinId)
            {
                Debug.Log("Virtual twin loaded and updated.");
                versionLogic.virtualTwin = true;
                AppManager.Instance.FindAndSetVirtualTwin(true);
            }

            newVersion.transform.SetParent(branchesContainer.GetChild(0)); // TODO find correct branch

            // The glTF result is the complete scene, including light and camera. Only keep the real mesh, destroy other or dont even create other
            var model = result.transform.GetChild(0).gameObject;
            model.transform.SetParent(newVersion.transform);

            model.name = "model";
            var coll = model.AddComponent<BoxCollider>();
            ColliderToFit.FitToChildren(model); // todo

            model.tag = "VersionObjectArea";

            // Destroy the glTF scene, the required model was already moved out
            Destroy(result);
        }

        Debug.Log("Building GameObject from glTF done.");
    }

    public bool IsReady()
    {
        return ready;
    }
}
