#pragma strict
import MiniJSON;

//General Variables
public var getLevelURL : String;
public var getLevelDataURL : String;
public var longitude : float;
public var latitude : float;
public var radius : int = 5.0;
public var forceTesting : boolean;
public var levelLoaderGameObject : GameObject;
private var touch : Touch;
private var levelsLoaded : boolean = false;
private var levelList : List.<System.Object>;
private var scene : Dictionary.<String, System.Object>;

//GUI Variables
var uiSkin : GUISkin;
var titleStyle : GUIStyle;
var descriptionStyle : GUIStyle;
var refreshImage : Texture;
var boxHeight : int = 150;
private var scrollPadding = 30;
private var scrollPosition : Vector2 = Vector2.zero;


function OnGUI () {
    var i = 1;
    GUI.skin = uiSkin;
    GUI.Box (Rect (0,0,Screen.width,50), "");
    GUI.Label(new Rect (10, 0, Screen.width/3, 45), "Search for Scenes", titleStyle);
    GUI.Label(new Rect (Screen.width / 3 + 75, 0, Screen.width/3, 45), "Search Radius: " + radius + " mi.", descriptionStyle);
    radius = GUI.HorizontalSlider (Rect ((Screen.width / 3) * 2, 20, Screen.width/4, 30), radius, 1.0, 50.0);
    if(GUI.Button(Rect(((Screen.width / 3) * 3) - 50, 5, 40, 40), refreshImage))
        refreshLevels();
    if(levelsLoaded)
    {
        scrollPosition = GUI.BeginScrollView (Rect (0, 60, Screen.width, Screen.height - 60),
			scrollPosition, Rect (0, 0, Screen.width - 20, scrollPadding));
        for(level in levelList)
        {
            scene = levelList[i-1] as Dictionary.<String, System.Object>;
            var sceneName : String = scene["name"];
            var sceneDescription : String = scene["description"];
            var upvotes : int = int.Parse(scene["votes_up"]);
            var downvotes : int = int.Parse(scene["votes_down"]);
            var id : int = int.Parse(scene["id"]);
            var score : String = (upvotes - downvotes).ToString();
            var spacing = (boxHeight + 10) * (i - 1);
            var boxWidth : float;
            if(scrollPadding < (Screen.height - 60))
                boxWidth = Screen.width - 5;
            else
                boxWidth = Screen.width - 20;
            GUI.Box(Rect(2, spacing, boxWidth, boxHeight),"");
            GUI.Label(new Rect ((boxWidth / 3) - (boxWidth / 6) - 10, spacing + 8, boxWidth / 3, 20), "Title", descriptionStyle);
            GUI.Label(new Rect (10, spacing + 8, boxWidth / 3, boxHeight - 20), sceneName, titleStyle);
            GUI.Label(new Rect ((boxWidth / 3) + 10, spacing + 8, boxWidth / 3, 20), "Description", descriptionStyle);
            GUI.Label(new Rect ((boxWidth / 3) + 10, spacing + 10, boxWidth / 3, boxHeight - 20), sceneDescription , descriptionStyle);

            GUI.Label(new Rect (((boxWidth / 3) * 2) + 10, spacing + 65, boxWidth / 3, 20), "Score: " + score, descriptionStyle);
            if(GUI.Button(Rect(((boxWidth / 3) * 2) + 120, spacing + 20, (boxWidth / 5), boxHeight - 40), "Search for This!"))
                loadLevel(id, sceneName, sceneDescription, int.Parse(score));
            i++;
            if((i * boxHeight) > scrollPadding)
                scrollPadding = i * boxHeight;
        }

        // End the scroll view that we began above.
        GUI.EndScrollView ();
    } else {
        GUI.Label(Rect((Screen.width / 2) - 75, (Screen.height) / 2 - 50, Screen.width, 100), "Loading data . . .", descriptionStyle);
    }   
    if(GUI.Button(Rect(0, Screen.height - (Screen.height * 0.1f), Screen.width, (Screen.height * 0.1f)), "Main Menu"))
        Application.LoadLevel(0);
}

function loadLevel(id : int, title : String, description : String, score : int)
{
    levelsLoaded = false;
    print("POST - Load " + id);

    // Create a Web Form
    var postform = new WWWForm();
    postform.AddField("id", id);

    // POST to the Web Form
    var w = new WWW(getLevelDataURL, postform);
    yield w;
    if (!String.IsNullOrEmpty(w.error)) {
        print(w.error);
    }
    else {
        var results = w.text;
        print("Retreived data: " + results);
        var loaderComponent = levelLoaderGameObject.GetComponent(InstantiateLevel);
        loaderComponent.setData(title, description, score, id, results);
    }
}

function refreshLevels()
{
    levelsLoaded = false;
    levelList = new List.<System.Object>();
    scene = new Dictionary.<String, System.Object>();
    getLevelJSON(longitude, latitude, radius);
}

function getLocation () {
    if(forceTesting)
        getLevelJSON(longitude, latitude, radius);

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
        getLevelJSON(longitude, latitude, radius);
    }
    // Stop service if there is no need to query location updates continuously
    Input.location.Stop ();
}

function Start() 
{
    getLocation();
}

function getLevelJSON(theLong : float, theLat : float, theRadius : int)
{
    print("POST - longitude: " + theLong + " latitude: " + theLat + " radius: " + theRadius + "m");

    // Create a Web Form
    var postform = new WWWForm();
    postform.AddField("latitude", theLat.ToString());
    postform.AddField("longitude", theLong.ToString());
    postform.AddField("radius", theRadius);

    // POST to the Web Form
    var w = new WWW(getLevelURL, postform);
    yield w;
    if (!String.IsNullOrEmpty(w.error)) {
        print(w.error);
    }
    else {
        var results = w.text;
        print("Retreived data: " + results);
        parseLevelData(results);
    }
}

function parseLevelData(levelData : String)
{
    var obj = Json.Deserialize(levelData);
    print(obj.GetType());
    levelList = Json.Deserialize(levelData) as List.<System.Object>;
    levelsLoaded = true;
}

function Update () {
    if(Input.touchCount > 0)
    {
        touch = Input.touches[0];
        if (touch.phase == TouchPhase.Moved)
        {
            scrollPosition.y += (touch.deltaPosition.y * 5);
        }
    }
}
