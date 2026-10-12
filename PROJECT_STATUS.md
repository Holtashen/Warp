# Warp - Project Status

## 1. Project Information

- Project name: Warp
- Engine: Unity
- Unity version: 2022.3.62f3c1
- Git branch: main
- Remote repository: Holtashen/Warp
- Repository visibility: Private

## 2. Project Goal

Develop a dialogue and narrative system with branching
choices and a visual dialogue graph editor.

## 3. Known Existing Components

Runtime:
- DialogueData.cs
- DialogueNode.cs
- DialogueManager.cs
- DialogueUI.cs
- ChoiceUI.cs
- GameState.cs

Editor:
- DialogueGraphWindow.cs
- DialogueGraphView.cs
- DialogueNodeView.cs

Note: Verify every filename against the current project.
This list is based on previous development records.

## 4. Verified Git Status

- Initial commit exists.
- Remote repository has been published.
- Working tree was clean at the last check.
- No files were listed by git lfs ls-files.

## 5. Current Development Status

- Previous records indicate that the graph editor can load
  nodes from Chapter01.asset.
- Automatic node arrangement and position persistence
  still need to be verified in the current project.

## 6. Known Issues

- Current Unity Console status: Not checked.
- Runtime dialogue behavior: Not fully verified.
- Graph connections and serialization: Not fully verified.

## 7. Next Steps

1. Inspect the current scripts and their dependencies.
2. Verify the current graph editor behavior.
3. Establish repeatable tests.
4. Implement the next feature only after the current state
   has been confirmed.

## 8. Update Rules

After each development session, update:
- Files changed
- Features implemented
- Tests performed and results
- Known issues
- Next task

Never mark an untested feature as verified.