# Razab USA — Thanksgiving 2026 Email Campaign
### Production-Ready Table-Based HTML Email & Creative Asset Suite

This repository contains the complete production-ready email template and creative art direction for **Razab USA's Thanksgiving 2026 Campaign ("The Harvest Table")**, modeled after the editorial, vintage-gothic-meets-warm-parchment aesthetic of `halloween-2026-email3`.

---

## 1. Campaign & Offer Architecture
- **Brand:** Razab (Premium High-Borosilicate Glass Kitchenware)
- **Campaign Title:** The Harvest Table — *Feast Today. Fresh All Week.*
- **Offer / Promo Code:** **20% off** every Razab glass container with code **`HARVEST`**
- **Campaign Window:** November 20, 2026 — November 27, 2026 (Ends Black Friday midnight at 11:59 PM EST)
- **Website & Primary URLs:**
  - **Home:** `https://razab.com/?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026`
  - **Shop by Need:** `https://razab.com/pages/fit-finder?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026`
  - **About Us:** `https://razab.com/pages/about-us?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026`
  - **The Host's Set (30 Pc):** `https://razab.com/collections/all?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026&utm_content=p_host`
  - **The Potluck Carrier (6500ML):** `https://razab.com/products/6500-ml-large-food-storage-container?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026&utm_content=p_carry`
  - **The Pastry & Pie Keeper (3900ML):** `https://razab.com/products/3900ml-large-glass-food-storage-container-with-airtight-lid?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026&utm_content=p_keeper`
  - **The Holiday Baker (Glass Loaf Pans):** `https://razab.com/collections/glass-loaf-pans-with-lids?utm_source=email&utm_medium=newsletter&utm_campaign=thanksgiving2026&utm_content=p_loaf`

---

## 2. Visual System & Color Palette
The design translates the moody parchment aesthetic of the Halloween campaign into a warm, candlelit heirloom autumn palette:
- **Outer Shell / Mailbox Margin:** `#511F16` (Deep Espresso / Roasted Chestnut)
- **Top Nav Bar:** `#9C2F2F` (Spiced Cranberry / Terracotta Rust) with ivory links & gold `✦` dividers
- **Primary Parchment Canvas:** `#EADBC8` (Warm Linen / Cream Parchment)
- **Secondary Accent Canvas:** `#DFCBB2` (Toasted Wheat / Warm Tan)
- **Dark Editorial & Countdown Band:** `#231914` (Warm Hearth / Hearthstone Dark Brown)
- **Accents & Borders:**
  - Cranberry Accent: `#9C2F2F`
  - Burnished Copper / Gold: `#B6562A` / `#E6B382`
  - Deep Charcoal Brown: `#2A1E17`
  - Text Color on Parchment: `#382A20`

---

## 3. Email File Structure
```text
Razab Thanks giving/
├── thanksgiving-2026-email.html   # Complete, production-ready, table-based HTML email
├── PROMPTS.md                      # Detailed Midjourney v6.1 & DALL-E 3 visual generation prompts
├── README.md                       # Deployment guide, QA testing, and campaign overview
├── generate_placeholders.py        # Python script that generates placeholder assets
├── generate_extra.py               # Additional asset generator script
└── assets/                         # Local asset suite (exact dimensions, gradients, & logos)
    ├── banner-autumn-vines.png     # 600x52px top botanical vine trim
    ├── razab-logo.png              # 178x44px brand logo
    ├── razab-logo-light.png        # 150x37px inverted brand logo
    ├── hero-thanksgiving.jpg       # 600x700px hero deckle-edge visual
    ├── dinner-bell-clock.jpg       # 552x310px hearth clock urgency visual
    ├── flourish.png                # 552x40px antique wheat flourish
    ├── t-harvest.png               # 552x125px "The Harvest Collection" header
    ├── p-host.jpg                  # 260x320px Product 1: The Host's Set (30 Pc)
    ├── p-carry.jpg                 # 260x320px Product 2: The Potluck Carrier (6500ML)
    ├── p-keeper.jpg                # 260x320px Product 3: The Pastry Keeper (3900ML)
    ├── p-loaf.jpg                  # 260x320px Product 4: The Holiday Baker (Loaf Pans)
    ├── t-loaf.png                  # 552x150px "No. III — The Thanksgiving Loaf Pans"
    ├── pantry-thanksgiving.jpg     # 552x700px Spotlight editorial loaf pan visual
    ├── harvest-footer.jpg          # 600x210px atmospheric autumn farmhouse landscape
    ├── fade-linen-hearth.jpg       # 600x90px smooth color transition (#EADBC8 -> #231914)
    ├── fade-hearth-linen.jpg       # 600x90px smooth color transition (#231914 -> #EADBC8)
    ├── fade-linen-wheat.jpg        # 600x50px smooth color transition (#EADBC8 -> #DFCBB2)
    ├── fade-wheat-linen.jpg        # 600x50px smooth color transition (#DFCBB2 -> #EADBC8)
    └── fade-wheat-hearth.jpg       # 600x90px smooth color transition (#DFCBB2 -> #231914)
```

---

## 4. Key Email Development Standards
1. **600px Fixed Architecture with Fluid Mobile Scaling:**
   - Standard 600px desktop table centered with `align="center"`.
   - Complete conditional MSO wrappers `<!--[if mso]><table role="presentation" width="600"...>` for Outlook 2013/2016/2019/2021.
2. **Hidden Preheader with Zero-Width Non-Joiners:**
   - The preheader text is followed by an invisible chain of `&#847;&zwnj;&nbsp;` characters to prevent client inbox previews from trailing into footer or navigation text.
3. **Bulletproof Table Buttons with VML:**
   - All call-to-action buttons feature Outlook VML shape fallbacks (`<v:rect>` with `<w:anchorlock/>`) alongside pure CSS animated modern buttons.
4. **Graceful Progressive Enhancement:**
   - Subtle keyframe animations (`rz-breathe` button pulse, `rz-candle` subtle flicker, `rz-glow` amber hearth aura) enhance Apple Mail, iOS Mail, and Webkit clients while degrading gracefully to clean static styling in Gmail and Outlook.
   - Respects user accessibility preferences via `@media (prefers-reduced-motion: reduce)`.
5. **Universal Image Alt Texts & Semantic Typography:**
   - Every image carries descriptive alt text so users with blocked images still understand the offer, story, and products.
   - Text colors maintain high contrast compliance (WCAG AA).

---

## 5. ESP Deployment Instructions

### Klaviyo
1. In Klaviyo, go to **Email Templates** > **Create Template** > **Import HTML**.
2. Paste the contents of `thanksgiving-2026-email.html`.
3. The merge tags `{{ unsubscribe_url }}`, `{{ preferences_url }}`, and `{{ view_in_browser_url }}` are already formatted for Klaviyo compatibility.

### Mailchimp
1. In Mailchimp, choose **Campaigns** > **Email Templates** > **Code your own** > **Paste in code**.
2. If necessary, swap `{{ unsubscribe_url }}` with `*|UNSUB|*` and `{{ view_in_browser_url }}` with `*|ARCHIVE|*`.

### Shopify Email / Omnisend / Braze
- Replace relative image URLs (`assets/filename.jpg`) with your CDN or Shopify files hosted URLs (e.g. `https://cdn.shopify.com/s/files/...`) once generated and uploaded.

