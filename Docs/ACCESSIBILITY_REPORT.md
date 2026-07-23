# Accessibility Testing Report

Scope: the duty list asks to **perform accessibility testing**, not to build new
accessibility features - this is a testing pass against the game as it exists
(a bare gameplay template with **no menu, HUD, or settings screen of any
kind**), producing pass/fail findings and backlog tickets for real gaps.
Building a full accessibility feature set (remappable controls UI, colorblind
filters, etc.) is out of scope for this pass and is called out below as
backlog, not silently implemented.

Method: static/code review of every script in
`Assets/Space Shooter Template FREE/Scripts` plus the WebGL build smoke-tested
in `CROSS_PLATFORM_REPORT.md`, checked against WCAG-style categories adapted
for a real-time arcade shooter.

| # | Check | Result | Notes |
|---|---|---|---|
| 1 | Enemy/Player/Bonus distinguishable without color | **Pass** | Distinguished primarily by sprite shape (ship vs. saucer vs. power-up ring), not color alone. The new Boss is the same base sprite scaled 3.5x - size and context distinguish it even without its red tint. |
| 2 | Single input modality with no alternative | **Fail** | `PlayerMoving.cs` only supports mouse-drag (desktop) or touch-drag (mobile), gated by `#if UNITY_STANDALONE/EDITOR` vs `#if UNITY_IOS/ANDROID`. There is no keyboard (arrow keys/WASD) alternative on desktop at all - a user who can't use a mouse precisely (motor impairment, or simply prefers keyboard) has no way to play. See ticket in Bug Tracking. |
| 3 | Critical information conveyed by audio alone | **Pass** | No script plays a sound that is the *only* carrier of gameplay-critical information (no audio-only warnings found in code); all feedback (hits, shield depletion, destruction) is driven by VFX/state, which is inherently visual. |
| 4 | Seizure risk (rapid flashing effects) | **Not fully verified** | Explosion/hit VFX are Unity ParticleSystems whose exact flash rate wasn't measured frame-by-frame; a single-burst explosion is low-risk by genre convention, but I did not instrument this quantitatively. Recommend a manual pass: watch several explosions in the WebGL build and confirm nothing strobes for a sustained period. |
| 5 | Text legibility / scalable UI text | **N/A** | The project has no UI text anywhere (no HUD, score, menu, or dialogue) - nothing to evaluate here. |
| 6 | Difficulty/assist options (extra lives, slow-mo, invincibility) | **Fail** | `Player.GetDamage` destroys the player on any hit, any damage value, with no health pool and no assist/difficulty toggle. Combined with #2, this makes the game fully inaccessible to anyone who can't precisely mouse/touch-drag under time pressure. Out of scope to fix here (would be a real feature, not a bug), logged as backlog. |
| 7 | Reduced-motion consideration | **Fail (minor)** | `RepeatingBackground`/`DirectMoving`/planets are continuously-scrolling parallax with no way to reduce or disable motion; low severity for this genre, but noted for completeness. |
| 8 | Colorblind-specific simulation pass | **Not executed** | No colorblind-simulation tool was run against the actual rendered build in this session; row 1's structural (shape-based) distinction is a reasonable proxy but isn't a substitute for actually viewing the build through a deuteranopia/protanopia filter. Recommend as a manual follow-up once on a machine with such a tool (e.g. browser extension, or Chrome DevTools' vision-deficiency emulation) against the WebGL build. |

## Summary

The game is honestly **not accessible** to players who can't precisely
mouse/touch-drag in real time - there's no keyboard alternative and no
difficulty/assist option, and single-hit death removes any margin for error.
Nothing here is a "bug" in the sense of broken code; it's the template's
existing design, unchanged by this pass. The two concrete, cheap wins
(keyboard input alternative; at minimum an invincibility/assist toggle) are
filed in `BUG_TRACKING.md` as backlog enhancements rather than implemented
in this pass, since building them is a feature-scope decision, not a QA fix,
and wasn't requested as such.
