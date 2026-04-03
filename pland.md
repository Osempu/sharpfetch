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

### Day 4 (⌚ In Progress ... )
- [ ] Plan and implement a way to let the user decide what to print and from there refactor and make no icons logic work as well as for minimal flag and any other existing option for the cli.
- [ ] Get disk space and memory charts into the panel with their grouped info
- [ ] Fix and refator --no-icons logic
- [ ] Refactor and maybe rename --minimal flag to something like (--output "minimal | full")
- [x] Add different output format (eg. panels, trees, left panel)
- [ ] Plan to implement configuration (JSON File) for CLI app.
- [ ] Add a colouring options (eg. rainbow, no color, color per group).

## System info Output Formats
- Panels: Display all system info inside panel.
- Trees: Use trees widget to display system info using trees.
- Left Panel: Get all sys info titles (or props) inside a left panel and the values to the right without any border or panel.
- Plain: Display system info without using any visual widget, just as neofetch or fastfech do it right out of the box.