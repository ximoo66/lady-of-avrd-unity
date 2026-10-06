# The Lady of AVRD

Mixed-reality AI companion for Meta Quest 3, combining voice interaction, spatial interfaces and generative tools.

[Experience, trailer and project details](https://www.omidameri.com/project-lady.html) · [Omid Ameri's portfolio](https://www.omidameri.com/)

## The experience

This repository contains the Unity client. Voice and tool requests communicate with a separate AI backend through WebSockets. The complete project also includes speech recognition, language-model inference, speech synthesis and image/3D generation services; those services are not bundled in this client repository.

## Explore the implementation

This public repository is a source-code showcase of a collaborative university project. It contains selected project scripts, their Unity metadata, the package manifest and the original editor version. It does not contain the complete playable project.

Project code is organized under:

- `Assets/_Scripts/`


| Area | Entry point |
| --- | --- |
| Voice and backend communication | [AIManagerMain.cs](Assets/_Scripts/AIManagerMain.cs) |
| WebSocket connection lifecycle | [NetworkConnection.cs](Assets/_Scripts/NetworkConnection.cs) |
| Tool request handling | [ToolCallManager.cs](Assets/_Scripts/ToolCallManager.cs) |
| Spatial mind maps | [MindMapManager.cs](Assets/_Scripts/MindMapInteraction/MindMapManager.cs) |
| Onboarding | [OnboardingManager.cs](Assets/_Scripts/OnboardingManager.cs) |

## Dependencies and running the project

The original project uses Unity **6000.3.11f1**. Review `Packages/manifest.json` inside the project folder for its package dependencies. To use these scripts, create an appropriate Unity project and restore the required packages. Scenes, prefabs, input bindings, art, audio and third-party plugins must be obtained and configured separately; cloning this showcase alone will not reproduce the game or experience.

Third-party Asset Store packages, vendor SDK source, course starter code, tutorial examples, models, textures, audio, compiled builds and generated editor files are intentionally excluded. Any plugins referenced by the scripts must be installed from their original publishers under the applicable licenses.

The Unity client also references project configuration types such as `LocalConfig` and `Config`, as well as external plugins (including DOTween). Backend addresses and authentication values are not supplied here. Configure these separately for your own development environment.

## Authorship and provenance

Developed collaboratively by the project team, including Omid Ameri. The code is presented as team work, not as an assertion that every file was written by one person. The complete project, contributor history, branches and tags are preserved in a separate private archive. This public showcase begins with a fresh snapshot so excluded files cannot be recovered from older commits.

No blanket open-source license is granted by this publication. Existing authorship and rights remain applicable; obtain permission from the relevant authors before reusing code.
