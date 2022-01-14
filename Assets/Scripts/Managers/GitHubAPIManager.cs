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
    private ShowcaseObjLoader showcaseObjLoader;

    private float loadingProgress = 0f;
    private bool ready = false;

    void Start()
    {
        branchesContainer = AppManager.Instance.GetTimelineContainer().transform.GetChild(0);
        progressIndicator = FindObjectOfType<ProgressIndicatorObjectDisplay>();
        showcaseObjLoader = GetComponent<ShowcaseObjLoader>();

        StartCoroutine(CollectDataFromAPI());
    }

    IEnumerator CollectDataFromAPI()
    {
        Debug.Log("Start to collect data from GitHub API ...");

        OpenProgressIndicator();

        if (showcaseObjLoader.doShowcase)
        {
            // This variant uses pre-imported .obj files. No internet connection is required.
            yield return StartCoroutine(BuildShowcaseTimeline());
        }
        else
        {
            yield return StartCoroutine(GetListOfBranches());

            yield return StartCoroutine(GetListOfCommits());

            if (loadingProgress != 1f)
            {
                progressIndicator.Message = "Loading failed";
            }
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

        // Reflect state in progress indicator
        progressIndicator.Message = "Loading list of commits";

        Debug.Log("Collect information about commits ...");

        string commitsURL = baseURL + baseRepo + "commits";
        // Example URL: https://api.github.com/repos/MaximilianLetter/model-comparison-by-github/commits

        UnityWebRequest repoCommitsRequest = UnityWebRequest.Get(commitsURL);
        repoCommitsRequest.SetRequestHeader("Authorization", accessToken);

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
            // Reflect state in progress indicator
            loadingProgress = ((float)i / listCommitsInfo.Count);
            progressIndicator.Message = "Loading commits " + Mathf.RoundToInt(loadingProgress * 100) + "%";

            Debug.Log(listCommitsInfo[i]["commit"]["message"]);
            string commitID = listCommitsInfo[i]["sha"];
            // TODO what about node id, is that a link to branches?

            // Get single commit information

            string singleCommitURL = baseURL + baseRepo + "commits/" + commitID;
            UnityWebRequest singleCommitRequest = UnityWebRequest.Get(singleCommitURL);
            singleCommitRequest.SetRequestHeader("Authorization", accessToken);

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
            modelDataRequest.SetRequestHeader("Authorization", accessToken);

            yield return modelDataRequest.SendWebRequest();

            if (modelDataRequest.isNetworkError || modelDataRequest.isHttpError)
            {
                Debug.LogError(modelDataRequest.error);
                yield break;
            }

            var modelInfoRaw = modelDataRequest.downloadHandler.data;

            Debug.Log("File found and downloaded.");
            BuildObject(modelInfoRaw, singleCommitInfo);
        }
        loadingProgress = 1f;

        Debug.Log("Processing commits done.");
    }

    /// <summary>
    /// Builds a GameObject out of glTF data and populates fields with commit information.
    /// </summary>
    /// <param name="data">glTF raw data.</param>
    /// <param name="info">Info about commit that is provided by API call.</param>
    async void BuildObject(byte[] data, JSONNode info)
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
                versionLogic.virtualTwin = true;
                Debug.Log("Virtual twin loaded and updated.");
            }

            newVersion.transform.SetParent(branchesContainer.GetChild(0)); // TODO find correct branch

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

    IEnumerator BuildShowcaseTimeline()
    {
        string showcaseVirtTwinID = showcaseObjLoader.virtTwinID.ToString();
        GameObject[] allObjs = showcaseObjLoader.allObjs;

        // Build branches
        //for (int i = 0; i < branchesInfo.Count; i++)
        //{
        var newBranch = Instantiate(branchPrefab, branchesContainer);

        var branchLogic = newBranch.GetComponent<Branch>();
        branchLogic.index = 0;
        branchLogic.branchName = "main";
        newBranch.name = branchLogic.branchName;
        newBranch.transform.SetAsFirstSibling();

        Debug.Log("New branch created: " + branchLogic.branchName);
        //}

        // Load objects
        for (int i = 0; i < allObjs.Length; i++)
        {
            // Reflect state in progress indicator
            loadingProgress = ((float)i / allObjs.Length);
            progressIndicator.Message = "Loading commits " + Mathf.RoundToInt(loadingProgress * 100) + "%";

            var loadedOBJ = Instantiate(allObjs[i]);
            var newVersion = Instantiate(versionPrefab);
            var versionLogic = newVersion.GetComponent<VersionObject>();

            versionLogic.id = i.ToString(); ;
            versionLogic.description = "Exemplary description " + i;
            versionLogic.createdBy = "Maximilian Letter";
            versionLogic.createdAt = RandomDayFunc().ToString();

            if (versionLogic.id == showcaseVirtTwinID)
            {
                versionLogic.virtualTwin = true;
                Debug.Log("Virtual twin loaded and updated.");
            }

            newVersion.transform.SetParent(branchesContainer.GetChild(0)); // TODO find correct branch

            Transform modelContainer = versionLogic.GetModelContainer();

            // The glTF result is the complete scene, including light and camera
            // only keep the actual mesh, destroy other or dont even create other
            var model = loadedOBJ.transform.GetChild(0).gameObject;
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

            // Destroy the glTF scene, the required model was already moved out
            Destroy(loadedOBJ);

            yield return new WaitForSeconds(0.1f);
        }
        loadingProgress = 1f;
    }

    private System.DateTime RandomDayFunc()
    {
        System.DateTime start = new System.DateTime(1995, 1, 1);
        int range = (System.DateTime.Today - start).Days;
        int randomDay = Random.Range(0, range);
        System.DateTime randomDate = start.AddDays(randomDay);

        return randomDate;
    }

    public bool IsReady()
    {
        return ready;
    }
}
