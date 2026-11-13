// Generates thanksgiving-2026-email.html from one set of style tokens, so every section is formatted identically.
// Run:  node tools/build-email.js        (writes thanksgiving-2026-email.html, index.html and email.html)
const fs = require('fs');
const path = require('path');

/* ---------------- design tokens (the only place styles are defined) ---------------- */
const C = {
  linen: '#F6F0E6', white: '#FFFFFF', canvas: '#FAF7F2', chestnut: '#8F544A', ink: '#5A3D36',
  forest: '#64875D', honey: '#F0D79F', burnt: '#C87F4E', cream: '#EAE3D9'
};
const SANS = "'Inter',Arial,sans-serif";
const SERIF = "'Playfair Display',Georgia,serif";
const PAD = 40;           // side padding, desktop (20 on mobile via .rz-px)
const GAP = 56;           // vertical spacing between sections
const BASE = '?utm_source=email&amp;utm_medium=newsletter&amp;utm_campaign=thanksgiving2026&amp;utm_content=';
const RAW  = '?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026&utm_content=';
const u = (p, c) => `https://razab.com${p}${BASE}${c}`;
const uRaw = (p, c) => `https://razab.com${p}${RAW}${c}`;

const S = {
  eyebrow: `margin:0 0 8px 0; font-family:${SANS}; font-size:12px; line-height:18px; font-weight:600; color:${C.chestnut};`,
  h2:      `margin:0 0 16px 0; font-family:${SERIF}; font-size:32px; line-height:38px; font-weight:600; letter-spacing:-0.01em; color:${C.chestnut};`,
  h3:      `margin:0 0 8px 0; font-family:${SERIF}; font-size:22px; line-height:28px; font-weight:600; letter-spacing:-0.01em; color:${C.chestnut};`,
  body:    `margin:0 0 16px 0; font-family:${SANS}; font-size:15px; line-height:26px; font-weight:400; color:${C.ink};`,
  offer:   `margin:0 0 24px 0; font-family:${SANS}; font-size:15px; line-height:26px; font-weight:600; color:${C.chestnut};`,
  btn:     `display:inline-block; padding:16px 36px; font-family:${SANS}; font-size:13px; line-height:18px; font-weight:700; letter-spacing:0.08em; color:#FFFFFF; text-decoration:none; background-color:${C.forest}; border-radius:4px;`,
  // Template look: every section is a card with a 1px #EAE3D9 border and 12px radius.
  card:    `border-collapse:separate; border:1px solid ${C.cream}; border-radius:12px; overflow:hidden;`,
  shadow:  `box-shadow:0 12px 32px rgba(90,61,54,0.12);`,
  zero:    'font-size:0; line-height:0; mso-line-height-rule:exactly;',
};

/* ---------------- building blocks ---------------- */
const mso = (inner) => `<!--[if mso]><table role="presentation" width="600" align="center" cellpadding="0" cellspacing="0" border="0"><tr><td><![endif]-->\n${inner}\n      <!--[if mso]></td></tr></table><![endif]-->`;
// Outer margins (outside the 600px column) are always SIDE_BG; the inner 600px table keeps the section's own colour.
const SIDE_BG = '#D8C8B4';
// A thin honey edge dresses up the seam between the chestnut margin and each section's own colour.
const EDGE = `border-left:3px solid ${C.honey}; border-right:3px solid ${C.honey};`;
const band = (bg, rows) => `
  <tr>
    <td align="center" bgcolor="${SIDE_BG}" style="background-color:${SIDE_BG}; padding:0;">
      ${mso(`      <table role="presentation" class="rz-wrap" width="600" cellpadding="0" cellspacing="0" border="0" bgcolor="${bg}" style="width:600px; max-width:600px; background-color:${bg}; ${EDGE}">${rows}
      </table>`)}
    </td>
  </tr>`;
const row = (style, html, cls = 'rz-px') => `
        <tr><td class="${cls}" style="${style}">${html}</td></tr>`;
const gap = (from, to, h = GAP) => `
  <tr>
    <td align="center" height="${h}" bgcolor="${SIDE_BG}" style="height:${h}px; padding:0; ${S.zero} background-color:${SIDE_BG};">
      <table role="presentation" class="rz-wrap" width="600" cellpadding="0" cellspacing="0" border="0" bgcolor="${to}" style="width:600px; max-width:600px; ${EDGE}"><tr><td height="${h}" bgcolor="${to}" style="height:${h}px; padding:0; ${S.zero} background-color:${to};">&nbsp;</td></tr></table>
    </td>
  </tr>`;
