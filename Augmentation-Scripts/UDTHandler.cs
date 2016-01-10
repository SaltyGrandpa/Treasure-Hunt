using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;

public class UDTHandler : MonoBehaviour, IUserDefinedTargetEventHandler
{
    private UserDefinedTargetBuildingBehaviour mTargetBuildingBehaviour;
    private ObjectTracker mObjectTracker;
    private DataSet mBuiltDataSet;
    private bool mUdtInitialized = false;
    public ImageTargetBuilder.FrameQuality mFrameQuality = ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE;
    public GUISkin greenBox, yellowBox, redBox, primary;
    public string postLevelUrl;
    public string levelName = "", levelDescription = "", levelData = "";
    public GameObject[] augmentedObjects;
    private bool uploadScreen = false, uploadBegan = false;
    private float longitude, latitude;
    public int nextActive = 0;

    public ImageTargetBehaviour ImageTargetTemplate;
    // Use this for initialization
    void Start () {
        StartCoroutine(getLocation());
        mTargetBuildingBehaviour = GetComponent<UserDefinedTargetBuildingBehaviour>();
        if (mTargetBuildingBehaviour)
        {
            mTargetBuildingBehaviour.RegisterEventHandler(this);
        }
        VuforiaBehaviour.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaBehaviour.Instance.RegisterOnPauseCallback(OnPaused);
    }

    public void OnInitialized()
    {
        // look up the ImageTracker once and store a reference
        mObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        if (mObjectTracker != null)
        {
            // create a new dataset
            mBuiltDataSet = mObjectTracker.CreateDataSet();
            mObjectTracker.ActivateDataSet(mBuiltDataSet);

            // remember that the component has been initialized
            mUdtInitialized = true;
        }
    }

    public void OnFrameQualityChanged(ImageTargetBuilder.FrameQuality frameQuality)
    {
        mFrameQuality = frameQuality;
    }

    public void OnNewTrackableSource(TrackableSource trackableSource)
    {
        // deactivates the dataset first
        mObjectTracker.DeactivateDataSet(mBuiltDataSet);

        // Destroy the oldest target if the dataset is full
        if (mBuiltDataSet.HasReachedTrackableLimit())
        {
            IEnumerable<Trackable> trackables = mBuiltDataSet.GetTrackables();
            Trackable oldest = null;
            foreach (Trackable trackable in trackables)
            {
                if (oldest == null || trackable.ID < oldest.ID)
                    oldest = trackable;
            }
            if (oldest != null)
            {
                mBuiltDataSet.Destroy(oldest, true);
            }
        }

        // get predefined trackable (template) and instantiate it
        ImageTargetBehaviour imageTargetCopy = (ImageTargetBehaviour)Instantiate(ImageTargetTemplate);

        // add the trackable to the data set and activate it
        mBuiltDataSet.CreateTrackable(trackableSource, imageTargetCopy.gameObject);

        // Re-activate the dataset
        mObjectTracker.ActivateDataSet(mBuiltDataSet);
    }

