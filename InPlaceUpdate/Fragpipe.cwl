cwlVersion: v1.2
class: CommandLineTool
hints:
  DockerRequirement:
    dockerImageId: "fragpipe"
    dockerFile: {$include: "Dockerfile"}
requirements:
  - class: InitialWorkDirRequirement
    listing:
      # this specifies the name of the root folder
      - entryname: arc
        entry: $(inputs.arcDirectory)
        writable: true
  - class: InplaceUpdateRequirement
    inplaceUpdate: true
  - class: NetworkAccess
    networkAccess: true
baseCommand: [/fragpipe_bin/fragPipe-22.0/fragpipe/bin/fragpipe]
inputs:
  arcDirectory:
    type: Directory
  headless:
    type: boolean
    inputBinding:
      position: 1
      prefix: --headless
  workflow:
    type: File
    inputBinding:
      position: 2
      prefix: --workflow
  manifest:
    type: File
    inputBinding:
      position: 3
      prefix: --manifest
  workdir:
    type: string
    inputBinding:
      position: 4
      prefix: --workdir
  toolsFolder:
    type: string
    inputBinding:
      position: 5
      prefix: --config-tools-folder
  threads:
    type: int
    inputBinding:
      position: 6
      prefix: --threads
outputs:
  runResult:
    type: Directory
    outputBinding:
      glob: $(inputs.workdir)