# SharpFetch Roadmap
### Day 1 Completed ‼️🎉
- [x] Print Super basic system information to console
- [x] Install Spectre.Console package to beautify console app
- [x] Add a panel widget to organize system data
- [x] Install system.commandline packge to create the basics of the CLI app
- [x] Look for icons to display in cli output
- [x] Adding the first option to my command to display icons or add colour.
-----
### Day 2 Completed ‼️🎉
- [x] Change Panel widget to a Grid widget and put all system data inside it.
- [x] Refactor code and place system info logic in one file as well as for spectre console logic
- [x] Extend system info adding disk space, runtime, uptime, memory, cpu.
- [x] Create helper functions for disk space conversion, etc. (optional).
----
### Day 3 Completed ‼️🎉
- [x] Add colour for the system info output
- [x] Fix GetMemory functionality to display correct values
- [x] Group Sys info and add a tree widget (We will add different ways of printing the information)
- [x] Reasearch if we can add Kernel, WM, terminal, shell, Bios, GPU, Datetime, packages if possible for next stream.
- [x] Add a minimal and full flags to display minimal system information or full info.
- [x] Add Charts widget to show memory and diskspace.

### Day 4 - 7 Completed  ‼️🎉
- [x] Plan and implement a way to let the user decide what to print and from there refactor and make no icons logic work as well as for minimal flag and any other existing option for the cli.
- [x] Fix and refator --no-icons logic
- [x] Refactor and maybe rename --minimal flag to something like (--output "minimal | full")
- [x] Add different output format (eg. panels, trees, left panel)
- [x] Plan to implement configuration (JSON File) for CLI app.

### Day 8 Completed ‼️🎉
- [x] Add Missing Modules.
- [x] Create a grouping mechanism for modules.
- [x] Add Left panel render method.
- [x] Move any missing logic in ModuleResultRender from old render file (memory + disk BreakdownCharts).
- [x] Start planning the interactive configuration Wizard.
- [x] Adding showCharts and group CLI options.
- [x] Fixed the ShowExecutionTime logic.

### Day 9 Completed ‼️🎉 
- [x] Implement interactive configuration wizard.
- [x] Make the disk and memory charts leaner.

### Day 10 (In Progress ...)
- [x] Add nerd font icons version for output icons.
- [x] Crate option to display nerd font or emoji icons.
- [x] Refactor project structure and remove unsued files.
- [x] Create an appealing README file and add some captures.

### 🪳 Known Issues
- [ ] Breakdown Charts bug (Module printing duplication)
- [ ] Add Memory and Disk labels on chart and add Icons.
- [ ] Add the missing modules to the config wizard and remove non existing ones.
- [ ] Add missing icons for missing modules (Emojis & Nerd Font).
- [ ] Look for a cooler 🔥 banner for the config wizard.
- [ ] Change "Current Configuration" text to "Configuration Preview" or something more explainatory.
- [ ] Add padding to the grid at the configuration preview.
- [ ] Display the configuration preview on a table (**Optional**).
- [ ] Remove ```-generate-config``` option and decide to also remove ```--config``` option.
- [ ] Only display charts on formats that make sense to show them.