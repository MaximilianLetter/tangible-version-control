using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject[] objectsEasy;
    public GameObject[] objectsMedium;
    public GameObject[] objectsHard;

    public GameObject virtualTwinModel;
    public GameObject versionPrefab;
    public GameObject branchPrefab;

    [SerializeField]
    private int difficulty; // 0 - easy; 1 - medium; 2 - hard;

    [SerializeField]
    private int amountOfVersions; // + the virtual twin

    private bool ready;

    public void SetupExperiment()
    {
        Debug.Log("Difficulty: " + difficulty);
        Debug.Log("Amount of versions: " + amountOfVersions);
        StartCoroutine(InstantiateTimelineObjects());
    }

    IEnumerator InstantiateTimelineObjects()
    {
        var branches = FindObjectsOfType<Branch>();
        if (branches != null)
        {
            foreach (var b in branches)
            {
                Debug.Log("Destroy existing branch: " + b.name);
                Destroy(b.gameObject);
            }
        }

        // Wait until objects are really destroyed
        yield return null;

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
                loadedOBJ = Instantiate(objsToSpawn[i]);
            }
            var newVersion = Instantiate(versionPrefab);
            var versionLogic = newVersion.GetComponent<VersionObject>();

            versionLogic.id = i.ToString();
            versionLogic.description = "MESSAGE TO SET"; // TODO message is important, as this might be the description fitting to the looked for object
            versionLogic.createdBy = "Anonymous";
            versionLogic.createdAt = Random.Range(0, System.DateTime.Today.Hour).ToString();

            if (i == objsToSpawn.Length)
            {
                versionLogic.virtualTwin = true;
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

        // Reorder virtual twin
        singleBranch.transform.GetChild(objsToSpawn.Length).SetSiblingIndex(Random.Range(0, objsToSpawn.Length));

        ready = true;

        yield return null;

        AppManager.Instance.GetStartupManager().StartCoroutine("StartUp");
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

    public bool IsReady()
    {
        return ready;
    }
}
