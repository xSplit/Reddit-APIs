<?php
$search = 'https://www.google.com/search?q=';

if(count($argv)>2 && in_array($argv[2],hash_algos()))
{
  echo 'Working...'.PHP_EOL;
  foreach((is_file($argv[1])?array_map('rtrim',file($argv[1])):array(trim($argv[1]))) as $hash)
  {
    $r = file_get_contents($search.$hash);
    $ps = substr_count($r,'class="fl"');
    $found = false;
    $i=1;
    do
    {
       $urls = array_map(function($v){ return explode('&',$v)[0]; },explode('<li class="g"><h3 class="r"><a href="/url?q=',$r));
       array_shift($urls);
       foreach($urls as $url)
       {
           $d = new DOMDocument();
           @$d->loadHTML(@file_get_contents($url));
           $arr = nlist($d->childNodes);
          foreach(preg_split('/\s+|:/',implode(' ',$arr)) as $word)
          {
              if(hash($argv[2],trim($word))==$hash)
              {
                  echo 'Found '.$hash.' - '.$word.PHP_EOL;
                  $found = true;
                  break;
              }
          }
           if($found) break;
       }
       $r = file_get_contents($search.$hash.'&start='.($i*10));
       $i++;
    }while($i<$ps && !$found);
    if(!$found) echo 'Not found '.$hash.PHP_EOL;
  }
}else echo 'Invalid hash or hash type';

function nlist(DOMNodeList $d)
{
    $ret = array();
    for($i=0;$i<$d->length;$i++)
    {
        if($d->item($i)->hasChildNodes())
            foreach(nlist($d->item($i)->childNodes) as $n) $ret[] = $n;
        else $ret[] = $d->item($i)->textContent;
    }
    return $ret;
}
