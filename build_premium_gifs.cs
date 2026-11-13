using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class PremiumGifSuite {
    // Helper to make looping GIF
    public static void EnsureLooping(string path) {
        byte[] bytes = File.ReadAllBytes(path);
        string s = System.Text.Encoding.ASCII.GetString(bytes);
        if (s.Contains("NETSCAPE2.0")) return;
        if (bytes.Length < 13 || bytes[0] != 'G' || bytes[1] != 'I' || bytes[2] != 'F') return;

        int gctSize = 0;
        if ((bytes[10] & 0x80) != 0) {
            int gctEntries = 1 << ((bytes[10] & 0x07) + 1);
            gctSize = 3 * gctEntries;
        }

        int insertPos = 13 + gctSize;
        byte[] loopExt = new byte[] {
            0x21, 0xFF, 0x0B,
            (byte)'N', (byte)'E', (byte)'T', (byte)'S', (byte)'C', (byte)'A', (byte)'P', (byte)'E',
            (byte)'2', (byte)'.', (byte)'0',
            0x03, 0x01, 0x00, 0x00, 0x00
        };

        byte[] newBytes = new byte[bytes.Length + loopExt.Length];
        Buffer.BlockCopy(bytes, 0, newBytes, 0, insertPos);
        Buffer.BlockCopy(loopExt, 0, newBytes, insertPos, loopExt.Length);
        Buffer.BlockCopy(bytes, insertPos, newBytes, insertPos + loopExt.Length, bytes.Length - insertPos);

        File.WriteAllBytes(path, newBytes);
    }

    // 1. Candlestick GIF (56x56 px, warm linen background #FAF7F2)
    public static void BuildCandle(string outPath) {
        int w = 56, h = 56;
        int frames = 16;
        var encoder = new GifBitmapEncoder();

        for (int f = 0; f < frames; f++) {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                // Background #FAF7F2
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(250, 247, 242)), null, new Rect(0, 0, w, h));

                double angle = (f / (double)frames) * Math.PI * 2.0;
                double flickX = Math.Sin(angle) * 1.6;
                double flickY = Math.Cos(angle) * 1.2;

                // Candle stick (Brass #C87F4E with gold highlight #F7CC83)
                var brassBrush = new LinearGradientBrush(Color.FromRgb(200, 127, 78), Color.FromRgb(247, 204, 131), new Point(0, 0), new Point(1, 0));
                // Base
                dc.DrawRoundedRectangle(brassBrush, null, new Rect(17, 48, 22, 5), 2.5, 2.5);
                // Stem
                dc.DrawRoundedRectangle(brassBrush, null, new Rect(25, 30, 6, 19), 2, 2);
                // Bobeche cup
                dc.DrawRoundedRectangle(brassBrush, null, new Rect(21, 28, 14, 4), 2, 2);

                // Wick
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(60, 40, 30)), 1.5), new Point(28, 29), new Point(28 + flickX * 0.3, 23));

                // Outer warm glow aura
                byte auraAlpha = (byte)(45 + Math.Sin(angle * 2) * 18);
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(auraAlpha, 247, 204, 131)), null, new Point(28 + flickX * 0.2, 16 + flickY * 0.2), 14, 14);

                // Flame shape
                var flameGeom = new StreamGeometry();
                using (var ctx = flameGeom.Open()) {
                    Point top = new Point(28 + flickX * 0.8, 8 + flickY * 0.5);
                    ctx.BeginFigure(top, true, true);
                    ctx.BezierTo(new Point(top.X + 6, top.Y + 7), new Point(top.X + 6, top.Y + 12), new Point(28, 23), true, true);
                    ctx.BezierTo(new Point(top.X - 6, top.Y + 12), new Point(top.X - 6, top.Y + 7), top, true, true);
                }
                var flameBrush = new LinearGradientBrush(Color.FromRgb(255, 195, 75), Color.FromRgb(215, 95, 45), new Point(0.5, 0), new Point(0.5, 1));
                dc.DrawGeometry(flameBrush, null, flameGeom);

                // Inner bright core
                var coreGeom = new StreamGeometry();
                using (var ctx = coreGeom.Open()) {
                    Point coreTop = new Point(28 + flickX * 0.4, 14 + flickY * 0.3);
                    ctx.BeginFigure(coreTop, true, true);
                    ctx.BezierTo(new Point(coreTop.X + 2.5, coreTop.Y + 3.5), new Point(coreTop.X + 2.5, coreTop.Y + 6), new Point(28, 22), true, true);
                    ctx.BezierTo(new Point(coreTop.X - 2.5, coreTop.Y + 6), new Point(coreTop.X - 2.5, coreTop.Y + 3.5), coreTop, true, true);
                }
                dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(255, 255, 240)), null, coreGeom);
            }
            rtb.Render(dv);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
        }

        using (var fs = File.Create(outPath)) {
            encoder.Save(fs);
        }
        EnsureLooping(outPath);
    }

    // 2. Botanical Autumn Garland Vine Banner (600x42 px)
    public static void BuildVines(string outPath) {
        int w = 600, h = 42;
        int frames = 20;
        var encoder = new GifBitmapEncoder();

        // 16 decorative leaves
        var rnd = new Random(101);
        int count = 16;
        double[] baseXs = new double[count];
        double[] baseYs = new double[count];
        double[] rotations = new double[count];
        Color[] leafColors = new Color[] {
            Color.FromRgb(195, 214, 146), // Sage Leaf #C3D692
            Color.FromRgb(247, 204, 131), // Honey Gold #F7CC83
            Color.FromRgb(200, 127, 78),  // Terracotta #C87F4E
            Color.FromRgb(245, 230, 210)  // Warm Linen Tint
        };
        int[] colorIndices = new int[count];

        for (int i = 0; i < count; i++) {
            baseXs[i] = (w / (double)count) * i + rnd.Next(-8, 8);
            baseYs[i] = 21 + rnd.Next(-7, 7);
            rotations[i] = rnd.NextDouble() * 360.0;
            colorIndices[i] = rnd.Next(leafColors.Length);
        }

        for (int f = 0; f < frames; f++) {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                // Background #8F544A (Spiced Chestnut)
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(143, 84, 74)), null, new Rect(0, 0, w, h));

                // Gold hairline borders top and bottom
                var goldPen = new Pen(new SolidColorBrush(Color.FromArgb(180, 247, 204, 131)), 1.0);
                dc.DrawLine(goldPen, new Point(0, 0.5), new Point(w, 0.5));
                dc.DrawLine(goldPen, new Point(0, h - 0.5), new Point(w, h - 0.5));

                // Undulating vine path
                double cycle = (f / (double)frames) * Math.PI * 2.0;
                var vineGeom = new StreamGeometry();
                using (var ctx = vineGeom.Open()) {
                    ctx.BeginFigure(new Point(0, 21), false, false);
                    for (int s = 1; s <= 12; s++) {
                        double px = (w / 12.0) * s;
                        double py = 21 + Math.Sin((s * 0.9) + cycle) * 5.0;
                        ctx.LineTo(new Point(px, py), true, true);
                    }
                }
                dc.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromArgb(130, 247, 204, 131)), 1.5), vineGeom);

                // Draw floating & swaying leaves
                for (int i = 0; i < count; i++) {
                    double t = cycle + (i * 0.45);
                    double cx = baseXs[i] + Math.Sin(t) * 9.0;
                    double cy = baseYs[i] + Math.Cos(t * 1.4) * 4.5;
                    double curRot = rotations[i] + Math.Sin(t) * 25.0;

                    var transform = new TransformGroup();
                    transform.Children.Add(new RotateTransform(curRot, cx, cy));
                    dc.PushTransform(transform);

                    // Leaf geometry centered at cx, cy
                    var leaf = new StreamGeometry();
                    using (var ctx = leaf.Open()) {
                        ctx.BeginFigure(new Point(cx - 7, cy), true, true);
                        ctx.BezierTo(new Point(cx - 3, cy - 4.5), new Point(cx + 3, cy - 4.5), new Point(cx + 7, cy), true, true);
                        ctx.BezierTo(new Point(cx + 3, cy + 4.5), new Point(cx - 3, cy + 4.5), new Point(cx - 7, cy), true, true);
                    }
                    dc.DrawGeometry(new SolidColorBrush(leafColors[colorIndices[i]]), null, leaf);
                    dc.Pop();
                }
            }
            rtb.Render(dv);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
        }

        using (var fs = File.Create(outPath)) {
            encoder.Save(fs);
        }
        EnsureLooping(outPath);
    }

    // 3. Antique Hearth Urgency Clock GIF (552x280 px)
    public static void BuildClock(string outPath) {
        int w = 552, h = 280;
        int frames = 24; // 2 seconds loop
        var encoder = new GifBitmapEncoder();

        var tfGeorgiaBold = new Typeface(new FontFamily("Georgia"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var tfGeorgiaItalic = new Typeface(new FontFamily("Georgia"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);
        var tfArialBold = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        for (int f = 0; f < frames; f++) {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                // Background: Deep Espresso Hearthstone (#231914)
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(35, 25, 20)), null, new Rect(0, 0, w, h));

                // Double border: Outer Terracotta #C87F4E, Inner Warm Honey Gold #F7CC83
                dc.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(200, 127, 78)), 2.0), new Rect(4, 4, w - 8, h - 8));
                dc.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(247, 204, 131)), 1.0), new Rect(8, 8, w - 16, h - 16));

                // Clock on the Left
                double clockX = 135, clockY = 140, clockR = 98;

                // Outer warm aura
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(25, 247, 204, 131)), null, new Point(clockX, clockY), clockR + 10, clockR + 10);

                // Clock brass frame
                var frameBrush = new LinearGradientBrush(Color.FromRgb(210, 135, 80), Color.FromRgb(130, 75, 55), new Point(0, 0), new Point(1, 1));
                dc.DrawEllipse(frameBrush, null, new Point(clockX, clockY), clockR, clockR);

                // Clock face (Linen Parchment #FAF7F2 / #EADBC8)
                double faceR = clockR - 10;
                var faceBrush = new LinearGradientBrush(Color.FromRgb(250, 247, 242), Color.FromRgb(232, 218, 198), new Point(0, 0), new Point(1, 1));
                dc.DrawEllipse(faceBrush, null, new Point(clockX, clockY), faceR, faceR);

                // Dial ring
                dc.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromRgb(143, 84, 74)), 1.5), new Point(clockX, clockY), faceR - 5, faceR - 5);

                // Roman numerals
                string[] numerals = { "XII", "I", "II", "III", "IIII", "V", "VI", "VII", "VIII", "IX", "X", "XI" };
                for (int i = 0; i < 12; i++) {
                    double a = (i / 12.0) * Math.PI * 2.0;
                    double tr = faceR - 16;
                    var ftNum = new FormattedText(numerals[i], CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfGeorgiaBold, i % 3 == 0 ? 12 : 9, new SolidColorBrush(Color.FromRgb(55, 35, 25)), 96);
                    dc.DrawText(ftNum, new Point(clockX + Math.Sin(a) * tr - ftNum.Width / 2.0, clockY - Math.Cos(a) * tr - ftNum.Height / 2.0));
                }

                // Brand text
                var ftBrand = new FormattedText("RAZAB", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfArialBold, 7.5, new SolidColorBrush(Color.FromRgb(143, 84, 74)), 96);
                dc.DrawText(ftBrand, new Point(clockX - ftBrand.Width / 2.0, clockY + 28));

                // Clock hands (Time: ~11:58 PM)
                // Hour hand
                double hAngle = (11.96 / 12.0) * Math.PI * 2.0;
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(40, 22, 16)), 3.5), new Point(clockX, clockY), new Point(clockX + Math.Sin(hAngle) * 44, clockY - Math.Cos(hAngle) * 44));

                // Minute hand
                double mAngle = (58.5 / 60.0) * Math.PI * 2.0;
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(40, 22, 16)), 2.5), new Point(clockX, clockY), new Point(clockX + Math.Sin(mAngle) * 64, clockY - Math.Cos(mAngle) * 64));

                // Animated Second Hand: Sweeps forward smoothly
                double sAngle = ((f / (double)frames) * 6.0 / 60.0 + (50.0 / 60.0)) * Math.PI * 2.0;
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(215, 65, 45)), 1.5),
                    new Point(clockX - Math.Sin(sAngle) * 14, clockY + Math.Cos(sAngle) * 14),
                    new Point(clockX + Math.Sin(sAngle) * 70, clockY - Math.Cos(sAngle) * 70));

                // Brass center cap
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(200, 127, 78)), null, new Point(clockX, clockY), 4.5, 4.5);

                // Right-Side Copy & Typography
                double textX = 262;

                // Eyebrow badge
                var ftEye = new FormattedText("✦  THE HARVEST CLOCK IS TICKING  ✦", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfArialBold, 9.5, new SolidColorBrush(Color.FromRgb(247, 204, 131)), 96);
                dc.DrawText(ftEye, new Point(textX, 48));

                // Headline Line 1
                var ftH1 = new FormattedText("20% Off Expires At", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfGeorgiaBold, 22, new SolidColorBrush(Color.FromRgb(250, 247, 242)), 96);
                dc.DrawText(ftH1, new Point(textX, 72));

                // Headline Line 2
                var ftH2 = new FormattedText("Midnight, Nov 27", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfGeorgiaBold, 22, new SolidColorBrush(Color.FromRgb(250, 247, 242)), 96);
                dc.DrawText(ftH2, new Point(textX, 102));

                // Terracotta separator line
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(200, 127, 78)), 1.5), new Point(textX, 138), new Point(textX + 246, 138));

                // Subtitle
                var ftSub = new FormattedText("Code HARVEST ends at the stroke of 11:59 PM EST", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfGeorgiaItalic, 11, new SolidColorBrush(Color.FromRgb(195, 214, 146)), 96);
                dc.DrawText(ftSub, new Point(textX, 148));

                // Guarantee pill / Button badge
                dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(100, 135, 93)), null, new Rect(textX, 185, 240, 38), 5, 5);
                var ftBtn = new FormattedText("CLAIM HARVEST SAVINGS  →", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfArialBold, 11, new SolidColorBrush(Colors.White), 96);
                dc.DrawText(ftBtn, new Point(textX + (240 - ftBtn.Width) / 2.0, 185 + (38 - ftBtn.Height) / 2.0));
            }
            rtb.Render(dv);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
        }

        using (var fs = File.Create(outPath)) {
            encoder.Save(fs);
        }
        EnsureLooping(outPath);
    }

    // 4. Light "Harvest Table" clock (rendered 2x: 240x240, displayed at 120x120).
    //    Light linen face, Arabic quarter numerals only (no Roman numerals), sweeping second hand.
    //    Background matches the ticket stub (#FAF7F2) so the GIF sits flush in the offer card.
    public static void BuildClockLight(string outPath) {
        int w = 240, h = 240;
        int frames = 12;
        var encoder = new GifBitmapEncoder();
        var tfSerif = new Typeface(new FontFamily("Georgia"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var tfSans = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var canvas = Color.FromRgb(250, 247, 242);   // #FAF7F2
        var chestnut = Color.FromRgb(143, 84, 74);   // #8F544A
        var burnt = Color.FromRgb(200, 127, 78);     // #C87F4E
        var honey = Color.FromRgb(247, 204, 131);    // #F7CC83
        var ink = Color.FromRgb(90, 61, 54);         // #5A3D36
        double cx = w / 2.0, cy = h / 2.0, r = 108;

        for (int f = 0; f < frames; f++) {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                dc.DrawRectangle(new SolidColorBrush(canvas), null, new Rect(0, 0, w, h));
                // Warm candle-glow halo
                var halo = new RadialGradientBrush(Color.FromArgb(90, 247, 204, 131), Color.FromArgb(0, 247, 204, 131));
                dc.DrawEllipse(halo, null, new Point(cx, cy), r + 10, r + 10);
                // Thin brass bezel: burnt orange outer, honey inner
                dc.DrawEllipse(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(burnt), 4), new Point(cx, cy), r - 4, r - 4);
                dc.DrawEllipse(null, new Pen(new SolidColorBrush(honey), 2), new Point(cx, cy), r - 12, r - 12);
                // Minute ticks
                for (int i = 0; i < 60; i++) {
                    double a = i / 60.0 * Math.PI * 2;
                    bool major = i % 5 == 0;
                    double r1 = r - 20, r2 = major ? r - 32 : r - 25;
                    dc.DrawLine(new Pen(new SolidColorBrush(major ? chestnut : Color.FromRgb(234, 227, 217)), major ? 3 : 1.5),
                        new Point(cx + Math.Sin(a) * r1, cy - Math.Cos(a) * r1),
                        new Point(cx + Math.Sin(a) * r2, cy - Math.Cos(a) * r2));
                }
                // Arabic quarter numerals
                string[] nums = { "12", "3", "6", "9" };
                for (int q = 0; q < 4; q++) {
                    double a = q / 4.0 * Math.PI * 2, tr = r - 50;
                    var ft = new FormattedText(nums[q], CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSerif, 24, new SolidColorBrush(chestnut), 96);
                    dc.DrawText(ft, new Point(cx + Math.Sin(a) * tr - ft.Width / 2, cy - Math.Cos(a) * tr - ft.Height / 2));
                }
                var brand = new FormattedText("RAZAB", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSans, 11, new SolidColorBrush(burnt), 96);
                dc.DrawText(brand, new Point(cx - brand.Width / 2, cy + 24));
                // Hands at ~11:58
                double hA = 11.96 / 12 * Math.PI * 2, mA = 58.5 / 60 * Math.PI * 2;
                var handPen = new Pen(new SolidColorBrush(ink), 6) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                dc.DrawLine(handPen, new Point(cx, cy), new Point(cx + Math.Sin(hA) * 40, cy - Math.Cos(hA) * 40));
                handPen = new Pen(new SolidColorBrush(ink), 4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                dc.DrawLine(handPen, new Point(cx, cy), new Point(cx + Math.Sin(mA) * 72, cy - Math.Cos(mA) * 72));
                // Sweeping second hand
                double sA = ((f / (double)frames) * 6.0 / 60.0 + 50.0 / 60.0) * Math.PI * 2;
                dc.DrawLine(new Pen(new SolidColorBrush(burnt), 2),
                    new Point(cx - Math.Sin(sA) * 16, cy + Math.Cos(sA) * 16),
                    new Point(cx + Math.Sin(sA) * 80, cy - Math.Cos(sA) * 80));
                dc.DrawEllipse(new SolidColorBrush(burnt), null, new Point(cx, cy), 6, 6);
                dc.DrawEllipse(new SolidColorBrush(honey), null, new Point(cx, cy), 2.5, 2.5);
            }
            rtb.Render(dv);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
        }
        using (var fs = File.Create(outPath)) { encoder.Save(fs); }
        EnsureLooping(outPath);
    }

    // 4b. Full-width light dinner-bell clock banner (552x200) shown directly above the offer card.
    //     Light replacement for the dark dinner-bell-clock.gif: no Roman numerals, no baked-in button.
    //     Left/top/right 1px #EAE3D9 edge so it joins the white offer card below it.
    public static void BuildClockBanner(string outPath) {
        int w = 552, h = 200, frames = 12;
        var encoder = new GifBitmapEncoder();
        var tfSerif = new Typeface(new FontFamily("Georgia"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var tfSerifIt = new Typeface(new FontFamily("Georgia"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);
        var tfSans = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        Color canvas = Color.FromRgb(250, 247, 242), chestnut = Color.FromRgb(143, 84, 74), burnt = Color.FromRgb(200, 127, 78);
        Color honey = Color.FromRgb(247, 204, 131), ink = Color.FromRgb(90, 61, 54), border = Color.FromRgb(234, 227, 217);
        double cx = 108, cy = 100, r = 74;

        for (int f = 0; f < frames; f++) {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                dc.DrawRectangle(new SolidColorBrush(canvas), null, new Rect(0, 0, w, h));
                var edge = new Pen(new SolidColorBrush(border), 1);
                dc.DrawLine(edge, new Point(0.5, 0), new Point(0.5, h));
                dc.DrawLine(edge, new Point(w - 0.5, 0), new Point(w - 0.5, h));
                dc.DrawLine(edge, new Point(0, 0.5), new Point(w, 0.5));
                // Candle-glow halo, bezel, face
                dc.DrawEllipse(new RadialGradientBrush(Color.FromArgb(110, 247, 204, 131), Color.FromArgb(0, 247, 204, 131)), null, new Point(cx, cy), r + 14, r + 14);
                dc.DrawEllipse(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(burnt), 3), new Point(cx, cy), r - 3, r - 3);
                dc.DrawEllipse(null, new Pen(new SolidColorBrush(honey), 1.5), new Point(cx, cy), r - 9, r - 9);
                for (int i = 0; i < 60; i++) {
                    double a = i / 60.0 * Math.PI * 2; bool major = i % 5 == 0;
                    double r1 = r - 14, r2 = major ? r - 22 : r - 17;
                    dc.DrawLine(new Pen(new SolidColorBrush(major ? chestnut : border), major ? 2 : 1),
                        new Point(cx + Math.Sin(a) * r1, cy - Math.Cos(a) * r1), new Point(cx + Math.Sin(a) * r2, cy - Math.Cos(a) * r2));
                }
                string[] nums = { "12", "3", "6", "9" };
                for (int q = 0; q < 4; q++) {
                    double a = q / 4.0 * Math.PI * 2, tr = r - 34;
                    var ft = new FormattedText(nums[q], CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSerif, 15, new SolidColorBrush(chestnut), 96);
                    dc.DrawText(ft, new Point(cx + Math.Sin(a) * tr - ft.Width / 2, cy - Math.Cos(a) * tr - ft.Height / 2));
                }
                double hA = 11.96 / 12 * Math.PI * 2, mA = 58.5 / 60 * Math.PI * 2;
                dc.DrawLine(new Pen(new SolidColorBrush(ink), 4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new Point(cx, cy), new Point(cx + Math.Sin(hA) * 28, cy - Math.Cos(hA) * 28));
                dc.DrawLine(new Pen(new SolidColorBrush(ink), 2.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new Point(cx, cy), new Point(cx + Math.Sin(mA) * 48, cy - Math.Cos(mA) * 48));
                double sA = ((f / (double)frames) * 6.0 / 60.0 + 50.0 / 60.0) * Math.PI * 2;
                dc.DrawLine(new Pen(new SolidColorBrush(burnt), 1.5), new Point(cx - Math.Sin(sA) * 10, cy + Math.Cos(sA) * 10), new Point(cx + Math.Sin(sA) * 54, cy - Math.Cos(sA) * 54));
                dc.DrawEllipse(new SolidColorBrush(burnt), null, new Point(cx, cy), 4, 4);

                // Copy (sentence case, palette colors that pass contrast on #FAF7F2)
                double tx = 216;
                var eye = new FormattedText("The harvest clock is ticking", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSans, 13, new SolidColorBrush(chestnut), 96);
                dc.DrawText(eye, new Point(tx, 52));
                var h1 = new FormattedText("Midnight, November 27", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSerif, 24, new SolidColorBrush(chestnut), 96);
                dc.DrawText(h1, new Point(tx, 76));
                dc.DrawLine(new Pen(new SolidColorBrush(honey), 1), new Point(tx, 124), new Point(tx + 300, 124));
                var sub = new FormattedText("Code HARVEST ends at 11:59 PM EST", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tfSerifIt, 15, new SolidColorBrush(ink), 96);
                dc.DrawText(sub, new Point(tx, 134));
            }
            rtb.Render(dv);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
        }
        using (var fs = File.Create(outPath)) { encoder.Save(fs); }
        EnsureLooping(outPath);
    }

    // 6. Animated showpiece frames (560x560 PNGs, 4 s at 12 fps = 48 frames) for assets/gifs/offer-showpiece.gif.
    //    Frame 0 is the untouched photo (Outlook shows only frame 1). Every effect is zero at t=0 and t=4,
    //    so the loop is seamless: candle glow dips 100% -> 85%, embers/dust rise and fade, 2 leaves drift.
    //    Encode with ffmpeg palettegen/paletteuse (see CLAUDE.md).
    static double Smooth(double e0, double e1, double x) { double t = Math.Max(0, Math.Min(1, (x - e0) / (e1 - e0))); return t * t * (3 - 2 * t); }
    static double Life(double t, double start, double end, double fade) {
        if (t <= start || t >= end) return 0;
        return Smooth(start, start + fade, t) * (1 - Smooth(end - fade, end, t));
    }

    public static void BuildShowpieceFrames(string srcJpg, string outDir) {
        int w = 560, h = 560, frames = 48; double fps = 12;
        Directory.CreateDirectory(outDir);
        var src = new BitmapImage();
        src.BeginInit(); src.CacheOption = BitmapCacheOption.OnLoad; src.UriSource = new Uri(srcJpg); src.EndInit(); src.Freeze();
        // Base photo converted to 96 dpi so it maps 1:1 onto the 560x560 render target (no resampling).
        var basePx = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);
        int stride = w * 4; byte[] buf = new byte[stride * h]; basePx.CopyPixels(buf, stride, 0);
        var base96 = BitmapSource.Create(w, h, 96, 96, PixelFormats.Pbgra32, null, buf, stride);

        double fx = 200, fy = 90; // candle flame centre in the 560px photo
        // Embers rise from the flame; dust specks drift up through the window light. {x0, y0, dx, rise, start, end, radius, alpha}
        double[][] specks = {
            new double[]{198, 70, -6, 60, 0.30, 1.90, 2.6, 0.95},
            new double[]{203, 72,  8, 70, 1.40, 3.10, 2.2, 0.90},
            new double[]{199, 68,  2, 55, 2.30, 3.85, 2.4, 0.85},
            new double[]{350, 330, 10, 90, 0.25, 2.60, 2.0, 0.55},
            new double[]{430, 260, -8, 80, 1.10, 3.60, 1.8, 0.50},
            new double[]{300, 420, 6, 70, 1.80, 3.90, 1.6, 0.45},
            new double[]{480, 380, -4, 85, 0.50, 3.20, 1.7, 0.45},
        };
        // Leaves drift down slowly in the out-of-focus window area (top left). {x0, y0, x1, y1, start, end, size, sway, rot0, rot1}
        double[][] leaves = {
            new double[]{ 70,  18,  98, 150, 0.35, 3.75, 11.0, 10, -30,  70},
            new double[]{150,  40, 128, 190, 0.90, 3.95,  9.5,  8,  40, -50},
        };

        for (int i = 0; i < frames; i++) {
            double t = i / fps;
            var root = new ContainerVisual();
            var baseV = new DrawingVisual();
            using (var dc = baseV.RenderOpen()) {
                dc.DrawImage(base96, new Rect(0, 0, w, h));
                if (i > 0) {
                    // Flicker: warm light around the flame dips to 85% and back. f(0) = f(4) = 0.
                    double f = 0.55 * Math.Pow(Math.Sin(3 * Math.PI * t / 4), 2) + 0.30 * Math.Pow(Math.Sin(7 * Math.PI * t / 4), 2) + 0.15 * Math.Pow(Math.Sin(11 * Math.PI * t / 4), 2);
                    byte a = (byte)Math.Round(255 * 0.15 * f);
                    var dim = new RadialGradientBrush();
                    dim.GradientStops.Add(new GradientStop(Color.FromArgb(a, 120, 78, 46), 0.0));
                    dim.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(a * 0.6), 120, 78, 46), 0.45));
                    dim.GradientStops.Add(new GradientStop(Color.FromArgb(0, 120, 78, 46), 1.0));
                    dc.DrawEllipse(dim, null, new Point(fx, fy), 58, 80);
                }
            }
            root.Children.Add(baseV);

            if (i > 0) {
                var leafV = new DrawingVisual();
                leafV.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 1.4 };
                using (var dc = leafV.RenderOpen()) {
                    foreach (var L in leaves) {
                        double life = Life(t, L[4], L[5], 0.6); if (life <= 0) continue;
                        double p = (t - L[4]) / (L[5] - L[4]);
                        double x = L[0] + (L[2] - L[0]) * p + Math.Sin(p * Math.PI * 2) * L[7];
                        double y = L[1] + (L[3] - L[1]) * p;
                        double rot = L[8] + (L[9] - L[8]) * p; double s = L[6];
                        // Broad leaf with a point, a darker centre vein and a short stem.
                        var g = Geometry.Parse("M 0,-1 C 0.55,-0.75 1,-0.2 0.8,0.3 C 0.6,0.7 0.25,0.85 0,1 C -0.25,0.85 -0.6,0.7 -0.8,0.3 C -1,-0.2 -0.55,-0.75 0,-1 Z");
                        dc.PushTransform(new TranslateTransform(x, y));
                        dc.PushTransform(new RotateTransform(rot));
                        dc.PushTransform(new ScaleTransform(s * 0.85, s));
                        dc.PushOpacity(0.88 * life);
                        dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(222, 140, 66), Color.FromRgb(168, 86, 42), 90), null, g);
                        var vein = new Pen(new SolidColorBrush(Color.FromRgb(140, 70, 34)), 0.12);
                        dc.DrawLine(vein, new Point(0, -0.8), new Point(0, 1.0));
                        dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(140, 70, 34)), 0.14), new Point(0, 1.0), new Point(0.08, 1.35));
                        dc.Pop(); dc.Pop(); dc.Pop(); dc.Pop();
                    }
                }
                root.Children.Add(leafV);

                var speckV = new DrawingVisual();
                using (var dc = speckV.RenderOpen()) {
                    foreach (var S in specks) {
                        double life = Life(t, S[4], S[5], 0.45); if (life <= 0) continue;
                        double p = (t - S[4]) / (S[5] - S[4]);
                        double x = S[0] + S[2] * p + Math.Sin(p * Math.PI * 3) * 2.5;
                        double y = S[1] - S[3] * p;
                        byte al = (byte)Math.Round(255 * S[7] * life);
                        var b = new RadialGradientBrush();
                        b.GradientStops.Add(new GradientStop(Color.FromArgb(al, 255, 236, 180), 0.0));
                        b.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(al * 0.55), 247, 204, 131), 0.45));
                        b.GradientStops.Add(new GradientStop(Color.FromArgb(0, 247, 204, 131), 1.0));
                        dc.DrawEllipse(b, null, new Point(x, y), S[6] * 1.8, S[6] * 1.8);
                    }
                }
                root.Children.Add(speckV);
            }

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(root);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using (var fs = File.Create(Path.Combine(outDir, string.Format("f{0:000}.png", i)))) enc.Save(fs);
        }
    }

    // 7. Footer painting (1200x670 JPG, shown at 600x335) with all four edges faded into the row behind it.
    //    The row behind is a vertical gradient canvas #FAF7F2 -> chestnut #8F544A, so the top edge fades to canvas,
    //    the bottom to chestnut and each side to that same gradient's colour at that height. No seam on any edge.
    public static void BuildFooterFade(string srcJpg, string outJpg) {
        int W = 1200, H = 670, fadeTB = 120, fadeLR = 110;
        var src = new BitmapImage(); src.BeginInit(); src.CacheOption = BitmapCacheOption.OnLoad; src.UriSource = new Uri(srcJpg); src.EndInit();
        var scaled = new TransformedBitmap(src, new ScaleTransform((double)W / src.PixelWidth, (double)H / src.PixelHeight));
        var conv = new FormatConvertedBitmap(scaled, PixelFormats.Bgr24, null, 0);
        int stride = W * 3; byte[] px = new byte[stride * H]; conv.CopyPixels(px, stride, 0);
        double[] top = { 250, 247, 242 }, bot = { 143, 84, 74 };
        for (int y = 0; y < H; y++) {
            double ty = (double)y / (H - 1);
            double[] row = { top[0] + (bot[0] - top[0]) * ty, top[1] + (bot[1] - top[1]) * ty, top[2] + (bot[2] - top[2]) * ty };
            for (int x = 0; x < W; x++) {
                // weight of the background colour: 1 at an edge, easing to 0 inside the fade distance
                double wT = y < fadeTB ? 1 - Smooth(0, fadeTB, y) : 0;
                double wB = y > H - 1 - fadeTB ? Smooth(H - 1 - fadeTB, H - 1, y) : 0;
                double wL = x < fadeLR ? 1 - Smooth(0, fadeLR, x) : 0;
                double wR = x > W - 1 - fadeLR ? Smooth(W - 1 - fadeLR, W - 1, x) : 0;
                double wgt = Math.Max(Math.Max(wT, wB), Math.Max(wL, wR));
                if (wgt <= 0) continue;
                // top/bottom edges use their solid colour, sides use the gradient colour at this height
                double[] target = row;
                if (wT >= wL && wT >= wR && wT >= wB) target = top;
                else if (wB >= wL && wB >= wR) target = bot;
                int i = y * stride + x * 3; // Bgr24
                px[i]     = (byte)Math.Round(px[i]     * (1 - wgt) + target[2] * wgt);
                px[i + 1] = (byte)Math.Round(px[i + 1] * (1 - wgt) + target[1] * wgt);
                px[i + 2] = (byte)Math.Round(px[i + 2] * (1 - wgt) + target[0] * wgt);
            }
        }
        var outBmp = BitmapSource.Create(W, H, 96, 96, PixelFormats.Bgr24, null, px, stride);
        var enc = new JpegBitmapEncoder { QualityLevel = 90 };
        enc.Frames.Add(BitmapFrame.Create(outBmp));
        using (var fs = File.Create(outJpg)) enc.Save(fs);
    }

    // 5. Seamless linen texture tile (160x160 JPG) around #F6F0E6 for the email background.
    public static void BuildLinen(string outPath) {
        int w = 160, h = 160;
        var rnd = new Random(1126);
        double[] row = new double[h], col = new double[w];
        for (int y = 0; y < h; y++) row[y] = (rnd.NextDouble() - 0.5) * 5 + (rnd.NextDouble() < 0.06 ? -3 : 0);
        for (int x = 0; x < w; x++) col[x] = (rnd.NextDouble() - 0.5) * 5 + (rnd.NextDouble() < 0.06 ? -3 : 0);
        byte[] px = new byte[w * h * 3];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double weave = ((x + y) % 2 == 0) ? row[y] : col[x];
                double n = weave + (rnd.NextDouble() - 0.5) * 2.5;
                int i = (y * w + x) * 3;
                px[i] = (byte)Math.Max(0, Math.Min(255, 246 + n));
                px[i + 1] = (byte)Math.Max(0, Math.Min(255, 240 + n));
                px[i + 2] = (byte)Math.Max(0, Math.Min(255, 230 + n));
            }
        }
        var bmp = BitmapSource.Create(w, h, 96, 96, PixelFormats.Rgb24, null, px, w * 3);
        var enc = new JpegBitmapEncoder { QualityLevel = 88 };
        enc.Frames.Add(BitmapFrame.Create(bmp));
        using (var fs = File.Create(outPath)) { enc.Save(fs); }
    }
}