    void OnGUI()
    {
        if (!mUdtInitialized) return;

        if (!uploadScreen && !uploadBegan)
        {
            if (augmentedObjects.Length > 1)
            {
                GUI.skin = primary;
                if (GUI.Button(new Rect(0, 0, Screen.width, (Screen.height * 0.1f)), "Upload Treasures"))
                {
                    levelData = LevelSerializer.SerializeLevel();
                    uploadScreen = true;
                }
                if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH)
                {
                    GUI.skin = greenBox;
                    {
                        if (GUI.Button(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Change Treasure!"))
                        {
                            changeModel();
                        }
                    }
                }
                else if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM)
                {
                    GUI.skin = yellowBox;
                    {
                        if (GUI.Button(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Change Treasure!"))
                        {
                            changeModel();
                        }
                    }
                }
                else
                {
                    GUI.skin = redBox;
                    {
                        if (GUI.Button(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Change Treasure!"))
                        {
                            changeModel();
                        }
                    }
                }
            }
            else
            {
                // If Frame Quality is medium / high => show Button to build target
                if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH)
                {
                    GUI.skin = greenBox;
                    {
                        if (GUI.Button(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Place Object!"))
                        {
                            BuildNewTarget();
                        }
                    }
                }
                else
                {
                    if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM)
                    {
                        GUI.skin = yellowBox;
                        GUI.Box(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Not enough image detail...");
                    }
                    else
                    {
                        GUI.skin = redBox;
                        GUI.Box(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Not enough image detail...");
                    }
                }
            }
        } else {
            GUI.skin = primary;
            if (!uploadBegan)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Upload Treasures");
                GUI.Label(new Rect(0, 100, Screen.width, 40), "Treasure Title");
                levelName = GUI.TextField(new Rect(Screen.width * .05f, 150, Screen.width * .9f, 46), levelName, 128);
                GUI.Label(new Rect(0, 210, Screen.width, 40), "Description & Location");
                levelDescription = GUI.TextField(new Rect(Screen.width * .05f, 260, Screen.width * .9f, 46), levelDescription, 256);
                GUI.Label(new Rect(0, 325, Screen.width, 40), "Longitude: " + longitude + ", Latitude: " + latitude);
                if (GUI.Button(new Rect(Screen.width / 4 - 10, 390, Screen.width / 4, (Screen.height * 0.1f)), "Upload!"))
                {
                    StartCoroutine(BeginUpload());
                }
                if (GUI.Button(new Rect(Screen.width / 4 * 2 + 10, 390, Screen.width / 4, (Screen.height * 0.1f)), "Back"))
                {
                    uploadBegan = false;
                    uploadScreen = false;
                }
            }
            else
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Upload Treasures");
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Contacting Server and Uploading...\nYou will be redirected to the main menu upon completion.");
            }
        }
    }

    private void changeModel()
    {
        GameObject augmentedGO = GameObject.Find("UserDefinedTarget(Clone)");
        Transform augmentedTR = augmentedGO.gameObject.transform;
        int index = 0, indexActive = 0;
        List<GameObject> augmentedObjs = new List<GameObject>();
        foreach (Transform child in augmentedTR)
        {
            if (child.gameObject.activeSelf)
            {
                indexActive = index;
            }
            augmentedObjs.Add(child.gameObject);
            index++;
        }
        if(indexActive >= (augmentedObjs.Count - 1))
            nextActive = 0;
        else
            nextActive = indexActive + 1;

        index = 0;
        foreach (GameObject thisGO in augmentedObjs)
        {
            thisGO.SetActive(false);
            if (index == nextActive)
            {
                thisGO.SetActive(true);
                print("Set " + thisGO + " active.");
            }
            index++;
        }
    }

    private IEnumerator BeginUpload()
    {
        uploadBegan = true;
        // Create a Web Form
        WWWForm postform = new WWWForm();
        postform.AddField("level_name", levelName);
        postform.AddField("level_desc", levelDescription);
        postform.AddField("level_data", levelData);
        postform.AddField("longitude", longitude.ToString());
        postform.AddField("latitude", latitude.ToString());
        // POST to the Web Form
        WWW w = new WWW(postLevelUrl, postform);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            print(w.error);
        }
        else
        {
            string results = w.text;
            print("Post successful: " + results);
            Application.LoadLevel(0);
        }
    }

    private IEnumerator getLocation()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            print("Location services is disabled");
        }

        // Start service before querying location
        Input.location.Start();
        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status
                == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds (1);
            maxWait--;
        }
        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
        }
        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
        }
        // Access granted and location value could be retrieved
        else
        {
            print("Location: " + Input.location.lastData.latitude + " " +
                    Input.location.lastData.longitude + " " +
                    Input.location.lastData.altitude + " " +
                    Input.location.lastData.horizontalAccuracy + " " +
                    Input.location.lastData.timestamp);

            longitude = Input.location.lastData.longitude;
            latitude = Input.location.lastData.latitude;
        }
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }

    void LateUpdate()
    {
        augmentedObjects = GameObject.FindGameObjectsWithTag("AugmentedHandler");
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
    }

    private void BuildNewTarget()
    {
        string newTargetName = "SpawnedTarget";
        mTargetBuildingBehaviour.BuildNewTarget(newTargetName, ImageTargetTemplate.GetSize().x);
    }

    private void OnVuforiaStarted()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(
                CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }

}
