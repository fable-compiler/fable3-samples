// ts2fable 0.7.1
module rec SqlJs
open System
open Fable.Core
open Fable.Core.JS

//type Function = System.Action


type [<AllowNullLiteral>] IExports =
    abstract Database: DatabaseStatic
    abstract Statement: StatementStatic
    abstract StatementIterator: StatementIteratorStatic
    abstract SqlJs: InitSqlJsStatic

type SqlValue =
    U3<float, string, Uint8Array> option

type ParamsObject = SqlValue

type [<AllowNullLiteral>] ParamsCallback =
    [<Emit "$0($1...)">] abstract Invoke: obj: ParamsObject -> unit

type SqlJsConfig =
    abstract locateFile: url:string * scriptDirectory: string -> string

type BindParams =
    U2<ResizeArray<SqlValue>, ParamsObject> option

type [<AllowNullLiteral>] QueryExecResult =
    abstract columns: ResizeArray<string> with get, set
    abstract values: ResizeArray<ResizeArray<SqlValue>> with get, set

type [<AllowNullLiteral>] StatementIteratorResult =
    /// `true` if there are no more available statements
    abstract ``done``: bool with get, set
    /// the next available Statement (as returned by `Database.prepare`)
    abstract value: Statement with get, set

type [<AllowNullLiteral>] SqlJsStatic =
    abstract Database: Database with get, set
    abstract Statement: obj with get, set
    abstract FS: obj with get, set
    
    abstract register_for_idb: obj -> unit

type [<AllowNullLiteral>] InitSqlJsStatic = 
    [<Emit "$0($1...)">] abstract Invoke: ?config: SqlJsConfig -> Promise<SqlJsStatic>

type [<AllowNullLiteral>] Database =
    /// Close the database, and all associated prepared statements. The
    /// memory associated to the database and all associated statements will
    /// be freed.
    /// 
    /// **Warning**: A statement belonging to a database that has been closed
    /// cannot be used anymore.
    /// 
    /// Databases must be closed when you're finished with them, or the
    /// memory consumption will grow forever
    abstract close: unit -> unit
    /// <summary>Register a custom function with SQLite</summary>
    /// <param name="name">the name of the function as referenced in SQL statements.</param>
    /// <param name="func">the actual function to be executed.</param>
    abstract create_function: name: string * func: (ResizeArray<obj option> -> obj option) -> Database
    /// <summary>Execute an sql statement, and call a callback for each row of result.
    /// 
    /// Currently this method is synchronous, it will not return until the
    /// callback has been called on every row of the result. But this might
    /// change.</summary>
    /// <param name="sql">A string of SQL text. Can contain placeholders that will
    /// be bound to the parameters given as the second argument</param>
    /// <param name="params">Parameters to bind to the query</param>
    /// <param name="callback">Function to call on each row of result</param>
    /// <param name="done">A function that will be called when all rows have been
    /// retrieved</param>
    abstract each: sql: string * ``params``: BindParams * callback: ParamsCallback * ``done``: (unit -> unit) -> Database
    abstract each: sql: string * callback: ParamsCallback * ``done``: (unit -> unit) -> Database
    /// <summary>Execute an SQL query, and returns the result.
    /// 
    /// This is a wrapper against `Database.prepare`, `Statement.bind`, `Statement.step`, `Statement.get`, and `Statement.free`.
    /// 
    /// The result is an array of result elements. There are as many result elements as the number of statements in your sql string (statements are separated by a semicolon)</summary>
    /// <param name="sql">a string containing some SQL text to execute</param>
    /// <param name="params">When the SQL statement contains placeholders, you can
    /// pass them in here. They will be bound to the statement before it is
    /// executed. If you use the params argument as an array, you **cannot**
    /// provide an sql string that contains several statements (separated by
    /// `;`). This limitation does not apply to params as an object.</param>
    abstract exec: sql: string * ?``params``: BindParams -> ResizeArray<QueryExecResult>
    /// Exports the contents of the database to a binary array
    abstract export: unit -> Uint8Array
    /// Returns the number of changed rows (modified, inserted or deleted) by
    /// the latest completed `INSERT`, `UPDATE` or `DELETE` statement on the
    /// database. Executing any other type of SQL statement does not modify
    /// the value returned by this function.
    abstract getRowsModified: unit -> float
    /// Analyze a result code, return null if no error occured, and throw an
    /// error with a descriptive message otherwise
    abstract handleError: unit -> obj option
    /// <summary>Iterate over multiple SQL statements in a SQL string. This function
    /// returns an iterator over Statement objects. You can use a `for..of`
    /// loop to execute the returned statements one by one.</summary>
    /// <param name="sql">a string of SQL that can contain multiple statements</param>
    abstract iterateStatements: sql: string -> StatementIterator
    /// <summary>Prepare an SQL statement</summary>
    /// <param name="sql">a string of SQL, that can contain placeholders (`?`, `:VVV`, `:AAA`, `@AAA`)</param>
    /// <param name="params">values to bind to placeholders</param>
    abstract prepare: sql: string * ?``params``: BindParams -> Statement
    /// <summary>Execute an SQL query, ignoring the rows it returns.</summary>
    /// <param name="sql">a string containing some SQL text to execute</param>
    /// <param name="params">When the SQL statement contains placeholders, you can
    /// pass them in here. They will be bound to the statement before it is
    /// executed. If you use the params argument as an array, you **cannot**
    /// provide an sql string that contains several statements (separated by
    /// `;`). This limitation does not apply to params as an object.</param>
    abstract run: sql: string * ?``params``: BindParams -> Database

