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
}
