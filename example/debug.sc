import('./example/formatType.sc');
import('./example/test.sc'); // Show that circular dependencies don't work
// Adds a debug function which is equivalent to the print function, but prints stuff different colors depending on type. Avoids confusion between null and 'null' and between maps, strings, and nbts

// Types & colors
// null - purple
// number - blue
// string - orange
// list - yellow
// map - red
// iterator - Darker yellow
// function - magenta
// task - grey
// block - green
// entity - Darker red
// nbt - cyan
// text - Unchanged, usually formatted already


debug(...v) -> (
  allFormatted = '';
  for (v,
    formatted = formatType(_);
    actuallyFormatted = '';
    for (formatted,
      if (type(_) != 'text',
        actuallyFormatted += format(_);
      ,
        actuallyFormatted += _;
      );
    );

    allFormatted += actuallyFormatted + ' ';
  );

  print(allFormatted);
);

concat(a, b) -> (
  ret = copy(a);

  for (b,
    put(ret, null, _);
  );

  return(ret);
);