using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum ExperimentMode { Comparison, Timeline, Tutorial }

public class ExperimentManager : MonoBehaviour
{
    public ExperimentMode mode = ExperimentMode.Comparison;

    [SerializeField]
    private GameObject[] compObjAdd;
    [SerializeField]
    private GameObject[] compObjSub;
    [SerializeField]
    private GameObject[] compObjExch;

    //private GameObject[] compObjInUse;

    private GameObject virtTwinModel;
    private GameObject goalModel;

    [SerializeField]
    private GameObject tutorialVirtTwinModel;

    [SerializeField]
    private GameObject[] timelineObjects;

    public GameObject placeHolderModel;
    public GameObject virtualTwinModel;
    public GameObject versionPrefab;
    public GameObject branchPrefab;

    public GameObject conditionsPanel;
    public GameObject conditionsPanelToggleButton;

    private GameObject lookedForVersion;

    [SerializeField]
    private int difficulty; // 0 - easy; 1 - medium; 2 - hard;

    [SerializeField]
    private int amountOfVersions; // + the virtual twin

    public int participantID; // TODO do something with this

    private TimelineManager timelineManager;
    private ComparisonManager comparisonManager;
    private GameObject trackedObject;
    private TaskPanel taskPanel;
    private bool ready;
    private bool experimentRunning;
    private int experimentCounter;
    private float experimentStartTime;

    private bool setupWasDone;
    public bool readyForExperiment;

    void Start()
    {
        taskPanel = AppManager.Instance.GetTaskPanel();
        timelineManager = AppManager.Instance.GetTimelineManager();
        comparisonManager = AppManager.Instance.GetComparisonManager();
        trackedObject = AppManager.Instance.GetTrackedObjectLogic().gameObject;

        // To save results to
        Directory.CreateDirectory(Application.streamingAssetsPath + "/results/");

        if (mode == ExperimentMode.Tutorial) SetupExperiment();
    }

    public void SelectExperimentCondition(string type, bool variant)
    {
        int increment = 0;
        if (variant) increment = 2;

        if (type == "add")
        {
            virtTwinModel = compObjAdd[increment];
            goalModel = compObjAdd[increment + 1];
        }

        if (type == "sub")
        {
            virtTwinModel = compObjSub[increment];
            goalModel = compObjSub[increment + 1];
        }

        if (type == "exch")
        {
            virtTwinModel = compObjExch[increment];
            goalModel = compObjExch[increment + 1];
        }

        SetupExperiment(!setupWasDone);

        conditionsPanel.SetActive(false);
        conditionsPanelToggleButton.SetActive(true);
    }

    public void SetupExperiment(bool firstTime = false)
    {
        if (mode == ExperimentMode.Timeline)
        {
            Debug.Log("Difficulty: " + difficulty);
            Debug.Log("Amount of versions: " + amountOfVersions);
            StartCoroutine(InstantiateTimelineObjects(firstTime));

            if (firstTime)
            {
                experimentCounter = 0;
                taskPanel.SetStartInformation(ExperimentMode.Timeline);
            }
            else
            {
                experimentCounter++;
            }
            taskPanel.SetTextCounter(experimentCounter);
        }
        else if (mode == ExperimentMode.Comparison)
        {
            StartCoroutine(InstantiateComparison(firstTime));

            taskPanel.SetStartInformation(ExperimentMode.Comparison);
            taskPanel.SetTextCounter(experimentCounter);

            if (firstTime)
            {
                experimentCounter = 0;
                taskPanel.gameObject.SetActive(true);
            }
            else
            {
                experimentCounter++;
            }

            taskPanel.SetTextCounter(experimentCounter);
        }
        else // Tutorial
        {
            var startupManager = AppManager.Instance.GetStartupManager();
            startupManager.StartCoroutine(startupManager.StartUp(firstTime));

            var virtTwin = CreateVersionObjectFromModel(tutorialVirtTwinModel, 0, true);
            virtTwin.Initialize();
            AppManager.Instance.FindAndSetVirtualTwin();

            AppManager.Instance.GetTrackedObjectLogic().Initialize();
            //comparisonManager.Initialize();
            timelineManager.deactivated = true;

            trackedObject.SetActive(true);
            trackedObject.GetComponent<TrackedObject>().SetMaterial(comparisonManager.edgesMat);
        }

        if (!setupWasDone)
        {
            // Hide not required elements
            var actionPanel = AppManager.Instance.GetActionPanel();
            if (actionPanel != null)
            {
                actionPanel.gameObject.SetActive(false);
            }

            var timeline = AppManager.Instance.GetTimelineContainer();
            if (timeline != null)
            {
                timeline.gameObject.SetActive(false);
            }

            if (taskPanel != null)
            {
                taskPanel.gameObject.SetActive(false);
            }
        }

        setupWasDone = true;        
    }

    IEnumerator InstantiateComparison(bool firstTime)
    {
        if (firstTime)
        {
            var startupManager = AppManager.Instance.GetStartupManager();
            startupManager.StartCoroutine(startupManager.StartUp(firstTime));

            // create virtual twin
            var virtTwin = CreateVersionObjectFromModel(virtTwinModel, 0, true);
            virtTwin.Initialize();
            AppManager.Instance.FindAndSetVirtualTwin();

            yield return null;

            AppManager.Instance.GetTrackedObjectLogic().Initialize();
            comparisonManager.Initialize();

            timelineManager.deactivated = true;
        }

        yield return null;

        // Select a random object from the list of available objects
        //int randomIndex = Random.Range(0, compObjInUse.Length - 1);
        //GameObject objModel= compObjInUse[randomIndex];

        var comparedAgainstVersion = CreateVersionObjectFromModel(goalModel, 1);

        var voModel = comparedAgainstVersion.transform.GetChild(0).gameObject;
        var toModel = trackedObject.transform.GetChild(0).gameObject;

        // Required to for finding VersionObject in parent
        AppManager.Instance.GetTimelineContainer().SetActive(true);

        comparisonManager.StartComparison(toModel, voModel);

        AppManager.Instance.GetTimelineContainer().SetActive(false);
    }

