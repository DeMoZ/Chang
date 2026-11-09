# webgl-page-loading Specification

## Purpose

Defines how the WebGL web page starts the game: what the first screen shows, when the Unity build is downloaded, and what the Service Worker caches, so that only people who choose to play download the build.

## Requirements

### Requirement: Start screen before the game loads
When the page opens, it SHALL show a start screen with the KruChaang logo and a **Play** button, without waiting for any Unity build file. The button SHALL be reachable by keyboard and work with mouse and touch.

#### Scenario: First visit in a browser tab
- **WHEN** a person opens the game page in a browser tab
- **THEN** the start screen with the logo and the **Play** button is shown
- **AND** no loading progress bar is shown yet

### Requirement: No build download before Play
The page SHALL NOT request the Unity loader, framework, code or data files (`Build/*`) before the game is started by the **Play** button or by an autostart case. This SHALL hold for the page and for the Service Worker.

#### Scenario: A visit without clicking
- **WHEN** the page is opened and JavaScript runs, and nobody presses **Play**
- **THEN** no request to `Build/*` is made by the page or the Service Worker
- **AND** the page, `TemplateData/*`, `manifest.webmanifest`, `ServiceWorker.js` and `version.json` may still be requested

### Requirement: Play starts the game
Pressing **Play** SHALL start loading the game once: the start screen is replaced by the loading bar with progress, and the game appears when loading finishes. Further presses while loading SHALL have no effect.

#### Scenario: Press Play
- **WHEN** the person presses **Play**
- **THEN** the build files are requested, the loading bar shows progress, and the game starts when loading is done

#### Scenario: Double press
- **WHEN** the person presses **Play** twice quickly
- **THEN** the game is loaded only once

### Requirement: Autostart for an installed app
When the page runs as an installed app (PWA standalone display mode), it SHALL start the game without the **Play** button.

#### Scenario: Launch from the home screen
- **WHEN** the person opens the installed KruChaang app
- **THEN** the game starts loading at once, with no start screen button

### Requirement: Autostart after a version reload
When the page reloads itself because a newer build is deployed while the game was started in that tab, the reloaded page SHALL start the game without the **Play** button. A reload of a tab where the game was never started SHALL show the start screen as usual.

#### Scenario: New deploy while playing
- **WHEN** the game is running in a tab and the version check reloads the page for a new build
- **THEN** the reloaded page starts the new build without waiting for **Play**

#### Scenario: New deploy before Play
- **WHEN** the version check reloads a tab that still shows the start screen
- **THEN** the reloaded page shows the start screen again

### Requirement: Build files cached on first play
After the build files are downloaded for a play, the Service Worker SHALL keep them so that a later launch of the same build works offline and does not download them again.

#### Scenario: Second launch of the same build
- **WHEN** a person played once and opens the page again later (same build), then presses **Play**
- **THEN** the build files come from the cache, without network requests for `Build/*`

### Requirement: Page screens look like the in-game loading screen
The start screen and the build loading screen SHALL use the look of the in-game content loading screen: the same background, logo, progress bar and percent, with the same colors, sizes and positions at any window size, so that the hand-over from the page to the game shows no visual jump.

#### Scenario: Hand-over to the game
- **WHEN** the build finishes loading and the game shows its content loading screen
- **THEN** the background, the logo, the bar and the percent stay in the same places and colors (within a couple of pixels)

#### Scenario: Start screen
- **WHEN** the start screen is shown
- **THEN** it has the same background and logo, and the **Play** button in the game's primary button style where the progress bar will appear