// Every section sits in the same card: same border, radius and 24px inner padding (16px on mobile).
const card = (bg, inner, extra = '') => `<table role="presentation" class="rz-card" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="${bg}" style="${S.card} background-color:${bg};${extra}"><tr><td class="rz-cp" style="padding:24px;">${inner}</td></tr></table>`;
const eyebrow = (t, extra = '') => `<p class="rz-eyebrow" style="${S.eyebrow}${extra}">${t}</p>`;
const h2 = (t, extra = '') => `<h2 class="rz-h2" style="${S.h2}${extra}">${t}</h2>`;
const body = (t, extra = '') => `<p class="rz-body" style="${S.body}${extra}">${t}</p>`;
const img = (src, w, h, alt, extra = '', cls = '') =>
  `<img src="assets/${src}" width="${w}" height="${h}" alt="${alt}"${cls ? ` class="${cls}"` : ''} style="display:block; width:${w}px; max-width:100%; height:auto; ${extra}">`;

// One button style everywhere, with a VML twin for Outlook desktop.
const button = (href, rawHref, label, w) => `
<table role="presentation" cellpadding="0" cellspacing="0" border="0"><tr><td>
  <!--[if mso]>
  <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="${rawHref}" style="height:50px;v-text-anchor:middle;width:${w}px;" arcsize="8%" stroke="f" fillcolor="${C.forest}">
    <w:anchorlock/>
    <center style="color:#FFFFFF;font-family:Arial,sans-serif;font-size:13px;font-weight:bold;letter-spacing:1px;">${label}</center>
  </v:roundrect>
  <![endif]-->
  <!--[if !mso]><!-->
  <a class="rz-btn" href="${href}" style="${S.btn}">${label}</a>
  <!--<![endif]-->
</td></tr></table>`;

/* ---------------- content ---------------- */
const products = [
  { id: 'p_host',   p: '/collections/all', img: 'p-host.jpg',  alt: 'Razab 30 Pc Glass Food Storage Container Set',
    label: 'To host',  name: '30 Pc Glass Container&nbsp;Set', desc: 'Fifteen containers, fifteen airtight lids, endless Thanksgiving leftovers.' },
  { id: 'p_carry',  p: '/products/6500-ml-large-food-storage-container', img: 'p-carry.jpg', alt: 'Razab 6500ML Extra Large Food Storage Container with handle lid',
    label: 'To carry', name: '6500ML Extra Large&nbsp;Container', desc: 'A sturdy handle-top lid for carrying casseroles and sides to family dinner.' },
  { id: 'p_keeper', p: '/products/3900ml-large-glass-food-storage-container-with-airtight-lid', img: 'p-keeper.jpg', alt: 'Razab 3900ML Airtight Container',
    label: 'To keep',  name: '3900ML Airtight Container', desc: 'Keep pecan bars, spiced cookies, and rolls bakery-fresh for days.' },
  { id: 'p_loaf',   p: '/collections/glass-loaf-pans-with-lids', img: 'p-loaf.jpg', alt: 'Razab Glass Loaf Pans with Lids',
    label: 'To bake',  name: 'Glass Loaf Pans with Lids', desc: 'Bake pumpkin bread, snap on the lid, and bring it warm to the table.' },
];
const IMG_W = 200, IMG_H = 268, COL_GAP = 24;

// A product row: image on one side, text on the other. rtl rows keep the image first when stacked on mobile.
const productRow = (pr, i, last) => {
  const rtl = i % 2 === 1;
  const href = u(pr.p, pr.id), raw = uRaw(pr.p, pr.id);
  const txtPad = rtl ? `0 ${COL_GAP}px 0 0` : `0 0 0 ${COL_GAP}px`;
  return `
              <tr>
                <td style="padding:0 0 ${last ? 0 : 24}px 0;">
                  ${card(C.white, `<table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"${rtl ? ' dir="rtl"' : ''}>
                    <tr>
                      <td class="rz-stack rz-col-img" width="${IMG_W}" valign="middle"${rtl ? ' dir="ltr"' : ''} style="padding:0; ${S.zero}">
                        <a href="${href}" style="display:block;">${img(pr.img, IMG_W, IMG_H, pr.alt, 'border-radius:6px;', 'rz-img')}</a>
                      </td>
                      <td class="rz-stack rz-col-txt" valign="middle" align="left"${rtl ? ' dir="ltr"' : ''} style="padding:${txtPad}; text-align:left;">
                        ${eyebrow(pr.label)}
                        <h3 class="rz-h3" style="${S.h3}"><a href="${href}" style="color:${C.chestnut}; text-decoration:none;">${pr.name}</a></h3>
                        ${body(pr.desc, 'margin:0 0 8px 0;')}
                        <p class="rz-offer" style="${S.offer}">20% off with HARVEST</p>
                        ${button(href, raw, 'Shop now', 144)}
                      </td>
                    </tr>
                  </table>`)}
                </td>
              </tr>`;
};

