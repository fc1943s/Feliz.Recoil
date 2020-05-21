﻿namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum(CaseRules.None);RequireQualifiedAccess>]
type RecoilValueTag =
    | Writeable
    | Readonly

/// Marker interface to designate read only RecoilValues.
type ReadOnly = interface end

/// Marker interface to designate mutable RecoilValues.
type ReadWrite = interface end

[<Erase>]
type RecoilValue<'T,'ReadPerm> =
    [<Emit("$0.key")>]
    member _.key : string = jsNative
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.tag")>]
    member _.tag : RecoilValueTag = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
type DefaultValue = interface end

/// Methods provided in selectors for reading RecoilValues.
[<Erase>]
type SelectorGetter =
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T,ReadOnly>) : 'T = jsNative
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T,ReadWrite>) : 'T = jsNative

/// Methods provided in selectors for composing new RecoilValues.
[<Erase>]
type SelectorMethods =
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T,ReadOnly>) : 'T = jsNative
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T,ReadWrite>) : 'T = jsNative

    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T -> 'T) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T -> DefaultValue) : unit = jsNative

    /// Sets the value of a RecoilValue back to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,ReadWrite>) : unit = jsNative

[<AutoOpen;Erase>]
module SelectorMagic =
    type SelectorMethods with
        /// Sets the value of a RecoilValue.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T) : unit = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum;RequireQualifiedAccess>]
type LoadableStateStr =
    | HasValue
    | HasError
    | Loading

/// Represents the current possible state of a Loadable.
type LoadableState<'T> =
    | HasValue of 'T
    | HasError of exn
    | Loading of JS.Promise<'T>

[<EditorBrowsable(EditorBrowsableState.Never)>]
type ResolvedLoadablePromiseInfo<'T> =
    abstract value: 'T

