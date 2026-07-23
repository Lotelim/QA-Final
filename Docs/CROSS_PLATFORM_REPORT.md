# Cross-Platform Testing Report

Scope: WebGL and mobile (Android/iOS), per the STP's non-functional testing section.
This report is explicit about what was **actually executed** in this environment
versus what is **documented for manual execution** by whoever has access to a
browser on another machine or a physical/emulated mobile device.

## WebGL - executed

A real WebGL build was produced from `Level_1`/`Level_2` (Unity 6000.2.13f1,
`Assets/Editor/LevelAuthoring/WebGLBuildScript.cs`, `Tools -> QA Final -> Build WebGL`),
build result: **Succeeded, 0 errors**, ~16.2 MB total.

It was then actually served locally (`Tools/serve-webgl.ps1`, a small static file
server - see note on compression below) and loaded in a real browser tab, where I
verified, via the browser's console/network panels and direct canvas inspection
(not just "it built"):

- All build artifacts (`WebGL.loader.js`, `WebGL.framework.js`, `WebGL.data`,
  `WebGL.wasm`) and template assets returned `200 OK` - no missing files.
- The Unity engine actually booted inside the page: WebGL2 context created,
  PhysX physics backend selected, Input Manager initialized, audio context
  resumed - **zero console errors** at load or after simulated input.
- Simulated a player-drag input (mouse-drag control scheme) on the canvas -
  no errors triggered.
- Captured the live canvas twice, several seconds apart, and confirmed the
  frames differ (the scrolling starfield background is genuinely animating,
  not a static/frozen frame) - i.e. the game loop is really running, not just
  displaying a splash screen.

**Limitation to be upfront about:** this environment has no way to render the
Browser pane to a full-resolution screenshot (the pane wasn't displayed on the
host screen during this session), so I could not visually confirm the Player
ship or enemies on-screen at readable resolution - only the animated
background was clearly visible in the low-res captures I could extract. The
absence of console errors during gameplay-relevant frames is good evidence
nothing is silently broken, but a human should still eyeball the actual build
once (`Tools/serve-webgl.ps1`, then open `http://localhost:8850`) before
calling WebGL fully signed off.

**Build note:** Unity's default WebGL compression (gzip) requires the web
server to send `Content-Encoding: gzip`, which most simple static servers
(including the one used here) don't do out of the box. `Tools/serve-webgl.ps1`
works around this by decompressing `Build/*.gz` once and pointing
`index.html` at the plain files - fine for local testing, but a real
deployment (itch.io, GitHub Pages, etc.) needs a host that either serves
gzip correctly or a build made with WebGL Compression Format set to
"Disabled".

## WebGL - manual (you should still do this)

- [ ] Test in actual Chrome, Firefox, and Safari (not just the engine used to
      smoke-test here) - WebGL/WASM support and audio-unlock behavior differ
      slightly per browser.
- [ ] Test on a slower/throttled connection - the `.data`/`.wasm` payloads are
      tens of MB; confirm the loading bar behaves correctly and the page
      doesn't appear to hang.
- [ ] Resize the browser window / test at a few common resolutions - confirm
      `PlayerMoving`'s border-clamping and `Boundary`'s viewport-relative
      sizing still keep the player and enemies on-screen (this is exactly
      the kind of thing the automated `PlayerMovingTests`/`BoundaryTests`
      cover at the unit level, but a real aspect ratio change is worth an
      eyeball pass too).
- [ ] Play through an entire Level_1 -> Level_2 transition and confirm no
      visual hitch or double-audio on scene load.

## Mobile (Android/iOS) - not executed, documented for manual testing

No installed Unity Editor in this environment has both the WebGL module
(needed above) and a matching mobile module, and there is no physical device
or emulator available here. `PlayerMoving.cs` already has touch-input code
behind `#if UNITY_IOS || UNITY_ANDROID`, so the mobile control path exists in
source but **was not exercised**. Manual test procedure for whoever has a
device/emulator:

1. Switch build target to Android (or iOS) in Build Settings, build & deploy
   `Level_1` to a real device or emulator.
2. **Touch controls**: drag the ship with a single finger; confirm it tracks
   the touch smoothly and stays clamped inside the screen at the device's
   actual aspect ratio (many phones are far taller/narrower than the 16:9
   the desktop camera assumes).
3. **Performance**: watch for frame drops once multiple waves + power-ups +
   planets are on screen simultaneously - `PoolingController` exists in this
   codebase specifically for this kind of churn but (see Bug Tracking) is
   currently dead code, so expect GC pressure from the constant
   Instantiate/Destroy pattern; note the actual FPS you observe.
4. **Screen sizes/notches**: test on at least one device with a notch/cutout
   and confirm no UI or gameplay-critical sprite is obscured.
5. **Orientation**: confirm the game behaves reasonably if the device is
   rotated (the template assumes landscape; note if portrait needs to be
   locked in Player Settings).
6. Record pass/fail and device/OS version for each check in the STD's manual
   test log.
