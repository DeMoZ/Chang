# Chang Documentation

Chang is a Unity app for learning Thai with Chang the Elephant.
This folder holds the development documentation: how the project is built, where the content comes from, and how it gets into the game.

> The docs live next to the code and change in the same commits and PRs as the code.
> If you change behavior, update the matching page.

## Contents

| Section | What it covers |
|---|---|
| [Getting started](getting-started.md) | Cloning, submodules, first run in the Editor |
| [Architecture](architecture.md) | Scenes, Zenject, FSMs, UI pattern, async, logging |
| [Learning logic](learning-logic.md) | Marks, question types, lesson flow, repetition, section sorting |
| [Content pipeline](content-pipeline.md) | Google Sheets → configs, keys, images, sounds, Addressables/CCD |
| [Localization](localization.md) | SimpleLocalization, CSV files, languages |
| [Profile, saves, auth](save-and-auth.md) | Player data, PlayerPrefs, Unity Cloud Save, login |
| [Build and deploy](build-and-deploy.md) | Platforms, WebGL, Unity Play, building bundles |
| [Conventions](conventions.md) | Code style and rules that are easy to break |
| [Glossary](glossary.md) | Project terms |
| [Known issues](known-issues.md) | Tech debt and inconsistencies found so far |
| [Architecture Decision Records](decisions/README.md) | Why things are done the way they are |

draw.io diagrams live in [`/UML`](../UML): `GameUML.drawio`, `MarkUML.drawio`, `SortUML.drawio`.

## How to maintain these docs

- Write in Markdown. Draw diagrams in [Mermaid](https://mermaid.js.org/); GitHub renders them inline.
- One page per topic. A new topic gets a new file and a row in the table above.
- Refer to code by its path from the repository root, e.g. `Assets/Project/Scripts/Game/FSM/GameFSM.cs`.
- The code is the source of truth. The docs explain *why* and *how things connect*; they don't restate every line.
- Record significant decisions ("we chose X over Y because…") as an ADR in [`decisions/`](decisions/README.md).
- Add bugs or tech debt you are not fixing right now to [known-issues.md](known-issues.md), or open a task in the [GitHub Project](https://github.com/users/DeMoZ/projects/1).