const navLink = (href, t) => `<a class="rz-nav-link" href="${href}" style="color:${C.ink}; text-decoration:none;">${t}</a>`;
const diamond = `<span style="color:${C.honey}; font-size:8px; letter-spacing:0; vertical-align:2px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9670;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>`;
const hair = `<table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tr><td height="1" bgcolor="${C.honey}" style="background-color:${C.honey}; height:1px; ${S.zero}">&nbsp;</td></tr></table>`;

/* ---------------- sections ---------------- */
const masthead = band(C.linen,
  row(`padding:${GAP / 2 - 4}px ${PAD}px 16px ${PAD}px; text-align:center; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.ink};`,
    `Trouble viewing the feast? &nbsp;<a href="{{ view_in_browser_url }}" style="color:${C.chestnut}; font-weight:600; text-decoration:underline;">View online</a>`) +
  row(`padding:0 ${PAD}px 32px ${PAD}px; text-align:center; font-family:${SANS}; font-size:12px; line-height:18px; font-weight:600; letter-spacing:0.14em; text-transform:uppercase; white-space:nowrap; color:${C.ink};`,
    navLink(u('/', 'nav_home'), 'Home') + diamond + navLink(u('/pages/fit-finder', 'nav_need'), 'Shop by Need') + diamond + navLink(u('/pages/about-us', 'nav_about'), 'About Us'), 'rz-px rz-nav') +
  row(`padding:0 ${PAD}px;`, `
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tr>
            <td valign="middle" style="${S.zero}">${hair}</td>
            <td class="rz-logo-cell" width="152" align="center" valign="middle" style="padding:0 32px; ${S.zero}">
              <a href="${u('/', 'logo')}" style="text-decoration:none; display:block;">${img('razab-logo.png', 152, 37, 'RAZAB', `margin:0 auto; color:${C.chestnut}; font-family:${SERIF}; font-size:24px; letter-spacing:8px;`)}</a>
            </td>
            <td valign="middle" style="${S.zero}">${hair}</td>
          </tr></table>`) +
  row(`padding:8px ${PAD}px 0 ${PAD}px; text-align:center;`, eyebrow('Premium glass kitchenware', 'margin:0;'))
);

// One invitation card: hero photo, hero copy, then the envelope on linen. 1px honey border, 6px radius.
const heroCard = band(C.linen, row(`padding:0 ${PAD}px;`, `
          <table role="presentation" class="rz-card" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="${C.white}" style="${S.card} background-color:${C.white}; ${S.shadow}">
            <tr><td style="padding:0; ${S.zero}">
              <a href="${u('/collections/all', 'hero')}" style="display:block;">${img('hero-thanksgiving.jpg', 518, 518, 'Thanksgiving Sale 20% Off — Use Code: HARVEST. Shop Now and Save 20% on Razab Borosilicate Glass Containers', 'border-radius:11px 11px 0 0;', 'rz-fluid rz-img')}</a>
            </td></tr>
            <tr><td class="rz-px" align="center" bgcolor="${C.white}" style="padding:32px ${PAD}px 24px ${PAD}px; background-color:${C.white};">
              <h1 class="rz-h1" style="margin:0 0 16px 0; font-family:${SERIF}; font-size:46px; line-height:50px; font-weight:600; letter-spacing:-0.01em; color:${C.chestnut};">Feast Today. <em style="font-style:italic; font-weight:500;">Fresh</em>&nbsp;All&nbsp;Week.</h1>
              ${body('From oven to dining table to fridge, Razab borosilicate glass locks in holiday flavor without a single plastic container in&nbsp;sight.', 'max-width:440px; margin:0 auto 24px auto;')}
              ${eyebrow('20% off ends November 27', 'margin:0;')}
            </td></tr>
            <tr><td height="32" bgcolor="${C.linen}" style="height:32px; padding:0; ${S.zero} background-color:${C.linen};">&nbsp;</td></tr>
            <tr><td bgcolor="${C.linen}" style="padding:0; background-color:${C.linen}; ${S.zero}">
              <a href="${u('/collections/all', 'envelope')}" style="display:block;">${img('gifs/envelope-open.gif', 518, 339, 'An invitation opens: Hurry, the Harvest Sale is live — 20% off every glass container with code HARVEST, ending midnight, November 27.', `border-radius:0 0 11px 11px; color:${C.chestnut}; font-family:${SANS}; font-size:14px; line-height:20px;`, 'rz-fluid rz-img')}</a>
            </td></tr>
          </table>`));

