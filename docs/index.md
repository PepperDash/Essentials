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

- Runs on Crestron **4-Series** and VC-4 Control System platforms (3-Series processors run [Essentials 1.x](https://github.com/PepperDash/Essentials/tree/essentials-1x))
- Reduced hardware overhead compared to S+ and Simpl solutions
- Quick development cycle
- Shared resources made easily available
- More flexibility with less code
- Configurable using simple JSON files

---

## Collaboration

Essentials is an open-source project and we encourage collaboration on this community project. For features that may not be useful to the greater community, or for just-plain learning, we want to remind developers to try writing plugins for Essentials. More information can be found here: [Plugins](~/docs/technical-docs/Plugins.md)

### Open-source-collaborative workflow

Essentials uses trunk-based development. Each major version has one long-lived trunk branch, and all work is merged into it through short-lived branches:

| Branch | Essentials version | Target framework |
|---|---|---|
| `essentials-3x` (the default branch) | 3.x | .NET 8 |
| `essentials-2x` | 2.x | .NET Framework 4.7.2 |
| `essentials-1x` | 1.x | .NET Compact Framework 3.5 |

Releases are made automatically from each trunk by [semantic-release](https://semantic-release.gitbook.io/). The release type comes from the commit messages, so they follow [Conventional Commits](https://www.conventionalcommits.org/): `fix:` makes a patch release, `feat:` a minor release. `essentials-2x` and `essentials-1x` are limited to their major version, but `essentials-3x` isn't, so a breaking change (a commit with a `BREAKING CHANGE:` footer; the `feat!:` shorthand isn't recognized) merged into it releases 4.0.0. Starting a new major version also means a new trunk and a release config change, so agree it with the maintainers first.

To contribute, follow this process:

1. Fork this repository ([More Info](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/working-with-forks))
2. Create a short-lived branch from the trunk for the version you're changing (`essentials-3x` for most work), named with a prefix such as `feature/` or `fix/` followed by a descriptive name.
   - Example: `feature/add-awesomeness` or `fix/really-big-oops`
3. Make commits as necessary (often is better), with Conventional Commits messages such as `fix: handle a missing serverUrl`. Use concise, descriptive language, leveraging issue notation and/or [Closing Keywords](https://help.github.com/articles/closing-issues-using-keywords) to ensure any issues addressed by your work are referenced accordingly.
4. Keep the branch small and up to date with its trunk, so it can be merged soon.
5. Create a Pull Request to merge your branch into the same trunk branch in the main repository.
6. Your Pull Request will be reviewed by our team and evaluated for inclusion into the main repository.

Next: [Get started](~/docs/Get-started.md)
