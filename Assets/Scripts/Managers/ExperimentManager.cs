using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject[] objectsEasy;
    public GameObject[] objectsMedium;
    public GameObject[] objectsHard;

    public GameObject virtualTwinPrefab;
    public GameObject branchPrefab;

    private TimelineManager timelineManager;

    private int difficulty; // 0 - easy; 1 - medium; 2 - hard;
    private int amountOfVersions; // + the virtual twin

    // Start is called before the first frame update
    void Start()
    {
        timelineManager = AppManager.Instance.GetTimelineManager();

        InstantiateTimelineObjects();
    }

    void InstantiateTimelineObjects()
    {
        var branchesContainer = AppManager.Instance.GetTimelineContainer().transform.GetChild(0);
        var singleBranch = Instantiate(branchPrefab, branchesContainer);

        // Create a branch
        var branchLogic = singleBranch.GetComponent<Branch>();
        branchLogic.index = 0;
        branchLogic.branchName = "Experiment Branch";
        branchLogic.lastCommit = "0";
        singleBranch.name = branchLogic.branchName;

        // Spawn the previously selected prefabs
        var objsToSpawn = RandomelySelectTimelineObjects();
        for (int i = 0; i < objsToSpawn.Length; i++)
        {
            var newVersion = Instantiate(objsToSpawn[i], singleBranch.transform);
            var versionLogic = newVersion.GetComponent<VersionObject>();

            versionLogic.id = i.ToString();
            versionLogic.description = "MESSAGE TO SET"; // TODO message is important, as this might be the description fitting to the looked for object
            versionLogic.createdBy = "Anonymous";
            versionLogic.createdAt = Random.Range(0, System.DateTime.Today.Hour).ToString();
        }

        // Spawn virtual twin at random position
        var virtTwin = Instantiate(virtualTwinPrefab);
        virtTwin.transform.SetSiblingIndex(Random.Range(0, singleBranch.transform.childCount));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    GameObject[] RandomelySelectTimelineObjects()
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
}
