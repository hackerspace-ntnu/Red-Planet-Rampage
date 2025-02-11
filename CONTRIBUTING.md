# Project details/requirements

- Unity version 2022.2.15f1
- Unity URP

# Conventions

## Git

Branch naming: `kebab-case`.
Prefix the branch name with a one-word description of the purpose of the branch,
e.g. `feature/main-menu` or `fix/wall-glitch`.

## Development

⚠ The following is no longer valid as we've moved past the student project phase.

The development process is a variant of scrum structured as follows:

- 2 week long sprints
- Milestones containing 1 to 2 sprints
- Short retrospectives at the end of milestones
- Public issue-, and github-boards
- All planned tasks are converted github issues and assigned to a team member
- Public demos after ended milestones, preferably at public events
- Official releases are made after ended milestones

## Assets

Developers should strive to only use self-made assets.
This rule is in place to encourage team members to learn all aspects of game development.
The second reasoning is to keep the repository as open source as possible.

# Troubleshooting

## URP on Linux

Add `-force-vulkan` to command line arguments to avoid glitched scene view due to URP.

## Building the game

There is a bug in this Unity version that gives build errors for missing
`System.Diagnostics.Tracing`. This is resolved by manually downgrading
the version in `Library/PackageCache/com.unity.burst@1.8.4/.Runtime/bcl.exe.config`. The dependency should be replaced with
the following:

```
<assemblyIdentity name="System.Diagnostics.Tracing" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
<bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
```
