# Warp - Architecture

## 1. Runtime System

Known components:

- DialogueData.cs
- DialogueNode.cs
- DialogueManager.cs
- DialogueUI.cs
- ChoiceUI.cs
- GameState.cs

Intended responsibilities, pending source-code verification:

- DialogueData: stores dialogue data.
- DialogueNode: represents a dialogue node.
- DialogueManager: coordinates dialogue execution.
- DialogueUI: presents dialogue to the player.
- ChoiceUI: presents dialogue choices.
- GameState: manages game state.

These responsibilities are provisional. Confirm them by
reading the current source code before changing the system.

## 2. Editor System

Known components:

- DialogueGraphWindow.cs
- DialogueGraphView.cs
- DialogueNodeView.cs

The editor is intended to provide a visual interface
for editing dialogue nodes.

Exact class relationships, graph connections, automatic
layout, and position serialization must be verified
against the current source code.

## 3. Data and Dependencies

Known data asset:
- Chapter01.asset

Other assets, references, serialization rules, and package
dependencies have not yet been fully inspected.

## 4. Compatibility Rules

Before changing a public class, field, or method:

1. Search for all references.
2. Check existing serialized assets and Inspector assignments.
3. Check compatibility with the other scripts.
4. Make a small change.
5. Compile and test before proceeding.

## 5. Verification Status

This document is an initial outline, not a complete
architecture specification.

Update it after inspecting the actual source files.