const story = band(C.white, row(`padding:0 ${PAD}px;`, card(C.white, `
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tr>
            <td class="rz-story" align="left" style="padding:0 0 0 24px; border-left:1px solid ${C.honey};">
              ${eyebrow('<span style="white-space:nowrap;">November 20 — November 27</span> &nbsp;·&nbsp; <span style="white-space:nowrap;">Limited time only</span>')}
              ${h2('Gather round. Leave the <em style="font-style:italic; font-weight:500;">plastic tubs</em> behind.', 'max-width:440px;')}
              ${body('Candlelight, second helpings, and a fridge full of Thanksgiving favorites — without a single stained plastic container in sight. Razab borosilicate glass goes straight from freezer to oven to dining table, locks in every flavor airtight, and lets you see every leftover at a glance.', 'max-width:456px; margin:0;')}
            </td>
          </tr></table>`)));

const collection = band(C.white,
  row(`padding:0 ${PAD}px 24px ${PAD}px; text-align:left;`,
    h2('The Harvest Collection', 'margin:0 0 4px 0;') + body('Curated for the table', 'margin:0;')) +
  row(`padding:0 ${PAD}px;`, `
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">${products.map((p, i) => productRow(p, i, i === products.length - 1)).join('')}
          </table>`) +
  row(`padding:24px ${PAD}px 0 ${PAD}px; text-align:center;`, card(C.white, `
          <p class="rz-body" style="${S.body} margin:0; text-align:center;"><span style="white-space:nowrap;">20% off every container&nbsp;&nbsp;·</span> &nbsp;<span style="white-space:nowrap;">Nov 20 to 27&nbsp;&nbsp;·</span> &nbsp;<span style="white-space:nowrap;">Ends midnight</span></p>`)));

const loafHref = u('/collections/glass-loaf-pans-with-lids', 'loaf');
const loafBtn = u('/collections/glass-loaf-pans-with-lids', 'loaf_link');
const loaf = band(C.canvas, row(`padding:0 ${PAD}px;`, card(C.canvas, `
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tr>
            <td class="rz-stack rz-col-img" width="${IMG_W}" valign="top" style="padding:0; ${S.zero}">
              <a href="${loafHref}" style="display:block;">${img('pantry-thanksgiving.jpg', IMG_W, IMG_H, 'Golden-crusted pumpkin and cranberry-walnut loaves cooling in Razab glass loaf pans', 'border-radius:6px;', 'rz-img')}</a>
            </td>
            <td class="rz-stack rz-col-txt" valign="top" align="left" style="padding:0 0 0 ${COL_GAP}px; text-align:left;">
              ${eyebrow('Fresh from the oven')}
              ${h2('The Thanksgiving Loaf&nbsp;Pans')}
              ${body('Spiced pumpkin loaf, cranberry-orange bread, warm stuffing bakes — bake it right in the glass, snap on the lid, and carry it straight to the dinner table. High-borosilicate glass handles oven heat effortlessly and shows off every golden crust.', 'margin:0 0 24px 0;')}
              ${button(loafBtn, uRaw('/collections/glass-loaf-pans-with-lids', 'loaf_link'), 'Bake for the feast', 226)}
            </td>
          </tr></table>`)));

const painting = `
  <tr>
    <td align="center" bgcolor="${SIDE_BG}" style="background-color:${SIDE_BG}; padding:0; ${S.zero}">
      ${mso(`      <table role="presentation" class="rz-wrap" width="600" cellpadding="0" cellspacing="0" border="0" bgcolor="${C.chestnut}" style="width:600px; max-width:600px; background-color:${C.chestnut}; ${EDGE}">
        <tr><td style="padding:0; ${S.zero}">${img('harvest-footer-1200.jpg', 600, 335, 'Scenic autumn harvest countryside landscape at golden sunset', '', 'rz-fluid rz-img')}</td></tr>
      </table>`)}
    </td>
  </tr>`;

