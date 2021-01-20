# Woolpack
A program like [Webpack](https://webpack.js.org/) or [Rollup](https://rollupjs.org/guide/en/) but for scarpet

## Features
The idea is that you can write scarpet in multiple files for nicer code and distribute it as one file so people won't have to download lots of files.

You can use the import function to essentially copy/paste code from one file to another. Circular dependencies are ignored. Unlike in native scarpet, the import function must be a file path. Importing individual symbols is not supported *yet*, and an import function with multiple arguments given will be ignored

Example: `import('./debug.sc');`

I am planing to add a bunch more in the future, so yeah

## How to use
In the folder you want to write scarpet in, create a file named `woolpack_config.json`. In there, you can set what the entry file is, what file to write to, and what folder to watch to automatically reload.

### Properties in config file
`entry`: Sets the file the program starts reading from

`write_location`: Where the bundled file should be written

`autoreload`: Sets a folder the program will watch. When a file in the folder changes, it will re-bundle the code

Note: `entry` and `write_location` are required, everything else is optional

Example `woolpack_config.json`:
```json
{
  "entry": "test.sc",
  "write_location": "C:/.../.minecraft/saves/your_world/scripts/test.sc",
  "autoreload": "./"
}
```

## How to install
1. Go to the releases page and download the release for your OS
2. Unzip the folder and add it to PATH
3. To start it, cd to the folder and run `woolpack`. Import statements and paths in the config file are relative to where you cd'd, and the config file must be in the folder you cd'd to