    IEnumerator InstantiateTimelineObjects(bool firstTime)
    {
        timelineManager.ResetValuesForTimelineChange();

        var branches = FindObjectsOfType<Branch>();
        if (branches != null)
        {
            foreach (var b in branches)
            {
                Debug.Log("Destroy existing branch: " + b.name);
                Destroy(b.gameObject);
            }
        }

        yield return null;

        taskPanel.ResetTaskPanel();

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

        if (firstTime)
        {
            // Always set virtual twin in the center
            var centerIndex = Mathf.RoundToInt(amountOfVersions / 2);
            var versionToSwapWith = singleBranch.transform.GetChild(centerIndex);

            var virtTwinObj = singleBranch.transform.GetChild(objsToSpawn.Length);
            virtTwinObj.SetSiblingIndex(centerIndex); // this works, however, sibling index is not correctly ordered
            virtTwinObj.GetComponent<VersionObject>().id = centerIndex.ToString();
            versionToSwapWith.SetSiblingIndex(objsToSpawn.Length);
            versionToSwapWith.GetComponent<VersionObject>().id = objsToSpawn.Length.ToString();

            taskPanel.ToggleBetweenButtons(false);

            readyForExperiment = false;
        }
        else
        {
            // Select randomly the looked for version
            var randomizedIndex = Random.Range(0, objsToSpawn.Length);
            lookedForVersion = singleBranch.transform.GetChild(randomizedIndex).gameObject;

            var modelCopy = lookedForVersion.transform.GetChild(0);
            Instantiate(modelCopy, taskPanel.GetModelContainer());

            // Reorder virtual twin and change ID with other version
            randomizedIndex = Random.Range(0, objsToSpawn.Length);
            var versionToSwapWith = singleBranch.transform.GetChild(randomizedIndex);

            var virtTwinObj = singleBranch.transform.GetChild(objsToSpawn.Length);
            virtTwinObj.SetSiblingIndex(randomizedIndex); // this works, however, sibling index is not correctly ordered
            virtTwinObj.GetComponent<VersionObject>().id = randomizedIndex.ToString();
            versionToSwapWith.SetSiblingIndex(objsToSpawn.Length);
            versionToSwapWith.GetComponent<VersionObject>().id = objsToSpawn.Length.ToString();

            readyForExperiment = true;
        }

        ready = true;

        yield return null;

        var startupManager = AppManager.Instance.GetStartupManager();
        startupManager.StartCoroutine(startupManager.StartUp(firstTime));
    }

    VersionObject CreateVersionObjectFromModel(GameObject baseModel, int id, bool virtTwin = false)
    {
        GameObject loadedOBJ = Instantiate(baseModel);
        GameObject newVersion = Instantiate(versionPrefab, AppManager.Instance.GetBranchContainer());
        VersionObject versionLogic = newVersion.GetComponent<VersionObject>();

        // Meta information
        versionLogic.id = id.ToString();
        versionLogic.description = "MESSAGE TO SET"; // TODO message is important, as this might be the description fitting to the looked for object
        versionLogic.createdBy = "Anonymous";
        versionLogic.createdAt = System.DateTime.Now.ToString();
        versionLogic.virtualTwin = virtTwin;

        // Switch model to the prefab
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

        return versionLogic;
    }

    /// <summary>
    /// Select a number of objects out of an object pool. Based on the set difficulty and number of versions defined.
    /// </summary>
    /// <returns>Array with the objects to spawn in the timeline.</returns>
    GameObject[] RandomlySelectTimelineObjects()
    {
        GameObject[] objectsToSpawn = new GameObject[amountOfVersions];

        //GameObject[] selectedArray;
        //switch (difficulty) {
        //    case 0:
        //        selectedArray = timelineObjects;
        //        break;
        //    case 1:
        //        selectedArray = timelineObjects;
        //        break;
        //    case 2:
        //        selectedArray = timelineObjects;
        //        break;
        //    default:
        //        Debug.Log("Difficulty not set to a value.");
        //        selectedArray = timelineObjects;
        //        break;
        //}

        List<GameObject> objPool = new List<GameObject>(timelineObjects);

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

    public void SaveResults(float timeRequired)
    {
        // Check for folder first
        string folderPath = Application.persistentDataPath + "/results";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = folderPath + "/participant" + participantID + ".csv";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "participant " + participantID + " | " + System.DateTime.Now + "\n\n");
        }

        // difficulty; trial count; time;
        // could be: amount of objects; date;
        string content = difficulty + ";" + experimentCounter + ";" + timeRequired + ";\n";

        File.AppendAllText(filePath, content);

        Debug.Log("Results saved");
    }

    public void SetExperimentRunning(bool status)
    {
        experimentRunning = status;

        timelineManager.ToggleDummyModels(false);
        
        // Experiment starts
        if (status)
        {
            experimentStartTime = Time.time;
        }
        else // Experiment stops
        {
            var endTime = Time.time;
            var timeRequired = endTime - experimentStartTime;

            SaveResults(timeRequired);
        }
    }

    public bool GetExperimentRunning()
    {
        return experimentRunning;
    }

    public bool IsReady()
    {
        return ready;
    }
}
