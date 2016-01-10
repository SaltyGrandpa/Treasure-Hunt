<?php 
	//Only execute script if all variables are set
	if(!(isset($_POST["id"])) ||
	(($_POST["id"] == ""))
	)
		die("ABORTED: Not enough information to get level data");
	else {
		
		
		try {
			//Begin database connection
			//Connect using my shared credentials file
			//Database hosted locally, database name is 'SeniorProject'
			include_once("../includes/sql.inc.php");
			$db = new PDO("mysql:host=localhost;dbname=SeniorProject;", DB_USER, DB_PASSWORD);
			$db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_OBJ);
		
			$id = $_POST["id"]; // radius of bounding circle in miles
			
			$sql = "SELECT data FROM augmented WHERE id = :id";
			$params = array(
				'id' => $id,
			);
			$points = $db->prepare($sql);
			$points->execute($params);
			$points->setFetchMode(PDO::FETCH_ASSOC);
			while($row = $points->fetch()) {
				echo $row['data'];	
			}
			
		//Display error if the post is unsuccessful
		} catch (Exception $e) {
			die("An error has occurred while submitting the level data: " . $e);
		}
	}
?>
