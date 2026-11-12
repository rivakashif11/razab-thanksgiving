using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

public class GifTools {
    public class AnimatedGifBuilder : IDisposable {
        private Stream _stream;
        private bool _first = true;

        public AnimatedGifBuilder(Stream stream) {
            _stream = stream;
        }

        public void AddFrame(Bitmap bmp, int delayMs) {
            using (var ms = new MemoryStream()) {
                bmp.Save(ms, ImageFormat.Gif);
                byte[] bytes = ms.ToArray();

                if (_first) {
                    _stream.Write(bytes, 0, 13);
                    if ((bytes[10] & 0x80) != 0) {
                        int gctSize = 3 * (1 << ((bytes[10] & 0x07) + 1));
                        _stream.Write(bytes, 13, gctSize);
                    }
                    byte[] loopExt = new byte[] {
                        0x21, 0xFF, 0x0B,
                        (byte)'N', (byte)'E', (byte)'T', (byte)'S', (byte)'C', (byte)'A', (byte)'P', (byte)'E',
                        (byte)'2', (byte)'.', (byte)'0',
                        0x03, 0x01, 0x00, 0x00, 0x00
                    };
                    _stream.Write(loopExt, 0, loopExt.Length);
                    _first = false;
                }

                int delay100th = delayMs / 10;
                byte[] gce = new byte[] {
                    0x21, 0xF9, 0x04,
                    0x09,
                    (byte)(delay100th & 0xFF),
                    (byte)((delay100th >> 8) & 0xFF),
                    0x00, 0x00
                };
                _stream.Write(gce, 0, gce.Length);

                int imgDescIdx = -1;
                for (int i = 13; i < bytes.Length; i++) {
                    if (bytes[i] == 0x2C) {
                        imgDescIdx = i;
                        break;
                    }
                }
                if (imgDescIdx != -1) {
                    int len = bytes.Length - imgDescIdx - 1;
                    _stream.Write(bytes, imgDescIdx, len);
                }
            }
        }

        public void Finish() {
            _stream.WriteByte(0x3B);
        }

        public void Dispose() {
            Finish();
        }
    }

