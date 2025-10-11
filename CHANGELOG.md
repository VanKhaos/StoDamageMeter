# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2025-10-11

### Fixed
- **Combat-Type Detection:** Fixed incorrect combat type classification (Space vs Ground)
  - Removed majority counting logic that caused false positives
  - Combat type is now determined by the first recognized enemy type in combat
  - Simpler and more reliable detection algorithm
- **Live Combat Overlay:** Overlay now properly clears after combat ends
  - Fixed PropertyChanged event not firing when combat finalized
  - Player list is now cleared when showing empty state
  - No more stale combat data displayed in overlay

### Changed
- Combat type detection simplified from ~25 lines to 3 lines of logic
- More robust and maintainable code

## [1.2.0] - 2025-10-11

### Added
- **Live Combat Overlay Window**
  - Always-on-top floating overlay displaying live combat data
  - Shows Player Name, DPS, and Total Damage in real-time
  - Draggable and resizable window
  - Pin functionality to lock overlay in place
  - Adjustable font size (10-24px) with live preview
  - 15 player-specific colors for easy identification
  - Smooth rank-change animations (250ms)
  - Demo mode for testing and development
- **Live Combat Tab**
  - Real-time combat parsing while playing
  - Updates every 0.5 seconds
  - Automatic combat detection (45-second timeout)
  - Combat type change detection (Space ↔ Ground)
  - Completed combats automatically added to combat list
- **GitHub Landing Page (README.md)**
  - Comprehensive documentation
  - Installation instructions
  - Feature overview
  - Development setup guide

### Fixed
- Combat log backup and trimming keeps last 30 combats
- File watcher properly restarts after log trimming
- Overlay closes correctly when main window closes
- Console window no longer appears in release builds

### Changed
- Default combat timeout increased from 30s to 45s
- Font sizes increased for better readability
- Overlay initial height optimized for 5 players (200px)

## [1.1.7] - 2025-10-10

### Added
- Clean release structure with Launcher architecture
- App folder contains all DLLs and executables
- Language folder for localization files
- Launcher with animated splashscreen

### Changed
- Simplified release structure (only 3 items in root)
- Professional startup experience with fade animations

## [1.1.4] - 2025-10-09

### Added
- Combat statistics with 3-level hierarchy (Player → Companion → Ability)
- Sortable columns (DPS, Total Damage, Max Hit, Crit %, Attacks)
- Combat type detection (Space vs Ground)
- Companion parsing (Pets, Away Team, Drones)
- Damage type visualization with colored icons
- Column separators for better readability

### Fixed
- UTF-8 encoding for special characters (ä, ö, ü)
- Correct date parsing for combat timestamps
- Time-based combat detection (30-second gaps)

### Changed
- WPF-UI integration with Fluent Design
- Star Trek Blue theme (#5B9BD5)
- Backend as standalone executable (no Python required)

## [1.0.0] - 2025-01-27

### Added
- Initial release
- Basic combat log parsing
- Player statistics display
- Dark theme UI
- Backend integration with OSCR parser


