module App

open Fable.Core
open Fable.Core.JsInterop
let [<Import("initBackend", from="absurd-sql/dist/indexeddb-main-thread")>] initBackend: obj -> unit = jsNative
module Url =
    type URLType =
      [<Emit("new $0($1,import.meta.url)")>] abstract CreateImportMetaUrl: url: string -> Browser.Types.URL
    let [<Global>] URL: URLType = jsNative
    
module CustomWorker =
    type CustomWorkerConstructor =
      [<Emit("new $0($1...)")>] abstract Create: url: Browser.Types.URL * ?options: Browser.Types.WorkerOptions -> Browser.Types.Worker
      
    let [<Global>] Worker: CustomWorkerConstructor = jsNative
    
    
let worker = CustomWorker.Worker.Create(Url.URL.CreateImportMetaUrl("./Worker.fs.js"))

initBackend(worker)

let api: Comlink.Remote<Worker.IApi> = Comlink.wrap(worker :?> Comlink.Protocol.Endpoint)


module KVTable =
  let createQuery =
    "CREATE TABLE IF NOT EXISTS kv (
      key TEXT PRIMARY KEY,
      value TEXT
    )"

module UsersTable =
  type User = {
    id: int
    name: string
    email: string
  }
  let mapUsers (result: SqlJs.QueryExecResult) :User [] =
    let idPos = result.columns.IndexOf "id"
    let namePos = result.columns.IndexOf "name"
    let emailPos = result.columns.IndexOf "email"
    JS.console.log($"idPos - {idPos}, namePos - {namePos}, emailPos - {emailPos}")
    if result.values.Count = 0 then
      Array.empty
    else
      result.values
      |> Seq.map (fun x ->
        let id = x.[idPos] |> string |> int
        let name =string  x.[namePos]
        let email = string x.[emailPos]
        {
          id = id
          name = name
          email = email
        }
      )
      |> Array.ofSeq
    
    
  let createQuery =
    "CREATE TABLE IF NOT EXISTS users (
      id INTEGER PRIMARY KEY,
      name TEXT,
      email TEXT
    )"

let window = Browser.Dom.window
let init() = promise {
  window.document.body.innerHTML <- window.document.body.innerHTML + "Loading..."
  do! api.setup()
  window.document.body.innerHTML <- window.document.body.innerHTML + "Worker ready"
  
  let! _ = api.execRaw(KVTable.createQuery)
  let! _ = api.execRaw(UsersTable.createQuery)
  
  let data = [|1..100|] |> Array.map (fun i -> (JS.Math.random() * 1000., i))
  let! _ = api.runMany "INSERT INTO kv (key, value) VALUES (?, ?)" !!data
  
  let! res = api.execRaw("SELECT * from kv")
  JS.console.log(res)
 
  let! res2 = api.runPreparedStep("SELECT SUM(key) FROM kv")
  JS.console.log("Summ of all keys is: " + res2?``SUM(key)``)
  
  let usersData = [|
    (1, "John Doe","test@example.com")
    (2, "Mike Wazovski", "test2@example.com")
  |]
  
  let! users = api.execRaw("SELECT * from users")
  let mapped = users |> Seq.tryHead |> Option.map (UsersTable.mapUsers)
  match mapped with
  | Some data ->
    JS.console.log($"Got {data.Length} users")
    JS.console.log(data)
  | None ->
    let! _ = api.runMany "INSERT INTO users (id,name, email) VALUES (?, ?, ?)" !!usersData
    let! users = api.execRaw("SELECT * from users")
    let mapped = users |> Seq.tryHead |> Option.map (UsersTable.mapUsers)
    JS.console.log(mapped)
  
}
init()
|> Promise.catch (fun x -> JS.console.error(x))
|> Promise.start