using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;

public class GitHubAPIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject branchPrefab;
    [SerializeField]
    private GameObject versionPrefab;
    [SerializeField]
    private Material alternativeDefaultMat;

    private readonly string baseURL = "https://api.github.com/repos/";
    //private readonly string baseRepo = "MaximilianLetter/model-comparison-by-github/";
    private readonly string baseRepo = "MaximilianLetter/glTF-models-dev/";
    private readonly string virtualTwinId = "809006d27a067559e8312fbd2bd14c480b091d04";
    private readonly string accessToken = "token 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"; // created by dump account

    private ProgressIndicatorObjectDisplay progressIndicator;
    private Transform branchesContainer;
    private List<Branch> branches;

    private float loadingProgress = 0f;
    private bool ready = false;

    void Start()
    {
        branchesContainer = AppManager.Instance.GetTimelineContainer().transform.GetChild(0);
        progressIndicator = FindObjectOfType<ProgressIndicatorObjectDisplay>();
        branches = new List<Branch>();

        StartCoroutine(CollectDataFromAPI());
    }

    IEnumerator CollectDataFromAPI()
    {
        Debug.Log("Start to collect data from GitHub API ...");

        OpenProgressIndicator();

        yield return StartCoroutine(GetListOfBranches());

        yield return StartCoroutine(GetListOfCommits());

        if (loadingProgress != 1f)
        {
            progressIndicator.Message = "Loading failed";
        }

        Debug.Log("Data collection done.");

        yield return new WaitForSeconds(0.5f); // not user clean, wait until the last object is build

        ready = true;
    }

    private async void OpenProgressIndicator()
    {
        progressIndicator.gameObject.SetActive(true);

        await progressIndicator.OpenAsync();

        while (loadingProgress < 1)
        {
            progressIndicator.Progress = loadingProgress;
            await Task.Yield();
        }

        progressIndicator.Message = "Loading done";

        await progressIndicator.CloseAsync();

        progressIndicator.gameObject.SetActive(false);
    }

    /// <summary>
    /// Get a list of all branches from the base repository from GitHub and create a branch GameObject for each entry.
    /// </summary>
    /// <returns></returns>
    IEnumerator GetListOfBranches()
    {
        // API call and provided information
        // https://docs.github.com/en/rest/reference/branches#list-branches

        // Reflect state in progress indicator
        progressIndicator.Message = "Loading branches";

        Debug.Log("Collect information about branches ...");

        string branchesURL = baseURL + baseRepo + "branches";
        // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/branches

        UnityWebRequest repoBranchesRequest = UnityWebRequest.Get(branchesURL);
        repoBranchesRequest.SetRequestHeader("Authorization", accessToken);

        yield return repoBranchesRequest.SendWebRequest();

        if (repoBranchesRequest.result == UnityWebRequest.Result.ConnectionError || repoBranchesRequest.result == UnityWebRequest.Result.ProtocolError || repoBranchesRequest.result == UnityWebRequest.Result.DataProcessingError)
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
            branchLogic.lastCommit = branchesInfo[i]["commit"]["sha"];
            newBranch.name = branchLogic.branchName;
            newBranch.transform.SetAsFirstSibling();

            Debug.Log("New branch created: " + branchLogic.branchName);
            branches.Add(branchLogic);
        }

        // Make sure the main branch is called first
        branches.Reverse();

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

        // Reflect state in progress indicator
        progressIndicator.Message = "Loading list of commits";

        Debug.Log("Collect information about commits ...");

        List<string> processedCommits = new List<string>();

        for (int j = 0; j < branches.Count; j++)
        {
            string commitsURL = baseURL + baseRepo + "commits" + "?sha=" + branches[j].branchName;
            // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/commits

            UnityWebRequest repoCommitsRequest = UnityWebRequest.Get(commitsURL);
            repoCommitsRequest.SetRequestHeader("Authorization", accessToken);

            yield return repoCommitsRequest.SendWebRequest();

            if (repoCommitsRequest.result == UnityWebRequest.Result.ConnectionError || repoCommitsRequest.result == UnityWebRequest.Result.ProtocolError || repoCommitsRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(repoCommitsRequest.error);
                yield break;
            }

            JSONNode listCommitsInfo = JSON.Parse(repoCommitsRequest.downloadHandler.text);

            Debug.Log("Total amount of commits for branch " + branches[j].branchName + ": " + listCommitsInfo.Count);
            for (int i = 0; i < listCommitsInfo.Count; i++)
            {
                // Reflect state in progress indicator
                loadingProgress = ((float)i / listCommitsInfo.Count * ((float)j / branches.Count));
                progressIndicator.Message = "Loading commits " + Mathf.RoundToInt(loadingProgress * 100) + "%";

                Debug.Log(listCommitsInfo[i]["commit"]["message"]);
                string commitID = listCommitsInfo[i]["sha"];
                
                // Do not double process or download content of commits
                if (processedCommits.Contains(commitID)) continue;
                else processedCommits.Add(commitID);

                // Get single commit information

                string singleCommitURL = baseURL + baseRepo + "commits/" + commitID;
                UnityWebRequest singleCommitRequest = UnityWebRequest.Get(singleCommitURL);
                singleCommitRequest.SetRequestHeader("Authorization", accessToken);

                yield return singleCommitRequest.SendWebRequest();

                if (singleCommitRequest.result == UnityWebRequest.Result.ConnectionError || singleCommitRequest.result == UnityWebRequest.Result.ProtocolError || singleCommitRequest.result == UnityWebRequest.Result.DataProcessingError)
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
                modelDataRequest.SetRequestHeader("Authorization", accessToken);

                yield return modelDataRequest.SendWebRequest();

                if (modelDataRequest.result == UnityWebRequest.Result.ConnectionError || modelDataRequest.result == UnityWebRequest.Result.ProtocolError || modelDataRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError(modelDataRequest.error);
                    yield break;
                }

                var modelInfoRaw = modelDataRequest.downloadHandler.data;

                Debug.Log("File found and downloaded.");
                BuildObject(modelInfoRaw, singleCommitInfo, i);
            }
        }

        
        loadingProgress = 1f;

        Debug.Log("Processing commits done.");
    }

    /// <summary>
    /// Builds a GameObject out of glTF data and populates fields with commit information.
    /// </summary>
    /// <param name="data">glTF raw data.</param>
    /// <param name="info">Info about commit that is provided by API call.</param>
    async void BuildObject(byte[] data, JSONNode info, int commitNumber)
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
            versionLogic.sequence = commitNumber;
            versionLogic.description = info["commit"]["message"];
            //versionLogic.createdBy = info["commit"]["author"]["name"];
            versionLogic.createdBy = "Anonymous";
            versionLogic.createdAt = info["commit"]["author"]["date"];

            if (versionLogic.id == virtualTwinId)
            {
                versionLogic.virtualTwin = true;
                Debug.Log("Virtual twin loaded and updated.");
            }

            //newVersion.transform.SetParent(branchesContainer.GetChild(0)); // TODO find correct branch
            // Sort into correct branch
            bool sortedIn = false;
            foreach (var branch in branches)
            {
                // Commit fits to branch
                if (branch.lastCommit == versionLogic.id)
                {
                    newVersion.transform.SetParent(branch.transform);

                    // Check commit parents
                    var commitParents = info["parents"];
                    if (commitParents.Count > 0)
                    {
                        // Override branch last commit with parent commit
                        branch.lastCommit = commitParents[0]["sha"];

                        if (commitParents.Count > 1)
                        {
                            Debug.Log("MERGE found");
                        }
                    }

                    sortedIn = true;
                }
            }

            if (!sortedIn)
            {
                Debug.LogError("Commit couldnt be resolved to a branch!");
            }

            Transform modelContainer = versionLogic.GetModelContainer();

            // The glTF result is the complete scene, including light and camera
            // only keep the actual mesh, destroy other or dont even create other
            var model = result.transform.GetChild(0).gameObject;
            model.transform.SetParent(modelContainer);

            modelContainer.localScale = model.transform.localScale;
            modelContainer.localPosition = model.transform.transform.localPosition;
            modelContainer.localRotation = model.transform.localRotation;

            model.transform.localScale = Vector3.one;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            model.name = "model";

            ColliderToFit.FitToChildren(modelContainer.gameObject);

            versionLogic.Initialize();

            if (alternativeDefaultMat != null)
            {
                versionLogic.OverrideBaseMaterial(alternativeDefaultMat);
                Debug.Log("Base material overridden");
            }

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
