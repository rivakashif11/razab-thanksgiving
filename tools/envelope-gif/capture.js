const { chromium } = require('playwright');
const { pathToFileURL } = require('url');
const path = require('path');
const fs = require('fs');

const FPS = 12, DUR = 7, N = FPS * DUR;

(async () => {
  const dir = __dirname;
  const out = path.join(dir, 'frames');
  fs.rmSync(out, { recursive: true, force: true });
  fs.mkdirSync(out, { recursive: true });

  // use the installed Chrome rather than downloading a bundled Chromium
  const browser = await chromium.launch({ channel: 'chrome' });
  const page = await browser.newPage({
    viewport: { width: 600, height: 360 },
    deviceScaleFactor: 1
  });
  await page.goto(pathToFileURL(path.join(dir, 'scene.html')).href);
  await page.waitForFunction(() => window.__fontsReady === true, null, { timeout: 20000 });

  const stage = page.locator('#stage');
  for (let i = 0; i < N; i++) {
    const t = i / FPS;
    await page.evaluate((tt) => window.setT(tt), t);
    await page.waitForTimeout(10);          // let style + layout settle
    await stage.screenshot({ path: path.join(out, String(i).padStart(3, '0') + '.png') });
  }

  await browser.close();
  console.log('captured ' + N + ' frames at ' + FPS + ' fps (' + DUR + 's)');
})().catch(e => { console.error('FAILED:', e.message); process.exit(1); });
