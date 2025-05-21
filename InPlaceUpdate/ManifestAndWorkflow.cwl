cwlVersion: v1.2
class: CommandLineTool
hints:
  DockerRequirement:
    dockerPull: mcr.microsoft.com/dotnet/sdk:8.0-noble 
requirements:
  - class: InitialWorkDirRequirement
    listing:
      - entryname: arc
        entry: $(inputs.arcDirectory)
        writable: true
  - class: InplaceUpdateRequirement
    inplaceUpdate: true
  - class: EnvVarRequirement
    envDef:
      - envName: DOTNET_NOLOGO
        envValue: "true"
  - class: NetworkAccess
    networkAccess: true
baseCommand: [dotnet, fsi, "./arc/workflows/Fragpipe/scripts/manifestAndWorkflow.fsx"]
inputs:
  arcDirectory:
    type: Directory
  runName:
    type: string
    inputBinding:
      position: 1
  assayName:
    type: string
    inputBinding:
      position: 2
  experimentColumn:
    type: string
    inputBinding:
      position: 3
  replicateColumn:
    type: string
    inputBinding:
      position: 4
  acquisitionColumn:
    type: string
    inputBinding:
      position: 5
  fastAPath:
    type: string
    inputBinding:
      position: 6
  ddaOnly:
    type: string
    inputBinding:
      position: 7
outputs:
  mountdir:
    type: Directory
    outputBinding:
      glob: ./arc
  manifest:
    type: File
    outputBinding:
      glob: ./arc/runs/$(inputs.runName)/files.fp-manifest
  workflow:
    type: File
    outputBinding:
      glob: ./arc/runs/$(inputs.runName)/fragpipe.workflow
