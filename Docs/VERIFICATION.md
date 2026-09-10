# Progression and automation responsiveness verification — 2026-09-10

Branch: progression-foundation, based on the approved URP commit b929e4d.

## Results

| Check | Result |
|---|---|
| Unity 6000.6.0f1 / URP 17.6.0 Windows build | Succeeded |
| Existing simulation suite, including saved front riffle geometry | 83 passed |
| Progression / automation / save suite | 102 passed (39 responsiveness checks added) |
| Original manual Windows smoke suite at 1366×768 | 40 passed; process exited 0 |
| Full earned Windows chapter at 1920×1080 | 185 passed; process exited 0 |
| Full earned Windows chapter at 1280×1024 | 185 passed; process exited 0 |
| Final player logs | No shader errors or exceptions |
| Rendered screenshots | Inspected at all three sizes; pan and UI remain readable |

The full chapter tests buy all nine tool levels and all three helpers using gold earned through the real game commands. They subsequently complete three assisted scoops, restore gold/upgrades/toggles and camp objects from disk, and reset all progression. Test saves live in unique Playtest/Verification subfolders. Normal player progress is never loaded or written by either verification mode.

The original smoke checks still cover stationary manual input, separate action plateaus and recovery, front-only stone exits, collect-once behavior, upgrades, and collection with obstructed stones. Their later sluice-enabled plateau expectation now reflects 72% compacted sediment.

The added suite verifies tool-tier gates and exact charges, independent Work/Wash assistance, manual priority, combined alternation without permanent simultaneous intents, 30/60/120 Hz completion, preserved gold, readiness-only collection and reveal delay, paused timers, toggles, round-trip saves, atomic replacement, backup recovery, corrupt-file preservation, reset and deterministic gold variation.

## Balance

- Focused route: 20.7 minutes, 79 pans; longest purchase gap 3.1 minutes.
- Unhurried route: 28.7 minutes, 79 pans; longest purchase gap 4.2 minutes.
- Windows integration route: roughly 80 pans to the fully assisted chapter.

These are simulated routes, with explicitly documented learning/reading/collection allowances. They are not imposed delays. The Windows test accelerates game commands, so its wall-clock duration is not a player completion-time measurement. The user subsequently hand-played the full chapter and approved its overall pacing. The responsiveness pass preserves these estimates and purchase counts.

## Responsiveness pass

- Auto Collect reveal: 2.2 seconds before, 0.6 seconds after, verified within one frame at 30/60/120 Hz. Manual E follows the same tested command path and credits ready gold immediately before assistance runs.
- Normal phase guard: 0.8 seconds before, 0.15 seconds after. A depleted Wash plateau or fully prepared Work phase switches on the next simulation tick regardless of that guard. Releasing a manual hold re-evaluates the needed action immediately. Widely separated sediment thresholds continue to prevent flickering; there is no permanent simultaneous automatic hold.
- Manual Work and Wash retain full intent strength and win on the input frame. Regressions exercise opposite-action takeovers while both helpers are enabled, release in both directions, and explicit combined manual holds.
- The compact existing status strip now explicitly shows Work / Wash / Collect / Waiting, with an assisted, manual, or waiting reason. Collect status updates on the frame that readiness is reached. Both new Collect captures were visually inspected.
- Added runtime checks exercise E during the reveal, repeated E after collection, and frozen collection during a blocked interval followed by prompt collection on resume. Each full chapter player run passes 185 checks, up from 177.
- Representative ten seeded loads at 60 Hz: tier 2 assisted processing remains about 12.0 seconds versus 10.3 seconds for attentive manual preparation followed by Wash. Fully upgraded unattended completion falls from about 12.7 seconds to 11.1 seconds; the same manual comparison route takes about 9.1 seconds. No automation-strength penalty was introduced. The manual comparison assumes prompt input and collection; it does not imply every manual strategy beats assistance.
- Chapter pricing and the 20.7–28.7 minute route are unchanged. Normal separation phases already exceed the new guard, and the measured chapter ends at the Auto Collect purchase. Its shorter reveal improves subsequent unattended cycles by about 1.6 seconds each.
- Work/Wash simulation, upgrade costs, camp content, save format, and rendering are unchanged. Verification uses disposable saves, preserving the user's hand-played progress.

## Captures

Full chapter captures are in:
- Playtest/Chapter-1920x1080/
- Playtest/Chapter-1280x1024/

Each contains:
1. 01-manual-camp.png
2. 02-first-gold.png
3. 03-first-improvement.png
4. 04-assistance-1.png
5. 05-assistance-2.png
6. 06-assistance-3.png
7. 07-complete-camp.png
8. 08-assisted-gold.png
9. 09-field-guide.png
10. 10-collect-status.png (new reveal-delay and immediate E-collection check)

The nine original smoke captures are refreshed in Playtest/ at 1366×768. Logs are in Logs/build.log, Logs/responsiveness-smoke.log and Logs/responsiveness-chapter-*.log. Reports are Playtest/simulation-checks.txt, Playtest/progression-checks.txt, Playtest/runtime-checks.txt and each chapter folder's runtime-chapter-checks.txt.

Builds/Windows/Riffle.exe and Builds/Riffle-Windows.zip are the playtest deliverables. Builds, captures, logs and test saves are intentionally ignored by Git.

## Remaining polish

The pan's harsh shadow wedges and excessively lifted high-level riffles have been corrected. Broad environment shadows, the simple water treatment and more expressive helper animation remain optional polish. Taller aspect ratios crop peripheral scenery, while keeping the pan and progression usable. No hardware performance benchmark or full-duration human playthrough is claimed.
