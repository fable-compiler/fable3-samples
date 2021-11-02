module Worker

open System
open Fable.Core
open Fable.Core.JsInterop
let initSqlJs: SqlJs.InitSqlJsStatic = importDefault "@jlongster/sql.js"
let [<Import("SQLiteFS", from="absurd-sql")>] sqlitefs: obj = jsNative

let IndexedDBBackend: JsConstructor = importDefault "absurd-sql/dist/indexeddb-backend"


let pragmaParams = [|
   "PRAGMA cache_size=-5000"
   "PRAGMA page_size=8192"
   "PRAGMA journal_mode=MEMORY"
   "PRAGMA mmap_size=0"
   "PRAGMA synchronous=NORMAL"
   "PRAGMA temp_store=MEMORY"
|]
let pragmaString = String.Join(";",pragmaParams)

let mutable _db : SqlJs.Database option = None

let db() =
   match _db with
   | None -> failwith "not initialized"
   | Some db -> db


type SqlJsConfig() =
   interface SqlJs.SqlJsConfig with
      override _.locateFile(s,_)= s
   
let setupSqlJs (dbPath:string) = promise {
   let! SQL = initSqlJs.Invoke(SqlJsConfig())
   
   let sqlFs = createNew sqlitefs (SQL.FS, IndexedDBBackend.Create())
   
   SQL.register_for_idb(sqlFs)
   
   SQL.FS?mkdir("/sql")
   SQL.FS?mount(sqlFs, {||},"/sql")
   let db: SqlJs.Database = createNew SQL.Database (dbPath, {|filename=true|}) :?> SqlJs.Database
   
   JS.console.log("Executing...")
   JS.console.log(pragmaString)
   db?exec(pragmaString)
   db?exec("VACUUM")
   _db <- Some db
}

let setup() = promise {
    // with /sql/ namespace
    let dbname = "/sql/db.sqlite"
    do! setupSqlJs(dbname)
}


let execRaw (query: string) = promise {
   JS.console.log("Executing...")
   JS.console.log(query)
   return db().exec(query)
}

let runMany (query:string) argsList =
   promise {
      JS.console.log("BEGIN TRANSACTION")
      db().exec("BEGIN TRANSACTION") |> ignore
      JS.console.log("Preparing...")
      JS.console.log(query)
      let stmt = db().prepare(query)
      let results = argsList
                    |> Array.map (fun args ->
                       stmt.run(!!args)
                       stmt.getAsObject()
                    )
      stmt.free() |> ignore
      JS.console.log("Commit")
      db().exec("COMMIT") |> ignore
      return results
   }

let runPreparedStep(query: string) = promise {
   JS.console.log("Preparing...")
   JS.console.log(query)
   let stmt = db().prepare(query)
   stmt.step() |> ignore
   let result = stmt.getAsObject()
   stmt.free() |> ignore
   return result
}


type IApi =
   abstract member execRaw: string -> JS.Promise<ResizeArray<SqlJs.QueryExecResult>>
   abstract member setup: unit -> JS.Promise<unit>
   
   abstract member runMany: string -> obj [] -> JS.Promise<SqlJs.ParamsObject []>
   abstract member runPreparedStep: string-> JS.Promise<SqlJs.ParamsObject>
   
let api = createObj [
                      "setup" ==> setup
                      "execRaw" ==> execRaw
                      "runMany" ==> runMany
                      "runPreparedStep" ==> runPreparedStep
                      ]

Comlink.expose(api)
   
   