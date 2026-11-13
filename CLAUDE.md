# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A single-email marketing project: Razab USA's Thanksgiving 2026 campaign ("The Harvest Table", 20% off with code `HARVEST`, Nov 20–27 2026, ends 11:59 PM EST Nov 27). The deliverable is a table-based HTML email plus its image assets.

## Files that matter

- **The email is generated: edit `tools/build-email.js`, not the HTML.** `node tools/build-email.js` writes `thanksgiving-2026-email.html`, `index.html` and `email.html` (byte-identical; `index.html` is what Vercel serves via `vercel.json`). Hand edits to those three files are overwritten on the next run. The script holds one set of style tokens (colors, type, spacing, button, card) and small builders (`band`, `gap`, `eyebrow`, `h2`, `body`, `button`, `productRow`), so every section is formatted the same way by construction. Copy and links live in the same file. Node is installed (`winget install OpenJS.NodeJS.LTS`); in a fresh shell, put `C:\Program Files\nodejs` on PATH.
- `tools/audit-email.js` — Playwright audit: `node tools/audit-email.js 700,600,390` (needs `npm i playwright` and the installed Chrome). For each width it checks horizontal scroll, section-table width, side padding, and that all h2s, eyebrows, body text, buttons, cards and images compute identically, then saves full-page screenshots to `AUDIT_OUT` (default: the OS temp dir). Run it after every change to the generator.
- `halloween-ref.html` — the earlier Halloween campaign email this one was modeled on. Reference only; don't edit.
- `assets/` — every image the email references via relative `assets/...` paths. Many files there are unused leftovers (`raw-*`, `*-backup`, `fade-*`, the old clock GIFs, `offer-showpiece.*`, files with spaces in the name). The email uses exactly: `gifs/envelope-open.gif`, `hero-thanksgiving.jpg`, `p-host/p-carry/p-keeper/p-loaf.jpg`, `pantry-thanksgiving.jpg`, `harvest-footer-1200.jpg`, `razab-logo.png`, `razab-logo-light.png`.
- `PROMPTS.md` — Midjourney/DALL-E prompts for the photographic assets. `README.md` §2 still describes the original dark palette; trust this file and the generator.

## Design rules (all enforced by the generator's tokens)

Concept: "The Harvest Table", a printed dinner invitation on warm linen. Keep it clearly different from `halloween-ref.html`: no dark hero, no stat strip, no Roman numerals, no clock or countdown imagery, no repeated flourish dividers.