const fText = (t, extra = '') => `<p style="margin:0 0 24px 0; font-family:${SANS}; font-size:15px; line-height:26px; color:${C.canvas};${extra}">${t}</p>`;
const footer = band(C.chestnut, row(`padding:40px ${PAD}px ${GAP}px ${PAD}px; text-align:center;`, `
          <a href="${u('/', 'footer')}" style="text-decoration:none; display:inline-block;">${img('razab-logo-light.png', 136, 34, 'RAZAB', `margin:0 auto 24px auto; color:${C.canvas}; font-family:${SERIF}; font-size:22px; letter-spacing:8px;`)}</a>
          <p class="rz-serif" style="margin:0 0 8px 0; font-family:${SERIF}; font-style:italic; font-weight:400; font-size:22px; line-height:28px; color:${C.canvas};">Happy Thanksgiving, from our hearth to yours.</p>
          ${fText('Bake with joy. Feast with family. Preserve beautifully.')}
          <p style="margin:0 0 32px 0; font-family:${SANS}; font-size:12px; line-height:18px; letter-spacing:0.14em; text-transform:uppercase; font-weight:600;">
            <a class="rz-foot-link" href="https://instagram.com/razabhomegoods" style="color:${C.canvas}; text-decoration:none;">Instagram</a><span style="color:${C.cream};">&nbsp;&nbsp;&nbsp;·&nbsp;&nbsp;&nbsp;</span><a class="rz-foot-link" href="https://facebook.com/RazabHomeGoods" style="color:${C.canvas}; text-decoration:none;">Facebook</a><span style="color:${C.cream};">&nbsp;&nbsp;&nbsp;·&nbsp;&nbsp;&nbsp;</span><a class="rz-foot-link" href="${u('/', 'footer_store')}" style="color:${C.canvas}; text-decoration:none;">razab.com</a>
          </p>
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:456px; margin:0 auto 32px auto; border-top:1px solid ${C.honey}; border-bottom:1px solid ${C.honey};"><tr>
            <td class="rz-sum rz-sum-first" width="33%" align="center" style="padding:16px 8px;"><p style="margin:0; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.cream};">Promo code</p><p style="margin:0; font-family:${SANS}; font-size:15px; line-height:26px; font-weight:700; color:${C.canvas};">HARVEST</p></td>
            <td class="rz-sum" width="34%" align="center" style="padding:16px 8px; border-left:1px solid ${C.honey}; border-right:1px solid ${C.honey};"><p style="margin:0; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.cream};">Savings</p><p style="margin:0; font-family:${SANS}; font-size:15px; line-height:26px; font-weight:700; color:${C.canvas};">20% Off</p></td>
            <td class="rz-sum" width="33%" align="center" style="padding:16px 8px;"><p style="margin:0; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.cream};">Expires</p><p style="margin:0; font-family:${SANS}; font-size:15px; line-height:26px; font-weight:700; color:${C.canvas};">Nov 27 · 11:59 PM</p></td>
          </tr></table>
          <p style="margin:0 auto 16px auto; max-width:420px; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.canvas};">One use per customer &nbsp;·&nbsp; Cannot be combined with other offers &nbsp;·&nbsp; High-borosilicate glassware crafted to last generations</p>
          <p style="margin:0 0 16px 0; font-family:${SANS}; font-size:12px; line-height:18px; letter-spacing:0.08em; text-transform:uppercase;">
            <a href="{{ unsubscribe_url }}" style="color:${C.canvas}; text-decoration:underline;">Unsubscribe</a><span style="color:${C.cream};">&nbsp;&nbsp;|&nbsp;&nbsp;</span><a href="{{ preferences_url }}" style="color:${C.canvas}; text-decoration:underline;">Preferences</a><span style="color:${C.cream};">&nbsp;&nbsp;|&nbsp;&nbsp;</span><a href="{{ view_in_browser_url }}" style="color:${C.canvas}; text-decoration:underline;">View in Browser</a>
          </p>
          <p style="margin:0; font-family:${SANS}; font-size:12px; line-height:18px; color:${C.cream};">{{ company_address | default: "Razab USA · 100 Plaza Drive · Secaucus, NJ 07094" }}</p>`));

