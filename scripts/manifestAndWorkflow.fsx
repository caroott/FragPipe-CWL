#r "nuget: ARCtrl.NET, 2.0.2"
#r "nuget: ARCtrl.QueryModel, 2.0.2"

open System.IO
open ARCtrl
open ARCtrl.NET
open ARCtrl.QueryModel



let rec searchForFile directory fileName =
    try
        let files = Directory.GetDirectories(directory, fileName)
        if files.Length > 0 then
            Some (Directory.GetParent files.[0]).FullName
        else
            Directory.GetDirectories(directory)
            |> Array.tryPick (fun subDir -> printfn "subdir: %s" subDir;searchForFile subDir fileName)
    with
    | _ -> None

let args: string array = fsi.CommandLineArgs |> Array.tail

let runName = args.[0]

let assay = args.[1]
let arcPath = @"./arc"

let assayFolder = $"./arc/assays/{assay}/dataset"

let experimentColumn, experimentColumnType = args.[2].Split ","|> fun x -> x.[0].Trim(),x.[1].Trim()

let replicateColumn, replicateColumnType = args.[3].Split ","|> fun x -> x.[0].Trim(),x.[1].Trim()

let acquisitionColumn, acquisitionColumnType = args.[4].Split ","|> fun x -> x.[0].Trim(),x.[1].Trim()

let fastAPath = args.[5]

let ddaOnly = 
    match args.[6] with
    | "TRUE" -> true
    | "FALSE" -> false
    | _ -> failwith "invalid argument for dda only"

let arc = ARC.load arcPath
let i = arc.ISA.Value

let tables = i.ArcTables

let matchIsaValue (iv: ISAValue) =
    match iv.TryValueText with
    | Some t -> t
    | None -> ""
    
let getExperimentColumn (file:string) =
    match experimentColumnType with
    | "Parameter" -> (tables).ParametersOf(file).WithName(experimentColumn).[0] |> matchIsaValue
    | "Characteristic" -> (tables).CharacteristicsOf(file).WithName(experimentColumn).[0] |> matchIsaValue
    | "Factor" -> (tables).FactorsOf(file).WithName(experimentColumn).[0] |> matchIsaValue
    | _ -> failwith $"{tables} not supported"

let getReplicateColumn (file:string) =
    match replicateColumnType with
    | "Parameter" -> (tables).ParametersOf(file).WithName(replicateColumn).[0] |> matchIsaValue
    | "Characteristic" -> (tables).CharacteristicsOf(file).WithName(replicateColumn).[0] |> matchIsaValue
    | "Factor" -> (tables).FactorsOf(file).WithName(replicateColumn).[0] |> matchIsaValue
    | _ -> failwith $"{tables} not supported"

let getAcquisitionColumn (file:string) =
    match acquisitionColumnType with
    | "Parameter" -> (tables).ParametersOf(file).WithName(acquisitionColumn).[0] |> matchIsaValue
    | "Characteristic" -> (tables).CharacteristicsOf(file).WithName(acquisitionColumn).[0] |> matchIsaValue
    | "Factor" -> (tables).FactorsOf(file).WithName(acquisitionColumn).[0] |> matchIsaValue
    | _ -> failwith $"{tables} not supported"


let outputTable = i.GetAssay(assay).GetTable "MassSpectrometry"
let outputFiles =
    outputTable.GetColumn(outputTable.ColumnCount - 1).Cells
    |> Array.map (fun x -> 
        match x.AsData.FilePath with
        | Some v -> v
        | None -> failwithf "%A" x
    )

let fileLocations =
    outputFiles
    |> Array.map (fun name ->
        printfn "now searching: %s" name
        let parentDir =
            searchForFile assayFolder name
        $"{parentDir.Value}/{name}"
    )

let experiments =
    outputFiles
    |> Array.map (fun name ->
        getExperimentColumn name
    )

let replicates =
    outputFiles
    |> Array.map (fun name ->
        getReplicateColumn name
    )

let acquisitionModes =
    outputFiles
    |> Array.map (fun name ->
        let aM = (getAcquisitionColumn name).ToLower()
        if aM.Contains "dda" then "DDA"
        elif aM.Contains "dia" then "DIA"
        else failwith "unknown acquisition type"
    )

Array.mapi (fun i fileLocation ->
    if ddaOnly then
        if acquisitionModes.[i] = "DDA" then
            sprintf "%s\t%s\t%s\t%s" fileLocation experiments.[i] replicates.[i] acquisitionModes.[i]
            |> Some
        else None
    else
        sprintf "%s\t%s\t%s\t%s" fileLocation experiments.[i] replicates.[i] acquisitionModes.[i]
        |> Some
) fileLocations
|> Array.choose id
|> fun s -> File.WriteAllLines (Path.Combine [|arcPath; "runs"; runName; "files.fp-manifest"|], s)

let workflow =
    let file = System.IO.File.ReadAllLines (Path.Combine [|arcPath; "runs"; runName; "fragpipe.workflow"|])
    if file |> Array.exists (fun line -> line.StartsWith "database.db-path=") then
        file
        |> Array.map (fun line ->
            if line.StartsWith "database.db-path=" then
                let fastALine = (line.Split "=").[0] + "=" + fastAPath
                printfn "%A" fastALine
                fastALine
            else
                line
        )
    else
        Array.append file [|"database.db-path=" + fastAPath|]
    |> fun s -> File.WriteAllLines(Path.Combine [|arcPath; "runs"; runName; "fragpipe.workflow"|], s)