/// A RecoilValue that may not yet be resolved to a value.
[<Erase>]
type Loadable<'T> =
    /// Tries to get the async operation of a Loadable.
    member inline this.asyncMaybe () =
        this.promiseMaybe() |> Option.map Async.AwaitPromise
    /// Gets the async operation of a Loadable or throws.
    member inline this.asyncOrThrow () =
        this.promiseOrThrow() |> Async.AwaitPromise

    /// Tries to get the exception of a Loadable.
    [<Emit("$0.errorMaybe()")>]
    member _.errorMaybe () : exn option = jsNative

    /// Gets the exception of a Loadable or throws.
    [<Emit("$0.errorOrThrow()")>]
    member _.errorOrThrow () : exn = jsNative

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.getValue()")>]
    member _.getValue' () : 'T = jsNative
    /// Gets the result of a Loadable.
    member inline this.getValue () = 
        try this.getValue'() |> Ok
        with e -> Error e

    /// Maps the value of a Loadable.
    [<Emit("$0.map($1)")>]
    member _.map (mapping: 'T -> JS.Promise<'U>) : Loadable<'U> = jsNative
    /// Maps the value of a Loadable.
    member inline this.map (mapping: 'T -> Async<'U>) =
        let test = fun o -> o |> mapping |> Async.StartAsPromise
        this.map(test)
    
    /// Tries to get the promise of a Loadable.
    [<Emit("$0.promiseMaybe()")>]
    member _.promiseMaybe () : JS.Promise<'T> option = jsNative

    /// Gets the promise of a Loadable or throws.
    [<Emit("$0.promiseOrThrow()")>]
    member _.promiseOrThrow () : JS.Promise<'T> = jsNative

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.state")>]
    member _.state' : LoadableStateStr = jsNative
    /// Gets the current state and corresponding value of a Loadable.
    member inline this.state () =
        match this.state' with
        | LoadableStateStr.Loading ->
            unbox<JS.Promise<ResolvedLoadablePromiseInfo<'T>>> this.contents
            |> Promise.map (fun o -> o.value)
            |> LoadableState.Loading 
        | LoadableStateStr.HasError ->
            LoadableState.HasError (unbox<exn> this.contents)
        | LoadableStateStr.HasValue ->
            LoadableState.HasValue (unbox<'T> this.contents)

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.contents")>]
    member _.contents : U3<'T,exn,JS.Promise<ResolvedLoadablePromiseInfo<'T>>> = jsNative

    /// Converts the Loadable to an async operation.
    member inline this.toAsync () =
        this.toPromise'() 
        |> Promise.map(fun o -> o?value)
        |> Async.AwaitPromise

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.toPromise()")>]
    member _.toPromise' () : JS.Promise<obj> = jsNative

    /// Converts the Loadable to a promise.
    member inline this.toPromise () =
        this.toPromise'()
        |> Promise.map(fun o -> o?value)

    /// Tries to get the value of a Loadable.
    [<Emit("$0.valueMaybe()")>]
    member _.valueMaybe () : 'T option = jsNative

    /// Gets the value of a Loadable or throws.
    [<Emit("$0.valueOrThrow()")>]
    member _.valueOrThrow () : 'T = jsNative

[<AutoOpen;Erase>]
module LoadableMagic =
    type Loadable<'T> with
        /// Maps the value of a Loadable.
        [<Emit("$0.map($1)")>]
        member _.map (mapping: 'T -> 'U) : Loadable<'U> = jsNative

[<Erase>]
type RootInitializer =
    /// Sets the initial value of a single atom to the provided value.
    [<Emit("$0.set($1, $2")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, currentValue: 'T) : unit = jsNative
    
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided map. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    [<Emit("$0.setUnvalidatedAtomValues($1")>]
    member _.setUnvalidatedAtomValues (atomValues: Map<string,'T>) : unit = jsNative
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided key-value list. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    member inline this.setUnvalidatedAtomValues (atomValues: (string * 'T) list) =
        this.setUnvalidatedAtomValues(Map.ofList atomValues)

[<Erase>]
type CallbackMethods =
    /// Gets the async operation of a RecoilValue.
    member inline this.getAsync (recoilValue: RecoilValue<'T,_>) = 
        this.getPromise(recoilValue) |> Async.AwaitPromise

    /// Gets the Loadable of a RecoilValue.
    [<Emit("$0.getLoadable($1)")>]
    member _.getLoadable (recoilValue: RecoilValue<'T,_>) : Loadable<'T> = jsNative

    /// Gets the promise of a RecoilValue.
    [<Emit("$0.getPromise($1)")>]
    member _.getPromise (recoilValue: RecoilValue<'T,_>) : JS.Promise<'T> = jsNative

    /// Sets a RecoilValue to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,ReadWrite>) : unit = jsNative

    /// Sets a RecoilValue using the updater function.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, updater: 'T -> 'T) : unit = jsNative

[<AutoOpen;Erase>]
module CallbackMagic =
    type CallbackMethods with
        /// Sets a RecoilValue to the given value.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T) : unit = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum;RequireQualifiedAccess>]
type PersistenceTypeWithNone =
    | Log
    | None
    | Url

[<StringEnum;RequireQualifiedAccess>]
type PersistenceType =
    | Log
    | Url

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member inline fromTypeWIthNone (pType: PersistenceTypeWithNone) =
        match pType with
        | PersistenceTypeWithNone.None -> None
        | PersistenceTypeWithNone.Log -> Some PersistenceType.Log
        | PersistenceTypeWithNone.Url -> Some PersistenceType.Url

[<Erase>]
type PersistenceInfo =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.type")>]
    member _.typeInner : PersistenceTypeWithNone = jsNative
    
    member inline this.type' =
        PersistenceType.fromTypeWIthNone(this.typeInner)

    [<Emit("$0.backButton")>]
    member _.backButton : bool = jsNative

[<NoComparison;NoEquality>]
type PersistenceSettings<'T,'U> =
    { Type: PersistenceType
      Backbutton: bool option 
      Validator: ('U -> 'T option) }

    static member inline CreateObj (settings: PersistenceSettings<'T,'U>) =
        [ "type" ==> settings.Type
          if settings.Backbutton.IsSome then
              "backbutton" ==> settings.Backbutton.Value
          "validator" ==> 
              (fun (v: 'U, def: DefaultValue) ->
                  match settings.Validator(v) with
                  | Some res -> box res
                  | None -> box def) ]
        |> createObj

[<Erase>]
type AtomInfo =
    [<Emit("$0.persistence_UNSTABLE")>]
    member _.persistence : PersistenceInfo = jsNative

[<Erase>]
type TransactionObservation<'Values,'Metadata> =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.atomValues")>]
    member _.atomValues' : JS.Map<string,'Values> = jsNative
    /// The current value of every atom that is both persistable (persistence
    /// type not set to 'none') and whose value is available (not in an
    /// error or loading state).
    member inline this.atomValues = 
        this.atomValues'.entries() |> Map.ofSeq
        
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.previousAtomValues")>]
    member _.previousAtomValues' : JS.Map<string,'Values> = jsNative
    /// The value of every persistable and available atom before
    /// the transaction began.
    member inline this.previousAtomValues = 
        this.previousAtomValues'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.atomInfo")>]
    member _.atomInfo' : JS.Map<string,AtomInfo> = jsNative
    /// A map containing the persistence settings for each atom. Every key
    /// that exists in atomValues will also exist in atomInfo.
    member inline this.atomInfo = 
        this.atomInfo'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.modifiedAtoms")>]
    member _.modifiedAtoms' : JS.Set<string> = jsNative
    /// The set of atoms that were written to during the transaction.
    member inline this.modifiedAtoms = 
        this.modifiedAtoms'.entries() |> Set.ofSeq

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("Object.entries($0.transactionMetadata)")>]
    member _.transactionMetadata' : ResizeArray<string * 'Metadata> = jsNative
    /// Arbitrary information that was added via the
    /// useSetUnvalidatedAtomValues hook. Useful for ignoring the useSetUnvalidatedAtomValues
    /// transaction, to avoid loops.
    member inline this.getTransactionMetadata () =
        this.transactionMetadata'
        |> Map.ofSeq

type StoreSubscriber =
    [<Emit("$0.release()")>]
    member _.release () : unit = jsNative

[<StringEnum;RequireQualifiedAccess>]
type FireNodeWhen =
    | Enqueue
    | Now

[<Erase>]
type TreeState<'T> =
    // Information about the TreeState itself:

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.isSnapshot")>]
    member _.isSnapshot : bool = jsNative
    
    [<Emit("$0.transactionMetadata")>]
    member _.transactionMetadata : 'T = jsNative
    
    // ATOMS
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.dirtyAtoms")>]
    member _.dirtyAtoms' : JS.Set<string> = jsNative
    member inline this.dirtyAtoms = 
        this.dirtyAtoms'.entries() |> Set.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.atomValues")>]
    member _.atomValues' : JS.Map<string, Loadable<_>> = jsNative
    member inline this.atomValues = 
        this.atomValues'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.nonValidatedAtoms")>]
    member _.nonValidatedAtoms' : JS.Map<string, obj> = jsNative
    member inline this.nonValidatedAtoms = 
        this.nonValidatedAtoms'.entries() |> Map.ofSeq
    
    // NODE GRAPH -- will soon move to StoreState
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.nodeDeps")>]
    member _.nodeDeps' : JS.Map<string, Set<string>> = jsNative
    /// Upstream Node dependencies
    member inline this.nodeDeps = 
        this.nodeDeps'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.nodeToNodeSubscriptions")>]
    member _.nodeToNodeSubscriptions' : JS.Map<string, Set<string>> = jsNative
    /// Downstream Node subscriptions.
    member inline this.nodeToNodeSubscriptions = 
        this.nodeToNodeSubscriptions'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.nodeToComponentSubscriptions")>]
    member _.nodeToComponentSubscriptions' : JS.Map<string, Map<int, string * (TreeState<'T> -> unit)>> = jsNative
    member inline this.nodeToComponentSubscriptions = 
        this.nodeToComponentSubscriptions'.entries() |> Map.ofSeq

/// StoreState represents the state of a Recoil context. 
///
/// It is global and mutable.
///
/// It is updated only during effects, except that the nextTree property is updated
/// when atom values change and async requests resolve, and suspendedComponentResolvers
/// is updated when components are suspended.
[<Erase>]
type StoreState<'T> =
    /// The "current" TreeState being either directly read from (legacy) or passed
    /// to useMutableSource (when in use). It is replaced with nextTree when
    /// a transaction is completed or async request finishes.
    [<Emit("$0.currentTree")>]
    member _.currentTree : TreeState<'T> = jsNative

    /// The TreeState that is written to when during the course of a transaction
    /// (generally equal to a React batch) when atom values are updated.
    [<Emit("$0.nextTree")>]
    member _.nextTree : TreeState<'T> option = jsNative

    /// For observing transactions.
    [<Emit("$0.+transactionSubscriptions")>]
    member _.transactionSubscriptions' : JS.Map<int, Store<'T> * TreeState<'T> -> unit> = jsNative
    member inline this.transactionSubscriptions = 
        this.transactionSubscriptions'.entries() |> Map.ofSeq

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.+queuedComponentCallback")>]
    member _.queuedComponentCallback' : ResizeArray<TreeState<'T> -> unit> = jsNative
    /// Callbacks to render external components that are subscribed to nodes,
    /// these are executed at the end of the transaction or asynchronously.
    member inline this.queuedComponentCallback =
        this.queuedComponentCallback' |> List.ofSeq

    /// Promise resolvers to wake any components we suspended with React Suspense.
    [<Emit("$0.+suspendedComponentResolvers")>]
    member _.suspendedComponentResolvers : JS.Set<unit -> unit> = jsNative

and Store<'T> =
    [<Emit("$0.getState()")>]
    member _.getState () : StoreState<'T> = jsNative

    [<Emit("$0.replaceState($1)")>]
    member _.replaceState (map: TreeState<'T> -> TreeState<'T>) : unit = jsNative
    
    [<Emit("$0.subscribeToTransactions($1)")>]
    member _.subscribeToTransactions (subscriber: (Store<'T> * TreeState<'T>) -> unit) : StoreSubscriber = jsNative
    
    [<Emit("$0.addTransactionMetadata($1)")>]
    member _.addTransactionMetadata (metadata: 'T) : unit = jsNative
    
    [<Emit("$0.fireNodeSubscriptions($1, $2)")>]
    member _.fireNodeSubscriptions (updatedNodes: Set<string>, when': FireNodeWhen) : unit = jsNative

type CacheImplementation<'T> =
    [<Emit("$0.+get($1)")>]
    abstract get: 'U -> U2<Loadable<'T>,unit>
    [<Emit("$0.+set($1)")>]
    abstract set: ('U * Loadable<'T>) -> CacheImplementation<'T>