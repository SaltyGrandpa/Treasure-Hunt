<?php 
	//Only execute script if all variables are set
	if(!(isset($_POST["level_name"]) && isset($_POST["level_desc"]) && isset($_POST["level_data"]) && isset($_POST["latitude"]) && isset($_POST["longitude"])) &&
	  (($_POST["level_name"] == "") || ($_POST["level_desc"] == "") || ($_POST["level_data"] == "") || ($_POST["latitude"] == "") || ($_POST["longitude"] == ""))
	)
		die("ABORTED: Not enough information to add level to database.");
	else {
		//Level Information
		$level_name = $_POST["level_name"];
		$level_desc = $_POST["level_desc"];
		$level_data = $_POST["level_data"];
		//GPS Information
		$latitude = $_POST["latitude"];
		$longitude = $_POST["longitude"];
		//Begin database connection
		//Connect using my shared credentials file
		//Database hosted locally, database name is 'SeniorProject'
		include_once("../includes/sql.inc.php");
		$db = new PDO("mysql:host=localhost;dbname=SeniorProject;", DB_USER, DB_PASSWORD);
		try {
		//Begin prepared database statement
		$STH = $db->prepare("INSERT INTO augmented (name, description, data, longitude, latitude) VALUES (:name,  :description,  :data,  :longitude,  :latitude)");
		$STH->bindParam(':name', $level_name, PDO::PARAM_STR);
		$STH->bindParam(':description', $level_desc, PDO::PARAM_STR);
		$STH->bindParam(':data', $level_data, PDO::PARAM_STR);
		$STH->bindParam(':longitude', $longitude, PDO::PARAM_STR);
		$STH->bindParam(':latitude', $latitude, PDO::PARAM_STR);
		$STH->execute();
		echo("Added " . htmlspecialchars($level_name) . " to the database.");
		//Display error if the post is unsuccessful
		} catch (Exception $e) {
			die("An error has occurred while submitting the level data: " . $e);
		}
	}
	?>
