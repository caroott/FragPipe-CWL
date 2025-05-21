cwlVersion: v1.2
class: Workflow

requirements:
  - class: MultipleInputFeatureRequirement
inputs:
  arcDirectory: Directory
  runName: string
  assayName: string
  experimentColumn: string
  replicateColumn: string
  acquisitionColumn: string
  fastAPath: string
  ddaOnly: string
  headless: boolean
  workdir: string
  toolsFolder: string
  threads: int

steps:
  manifestAndWorkflow:
    run: ManifestAndWorkflow.cwl
    in:
      arcDirectory: arcDirectory
      runName: runName
      assayName: assayName
      experimentColumn: experimentColumn
      replicateColumn: replicateColumn
      acquisitionColumn: acquisitionColumn
      fastAPath: fastAPath
      ddaOnly: ddaOnly
    out: [mountdir, manifest, workflow]
  Fragpipe:
    run: Fragpipe.cwl
    in:
      arcDirectory: manifestAndWorkflow/mountdir
      headless: headless
      workflow: manifestAndWorkflow/workflow
      manifest: manifestAndWorkflow/manifest
      workdir: workdir
      toolsFolder: toolsFolder
      threads: threads
    out: [runResult]
outputs:
  result:
    type: Directory
    outputSource: Fragpipe/runResult