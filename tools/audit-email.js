const { chromium } = require('playwright');
const { pathToFileURL } = require('url');
const path = require('path');
const fs = require('fs');

const FILE = path.resolve(__dirname, '..', 'index.html');
const OUT = process.env.AUDIT_OUT || require('os').tmpdir();
const widths = (process.argv[2] || '700,600,390').split(',').map(Number);

(async () => {
  const browser = await chromium.launch({ channel: 'chrome' });
  const report = {};
  for (const W of widths) {
    const page = await browser.newPage({ viewport: { width: W, height: 900 }, deviceScaleFactor: 1 });
    await page.goto(pathToFileURL(FILE).href);
    await page.waitForLoadState('load');
    await page.waitForTimeout(600);

    const r = await page.evaluate(() => {
      const cs = (el) => getComputedStyle(el);
      const uniq = (arr) => [...new Set(arr)];
      const q = (s) => [...document.querySelectorAll(s)];
      const out = {};
      const vw = document.documentElement.clientWidth;

      // 1. width / overflow
      out.viewport = vw;
      out.scrollWidth = document.documentElement.scrollWidth;
      out.horizontalScroll = document.documentElement.scrollWidth > vw;
      out.wrapWidths = uniq(q('table.rz-wrap').map(t => Math.round(t.getBoundingClientRect().width)));
      out.wrapCount = q('table.rz-wrap').length;
      out.overflowing = q('body *').filter(e => { const b = e.getBoundingClientRect(); return b.width > 0 && (b.right > vw + 0.5 || b.left < -0.5); })
        .map(e => e.tagName + '.' + (e.className || '') + ' ' + Math.round(e.getBoundingClientRect().left) + '..' + Math.round(e.getBoundingClientRect().right)).slice(0, 8);

      // 2. side padding of every section content cell
      out.sidePad = uniq(q('td.rz-px').filter(e => !e.closest('table.rz-card')).map(e => cs(e).paddingLeft + '/' + cs(e).paddingRight));

      // 3. typography groups
      const sig = (el, props) => props.map(p => cs(el)[p]).join(' | ');
      out.h2 = uniq(q('h2.rz-h2').map(e => sig(e, ['fontFamily', 'fontSize', 'lineHeight', 'fontWeight', 'color', 'letterSpacing'])));
      out.h2count = q('h2.rz-h2').length;
      out.h3 = uniq(q('h3.rz-h3').map(e => sig(e, ['fontFamily', 'fontSize', 'lineHeight', 'fontWeight', 'color', 'letterSpacing'])));
      out.eyebrow = uniq(q('p.rz-eyebrow').map(e => sig(e, ['fontFamily', 'fontSize', 'lineHeight', 'fontWeight', 'color', 'letterSpacing', 'textTransform', 'fontStyle'])));
      out.eyebrowCount = q('p.rz-eyebrow').length;
      out.body = uniq(q('p.rz-body').map(e => sig(e, ['fontFamily', 'fontSize', 'lineHeight', 'fontWeight', 'color'])));
      out.bodyCount = q('p.rz-body').length;
      out.btn = uniq(q('a.rz-btn').map(e => sig(e, ['fontSize', 'fontWeight', 'letterSpacing', 'paddingTop', 'paddingRight', 'borderRadius', 'backgroundColor', 'color'])));
      out.btnCount = q('a.rz-btn').length;
      out.btnWidthMatchesLabel = q('a.rz-btn').map(e => e.textContent + ':' + Math.round(e.getBoundingClientRect().width) + 'x' + Math.round(e.getBoundingClientRect().height));

      // 4. cards
      out.cards = q('table.rz-card').map(e => sig(e, ['borderTopWidth', 'borderTopStyle', 'borderTopColor', 'borderTopLeftRadius']));
      out.borderStyles = uniq(q('body *').filter(e => e.tagName !== 'IMG').flatMap(e => {
        const s = cs(e); return ['Top', 'Right', 'Bottom', 'Left'].filter(d => parseFloat(s['border' + d + 'Width']) > 0 && s['border' + d + 'Style'] !== 'none')
          .map(d => s['border' + d + 'Width'] + ' ' + s['border' + d + 'Style'] + ' ' + s['border' + d + 'Color']);
      }));

      // 5. images
      out.images = q('img').map(e => ({
        src: e.getAttribute('src').replace('assets/', ''), wAttr: e.getAttribute('width'), hAttr: e.getAttribute('height'),
        display: cs(e).display, maxW: cs(e).maxWidth, rendered: Math.round(e.getBoundingClientRect().width) + 'x' + Math.round(e.getBoundingClientRect().height),
        radius: cs(e).borderTopLeftRadius
      }));

      // 6. stacking: for each product/loaf row, are the image and text stacked (same left) or side by side?
      out.rows = q('td.rz-col-img').map(td => {
        const img = td.getBoundingClientRect(); const txt = td.parentElement.querySelector('.rz-col-txt').getBoundingClientRect();
        return (txt.top >= img.bottom - 1 ? 'stacked' : 'side-by-side') + ' img@' + Math.round(img.left) + '..' + Math.round(img.right) + ' txt@' + Math.round(txt.left) + '..' + Math.round(txt.right) + (txt.top >= img.bottom - 1 ? '' : ' (img ' + (img.left < txt.left ? 'left' : 'right') + ')');
      });

      // 7. vertical gaps between section bands
      const bands = [...document.querySelectorAll('body > table > tbody > tr')].map(tr => { const b = tr.getBoundingClientRect(); return { h: Math.round(b.height), top: Math.round(b.top) }; });
      out.bandHeights = bands.map(b => b.h);
      out.pageHeight = document.documentElement.scrollHeight;
      return out;
    });
    report[W] = r;
    await page.screenshot({ path: path.join(OUT, `audit-${W}.png`), fullPage: true });
    await page.close();
  }
  fs.writeFileSync(path.join(OUT, 'audit.json'), JSON.stringify(report, null, 2));
  await browser.close();

  // ----- printable summary -----
  for (const W of widths) {
    const r = report[W];
    console.log(`\n=================== viewport ${W}px ===================`);
    console.log(`horizontal scroll: ${r.horizontalScroll} (scrollWidth ${r.scrollWidth} / viewport ${r.viewport})`);
    console.log(`section tables: ${r.wrapCount} x width(s) ${JSON.stringify(r.wrapWidths)}`);
    console.log(`elements overflowing viewport: ${r.overflowing.length ? '\n   ' + r.overflowing.join('\n   ') : 'none'}`);
    console.log(`section side padding (left/right): ${JSON.stringify(r.sidePad)}`);
    const grp = (name, arr, n) => console.log(`${name} (${n}): ${arr.length === 1 ? 'IDENTICAL -> ' + arr[0] : 'DIFFERENT (' + arr.length + ' variants):\n   ' + arr.join('\n   ')}`);
    grp('h2 headings', r.h2, r.h2count); grp('h3 product names', r.h3, r.h3.length ? 4 : 0);
    grp('eyebrow labels', r.eyebrow, r.eyebrowCount); grp('body text', r.body, r.bodyCount); grp('buttons', r.btn, r.btnCount);
    console.log(`button sizes: ${r.btnWidthMatchesLabel.join(', ')}`);
    console.log(`cards: ${JSON.stringify(r.cards)}`);
    console.log(`distinct border styles in use: ${JSON.stringify(r.borderStyles)}`);
    console.log('images:'); r.images.forEach(i => console.log(`   ${i.src.padEnd(30)} attr ${i.wAttr}x${i.hAttr}  display:${i.display}  max-width:${i.maxW}  rendered ${i.rendered}  radius ${i.radius}`));
    console.log('rows:'); r.rows.forEach(x => console.log('   ' + x));
    console.log(`band heights: ${r.bandHeights.join(', ')}   page height ${r.pageHeight}`);
  }
})().catch(e => { console.error('FAILED', e); process.exit(1); });
