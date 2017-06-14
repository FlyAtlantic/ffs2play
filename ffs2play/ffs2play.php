<?php

$liste = glob("FFS2Play*.zip");
if (count($liste) > 0)
{
	rsort ($liste, SORT_STRING );
	$fichier = $liste[0];
	$nom = preg_replace('/\\.[^.\\s]{3,4}$/', '', $fichier);
	$parse = explode("_", $nom);
	if (strtolower($parse[0])=="ffs2play")
	{
		if(empty($_GET['download']))
		{
			$xml = new SimpleXMLElement('<xml/>');
			$version = $xml->addChild('version');
			$version->addAttribute('major',$parse[1]);
			$version->addAttribute('minor',$parse[2]);
			$version->addAttribute('build',$parse[3]);
			$xml->addchild('url',"http://" . $_SERVER['SERVER_NAME']."/" . $fichier);
			Header('Content-type: text/xml');
			print($xml->asXML());
		}
		else
		{
			header('Content-Type: application/octet-stream');
			$poids = filesize($fichier);
			header('Content-Length: '. $poids);
			header('Content-disposition: attachment; filename='. $fichier);
			header('Pragma: no-cache');
			header('Cache-Control: no-store, no-cache, must-revalidate, post-check=0, pre-check=0');
			header('Expires: 0');
			readfile($fichier);
		}
	}
}

?>
