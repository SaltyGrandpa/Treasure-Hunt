<?php 
	//Only execute script if all variables are set
	if(!(isset($_POST["level_id"]) && isset($_POST["vote_up"])) ||
	  (($_POST["level_id"] == "") || ($_POST["vote_up"] == ""))
	)
		die("ABORTED: Not enough information to vote on level.");
	else {
		//Level Information
		$level_id = $_POST["level_id"];
		$vote_up = $_POST["vote_up"];
		//Begin database connection
		//Connect using my shared credentials file
		//Database hosted locally, database name is 'SeniorProject'
		include_once("../includes/sql.inc.php");
		$db = new PDO("mysql:host=localhost;dbname=SeniorProject;", DB_USER, DB_PASSWORD);
		try {
		//Begin prepared database statement
		if($vote_up == "true")
			$STH = $db->prepare("UPDATE augmented SET votes_up = votes_up + 1 WHERE id = :id");
		else 
			$STH = $db->prepare("UPDATE augmented SET votes_down = votes_down + 1 WHERE id = :id");
		$STH->bindParam(':id', $level_id, PDO::PARAM_INT);
		$STH->execute();
		echo("Vote casted.");
		//Display error if the post is unsuccessful
		} catch (Exception $e) {
			die("An error has occurred while submitting the vote: " . $e);
		}
	}
?>
