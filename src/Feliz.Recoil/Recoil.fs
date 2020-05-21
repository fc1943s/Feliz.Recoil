namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open Feliz

[<RequireQualifiedAccess>]
type Recoil =
    /// Creates a RecoilValue with a default value of what the 
    /// promise resolves to.
    static member inline atom (key: string, defaultValue: JS.Promise<'T>, ?persistence: PersistenceSettings<'T,'U>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue
              if persistence.IsSome then
                   "persistence_UNSTABLE" ==> PersistenceSettings.CreateObj(persistence.Value) ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value of what the 
    /// async returns.
    static member inline atom (key: string, defaultValue: Async<'T>, ?persistence: PersistenceSettings<'T,'U>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> (defaultValue |> Async.StartAsPromise)
              if persistence.IsSome then
                   "persistence_UNSTABLE" ==> PersistenceSettings.CreateObj(persistence.Value) ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value the given RecoilValue.
    static member inline atom (key: string, defaultValue: RecoilValue<'T,_>, ?persistence: PersistenceSettings<'T,'U>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue
              if persistence.IsSome then
                   "persistence_UNSTABLE" ==> PersistenceSettings.CreateObj(persistence.Value) ]
            |> createObj
        )

    /// Used in selectors to get the RecoilValue's default value or to 
    /// set the RecoilValue to the default value.
    static member inline defaultValue = Bindings.Recoil.defaultValue.Create()

    /// Provides the context in which atoms have values. 
    /// 
    /// Must be an ancestor of any component that uses any Recoil hooks. 
    /// 
    /// Multiple roots may co-exist; atoms will have distinct values 
    /// within each root. If they are nested, the innermost root will 
    /// completely mask any outer roots.
    static member inline root (children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj ["children" ==> Interop.reactApi.Children.toArray children])
    /// Provides the context in which atoms have values. 
    /// 
    /// Must be an ancestor of any component that uses any Recoil hooks. 
    /// 
    /// Multiple roots may co-exist; atoms will have distinct values 
    /// within each root. If they are nested, the innermost root will 
    /// completely mask any outer roots.
    ///
    /// The initilizer parameter is a function that will be called when 
    /// the root is first rendered, which can set initial values for atoms.
    static member inline root (initializer: RootInitializer -> unit, children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj [
            "props" ==> (createObj [ "initializeState" ==> System.Func<_,_,_>(fun o _  -> o?set |> initializer) ])
            "children" ==> Interop.reactApi.Children.toArray children
        ])

    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector (key: string, get: SelectorGetter -> JS.Promise<'U>, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> get
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector (key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set)
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector (key: string, get: SelectorGetter -> Async<'U>, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> (get >> Async.StartAsPromise)
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector (key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> (get >> Async.StartAsPromise)
              "set" ==> System.Func<_,_,_>(set)
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector (key: string, get: SelectorGetter -> RecoilValue<'U,_>, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> get
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector (key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U>) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set)
              if cacheImplementation.IsSome then
                "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
            |> createObj
        )

    /// Creates a callback function that allows for fetching values of RecoilValue(s).
    static member inline useCallback (f: (CallbackMethods -> 'T -> 'U), ?deps: obj []) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f), ?deps = (deps |> Option.map ResizeArray))

    /// Creates a callback function that allows for fetching values of RecoilValue(s),
    /// but will always stay up-to-date with the required depencencies and reduce re-renders.
    ///
    /// This should *not* be used when the callback determines the result of the render.
    static member inline useCallbackRef (f: (CallbackMethods -> 'T -> 'U)) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f))
        |> React.useCallbackRef

    /// Returns a function that will reset the value of a RecoilValue to its default.
    static member inline useResetState (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useResetRecoilState<'T>(recoilValue)

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// If the RecoilValue is pending, this will suspend the compoment and initiate the
    /// retrieval of the value. If evaluating the RecoilValue resulted in an error, this will
    /// throw the error so that the nearest React error boundary can catch it.
    static member inline useState (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// Returns a Loadable which can indicate whether the RecoilValue is available, pending, or
    /// unavailable due to an error.
    static member inline useStateLoadable (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// Returns a Loadable which can indicate whether the RecoilValue is available, pending, or
    /// unavailable due to an error.
    ///
    /// The setter function takes a function that takes the current value and returns 
    /// the new one.
    static member inline useStateLoadablePrev (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * (('T -> 'T) -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// The setter function takes a function that takes the current value and returns 
    /// the new one.
    static member inline useStatePrev (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * (('T -> 'T) -> unit)>

    /// Returns a function that allows the value of a RecoilValue to be updated, but does
    /// not subscribe the compoment to changes to that RecoilValue.
    static member inline useSetState (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> unit>

    /// Returns a function that allows the value of a RecoilValue to be updated via
    /// a function that accepts the current value and produces the new value.
    static member inline useSetStatePrev (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<('T -> 'T) -> unit>

    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided map. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    ///
    /// TransactionMetadata should should be a record or anonymous record mapping
    /// atom/selector keys to the data you want to set alongside them.
    static member inline useSetUnvalidatedAtomValues (values: Map<string, 'Value>, ?transactionMetadata: 'Metadata) =
        Bindings.Recoil.useSetUnvalidatedAtomValues (
            values |> Map.toJS, 
            ?transactionMetadata = (transactionMetadata |> Option.map toPlainJsObj)
        )
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided key-value list. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    ///
    /// TransactionMetadata should should be a record or anonymous record mapping
    /// atom/selector keys to the data you want to set alongside them.
    static member inline useSetUnvalidatedAtomValues (values: (string * 'Value) list, ?transactionMetadata: 'Metadata) =
        Bindings.Recoil.useSetUnvalidatedAtomValues (
            JS.Constructors.Map.Create(values), 
            ?transactionMetadata = (transactionMetadata |> Option.map toPlainJsObj)
        )

    /// Calls the given callback after any atoms have been modified and the consequent
    /// component re-renders have been committed. This is intended for persisting
    /// the values of the atoms to storage. The stored values can then be restored
    /// using the useSetUnvalidatedAtomValues hook.
    ///
    /// The callback receives the following info:
    /// 
    /// atomValues: 
    /// The current value of every atom that is both persistable (persistence
    /// type not set to 'none') and whose value is available (not in an
    /// error or loading state).
    ///
    /// previousAtomValues: 
    /// The value of every persistable and available atom before the transaction began.
    ///
    /// atomInfo: 
    /// A map containing the persistence settings for each atom. Every key
    /// that exists in atomValues will also exist in atomInfo.
    ///
    /// modifiedAtoms: The set of atoms that were written to during the transaction.
    ///
    /// transactionMetadata: 
    /// Arbitrary information that was added via the useSetUnvalidatedAtomValues hook. 
    /// 
    /// Useful for ignoring the useSetUnvalidatedAtomValues transaction, to avoid loops.
    static member inline useTransactionObservation (callback: TransactionObservation<'Values,'Metadata> -> unit) =
        Bindings.Recoil.useTransactionObservation(callback)

    /// Subscribes to the store.
    static member inline useTransactionSubscription (callback: Store<'T> * TreeState<'T> -> 'U) =
        Bindings.Recoil.useTransactionSubscription<'T,'U>(callback)

    /// Returns the value represented by the RecoilValue.
    /// 
    /// If the value is pending, it will throw a Promise to suspend the component.
    /// 
    /// If the value is an error, it will throw it for the nearest React error boundary.
    /// 
    /// This will also subscribe the component for any updates in the value.
    static member inline useValue (recoilValue: RecoilValue<'T,'Mode>) =
        Bindings.Recoil.useRecoilValue<'T,'Mode>(recoilValue)

    /// Returns the Loadable of a RecoilValue.
    ///
    /// This will also subscribe the component for any updates in the value.
    static member inline useValueLoadable (recoilValue: RecoilValue<'T,'Mode>) =
        Bindings.Recoil.useRecoilValueLoadable<'T,'Mode>(recoilValue)
        
//[<Erase;RequireQualifiedAccess>]
//module Recoil =
    //[<Erase>]
    //type Family =
    //    static member inline atom<'T,'P> (defaultValue: 'P -> 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> (defaultValue |> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> (defaultValue >> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> (defaultValue |> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> (defaultValue >> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

[<AutoOpen;Erase>]
module RecoilMagic =
    type Recoil with
        /// Creates a RecoilValue with the given default value.
        static member inline atom (key: string, defaultValue: 'T, ?persistence: PersistenceSettings<'T,'U>) =
            Bindings.Recoil.atom<'T> (
                [ "key" ==> key
                  "default" ==> defaultValue
                  if persistence.IsSome then
                       "persistence_UNSTABLE" ==> PersistenceSettings.CreateObj(persistence.Value) ]
                |> createObj
            )

        /// Derives state and returns a RecoilValue via the provided get function.
        static member inline selector (key: string, get: SelectorGetter -> 'U, ?cacheImplementation: CacheImplementation<'U>) =
            Bindings.Recoil.selector<'U,ReadOnly> (
                [ "key" ==> key
                  "get" ==> get
                  if cacheImplementation.IsSome then
                    "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
                |> createObj
            )
        /// Derives state and returns a RecoilValue via the provided get function.
        ///
        /// Applies state changes via the provided set function.
        static member inline selector (key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U>) =
            Bindings.Recoil.selector<'U,ReadWrite> (
                [ "key" ==> key
                  "get" ==> get
                  "set" ==> System.Func<_,_,_>(set)
                  if cacheImplementation.IsSome then
                    "cacheImplementation_UNSTABLE" ==> cacheImplementation.Value  ]
                |> createObj
            )

        /// Creates a callback function that allows for fetching values of RecoilValue(s).
        static member inline useCallback (f: (CallbackMethods -> 'U), ?deps: obj []) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f), ?deps = (deps |> Option.map ResizeArray))

        /// Creates a callback function that allows for fetching values of RecoilValue(s),
        /// but will always stay up-to-date with the required depencencies and reduce re-renders.
        static member inline useCallbackRef (f: (CallbackMethods -> 'U)) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f))
            |> React.useCallbackRef

    //type Recoil.Family with
    //    static member inline atom<'T,'P> (defaultValue: 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )
