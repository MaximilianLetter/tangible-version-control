using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject[] objectsEasy;
    public GameObject[] objectsMedium;
    public GameObject[] objectsHard;

    public GameObject placeHolderModel;
    public GameObject virtualTwinModel;
    public GameObject versionPrefab;
    public GameObject branchPrefab;

    private GameObject lookedForVersion;
    public GameObject taskPanel;
    private Transform modelImageTransform;

    [SerializeField]
    private int difficulty; // 0 - easy; 1 - medium; 2 - hard;

    [SerializeField]
    private int amountOfVersions; // + the virtual twin


    private TimelineManager timelineManager;
    private bool ready;

    void Start()
    {
        modelImageTransform = taskPanel.transform.GetChild(0);
        timelineManager = AppManager.Instance.GetTimelineManager();
    }

    public void SetupExperiment(bool firstTime = false)
    {
        Debug.Log("Difficulty: " + difficulty);
        Debug.Log("Amount of versions: " + amountOfVersions);
        StartCoroutine(InstantiateTimelineObjects(firstTime));
    }

    IEnumerator InstantiateTimelineObjects(bool firstTime)
    {
        timelineManager.ResetForTimelineChange();

        var branches = FindObjectsOfType<Branch>();
        if (branches != null)
        {
            foreach (var b in branches)
            {
                Debug.Log("Destroy existing branch: " + b.name);
                Destroy(b.gameObject);
            }
        }

        // Destroy model of last searched for object
        if (modelImageTransform.childCount > 0)
        {
            var oldLookedForVersion = modelImageTransform.GetChild(0);
            Destroy(oldLookedForVersion.gameObject);
        }

        // Wait until objects are really destroyed
        yield return new WaitForSeconds(0.1f);

        var branchesContainer = AppManager.Instance.GetTimelineContainer().transform.GetChild(0);
        var singleBranch = Instantiate(branchPrefab, branchesContainer);

        // Create a branch
        var branchLogic = singleBranch.GetComponent<Branch>();
        branchLogic.index = 0;
        branchLogic.branchName = "Experiment Branch";
        branchLogic.lastCommit = "0";
        singleBranch.name = branchLogic.branchName;

        // Spawn the previously selected prefabs
        var objsToSpawn = RandomlySelectTimelineObjects();
        for (int i = 0; i < objsToSpawn.Length + 1; i++)
        {
            GameObject loadedOBJ;
            // Virtual twin extra condition
            if (i == objsToSpawn.Length)
            {
                loadedOBJ = Instantiate(virtualTwinModel);
            }
            else
            {
                if (firstTime)
                {
                    // Could alternatively load a placeholder object for first placement such as cubes
                    loadedOBJ = Instantiate(placeHolderModel);
                }
                else
                {
                    loadedOBJ = Instantiate(objsToSpawn[i]);
                }
            }
            var newVersion = Instantiate(versionPrefab);
            var versionLogic = newVersion.GetComponent<VersionObject>();

            versionLogic.id = i.ToString();
            versionLogic.description = "MESSAGE TO SET"; // TODO message is important, as this might be the description fitting to the looked for object
            versionLogic.createdBy = "Anonymous";
            versionLogic.createdAt = System.DateTime.Now.ToString();

            if (i == objsToSpawn.Length)
            {
                versionLogic.virtualTwin = true;
                versionLogic.name = "virtTwin";
            }

            newVersion.transform.SetParent(singleBranch.transform);

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
        }

        if (!firstTime)
        {
            // Select randomly the looked for version
            var randomizedIndex = Random.Range(0, objsToSpawn.Length);
            lookedForVersion = singleBranch.transform.GetChild(randomizedIndex).gameObject;

            var modelCopy = lookedForVersion.transform.GetChild(0);
            Instantiate(modelCopy, modelImageTransform);

            // Reorder virtual twin and change ID with other version
            randomizedIndex = Random.Range(0, objsToSpawn.Length);
            var versionToSwapWith = singleBranch.transform.GetChild(randomizedIndex);

            var virtTwinObj = singleBranch.transform.GetChild(objsToSpawn.Length);
            virtTwinObj.SetSiblingIndex(randomizedIndex); // this works, however, sibling index is not correctly ordered
            virtTwinObj.GetComponent<VersionObject>().id = randomizedIndex.ToString();
            versionToSwapWith.SetSiblingIndex(objsToSpawn.Length);
            versionToSwapWith.GetComponent<VersionObject>().id = objsToSpawn.Length.ToString();

            taskPanel.SetActive(true);
        }

        ready = true;

        yield return null;

        var startupManager = AppManager.Instance.GetStartupManager();
        startupManager.StartCoroutine(startupManager.StartUp(firstTime));
    }

    /// <summary>
    /// Select a number of objects out of an object pool. Based on the set difficulty and number of versions defined.
    /// </summary>
    /// <returns>Array with the objects to spawn in the timeline.</returns>
    GameObject[] RandomlySelectTimelineObjects()
    {
        GameObject[] objectsToSpawn = new GameObject[amountOfVersions];

        GameObject[] selectedArray;
        switch (difficulty) {
            case 0:
                selectedArray = objectsEasy;
                break;
            case 1:
                selectedArray = objectsMedium;
                break;
            case 2:
                selectedArray = objectsHard;
                break;
            default:
                Debug.Log("Difficulty not set to a value.");
                selectedArray = objectsEasy;
                break;
        }

        List<GameObject> objPool = new List<GameObject>(selectedArray);

        for (int i = 0; i < amountOfVersions; i++)
        {
            var randNum = Random.Range(0, objPool.Count);

            objectsToSpawn[i] = objPool[randNum];
            objPool.RemoveAt(randNum);
        }

        return objectsToSpawn;
    }

    public bool CheckSelectedVersion(GameObject version)
    {
        Debug.Log(version);
        Debug.Log(lookedForVersion);

        var id1 = version.GetComponentInParent<VersionObject>().id;
        var id2 = lookedForVersion.GetComponent<VersionObject>().id;

        if (id1 == id2)
        {
            return true;
        }

        return false;
    }

    public bool IsReady()
    {
        return ready;
    }
}
