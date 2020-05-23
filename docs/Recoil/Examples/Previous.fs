﻿[<RequireQualifiedAccess>]
module Samples.Previous

open Css
open Feliz
open Feliz.Recoil
open Zanaptak

let textState = Recoil.atom("textState", "Hello world!")

let inner = React.functionComponent(fun () ->
    let text,setText = Recoil.useStatePrev(textState)

    Html.div [
        Html.div [
            prop.text (sprintf "Atom current value: %s" text)
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.onTextChange (fun s -> setText(fun current -> current + s))
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        inner()
    ])