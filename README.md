![redplanetrampagetitlerender](https://github.com/hackerspace-ntnu/Red-Planet-Rampage/assets/54811121/e720e411-dc90-4194-94c1-c08b0e99c6f2)

---

Red Planet Rampage is a first person shooter game, where you fight against other players in arenas with dynamically constructed weapons made from weapon parts.

The weapon parts are obtained from auctions in-between shooting rounds where players bid against each other. Due to the amount of weapon parts, there are hundreds of possible weapon combinations!

---

Two players shooting at each other with their unique weapons:
![Gif of 2 players shooting their weapons](https://github.com/hackerspace-ntnu/Red-Planet-Rampage/assets/54811121/de4e0f91-9975-4fbd-951b-6a6ccb44674e)

The auction for weapon parts:
![Gif of 2 players bididng on weapon parts](https://github.com/hackerspace-ntnu/Red-Planet-Rampage/assets/54811121/ad7fa86e-2e0b-4448-870d-b1007b97c7b9)

### Project details
- Unity version 2022.2.15f1
- Unity URP

## Conventions

### Git

Branch naming: `kebab-case`.
Prefix the branch name with a one-word description of the purpose of the branch,
e.g. `feature/main-menu` or `fix/wall-glitch`.

### Development

The development process is a variant of scrum structured as follows:
- 2 week long sprints
- Milestones containing 1 to 2 sprints
- Short retrospectives at the end of milestones
- Public issue-, and github-boards
- All planned tasks are converted github issues and assigned to a team member
- Public demos after ended milestones, preferably at public events
- Official releases are made after ended milestones

### Assets

Developers should strive to only use self-made assets.
This rule is in place to encourage team members to learn all aspects of game development.
The second reasoning is to keep the repository as open source as possible.

## Troubleshooting

### URP on Linux

Add `-force-vulkan` to command line arguments to avoid glitched scene view due to URP.

### Building the game

There is a bug in this Unity version that gives build errors for missing 
"System.Diagnostics.Tracing". This is resolved by manually downgrading 
the version in "bcl.exe.config". The dependency should be replaced with 
the following:
```
<assemblyIdentity name="System.Diagnostics.Tracing" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
<bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
```
