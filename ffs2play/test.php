<?php
session_start();
$fin = time() + 60*60*24;
setcookie('ffst','essai',$fin);
echo $_COOKIE['ffst'];
?>