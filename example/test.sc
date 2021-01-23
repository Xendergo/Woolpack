import('./example/debug.sc');
import('https://raw.githubusercontent.com/gnembon/scarpet/master/programs/fundamentals/math.sc', 'hex', 7, 43); // Demonstrate importing from web and tree shaking

debug({'x' -> 'y', 'z' -> hex(64), 'a' -> bin(dot(l(1, 2), l(3, 4)))});