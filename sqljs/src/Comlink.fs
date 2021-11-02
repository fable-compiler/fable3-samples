// ts2fable 0.7.1
module rec Comlink
open System
open Fable.Core
open Fable.Core.JS

type Symbol = obj


let [<Import("proxyMarker","comlink")>] proxyMarker: Symbol = jsNative
let [<Import("createEndpoint","comlink")>] createEndpoint: Symbol = jsNative
let [<Import("releaseProxy","comlink")>] releaseProxy: Symbol = jsNative
let [<Import("transferHandlers","comlink")>] transferHandlers: Map<string, TransferHandler<obj, obj>> = jsNative


let [<Import("wrap","comlink")>] wrap(ep: Protocol.Endpoint): Remote<'T> = jsNative
let [<Import("expose","comlink")>] expose(obj: obj): unit = jsNative
let [<Import("transfer","comlink")>] transfer(obj:'T, transfers: ResizeArray<ArrayBuffer>): 'T = jsNative
let [<Import("proxy","comlink")>] proxy(obj:'T): obj = jsNative
let [<Import("windowEndpoint","comlink")>] windowEndpoint(w:Protocol.PostMessageWithOrigin): Protocol.Endpoint = jsNative

//type [<AllowNullLiteral>] IExports =
//    abstract expose: obj: obj option * ?ep: Protocol.Endpoint -> unit
//    abstract wrap: ep: Protocol.Endpoint * ?target: obj -> Remote<'T>
//    abstract transfer: obj: 'T * transfers: ResizeArray<ArrayBuffer> -> 'T
//    abstract proxy: obj: 'T -> obj
//    abstract windowEndpoint: w: Protocol.PostMessageWithOrigin * ?context: Protocol.EventSource * ?targetOrigin: string -> Protocol.Endpoint

/// Interface of values that were marked to be proxied with `comlink.proxy()`.
/// Can also be implemented by classes.
type [<AllowNullLiteral>] ProxyMarked =
    abstract ``[proxyMarker]``: obj with get, set

//type Promisify<'T> =
//    obj
//
//type Unpromisify<'P> =
//    obj
//
//type RemoteProperty<'T> =
//    obj
//
//type LocalProperty<'T> =
//    obj
//
//type ProxyOrClone<'T> =
//    obj
//
//type UnproxyOrClone<'T> =
//    obj

type [<AllowNullLiteral>] RemoteObject<'T> =
    interface end

type [<AllowNullLiteral>] LocalObject<'T> =
    interface end

/// Additional special comlink methods available on each proxy returned by `Comlink.wrap()`.
type [<AllowNullLiteral>] ProxyMethods =
//    abstract ``[createEndpoint]``: (unit -> Promise<MessagePo>) with get, set
    abstract ``[releaseProxy]``: (unit -> unit) with get, set

type Remote<'T> =
    'T

type MaybePromise<'T> =
    U2<Promise<'T>, 'T>

type [<AllowNullLiteral>] Local<'T> =
    interface end

/// Customizes the serialization of certain values as determined by `canHandle()`.
type [<AllowNullLiteral>] TransferHandler<'T, 'S> =
    /// Gets called for every value to determine whether this transfer handler
    /// should serialize the value, which includes checking that it is of the right
    /// type (but can perform checks beyond that as well).
    abstract canHandle: value: obj -> bool
    /// Gets called with the value if `canHandle()` returned `true` to produce a
    /// value that can be sent in a message, consisting of structured-cloneable
    /// values and/or transferrable objects.
    abstract serialize: value: 'T -> 'S * ResizeArray<ArrayBuffer>
    /// Gets called to deserialize an incoming value that was serialized in the
    /// other thread with this transfer handler (known through the name it was
    /// registered under).
    abstract deserialize: value: 'S -> 'T

module Node_adapter =
    type Endpoint = Protocol.Endpoint

    type [<AllowNullLiteral>] IExports =
        abstract nodeEndpoint: nep: NodeEndpoint -> Endpoint

    type [<AllowNullLiteral>] NodeEndpoint =
        abstract postMessage: message: obj option * ?transfer: ResizeArray<obj option> -> unit
//        abstract on: ``type``: string * listener: EventListenerOrEventListenerObject * ?options: NodeEndpointOnOptions -> unit
//        abstract off: ``type``: string * listener: EventListenerOrEventListenerObject * ?options: NodeEndpointOffOptions -> unit
        abstract start: (unit -> unit) option with get, set

    type [<AllowNullLiteral>] NodeEndpointOnOptions =
        interface end

    type [<AllowNullLiteral>] NodeEndpointOffOptions =
        interface end

module Protocol =

    /// Copyright 2019 Google Inc. All Rights Reserved.
    /// Licensed under the Apache License, Version 2.0 (the "License");
    /// you may not use this file except in compliance with the License.
    /// You may obtain a copy of the License at
    ///      http://www.apache.org/licenses/LICENSE-2.0
    /// Unless required by applicable law or agreed to in writing, software
    /// distributed under the License is distributed on an "AS IS" BASIS,
    /// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    /// See the License for the specific language governing permissions and
    /// limitations under the License.
    type [<AllowNullLiteral>] EventSource =
        abstract addEventListener: ``type``: string * listener: obj * ?options: EventSourceAddEventListenerOptions -> unit
        abstract removeEventListener: ``type``: string * listener: obj * ?options: EventSourceRemoveEventListenerOptions -> unit

    type [<AllowNullLiteral>] EventSourceAddEventListenerOptions =
        interface end

    type [<AllowNullLiteral>] EventSourceRemoveEventListenerOptions =
        interface end

    type [<AllowNullLiteral>] PostMessageWithOrigin =
        abstract postMessage: message: obj option * targetOrigin: string * ?transfer: ResizeArray<ArrayBuffer> -> unit

    type [<AllowNullLiteral>] Endpoint =
        inherit EventSource
        abstract postMessage: message: obj option * ?transfer: ResizeArray<ArrayBuffer> -> unit
        abstract start: (unit -> unit) option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] WireValueType =
        | [<CompiledName "RAW">] RAW
        | [<CompiledName "PROXY">] PROXY
        | [<CompiledName "THROW">] THROW
        | [<CompiledName "HANDLER">] HANDLER

    type [<AllowNullLiteral>] RawWireValue =
        abstract id: string option with get, set
        abstract ``type``: WireValueType with get, set
        abstract value: RawWireValueValue with get, set

    type [<AllowNullLiteral>] HandlerWireValue =
        abstract id: string option with get, set
        abstract ``type``: WireValueType with get, set
        abstract name: string with get, set
        abstract value: obj with get, set

    type WireValue =
        U2<RawWireValue, HandlerWireValue>

    type MessageID =
        string

    type [<StringEnum>] [<RequireQualifiedAccess>] MessageType =
        | [<CompiledName "GET">] GET
        | [<CompiledName "SET">] SET
        | [<CompiledName "APPLY">] APPLY
        | [<CompiledName "CONSTRUCT">] CONSTRUCT
        | [<CompiledName "ENDPOINT">] ENDPOINT
        | [<CompiledName "RELEASE">] RELEASE

    type [<AllowNullLiteral>] GetMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set
        abstract path: ResizeArray<string> with get, set

    type [<AllowNullLiteral>] SetMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set
        abstract path: ResizeArray<string> with get, set
        abstract value: WireValue with get, set

    type [<AllowNullLiteral>] ApplyMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set
        abstract path: ResizeArray<string> with get, set
        abstract argumentList: ResizeArray<WireValue> with get, set

    type [<AllowNullLiteral>] ConstructMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set
        abstract path: ResizeArray<string> with get, set
        abstract argumentList: ResizeArray<WireValue> with get, set

    type [<AllowNullLiteral>] EndpointMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set

    type [<AllowNullLiteral>] ReleaseMessage =
        abstract id: MessageID option with get, set
        abstract ``type``: MessageType with get, set
        abstract path: ResizeArray<string> with get, set

    type Message =
        U6<GetMessage, SetMessage, ApplyMessage, ConstructMessage, EndpointMessage, ReleaseMessage>

    type [<AllowNullLiteral>] RawWireValueValue =
        interface end
