import('./example/debug.sc');
import('https://raw.githubusercontent.com/gnembon/scarpet/master/programs/fundamentals/math.sc', 'hex', 'bin'); // Demonstrate importing from web and tree shaking

debug({'x' -> 'y', 'z' -> hex(64), 'a' -> bin(12)});