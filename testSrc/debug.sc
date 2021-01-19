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

formatType(v) -> (
  t = type(v);
  c = l();
  if (t == 'null',
    c: 0 = 'p '+v;
  , t == 'number',
    c: 0 = '#0080ff '+v;
  , t == 'string',
    c: 0 = '#ff8000 '+v;
  , t == 'list',
    for (v,
      if (_i == 0,
        c: 0 = 'y [';
      ,
        put(c, null, 'y , ');
      );
      c = concat(c, formatType(_))
    );
    put(c, null, 'y ]');

  , t == 'map',
    p = pairs(v);
    for (p,
      if (_i == 0,
        c: 0 = 'r {';
      ,
        put(c, null, 'r , ');
      );
      c = concat(c, formatType(_: 0));
      put(c, null, 'r : ');
      c = concat(c, formatType(_: 1));
    );
    put(c, null, 'r }');

  , t == 'nbt',
    n = parse_nbt(v);
    t2 = type(n);
    if (t2 == 'map',
      // If the nbt is like a map, parse it like a map but with nbt colors
      p = pairs(parse_nbt(v));
      for (p,
        if (_i == 0,
          c: 0 = '#00cccc {';
        ,
          put(c, null, '#00cccc , ');
        );
        put(c, null, '#00cccc '+_: 0+': ');
        c = concat(c, formatType(_: 1));
      );
      put(c, null, '#00cccc }');
    , t2 == 'list',
      // If the nbt is like a list, parse it like a list but with nbt colors
      for (n,
        if (_i == 0,
          c: 0 = '#00cccc [';
        ,
          put(c, null, '#00cccc , ');
        );
        c = concat(c, formatType(_))
      );
      put(c, null, '#00cccc ]');
    ,
      // If the nbt isn't like either, just give it nbt colors
      c: 0 = '#00cccc '+n;
    );

  , t == 'iterator',
    c: 0 = '#cccc00 '+v;
  , t == 'function',
    c: 0 = 'm '+v;
  , t == 'task',
    c: 0 = 'g '+v;
  , t == 'block',
    c: 0 = '#00ff00 '+v;
  , t == 'entity',
    c: 0 = '#ff2222 '+v;
  , t == 'text',
    c: 0 = v;
  );

  return(c);
);

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