type [<AllowNullLiteral>] DatabaseStatic =
    /// <summary>Represents an SQLite database</summary>
    /// <param name="data">An array of bytes representing an SQLite database file</param>
    [<Emit "new $0($1...)">] abstract Create: ?data: U2<float array, Buffer> -> Database

type [<AllowNullLiteral>] Statement =
    /// <summary>Bind values to the parameters, after having reseted the statement. If
    /// values is null, do nothing and return true.
    /// 
    /// SQL statements can have parameters, named '?', '?NNN', ':VVV',
    /// '@VVV', '$VVV', where NNN is a number and VVV a string. This function
    /// binds these parameters to the given values.
    /// 
    /// Warning: ':', '@', and '$' are included in the parameters names
    /// 
    /// ### Value types
    /// 
    /// |Javascript type|SQLite type|
    /// |-|-|
    /// |number|REAL, INTEGER|
    /// |boolean|INTEGER|
    /// |string|TEXT|
    /// |Array, Uint8Array|BLOB|
    /// |null|NULL|</summary>
    /// <param name="values">The values to bind</param>
    abstract bind: ?values: BindParams -> bool
    /// Free the memory used by the statement
    abstract free: unit -> bool
    /// Free the memory allocated during parameter binding
    abstract freemem: unit -> unit
    /// <summary>Get one row of results of a statement. If the first parameter is not
    /// provided, step must have been called before.</summary>
    /// <param name="params">If set, the values will be bound to the statement
    /// before it is executed</param>
    abstract get: ?``params``: BindParams -> ResizeArray<SqlValue>
    /// <summary>Get one row of result as a javascript object, associating column
    /// names with their value in the current row</summary>
    /// <param name="params">If set, the values will be bound to the statement, and
    /// it will be executed</param>
    abstract getAsObject: ?``params``: BindParams -> ParamsObject
    /// Get the list of column names of a row of result of a statement.
    abstract getColumnNames: unit -> ResizeArray<string>
    /// Get the SQLite's normalized version of the SQL string used in
    /// preparing this statement. The meaning of "normalized" is not
    /// well-defined: see
    /// [the SQLite documentation](https://sqlite.org/c3ref/expanded_sql.html).
    abstract getNormalizedSQL: unit -> string
    /// Get the SQL string used in preparing this statement.
    abstract getSQL: unit -> string
    /// Reset a statement, so that it's parameters can be bound to new
    /// values. It also clears all previous bindings, freeing the memory used
    /// by bound parameters.
    abstract reset: unit -> unit
    /// <summary>Shorthand for bind + step + reset Bind the values, execute the
    /// statement, ignoring the rows it returns, and resets it</summary>
    /// <param name="values">Value to bind to the statement</param>
    abstract run: ?values: BindParams -> unit
    /// Execute the statement, fetching the the next line of result, that can
    /// be retrieved with `Statement.get`.
    abstract step: unit -> bool

type [<AllowNullLiteral>] StatementStatic =
    [<Emit "new $0($1...)">] abstract Create: unit -> Statement

/// An iterator over multiple SQL statements in a string, preparing and
/// returning a Statement object for the next SQL statement on each
/// iteration.
/// 
/// You can't instantiate this class directly, you have to use a Database
/// object in order to create a statement iterator
type [<AllowNullLiteral>] StatementIterator =
//    inherit Iterator<Statement>
//    inherit Iterable<Statement>
//    abstract ``[Symbol.iterator]``: unit -> Iterator<Statement>
    /// Get any un-executed portions remaining of the original SQL string
    abstract getRemainingSql: unit -> string
    /// Prepare the next available SQL statement
    abstract next: unit -> StatementIteratorResult

/// An iterator over multiple SQL statements in a string, preparing and
/// returning a Statement object for the next SQL statement on each
/// iteration.
/// 
/// You can't instantiate this class directly, you have to use a Database
/// object in order to create a statement iterator
type [<AllowNullLiteral>] StatementIteratorStatic =
    [<Emit "new $0($1...)">] abstract Create: unit -> StatementIterator