- **Layout:** every section is a 600px table (`rz-wrap`, class required even on tables built outside `band()`/`gap()`, or the mobile `width:100%` override misses it and causes horizontal scroll). The full-width margin outside that 600px column is always solid `SIDE_BG` (`#000000`) — same color behind every section, like a mailbox background — while the 600px column itself keeps the section's own color (linen → white → canvas → footer painting → chestnut). Side padding is **40px desktop, 20px mobile** (`rz-px`). The space between sections is one solid **56px** row of `SIDE_BG` (`gap()`), no gradient.
- **Type:** headings are Playfair Display 600 in `#8F544A` with -0.01em: h1 46/50 (36 mobile), every section h2 32/38 (28 mobile), product names 22/28. Body is Inter 400 15/26 `#5A3D36`. Eyebrow labels are Inter 600 12/18 `#8F544A`, sentence case. Uppercase letter-spaced text appears only in the nav and the footer links. Fallbacks are Georgia/Arial (forced in Outlook by the MSO style block; add `rz-serif` to Playfair text that isn't a heading).
- **Buttons:** one style: solid Forest `#64875D`, 4px radius, 16px/36px padding, Inter 700 13px, 0.08em, with a VML twin for Outlook. White on Forest is 4.07:1, below 4.5; `#53734D` would pass (5.35:1).
- **Cards and borders:** the hero card (photo, copy, envelope) also has a soft shadow; images have a 6px radius. Template look: every section is a card with a 1px `#EAE3D9` border and 12px radius (`card()` in the generator, 24px inner padding, 16px on mobile), matching the live razab-thanksgiving.vercel.app layout; honey `#F0D79F` is used only for hairlines and the hero card shadow frame. A table needs `border-collapse:separate` for its radius to show.
- **Images:** width/height attributes, `display:block`, `max-width:100%`; image cells use `font-size:0; line-height:0; mso-line-height-rule:exactly`. Product photos are 896×1200 (shown 220×295); the footer painting is 1200 wide, shown at 600.
- **Product rows:** the image alternates left/right via `dir="rtl"` so it always stacks first on mobile. The breakpoint is 620px, so a 600px window already gets the mobile layout; check the desktop layout at 700px.
- **Limits:** keep the HTML under ~100 KB (Gmail clips at 102 KB; currently ~42 KB).
- Keep the preheader's `&#847;&zwnj;&nbsp;` padding. Every link carries `utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026` plus a per-link `utm_content` (`nav_home`, `nav_need`, `nav_about`, `logo`, `hero`, `envelope`, `p_host`, `p_carry`, `p_keeper`, `p_loaf`, `loaf`, `loaf_link`, `footer`, `footer_store`), with `&amp;` in hrefs. Merge tags are Klaviyo-style: `{{ unsubscribe_url }}`, `{{ preferences_url }}`, `{{ view_in_browser_url }}`, `{{ company_address }}`.

## Asset generator scripts

- **`python generate_placeholders.py` overwrites real artwork.** It writes flat placeholder images over `hero-thanksgiving.jpg`, the product photos, `pantry-thanksgiving.jpg`, `harvest-footer.jpg` and others, using the old dark palette. Don't run it unless the user explicitly wants placeholders back. `generate_extra.py` imports from it and regenerates the `t-*.png` title images. Both must run from the project root because they write to relative `assets/` paths.
- `create_gifs.cs` (System.Drawing) and `build_premium_gifs.cs` (WPF, newer sage palette) render the animated GIFs: candle glow, vine banner, countdown clock. `build_premium_gifs.cs` also has `BuildClockBanner` and `BuildClockLight` (their clock GIFs are no longer used), `BuildLinen` (a background texture, no longer used) and `BuildFooterFade` (the footer painting with its four edges faded). The email uses no clock GIFs at all. These files have no `Main`; they're static classes meant to be loaded from Windows PowerShell with `Add-Type` (WPF needs `-ReferencedAssemblies PresentationCore,PresentationFramework,WindowsBase`) and then called, e.g. `[PremiumGifSuite]::BuildFooterFade("$PWD\assets\harvest-footer.jpg", "$PWD\assets\harvest-footer-1200.jpg")`. Date and offer text ("Midnight, Nov 27", "HARVEST") is hard-coded in the clock generators.

## Previewing

Open `index.html` directly in a browser, or deploy the folder to Vercel as a static site. For screenshots, use Playwright (`tools/audit-email.js`) or headless Chrome (`--headless=new --screenshot --window-size=700,5000`). Headless Chrome won't go narrower than 500px, so take narrow shots with Playwright, which accepts any viewport. Before an ESP upload, the relative `assets/` image paths must be replaced with hosted CDN URLs.

## Rebuilding the showpiece GIF (no longer used in the email)

`PremiumGifSuite.BuildShowpieceFrames(srcJpg, outDir)` in `build_premium_gifs.cs` renders 48 PNG frames (4 s at 12 fps, 560×560). Frame 0 is the untouched photo, because Outlook shows only the first frame, and every effect (candle-glow dip to 85%, rising embers and dust, two drifting leaves) is zero at t=0 and t=4 so the loop is seamless. Encode with ffmpeg, installed via `winget install Gyan.FFmpeg`:

```
ffmpeg -framerate 12 -i outDir/f%03d.png -filter_complex "[0:v]split[a][b];[a]palettegen=max_colors=256:stats_mode=full[p];[b][p]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle" -loop 0 assets/gifs/offer-showpiece.gif
```

Use Bayer dithering. Error-diffusion dithers (sierra, floyd) re-dither the whole frame every time and push the file past 1 MB; Bayer plus `diff_mode=rectangle` keeps it around 460 KB.

## Rebuilding the envelope GIF

`assets/gifs/envelope-open.gif` (600×360, 7 s, 12 fps, loops forever) is a self-opening envelope: float + seal glint → wax seal cracks → flap rotates open on the honey-cream lining → cream card rises and scales up → text fades in → it all returns to the closed envelope so the loop is seamless. Frame 1 is the clean closed envelope, because Outlook shows only the first frame.

Sources live in `tools/envelope-gif/`: `scene.html` (the whole scene in CSS/SVG, with Playfair and Inter as local woff2 in `fonts/`) and `capture.js`. The scene has no CSS animations — `window.setT(t)` sets every element's state from the time in seconds, so frames are exact and reproducible. `scene.html?t=2.4` renders a single frame in a browser.

Tooling (already installed via winget/npm): Node, `playwright`, `gifsicle` (npm, binary at `node_modules/gifsicle/vendor/gifsicle.exe`) and `ffmpeg`. Playwright drives the installed Chrome with `channel: 'chrome'`, so no bundled browser download is needed.

```
cd tools/envelope-gif && npm install playwright gifsicle && node capture.js   # 84 PNGs -> frames/
ffmpeg -y -framerate 12 -i frames/%03d.png -filter_complex "[0:v]split[a][b];[a]palettegen=max_colors=256:stats_mode=full[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle" -loop 0 raw.gif
gifsicle --lossy=40 -O3 raw.gif -o ../../assets/gifs/envelope-open.gif
```

`bayer_scale=5` keeps the soft shadow smooth without the crosshatch that `bayer_scale=3` leaves on the flat envelope body; `dither=none` is smaller but contours the shadow. `--lossy=80` mushes the wax-seal emblem — keep it at 40. gifsicle merges the identical hold frames (84 → 55) and keeps the 7 s duration, so count frames by timestamp (`-vf fps=12`), not by GIF frame index.
