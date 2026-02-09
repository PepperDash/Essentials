# Welcome to PepperDash Essentials!

PepperDash Essentials is an open-source framework for control systems, built on Crestron's Simpl# Pro framework. It can be configured as a standalone program capable of running a wide variety of system designs and can also be used to augment other Crestron programs.

Essentials is a collection of C# libraries that can be used in many ways. It is a 100% configuration-driven framework that can be extended to add different workflows and behaviors, either through the addition of new device-types and classes, or via a plug-in mechanism. The framework is a collection of things that are all related and interconnected, but in general do not have strong dependencies on each other.

---

## Get started

- [Download an Essentials build or clone the repo](~/docs/Get-started.md)
- [Get started](~/docs/Get-started.md)
- [YouTube Video Series Playlist](https://youtube.com/playlist?list=PLKOoNNwgPFZdV5wDEBDZxTHu1KROspaBu)
- [Discord Server](https://discord.gg/6Vh3ssDdPs)

Or use the links to the left to navigate our documentation.

---

## Benefits

- Runs on Crestron 3-Series, **4-Series** and VC-4 Control System platforms
- Reduced hardware overhead compared to S+ and Simpl solutions
- Quick development cycle
- Shared resources made easily available
- More flexibility with less code
- Configurable using simple JSON files

---

## Collaboration

Essentials is an open-source project and we encourage collaboration on this community project. For features that may not be useful to the greater community, or for just-plain learning, we want to remind developers to try writing plugins for Essentials. More information can be found here: [Plugins](~/docs/technical-docs/Plugins.md)

### Open-source-collaborative workflow

The `main` branch always contain the latest stable version. The `development` branch is used for most development efforts.

[GitFlow](https://nvie.com/posts/a-successful-git-branching-model/) will be used as the workflow for this collaborative project. To contribute, follow this process:

1. Fork this repository ([More Info](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/working-with-forks))
2. Create a branch using standard GitFlow branch prefixes (feature/hotfix) followed by a descriptive name.
   - Example: `feature/add-awesomeness` or `hotfix/really-big-oops`
   - When working on a new feature or bugfix, branch from the `development` branch. When working on a hotfix, branch from `main`.
3. Make commits as necessary (often is better). And use concise, descriptive language, leveraging issue notation and/or [Closing Keywords](https://help.github.com/articles/closing-issues-using-keywords) to ensure any issues addressed by your work are referenced accordingly.
4. When the scope of the work for your branch is complete, make sure to update your branch in case further progress has been made since the repo was forked
5. Create a Pull Request to pull your branch into the appropriate branch in the main repository.
6. Your Pull Request will be reviewed by our team and evaluated for inclusion into the main repository.

Next: [Get started](~/docs/Get-started.md)