/* ---------------- page ---------------- */
const html = `<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="x-apple-disable-message-reformatting">
<meta name="format-detection" content="telephone=no, date=no, address=no, email=no">
<meta name="color-scheme" content="light">
<meta name="supported-color-schemes" content="light">
<title>Pull up a chair at The Harvest Table — 20% off at Razab</title>
<!--[if mso]>
<noscript><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch></o:OfficeDocumentSettings></xml></noscript>
<style>
  td,th,div,p,a,span { font-family:Arial,sans-serif !important; }
  h1,h2,h3,.rz-serif { font-family:Georgia,'Times New Roman',serif !important; }
</style>
<![endif]-->
<link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,500;0,600;1,400;1,500&family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
<style>
  :root { color-scheme: light; supported-color-schemes: light; }
  html, body { margin:0 !important; padding:0 !important; width:100% !important; background:${SIDE_BG}; }
  * { -ms-text-size-adjust:100%; -webkit-text-size-adjust:100%; }
  table, td { mso-table-lspace:0pt; mso-table-rspace:0pt; border-collapse:collapse; }
  img { -ms-interpolation-mode:bicubic; border:0; outline:none; text-decoration:none; display:block; max-width:100%; }
  a { text-decoration:none; }
  a[x-apple-data-detectors] { color:inherit !important; text-decoration:none !important; }
  u + #body a { color:inherit; text-decoration:none; }
  #MessageViewBody a { color:inherit; text-decoration:none; }

  @media screen {
    .rz-btn:hover { background-color:#53734D !important; }
    .rz-nav-link:hover { color:${C.chestnut} !important; }
    .rz-foot-link:hover { color:${C.honey} !important; }
  }

  /* ---------- Mobile (max-width: 620px): 20px side padding, columns stack ---------- */
  @media only screen and (max-width:620px) {
    .rz-wrap { width:100% !important; border-left:0 !important; border-right:0 !important; }
    .rz-px { padding-left:20px !important; padding-right:20px !important; }
    .rz-fluid, .rz-img { width:100% !important; max-width:100% !important; height:auto !important; }
    .rz-stack { display:block !important; width:100% !important; max-width:100% !important; box-sizing:border-box !important; }
    .rz-cp { padding:16px !important; }
    .rz-col-txt { padding:24px 0 0 0 !important; }
    .rz-nav { font-size:11px !important; letter-spacing:0.08em !important; }
    .rz-logo-cell { padding:0 16px !important; }
    .rz-h1 { font-size:36px !important; line-height:42px !important; }
    .rz-h2 { font-size:28px !important; line-height:34px !important; }
    .rz-story { padding-left:16px !important; }
    .rz-sum { display:block !important; width:100% !important; border-left:0 !important; border-right:0 !important; border-top:1px solid ${C.honey} !important; }
    .rz-sum-first { border-top:0 !important; }
  }
</style>
</head>
<body id="body" style="margin:0; padding:0; background-color:${SIDE_BG};">

<!-- ===================== PREHEADER ===================== -->
<div style="display:none; font-size:1px; color:${SIDE_BG}; line-height:1px; max-height:0; max-width:0; opacity:0; overflow:hidden; mso-hide:all;">
  Pull up a chair at The Harvest Table: 20% off every Razab glass container with code HARVEST — ends midnight, Nov 27.
  &#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;
</div>

<!--
  GENERATED by tools/build-email.js: edit the tokens there, not this file.
  Every section is a 600px table; the margins outside that 600px column are always ${SIDE_BG} (SIDE_BG).
  Side padding 40px (20px mobile). Spacing between sections is one solid ${GAP}px row of SIDE_BG.
  Colour flow inside the 600px column: linen -> white -> canvas -> footer painting (canvas to chestnut) -> chestnut.
-->
<table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="${SIDE_BG}" style="background-color:${SIDE_BG};">
${masthead}
${gap(C.linen, C.linen)}
${heroCard}
${gap(C.linen, C.white)}
${story}
${gap(C.white, C.white)}
${collection}
${gap(C.white, C.canvas)}
${loaf}
${gap(C.canvas, C.canvas)}
${painting}
${footer}
</table>
</body>
</html>
`;

const dir = path.join(__dirname, '..');
for (const f of ['thanksgiving-2026-email.html', 'index.html', 'email.html']) fs.writeFileSync(path.join(dir, f), html);
console.log('wrote 3 files, ' + Buffer.byteLength(html) + ' bytes');
