<?php 
	//Only execute script if all variables are set
	if(!(isset($_POST["latitude"]) && isset($_POST["longitude"]) && isset($_POST["radius"])) ||
	(($_POST["radius"] == "") || ($_POST["latitude"] == "") || ($_POST["longitude"] == ""))
	)
		die("ABORTED: Not enough information to find levels.");
	else {
		
		
		try {
			/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
			/*  Selection of points within specified radius of given lat/lon      (c) Chris Veness 2008-2014  */
			/*  Originally found at: http://www.movable-type.co.uk/scripts/latlong-db.html                    */
			/*  Modifications to use milage and adapt to database structure by Corey McCown                   */
			/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
			//Begin database connection
			//Connect using my shared credentials file
			//Database hosted locally, database name is 'SeniorProject'
			include_once("../includes/sql.inc.php");
			$db = new PDO("mysql:host=localhost;dbname=SeniorProject;", DB_USER, DB_PASSWORD);
			$db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_OBJ);
		
			$lat = $_POST["latitude"]; // latitude of centre of bounding circle in degrees
			$lon = $_POST["longitude"]; // longitude of centre of bounding circle in degrees
			$rad = $_POST["radius"]; // radius of bounding circle in miles
			
			$R = 3959;  // earth's mean radius, miles
		
			// first-cut bounding box (in degrees)
			$maxLat = $lat + rad2deg($rad/$R);
			$minLat = $lat - rad2deg($rad/$R);
			// compensate for degrees longitude getting smaller with increasing latitude
			$maxLon = $lon + rad2deg($rad/$R/cos(deg2rad($lat)));
			$minLon = $lon - rad2deg($rad/$R/cos(deg2rad($lat)));
	
			$sql = "SELECT id, name, description, votes_up, votes_down FROM augmented WHERE (latitude BETWEEN :minLat AND :maxLat) AND (longitude BETWEEN :minLon AND :maxLon) ORDER BY votes_up-votes_down DESC";
			$params = array(
				'minLat' => $minLat,
				'minLon' => $minLon,
				'maxLat' => $maxLat,
				'maxLon' => $maxLon,
			);
			$points = $db->prepare($sql);
			$points->execute($params);
			$results = $points->fetchAll(PDO::FETCH_ASSOC);
			$json = json_encode($results);
			echo $json;
			
		//Display error if the post is unsuccessful
		} catch (Exception $e) {
			die("An error has occurred while submitting the level data: " . $e);
		}
	}
?>
