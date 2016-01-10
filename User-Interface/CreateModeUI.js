#pragma strict

public var guiSkinPrimary : GUISkin;
public var postLevelUrl : String;
private var levelData : String;
private var augmentedObjects : GameObject[];
private var uploadScreen : boolean = false;
private var uploadBegan : boolean = false;
private var longitude : float;
private var latitude : float;
private var levelName : String = "";
private var levelDescription : String = "";

function OnGUI() {
    GUI.skin = guiSkinPrimary;
    if(!uploadScreen && !uploadBegan)
    {
        if(augmentedObjects.length > 0)
        {
            if(GUI.Button(Rect(0, 0, Screen.width, (Screen.height * 0.1f)), "Upload Treasures"))
            {
                levelData = LevelSerializer.SerializeLevel();
                uploadScreen = true;
            }
        }    
    } else {
        if(!uploadBegan)
        {
            GUI.Box(Rect(0,0,Screen.width,Screen.height),"Upload Treasures");
            GUI.Label(Rect(0,100,Screen.width,40),"Treasure Title");
            levelName = GUI.TextField (Rect (Screen.width * .05, 150, Screen.width * .9, 46), levelName, 128);
            GUI.Label(Rect(0,210,Screen.width,40),"Description & Location");
            levelDescription = GUI.TextField (Rect (Screen.width * .05, 260, Screen.width * .9, 46), levelDescription, 256);
            GUI.Label(Rect(0,325,Screen.width,40),"Longitude: " + longitude + ", Latitude: " + latitude);
            if(GUI.Button(Rect(Screen.width / 4 - 10, 390, Screen.width / 4, (Screen.height * 0.1f)), "Upload!"))
            {
                BeginUpload();
            }
            if(GUI.Button(Rect(Screen.width / 4 * 2 + 10, 390, Screen.width / 4, (Screen.height * 0.1f)), "Back"))
            {
                uploadBegan = false;
            }
         } else {
             GUI.Box(Rect(0,0,Screen.width,Screen.height),"Upload Treasures");
             GUI.Label(Rect(0,0,Screen.width,Screen.height),"Contacting Server and Uploading...\nYou will be redirected to the main menu upon completion.");
         }
    }
}

function BeginUpload() {
    uploadBegan = true;
    // Create a Web Form
    var postform = new WWWForm();
    postform.AddField("level_name", levelName);
    postform.AddField("level_desc", levelDescription);
    postform.AddField("level_data", levelData);
    postform.AddField("longitude", longitude);
    postform.AddField("latitude", latitude);
    // POST to the Web Form
    var w = new WWW(postLevelUrl, postform);
    yield w;
    if (!String.IsNullOrEmpty(w.error)) {
        print(w.error);
    }
    else {
        var results = w.text;
        print("Post successful: " + results);
    }
}

function getLocation () {
    // First, check if user has location service enabled
    if (!Input.location.isEnabledByUser)
    {
        print ("Location services is disabled");
        return;
    }
        
    // Start service before querying location
    Input.location.Start ();
    // Wait until service initializes
    var maxWait : int = 20;
    while (Input.location.status
            == LocationServiceStatus.Initializing && maxWait > 0) {
        yield WaitForSeconds (1);
        maxWait--;
    }
    // Service didn't initialize in 20 seconds
    if (maxWait < 1) {
        print ("Timed out");
        return;
    }
    // Connection has failed
    if (Input.location.status == LocationServiceStatus.Failed) {
        print ("Unable to determine device location");
        return;
    }
        // Access granted and location value could be retrieved
    else {
        print ("Location: " + Input.location.lastData.latitude + " " +
                Input.location.lastData.longitude + " " +
                Input.location.lastData.altitude + " " +
                Input.location.lastData.horizontalAccuracy + " " +
                Input.location.lastData.timestamp);

        longitude = Input.location.lastData.longitude;
        latitude = Input.location.lastData.latitude;
    }
    // Stop service if there is no need to query location updates continuously
    Input.location.Stop ();
}

function LateUpdate() {
    augmentedObjects = GameObject.FindGameObjectsWithTag("AugmentedObject");
}

function OnStart() 
{
    getLocation();
}