    // 1. Candle Flame GIF (56x56)
    public static void GenerateCandle(string outPath) {
        int w = 56, h = 56;
        int frames = 16;
        using (var fs = File.Create(outPath))
        using (var builder = new AnimatedGifBuilder(fs)) {
            for (int f = 0; f < frames; f++) {
                using (var bmp = new Bitmap(w, h))
                using (var g = Graphics.FromImage(bmp)) {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(250, 247, 242)); // #FAF7F2

                    double angle = (f / (double)frames) * Math.PI * 2.0;
                    float flicker = (float)(Math.Sin(angle) * 1.5);
                    float flickerY = (float)(Math.Cos(angle) * 1.0);

                    // Candlestick stem (Antique Brass #C87F4E / #F7CC83)
                    using (var brassBrush = new LinearGradientBrush(new Rectangle(24, 30, 8, 22), Color.FromArgb(200, 127, 78), Color.FromArgb(247, 204, 131), LinearGradientMode.Horizontal)) {
                        g.FillRectangle(brassBrush, 25, 30, 6, 20);
                        g.FillEllipse(brassBrush, 21, 28, 14, 5); // bobeche
                        g.FillEllipse(brassBrush, 17, 48, 22, 6); // base
                    }

                    // Candle wick
                    using (var wickPen = new Pen(Color.FromArgb(60, 40, 30), 1.5f)) {
                        g.DrawLine(wickPen, 28, 29, 28 + flicker * 0.3f, 23);
                    }

                    // Outer warm glow aura
                    int auraAlpha = (int)(55 + Math.Sin(angle * 2) * 15);
                    using (var auraBrush = new SolidBrush(Color.FromArgb(auraAlpha, 247, 204, 131))) {
                        g.FillEllipse(auraBrush, 14 - flicker * 0.3f, 5 - flickerY * 0.3f, 28, 28);
                    }

                    // Outer flame (Harvest Gold #F7CC83 -> Orange #C87F4E)
                    using (var flamePath = new GraphicsPath()) {
                        float fx = 28 + flicker * 0.8f;
                        float fy = 10 + flickerY * 0.6f;
                        flamePath.AddBezier(fx, fy, fx + 5.5f, fy + 7, fx + 5.5f, fy + 12, 28, 23);
                        flamePath.AddBezier(28, 23, fx - 5.5f, fy + 12, fx - 5.5f, fy + 7, fx, fy);
                        using (var flameBrush = new LinearGradientBrush(new Rectangle(20, 8, 16, 16), Color.FromArgb(250, 185, 80), Color.FromArgb(200, 95, 45), LinearGradientMode.Vertical)) {
                            g.FillPath(flameBrush, flamePath);
                        }
                    }

                    // Inner bright teardrop (Ivory Core)
                    using (var corePath = new GraphicsPath()) {
                        float fx = 28 + flicker * 0.5f;
                        float fy = 15 + flickerY * 0.4f;
                        corePath.AddBezier(fx, fy, fx + 2.5f, fy + 4, fx + 2.5f, fy + 6, 28, 22);
                        corePath.AddBezier(28, 22, fx - 2.5f, fy + 6, fx - 2.5f, fy + 4, fx, fy);
                        using (var coreBrush = new SolidBrush(Color.FromArgb(255, 255, 242))) {
                            g.FillPath(coreBrush, corePath);
                        }
                    }

                    builder.AddFrame(bmp, 75);
                }
            }
        }
    }

    // 2. Autumn Vines Garland Banner GIF (600x44)
    public static void GenerateVines(string outPath) {
        int w = 600, h = 44;
        int frames = 20;

        // Leaf definitions: x, y, size, color type, drift speed
        var rnd = new Random(42);
        int leafCount = 18;
        float[] lx = new float[leafCount];
        float[] ly = new float[leafCount];
        float[] lrot = new float[leafCount];
        Color[] lcolors = new Color[] {
            Color.FromArgb(195, 214, 146), // Sage Leaf #C3D692
            Color.FromArgb(200, 127, 78),  // Terracotta #C87F4E
            Color.FromArgb(247, 204, 131), // Honey Gold #F7CC83
            Color.FromArgb(143, 84, 74)    // Spiced Chestnut #8F544A
        };
        int[] ltypes = new int[leafCount];

        for (int i = 0; i < leafCount; i++) {
            lx[i] = (w / (float)leafCount) * i + rnd.Next(-10, 10);
            ly[i] = rnd.Next(8, h - 8);
            lrot[i] = (float)(rnd.NextDouble() * 360.0);
            ltypes[i] = rnd.Next(lcolors.Length);
        }

        using (var fs = File.Create(outPath))
        using (var builder = new AnimatedGifBuilder(fs)) {
            for (int f = 0; f < frames; f++) {
                using (var bmp = new Bitmap(w, h))
                using (var g = Graphics.FromImage(bmp)) {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    // Background: #8F544A (Spiced Chestnut background to match nav header)
                    g.Clear(Color.FromArgb(143, 84, 74));

                    // Delicate undulating gold vine line
                    using (var vinePen = new Pen(Color.FromArgb(120, 247, 204, 131), 1.5f)) {
                        var pts = new PointF[13];
                        for (int p = 0; p < pts.Length; p++) {
                            float px = (w / 12f) * p;
                            float py = (h / 2f) + (float)(Math.Sin((p * 0.8) + (f / (double)frames) * Math.PI * 2.0) * 5.0);
                            pts[p] = new PointF(px, py);
                        }
                        g.DrawCurve(vinePen, pts);
                    }

                    // Gentle floating/fluttering leaves
                    for (int i = 0; i < leafCount; i++) {
                        double t = (f / (double)frames) * Math.PI * 2.0 + (i * 0.5);
                        float cx = lx[i] + (float)(Math.Sin(t) * 8.0);
                        float cy = ly[i] + (float)(Math.Cos(t * 1.5) * 4.0);
                        float curRot = lrot[i] + (float)(Math.Sin(t) * 20.0);

                        var state = g.Save();
                        g.TranslateTransform(cx, cy);
                        g.RotateTransform(curRot);

                        using (var leafBrush = new SolidBrush(lcolors[ltypes[i]])) {
                            // Draw stylized botanical leaf
                            using (var path = new GraphicsPath()) {
                                path.AddArc(-8, -4, 16, 8, 0, 180);
                                path.AddArc(-8, -4, 16, 8, 180, 180);
                                g.FillPath(leafBrush, path);
                            }
                        }
                        g.Restore(state);
                    }

                    // Top and bottom decorative hairline
                    using (var borderPen = new Pen(Color.FromArgb(200, 247, 204, 131), 1f)) {
                        g.DrawLine(borderPen, 0, 0, w, 0);
                        g.DrawLine(borderPen, 0, h - 1, w, h - 1);
                    }

                    builder.AddFrame(bmp, 80);
                }
            }
        }
    }

    // 3. Antique Hearth Urgency Clock GIF (552x260)
    public static void GenerateClock(string outPath) {
        int w = 552, h = 260;
        int frames = 24; // 2 seconds loop at 83ms per frame

        using (var fs = File.Create(outPath))
        using (var builder = new AnimatedGifBuilder(fs)) {
            for (int f = 0; f < frames; f++) {
                using (var bmp = new Bitmap(w, h))
                using (var g = Graphics.FromImage(bmp)) {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    // Deep warm hearth background (#231914 -> #362218)
                    using (var bgBrush = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(35, 25, 20), Color.FromArgb(54, 34, 24), LinearGradientMode.ForwardDiagonal)) {
                        g.FillRectangle(bgBrush, 0, 0, w, h);
                    }

                    // Outer double border (Terracotta & Gold)
                    using (var borderPen1 = new Pen(Color.FromArgb(200, 127, 78), 2f))
                    using (var borderPen2 = new Pen(Color.FromArgb(247, 204, 131), 1f)) {
                        g.DrawRectangle(borderPen1, 4, 4, w - 8, h - 8);
                        g.DrawRectangle(borderPen2, 8, 8, w - 16, h - 16);
                    }

                    // Antique Clock on Left side
                    int clockX = 130, clockY = 130, clockR = 95;

                    // Outer brass rim glow
                    using (var glowBrush = new SolidBrush(Color.FromArgb(30, 247, 204, 131))) {
                        g.FillEllipse(glowBrush, clockX - clockR - 8, clockY - clockR - 8, (clockR + 8) * 2, (clockR + 8) * 2);
                    }

                    // Clock outer brass case
                    using (var caseBrush = new LinearGradientBrush(new Rectangle(clockX - clockR, clockY - clockR, clockR * 2, clockR * 2), Color.FromArgb(200, 127, 78), Color.FromArgb(143, 84, 74), LinearGradientMode.Vertical)) {
                        g.FillEllipse(caseBrush, clockX - clockR, clockY - clockR, clockR * 2, clockR * 2);
                    }

                    // Clock inner face (Parchment Linen #FAF7F2 / #EADBC8)
                    int faceR = clockR - 10;
                    using (var faceBrush = new LinearGradientBrush(new Rectangle(clockX - faceR, clockY - faceR, faceR * 2, faceR * 2), Color.FromArgb(250, 247, 242), Color.FromArgb(234, 219, 200), LinearGradientMode.ForwardDiagonal)) {
                        g.FillEllipse(faceBrush, clockX - faceR, clockY - faceR, faceR * 2, faceR * 2);
                    }

                    // Dial ring
                    using (var ringPen = new Pen(Color.FromArgb(143, 84, 74), 1.5f)) {
                        g.DrawEllipse(ringPen, clockX - faceR + 4, clockY - faceR + 4, (faceR - 4) * 2, (faceR - 4) * 2);
                    }

                    // Roman numerals (XII, III, VI, IX) and tick marks
                    using (var numFont = new Font("Georgia", 11, FontStyle.Bold))
                    using (var numBrush = new SolidBrush(Color.FromArgb(50, 30, 20)))
                    using (var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
                        g.DrawString("XII", numFont, numBrush, clockX, clockY - faceR + 18, sf);
                        g.DrawString("III", numFont, numBrush, clockX + faceR - 18, clockY, sf);
                        g.DrawString("VI", numFont, numBrush, clockX, clockY + faceR - 18, sf);
                        g.DrawString("IX", numFont, numBrush, clockX - faceR + 18, clockY, sf);

                        // Hour ticks
                        using (var tickPen = new Pen(Color.FromArgb(100, 60, 40), 2f)) {
                            for (int hIdx = 0; hIdx < 12; hIdx++) {
                                double a = (hIdx / 12.0) * Math.PI * 2.0;
                                float r1 = faceR - 6;
                                float r2 = faceR - 11;
                                g.DrawLine(tickPen,
                                    (float)(clockX + Math.Sin(a) * r1), (float)(clockY - Math.Cos(a) * r1),
                                    (float)(clockX + Math.Sin(a) * r2), (float)(clockY - Math.Cos(a) * r2));
                            }
                        }
                    }

                    // Brand inscription on clock face
                    using (var brandFont = new Font("Arial", 7, FontStyle.Bold))
                    using (var brandBrush = new SolidBrush(Color.FromArgb(143, 84, 74)))
                    using (var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
                        g.DrawString("RAZAB", brandFont, brandBrush, clockX, clockY + 28, sf);
                    }

                    // Clock hands: Time set to 11:58 (2 minutes before midnight)
                    // Hour hand pointing at ~11.95 (almost 12)
                    double hourAngle = (11.95 / 12.0) * Math.PI * 2.0;
                    using (var hourPen = new Pen(Color.FromArgb(40, 20, 15), 3.5f) { EndCap = LineCap.Round }) {
                        g.DrawLine(hourPen, clockX, clockY,
                            (float)(clockX + Math.Sin(hourAngle) * 44),
                            (float)(clockY - Math.Cos(hourAngle) * 44));
                    }

                    // Minute hand pointing at 58 minutes
                    double minAngle = (58.5 / 60.0) * Math.PI * 2.0;
                    using (var minPen = new Pen(Color.FromArgb(40, 20, 15), 2.5f) { EndCap = LineCap.Round }) {
                        g.DrawLine(minPen, clockX, clockY,
                            (float)(clockX + Math.Sin(minAngle) * 62),
                            (float)(clockY - Math.Cos(minAngle) * 62));
                    }

                    // Animated Second Hand: Sweeping/Ticking forward across the frames
                    double secAngle = ((f / (double)frames) * 6.0 / 60.0 + (50.0 / 60.0)) * Math.PI * 2.0;
                    using (var secPen = new Pen(Color.FromArgb(200, 60, 40), 1.5f)) {
                        g.DrawLine(secPen,
                            (float)(clockX - Math.Sin(secAngle) * 14),
                            (float)(clockY + Math.Cos(secAngle) * 14),
                            (float)(clockX + Math.Sin(secAngle) * 68),
                            (float)(clockY - Math.Cos(secAngle) * 68));
                    }

                    // Center brass pin
                    using (var pinBrush = new SolidBrush(Color.FromArgb(200, 127, 78))) {
                        g.FillEllipse(pinBrush, clockX - 4, clockY - 4, 8, 8);
                    }

                    // Right Side: Typography & Urgency Copy
                    int textX = 260;
                    using (var sfL = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center }) {
                        // Eyebrow badge
                        using (var eyeFont = new Font("Arial", 9, FontStyle.Bold))
                        using (var eyeBrush = new SolidBrush(Color.FromArgb(247, 204, 131))) { // Gold
                            g.DrawString("✦  THE HARVEST CLOCK IS TICKING  ✦", eyeFont, eyeBrush, textX, 55, sfL);
                        }

                        // Main Urgency Headline
                        using (var headFont = new Font("Georgia", 21, FontStyle.Bold))
                        using (var headBrush = new SolidBrush(Color.FromArgb(250, 247, 242))) { // Linen White
                            g.DrawString("20% Off Expires At", headFont, headBrush, textX, 90, sfL);
                            g.DrawString("Midnight, Nov 27", headFont, headBrush, textX, 120, sfL);
                        }

                        // Gold accent line
                        using (var linePen = new Pen(Color.FromArgb(200, 127, 78), 1.5f)) {
                            g.DrawLine(linePen, textX, 148, textX + 240, 148);
                        }

                        // Subtitle
                        using (var subFont = new Font("Georgia", 11, FontStyle.Italic))
                        using (var subBrush = new SolidBrush(Color.FromArgb(195, 214, 146))) { // Sage Leaf
                            g.DrawString("Code HARVEST ends at the stroke of 11:59 PM EST", subFont, subBrush, textX, 170, sfL);
                        }

                        // CTA indicator
                        using (var ctaFont = new Font("Arial", 10, FontStyle.Bold))
                        using (var ctaBrush = new SolidBrush(Color.FromArgb(247, 204, 131))) {
                            g.DrawString("CLAIM YOUR 20% DISCOUNT  →", ctaFont, ctaBrush, textX, 205, sfL);
                        }
                    }

                    builder.AddFrame(bmp, 83); // ~12 fps
                }
            }
        }
    }
}
