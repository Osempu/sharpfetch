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

### Day 9 (1🗓️ Next Session ... ) 
- [ ] Implement interactive configuration wizard.
- [ ] Maker the disk and memory charts leaner.
- [ ] Add nerd font icons version for output icons.
- [ ] Crate option to display nerd font or emoji icons.
- [ ] Refactor project structure and remove unsued files.
- [ ] Create an appealing README file and add some captures.

### 🗓️ Future Updates
- [x] Get disk space and memory charts into the panel with their grouped info
- [ ] Add a colouring options (eg. rainbow, no color, color per group).

## System info Output Formats
- Panels: Display all system info inside panel.
- Trees: Use trees widget to display system info using trees.
- Left Panel: Get all sys info titles (or props) inside a left panel and the values to the right without any border or panel.
- Plain: Display system info without using any visual widget, just as neofetch or fastfech do it right out of the box.