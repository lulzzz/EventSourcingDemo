﻿module DesignerApp

open Fable.Core
open Fable.Import
open Elmish
open Elmish.React

module C = Container

let runnable =
    Program.mkSimple C.init C.update C.view
    |> Program.withConsoleTrace
    |> Program.toHtml Program.run "elmish-app"

    // Program.mkProgram C.init C.update C.view
    // |> Program.withConsoleTrace
    // |> Program.run
    