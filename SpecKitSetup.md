## Setup Spec Kit

### Install

* Open command prompt with administrator
* Install Python is not installed yet
* Run command `pip install uv`
* Run command `uv tool install specify-cli --force --from git+https://github.com/github/spec-kit.git`
* When see warning like below

warning: `C:\Users\dchen\.local\bin` is not on your PATH. To use installed tools, run `set PATH="C:\\Users\\dchen\\.local\\bin;%PATH%"` or `uv tool update-shell`. 

in windows search bar, input env, modify user environment, add `C:\Users\dchen\.local\bin` to PATH when see warning below

* Optional install Github Cli by run command `winget install --id GitHub.cli`
`gh auth login`
`gh extension install github/gh-copilot`

### Use Spec Kit

* Run command `specify check` it will give current AI agent installed
* Run command `specify init PhotoGallery` this create project "PhotoGallery"
* Open "PhotoGallery" folder with Visual Studio Code (it should have GitHub Copilot extension installed already)
* In AI agent chat window, you can input the splash command as table below

### Spec Kit Commands

Command examples:
```
/speckit.constitution Create principles focused on code quality, testing standards, user experience consistency, and performance requirements
/speckit.specify Build an application that can help me organize my photos in separate photo albums. Albums are grouped by topics (such as Marine time provinces, Europe trip, local community, school activities) and can be re-organized by dragging and dropping on the main page. Albums are never in other nested albums. Within each album, photos are previewed in a tile-like interface.
```

Commands table

┌─────────────────────────────────────────────── Basic Commands ─────────────────────────────────────────-------──────┐
│  1. Go to the project folder: cd PhotoGallery                                                                       │
│  2. Start using slash commands with your AI agent:                                                                  │
│     2.1 /speckit.constitution - Establish project principles                                                        │
│     2.2 /speckit.specify - Create baseline specification                                                            │
│     2.3 /speckit.plan - Create implementation plan                                                                  │
│     2.4 /speckit.tasks - Generate actionable tasks                                                                  │
│     2.5 /speckit.implement - Execute implementation                                                                 │
│                                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────── Enhancement Commands ────────────────────────────────────────────────┐
│                                                                                                                     │
│  Optional commands that you can use for your specs (improve quality & confidence)                                   │
│                                                                                                                     │
│  ○ /speckit.clarify (optional) - Ask structured questions to de-risk ambiguous areas before planning (run before    │
│  /speckit.plan if used)                                                                                             │
│  ○ /speckit.analyze (optional) - Cross-artifact consistency & alignment report (after /speckit.tasks, before        │
│  /speckit.implement)                                                                                                │
│  ○ /speckit.checklist (optional) - Generate quality checklists to validate requirements completeness, clarity, and  │
│  consistency (after /speckit.plan)                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