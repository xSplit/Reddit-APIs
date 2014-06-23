<?php
$search = 'https://www.google.com/search?q=';

if(isset($argv[1],$argv[2]) && in_array($argv[2],hash_algos()))
{
    echo 'Working...'.PHP_EOL;
    $r = file_get_contents($search.$argv[1]);
    $ps = substr_count($r,'class="fl"');
    for($i=1;$i<$ps;$i++)
    {
       $urls = array_map(function($v){ return explode('&',$v)[0]; },explode('<li class="g"><h3 class="r"><a href="/url?q=',$r));
       array_shift($urls);
       foreach($urls as $url)
           foreach(preg_split('/\s+/',@file_get_contents($url)) as $word)
               if(hash($argv[2],$word)==$argv[1]) die('Found: '.$word.PHP_EOL);
       $r = file_get_contents($search.$argv[1].'&start='.($i*10));
    }
    die('Not found');
}else echo "Invalid hash or hash type";
