## Fable SQL.js app

- sql.js: wasm sqlite
- absurd-sql: indexeddb backend sql.js adapter
- comlink: WebWorker RPC

Based on:
[AbsurdSQL with type orm example](https://github.com/mizchi/absurd-sql-example-with-typeorm)

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 3.1 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/).


## Explanation on why this is cool:

https://jlongster.com/future-sql-web

## Deploy

You need to set CORP/COEP headers for absurd-sql(SharedArrayBuffer)

```
  Cross-Origin-Opener-Policy = same-origin
  Cross-Origin-Embedder-Policy = require-corp
```

## LICENSE

MIT

## Running

After cloning this project:

```
$ npm install
$ npm serve
```

You should be able to go to [http://localhost:8080/](http://localhost:8080/), and open the console to see some query results.


## Project structure

### npm

JS dependencies are declared in `package.json`, while `package-lock.json` is a lock file automatically generated.

### Webpack

[Webpack](https://webpack.js.org) is a JS bundler with extensions, like a static dev server that enables hot reloading on code changes.

### F#

The sample only several F# files: the project (.fsproj), auto-generated bindings using `ts2fable` (Comlink.fs, SqlJs.fs), Service Worker code in Worker.fs and the app itself in App.fs all in the `src` folder.