using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;

public class DownloadLevel : MonoBehaviour, IUserDefinedTargetEventHandler
{

    private UserDefinedTargetBuildingBehaviour mTargetBuildingBehaviour;
    private ObjectTracker mObjectTracker;
    private DataSet mBuiltDataSet;
    private bool mUdtInitialized = false;
    public ImageTargetBuilder.FrameQuality mFrameQuality = ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE;
    public GUISkin greenBox, yellowBox, redBox, primary;
    public string postLevelUrl;
    public string levelName = "", levelDescription = "", levelData = "";
    private GameObject[] augmentedObjects;
    private bool uploadScreen = false, uploadBegan = false;
    private float longitude, latitude;

    public ImageTargetBehaviour ImageTargetTemplate;
    // Use this for initialization
    void Start()
    {
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
                if (oldest == null || trackable.ID < oldest.ID)
                    oldest = trackable;

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

	void OnGUI () {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Load Level"))
        {
            LevelSerializer.LoadNow(levelData);
        }
        if (!mUdtInitialized) return;
        if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH)
        {
            GUI.skin = greenBox;
            GUI.Box(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Great image detail!");
        }
        if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM)
        {
            GUI.skin = yellowBox;
            GUI.Box(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Low image detail.");
        }
        if (mFrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_LOW)
        {
            GUI.skin = redBox;
            GUI.Box(new Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Very low image detail.");
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
