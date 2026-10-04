using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Chang.Profile;
using UnityEngine;

namespace Chang.Mascot
{
    /// <summary>How much of the mascot a picture shows.</summary>
    public enum MascotFraming
    {
        /// <summary>Everything, hats included.</summary>
        Full,

        /// <summary>The face without the space for a hat (avatars, option tiles).</summary>
        Face
    }

    /// <summary>
    /// Draws the parametric elephant as SVG markup. A port of the generator the Penpot page "Redesign · Mascot" was made with,
    /// so the game draws exactly the design's options. Coordinates are the generator's: the head is centered at (60, 56).
    /// </summary>
    public static class MascotSvg
    {
        private const string Ink = "#1D2140";
        private const string Gold = "#E3A21A";
        private const string White = "#FFFFFF";

        // Areas of the generator's coordinates shown by each framing.
        private static readonly Rect FullBox = new(-14f, -34f, 148f, 152f);
        private static readonly Rect FaceBox = new(-14f, 2f, 148f, 116f);

        /// <summary>The whole SVG document of a look, its viewBox grown to the <paramref name="aspect"/> (width / height) of the picture.</summary>
        public static string Build(MascotLook look, MascotFraming framing, float aspect)
        {
            // A hat doesn't fit the face framing, so a mascot with a hat is always drawn whole.
            var box = framing == MascotFraming.Full || look.Hat != 0 ? FullBox : FaceBox;
            return Document(Fit(box, aspect), Markup(look));
        }

        /// <summary>A color swatch (color options of the editor): a circle of 72% of the picture width with a thin outline.</summary>
        public static string Swatch(string color, float aspect)
        {
            var box = Fit(new Rect(0f, 0f, 100f, 100f), aspect);
            return Document(box, $"<circle cx=\"50\" cy=\"50\" r=\"35.5\" fill=\"{color}\" stroke=\"#E9DFCB\" stroke-width=\"1\"/>");
        }

        /// <summary>Grows the box on one side so it has the given aspect, keeping it centered.</summary>
        private static Rect Fit(Rect box, float aspect)
        {
            if (aspect <= 0f || float.IsNaN(aspect))
            {
                return box;
            }

            if (box.width / box.height < aspect)
            {
                var width = box.height * aspect;
                return new Rect(box.center.x - width * 0.5f, box.y, width, box.height);
            }

            var height = box.width / aspect;
            return new Rect(box.x, box.center.y - height * 0.5f, box.width, height);
        }

        private static string Document(Rect box, string markup)
        {
            // The content is shifted instead of using a negative viewBox origin.
            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{N(box.width)}\" height=\"{N(box.height)}\" viewBox=\"0 0 {N(box.width)} {N(box.height)}\">" +
                   $"<g transform=\"translate({N(-box.x)} {N(-box.y)})\">{markup}</g></svg>";
        }

        /// <summary>The mascot shapes, back to front.</summary>
        public static string Markup(MascotLook look)
        {
            var head = MascotCatalog.Color(MascotPart.Head, look.Head);
            var dark = Shade(head, -0.18);
            var blush = MascotCatalog.Color(MascotPart.BlushColor, look.BlushColor);
            var inner = Mix(blush, White, 0.25);
            var tusk = MascotCatalog.Color(MascotPart.TuskColor, look.TuskColor);

            return Ears(Option(look.Ears), dark, inner)
                   + $"<ellipse cx=\"60\" cy=\"56\" rx=\"32\" ry=\"34\" fill=\"{head}\"/>"
                   + $"<path d=\"M52 76 C52 92 56 104 66 108 C72 110 76 106 74 101 C66 100 66 92 68 76 Z\" fill=\"{head}\"/>"
                   + $"<path d=\"M58 92 C60 93 62 93 64 92 M60 99 C62 100 64 100 66 99\" fill=\"none\" stroke=\"{dark}\" stroke-width=\"1.4\" stroke-linecap=\"round\"/>"
                   + Tusks(Option(look.Tusks), tusk)
                   + Blush(Option(look.Blush), blush)
                   + Eyes(Option(look.Eyes))
                   + Mark(Option(look.Mark))
                   + Hat(Option(look.Hat));
        }

        private static int Option(int value) => Math.Clamp(value, 0, MascotLook.OptionCount - 1);

        // ---- ears (left side; the right one is mirrored) ----

        private static string Ears(int i, string dark, string inner)
        {
            var (shape, cx, cy, fold) = Ear(i);
            var scale = $"transform=\"translate({N(cx)} {N(cy)}) scale(0.62) translate({N(-cx)} {N(-cy)})\"";
            var left = shape(dark) + $"<g {scale}>{shape(inner)}</g>";
            if (fold)
            {
                left += $"<path d=\"M10 38 C20 48 20 60 10 70\" fill=\"none\" stroke=\"{Shade(dark, -0.2)}\" stroke-width=\"3\" stroke-linecap=\"round\"/>";
            }

            return $"<g>{left}</g>" + Mirror(left);
        }

        private static (Func<string, string> shape, float cx, float cy, bool fold) Ear(int i) => i switch
        {
            0 => (f => $"<ellipse cx=\"28\" cy=\"52\" rx=\"24\" ry=\"28\" fill=\"{f}\"/>", 28, 52, false),
            1 => (f => $"<circle cx=\"28\" cy=\"50\" r=\"25\" fill=\"{f}\"/>", 28, 50, false),
            2 => (f => $"<circle cx=\"34\" cy=\"48\" r=\"16\" fill=\"{f}\"/>", 34, 48, false),
            3 => (f => $"<ellipse cx=\"22\" cy=\"56\" rx=\"31\" ry=\"36\" fill=\"{f}\"/>", 22, 56, false),
            4 => (f => $"<path d=\"M38 34 C18 30 6 50 10 70 C13 84 24 92 30 92 C36 80 40 60 38 34 Z\" fill=\"{f}\"/>", 24, 62, false),
            5 => (f => $"<path d=\"M40 30 C20 18 0 26 -2 50 C-4 72 12 90 34 84 C30 70 34 50 40 30 Z\" fill=\"{f}\"/>", 18, 54, false),
            6 => (f => $"<path d=\"M38 62 C20 64 8 48 10 18 C26 22 38 38 38 62 Z\" fill=\"{f}\"/>", 26, 44, false),
            7 => (f => $"<path d=\"M26 74 C8 60 2 46 9 36 C15 28 24 30 26 39 C28 30 37 28 43 36 C50 46 44 60 26 74 Z\" fill=\"{f}\"/>", 26, 50, false),
            8 => (f => $"<path d=\"{Scallop(27, 52, 22, 9, 3)}\" fill=\"{f}\"/>", 27, 52, false),
            9 => (f => $"<rect x=\"6\" y=\"30\" width=\"40\" height=\"44\" rx=\"14\" fill=\"{f}\"/>", 26, 52, false),
            10 => (f => $"<ellipse cx=\"24\" cy=\"54\" rx=\"30\" ry=\"20\" fill=\"{f}\"/>", 24, 54, false),
            11 => (f => $"<ellipse cx=\"32\" cy=\"52\" rx=\"15\" ry=\"32\" fill=\"{f}\"/>", 32, 52, false),
            12 => (f => $"<ellipse cx=\"26\" cy=\"48\" rx=\"22\" ry=\"30\" transform=\"rotate(30 26 48)\" fill=\"{f}\"/>", 26, 48, false),
            13 => (f => $"<ellipse cx=\"26\" cy=\"58\" rx=\"22\" ry=\"30\" transform=\"rotate(-30 26 58)\" fill=\"{f}\"/>", 26, 58, false),
            14 => (f => $"<circle cx=\"22\" cy=\"42\" r=\"15\" fill=\"{f}\"/><circle cx=\"30\" cy=\"60\" r=\"17\" fill=\"{f}\"/><circle cx=\"13\" cy=\"62\" r=\"12\" fill=\"{f}\"/>", 22, 54, false),
            15 => (f => $"<path d=\"M38 30 C14 22 0 42 7 58 C12 70 24 70 27 62 C28 72 33 88 40 81 C45 60 47 40 38 30 Z\" fill=\"{f}\"/>", 24, 54, false),
            16 => (f => $"<path d=\"M40 24 A27 30 0 0 0 40 84 C33 64 33 44 40 24 Z\" fill=\"{f}\"/>", 26, 54, false),
            17 => (f => $"<path d=\"M38 40 C26 16 2 20 4 50 C6 76 26 86 38 70 C32 60 32 50 38 40 Z\" fill=\"{f}\"/>", 22, 52, false),
            18 => (f => $"<circle cx=\"22\" cy=\"40\" r=\"15\" fill=\"{f}\"/><circle cx=\"20\" cy=\"66\" r=\"17\" fill=\"{f}\"/>", 21, 53, false),
            _ => (f => $"<ellipse cx=\"26\" cy=\"52\" rx=\"24\" ry=\"28\" fill=\"{f}\"/>", 26, 52, true)
        };

        private static string Scallop(double cx, double cy, double r, int n, double amp)
        {
            var d = new StringBuilder();
            for (var i = 0; i <= n * 8; i++)
            {
                var a = i / (double)(n * 8) * Math.PI * 2;
                var rr = r + amp * Math.Cos(a * n);
                d.Append(i == 0 ? 'M' : 'L').Append(F1(cx + rr * Math.Cos(a))).Append(' ').Append(F1(cy + rr * Math.Sin(a))).Append(' ');
            }

            return d.Append('Z').ToString();
        }

        // ---- eyes ----

        private static string Eyes(int i)
        {
            if (i == 5)
            {
                return Eye(48, 0) + Eye(72, 3);
            }

            if (i == 15)
            {
                return $"<path d=\"M44 54 L51 58 L44 62\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>" +
                       $"<path d=\"M76 54 L69 58 L76 62\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";
            }

            var s = Eye(48, i) + Eye(72, i);
            if (i == 10)
            {
                s += $"<path d=\"M56 57 Q60 55 64 57\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2\"/>";
            }

            if (i == 11)
            {
                s += $"<path d=\"M56 58 Q60 56 64 58\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2\"/>";
            }

            return s;
        }

        private static string Eye(double x, int k)
        {
            const double y = 58;
            switch (k)
            {
                case 0:
                    return $"<ellipse cx=\"{N(x)}\" cy=\"{N(y)}\" rx=\"4.5\" ry=\"5.5\" fill=\"{Ink}\"/><circle cx=\"{N(x + 1.5)}\" cy=\"{N(y - 2)}\" r=\"1.6\" fill=\"{White}\"/>";
                case 1:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"6.5\" fill=\"{Ink}\"/><circle cx=\"{N(x + 2)}\" cy=\"{N(y - 2.5)}\" r=\"2.2\" fill=\"{White}\"/><circle cx=\"{N(x - 2)}\" cy=\"{N(y + 2.5)}\" r=\"1\" fill=\"{White}\"/>";
                case 2:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"3\" fill=\"{Ink}\"/>";
                case 3:
                    return $"<path d=\"M{N(x - 5)} {N(y + 2)} Q{N(x)} {N(y - 6)} {N(x + 5)} {N(y + 2)}\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2.8\" stroke-linecap=\"round\"/>";
                case 4:
                    return $"<path d=\"M{N(x - 5)} {N(y)} L{N(x + 5)} {N(y)}\" stroke=\"{Ink}\" stroke-width=\"2.6\" stroke-linecap=\"round\"/>" +
                           $"<path d=\"M{N(x - 3)} {N(y)} L{N(x - 4)} {N(y + 3)} M{N(x + 3)} {N(y)} L{N(x + 4)} {N(y + 3)}\" stroke=\"{Ink}\" stroke-width=\"1.6\" stroke-linecap=\"round\"/>";
                case 6:
                    return Eye(x, 0) +
                           $"<path d=\"M{N(x - 4)} {N(y - 5)} L{N(x - 6)} {N(y - 8)} M{N(x)} {N(y - 6)} L{N(x)} {N(y - 9)} M{N(x + 4)} {N(y - 5)} L{N(x + 6)} {N(y - 8)}\" stroke=\"{Ink}\" stroke-width=\"1.5\" stroke-linecap=\"round\"/>";
                case 7:
                    return $"<path d=\"{Star(x, y, 7, 2.8, 5)}\" fill=\"{Gold}\"/>";
                case 8:
                    return $"<path d=\"M{N(x)} {N(y + 6)} C{N(x - 8)} {N(y)} {N(x - 6)} {N(y - 6)} {N(x)} {N(y - 2)} C{N(x + 6)} {N(y - 6)} {N(x + 8)} {N(y)} {N(x)} {N(y + 6)} Z\" fill=\"#E0485F\"/>";
                case 9:
                    return $"<ellipse cx=\"{N(x)}\" cy=\"{N(y)}\" rx=\"5.5\" ry=\"7\" fill=\"{Ink}\"/><ellipse cx=\"{N(x + 1.5)}\" cy=\"{N(y - 2.5)}\" rx=\"2.4\" ry=\"3\" fill=\"{White}\"/><circle cx=\"{N(x - 2)}\" cy=\"{N(y + 3)}\" r=\"1.2\" fill=\"{White}\"/>";
                case 10:
                    return $"<rect x=\"{N(x - 8)}\" y=\"{N(y - 5)}\" width=\"16\" height=\"11\" rx=\"4\" fill=\"{Ink}\"/><path d=\"M{N(x - 4)} {N(y - 2)} L{N(x + 1)} {N(y - 2)}\" stroke=\"{White}\" stroke-opacity=\"0.6\" stroke-width=\"1.5\" stroke-linecap=\"round\"/>";
                case 11:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"8\" fill=\"{White}\" fill-opacity=\"0.45\" stroke=\"{Ink}\" stroke-width=\"2\"/><circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"2.6\" fill=\"{Ink}\"/>";
                case 12:
                    var brow = x > 60 ? $" transform=\"translate({N(2 * x)} 0) scale(-1 1)\"" : "";
                    return Eye(x, 0) + $"<path d=\"M{N(x - 6)} {N(y - 9)} L{N(x + 5)} {N(y - 7)}\" stroke=\"{Ink}\" stroke-width=\"2.2\" stroke-linecap=\"round\"{brow}/>";
                case 13:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"6.5\" fill=\"{White}\" stroke=\"{Ink}\" stroke-width=\"1.6\"/><circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"2.6\" fill=\"{Ink}\"/>";
                case 14:
                    return $"<ellipse cx=\"{N(x)}\" cy=\"{N(y)}\" rx=\"5\" ry=\"6\" fill=\"{White}\" stroke=\"{Ink}\" stroke-width=\"1.4\"/><circle cx=\"{N(x + 2)}\" cy=\"{N(y)}\" r=\"2.8\" fill=\"{Ink}\"/>";
                case 16:
                    return $"<path d=\"M{N(x)} {N(y - 7)} C{N(x + 5)} {N(y - 1)} {N(x + 5)} {N(y + 5)} {N(x)} {N(y + 5)} C{N(x - 5)} {N(y + 5)} {N(x - 5)} {N(y - 1)} {N(x)} {N(y - 7)} Z\" fill=\"{Ink}\"/><circle cx=\"{N(x + 1.5)}\" cy=\"{N(y)}\" r=\"1.3\" fill=\"{White}\"/>";
                case 17:
                    return $"<path d=\"M{N(x - 5)} {N(y - 1)} Q{N(x)} {N(y + 6)} {N(x + 5)} {N(y - 1)} Q{N(x)} {N(y + 2)} {N(x - 5)} {N(y - 1)} Z\" fill=\"{Ink}\" stroke=\"{Ink}\" stroke-width=\"1.6\" stroke-linejoin=\"round\"/>";
                case 18:
                    return $"<path d=\"M{N(x)} {N(y)} m0 0 a1.5 1.5 0 1 1 1.5 1.5 a3 3 0 1 1 -3 -3 a4.5 4.5 0 1 1 4.5 4.5\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"1.6\" stroke-linecap=\"round\"/>";
                case 19:
                    return Eye(x, 0) + $"<path d=\"M{N(x - 5)} {N(y - 9)} Q{N(x)} {N(y - 12)} {N(x + 5)} {N(y - 9)}\" fill=\"none\" stroke=\"{Ink}\" stroke-width=\"2\" stroke-linecap=\"round\"/>";
                default:
                    return "";
            }
        }

        // ---- tusks ----

        private static string Tusks(int i, string color)
        {
            string Stroke(string d, double width) =>
                $"<path d=\"{d}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"{N(width)}\" stroke-linecap=\"round\"/>";

            if (i == 16)
            {
                // Broken one: a classic tusk on the left, a stub on the right.
                return Stroke("M41 82 C38 88 40 94 46 95", 4) + Mirror(Stroke("M42 82 L42 86", 5));
            }

            var left = i switch
            {
                1 => Stroke("M41 82 C38 88 40 94 46 95", 4),
                2 => Stroke("M42 82 L41 90", 4),
                3 => Stroke("M41 82 C36 92 38 102 48 104", 4.5),
                4 => Stroke("M41 82 C33 96 36 110 50 112", 5),
                5 => Stroke("M41 82 C34 92 36 100 44 98 C48 96 47 91 44 91", 3.5),
                6 => Stroke("M42 82 L34 100", 4),
                7 => Stroke("M41 82 C39 86 40 89 44 90", 7),
                8 => Stroke("M41 82 C36 94 40 104 50 106", 2.5),
                9 => Stroke("M41 82 C36 90 30 94 24 92", 4),
                10 => Stroke("M41 82 C38 92 44 98 52 96", 4),
                11 => Stroke("M41 82 C38 88 40 94 46 95", 4) + Stroke("M46 84 C44 88 45 91 49 92", 3),
                12 => $"<path d=\"M37 80 L46 80 L40 99 Z\" fill=\"{color}\" stroke=\"{color}\" stroke-width=\"1.5\" stroke-linejoin=\"round\"/>",
                13 => Stroke("M42 82 L42 86", 6.5),
                14 => Stroke("M41 82 C28 92 30 110 46 108 C54 106 54 98 48 96", 5),
                15 => $"<circle cx=\"41\" cy=\"85\" r=\"2.8\" fill=\"{color}\"/>",
                17 => Stroke("M41 82 C38 88 40 94 46 95", 4) + $"<circle cx=\"46\" cy=\"95\" r=\"2.7\" fill=\"{Gold}\"/>",
                18 => Stroke("M41 82 C35 77 33 71 37 64", 4),
                19 => Stroke("M41 82 C34 88 46 94 40 102", 4),
                _ => ""
            };

            return left.Length == 0 ? "" : left + Mirror(left);
        }

        // ---- forehead mark ----

        private static string Mark(int i)
        {
            switch (i)
            {
                case 1:
                    return $"<path d=\"M60 26 L66 34 L60 42 L54 34 Z\" fill=\"{Gold}\"/><circle cx=\"60\" cy=\"34\" r=\"2.6\" fill=\"{White}\"/>";
                case 2:
                    return "<circle cx=\"60\" cy=\"36\" r=\"3.8\" fill=\"#D63A4A\"/>";
                case 3:
                    return "<path d=\"M60 40 C56 36 56 30 60 26 C64 30 64 36 60 40 Z\" fill=\"#E0688F\"/>" +
                           "<path d=\"M60 40 C54 40 50 36 49 31 C54 31 58 34 60 40 Z\" fill=\"#F29BB6\"/>" +
                           "<path d=\"M60 40 C66 40 70 36 71 31 C66 31 62 34 60 40 Z\" fill=\"#F29BB6\"/>";
                case 4:
                    return Petals(60, 34, 5, 4.5, 3, "#F7A1B5") + $"<circle cx=\"60\" cy=\"34\" r=\"2.6\" fill=\"{Gold}\"/>";
                case 5:
                    return $"<path d=\"{Star(60, 34, 8, 3.6, 5)}\" fill=\"{Gold}\"/>";
                case 6:
                    return "<path d=\"M60 41 C52 35 52 28 57 28 C59 28 60 30 60 31 C60 30 61 28 63 28 C68 28 68 35 60 41 Z\" fill=\"#E0485F\"/>";
                case 7:
                    return $"<path d=\"M64 26 A8 8 0 1 0 64 42 A10 10 0 0 1 64 26 Z\" fill=\"{Gold}\"/>";
                case 8:
                    return $"<circle cx=\"60\" cy=\"34\" r=\"4.5\" fill=\"{Gold}\"/>" + string.Concat(Enumerable.Range(0, 8).Select(j =>
                        $"<path d=\"M60 26 L60 23\" stroke=\"{Gold}\" stroke-width=\"2\" stroke-linecap=\"round\" transform=\"rotate({j * 45} 60 34)\"/>"));
                case 9:
                    return $"<circle cx=\"53\" cy=\"34\" r=\"2.4\" fill=\"{Gold}\"/><circle cx=\"60\" cy=\"31\" r=\"2.4\" fill=\"{Gold}\"/><circle cx=\"67\" cy=\"34\" r=\"2.4\" fill=\"{Gold}\"/>";
                case 10:
                    return $"<path d=\"M60 26 C64 32 65 36 60 42 C55 36 56 32 60 26 Z\" fill=\"#3A73D6\" stroke=\"{Gold}\" stroke-width=\"2\"/>";
                case 11:
                    return $"<path d=\"M60 44 C52 40 54 32 60 30 C57 34 60 37 62 34 C64 30 60 26 62 22 C68 28 70 40 60 44 Z\" fill=\"{Gold}\"/>";
                case 12:
                    return "<path d=\"M60 42 C52 36 54 28 64 24 C66 32 64 38 60 42 Z\" fill=\"#3FA06A\"/><path d=\"M60 42 L62 30\" stroke=\"#2C7A4E\" stroke-width=\"1.4\"/>";
                case 13:
                    return $"<path d=\"{Star(60, 34, 8, 2.2, 4)}\" fill=\"{Gold}\"/>";
                case 14:
                    var beads = new[] { 46, 52, 60, 68, 74 }.Select((x, j) =>
                        $"<circle cx=\"{x}\" cy=\"{(j == 2 ? 58 : 51 + Math.Abs(j - 2))}\" r=\"2\" fill=\"{Gold}\"/>");
                    return $"<path d=\"M42 30 C46 22 74 22 78 30 L72 48 L60 54 L48 48 Z\" fill=\"{Gold}\"/>" +
                           "<path d=\"M48 32 L72 32 M50 38 L70 38 M53 44 L67 44\" stroke=\"#A86F06\" stroke-width=\"1.6\"/>" +
                           "<circle cx=\"60\" cy=\"40\" r=\"3\" fill=\"#D63A4A\"/>" + string.Concat(beads);
                case 15:
                    return string.Concat(new[] { 36, 44, 52, 60, 68, 76, 84 }.Select((x, j) =>
                        $"<circle cx=\"{x}\" cy=\"{N(30 - Math.Sin(j / 6.0 * Math.PI) * 6)}\" r=\"{(j % 2 == 1 ? "3" : "3.6")}\" fill=\"{(j % 3 == 1 ? "#E0485F" : White)}\" stroke=\"#E9DFCB\" stroke-width=\"0.8\"/>"));
                case 16:
                    return "<rect x=\"50\" y=\"29\" width=\"20\" height=\"12\" rx=\"3\" fill=\"#D63A4A\"/><rect x=\"50\" y=\"31\" width=\"20\" height=\"8\" fill=\"#FFFFFF\"/><rect x=\"50\" y=\"33\" width=\"20\" height=\"4\" fill=\"#2F3C9E\"/>";
                case 17:
                    return $"<path d=\"M36 34 C46 26 74 26 84 34 L84 38 C74 30 46 30 36 38 Z\" fill=\"{Gold}\"/>" +
                           "<circle cx=\"60\" cy=\"31\" r=\"3\" fill=\"#D63A4A\"/><circle cx=\"48\" cy=\"33\" r=\"2.2\" fill=\"#3A73D6\"/><circle cx=\"72\" cy=\"33\" r=\"2.2\" fill=\"#3A73D6\"/>";
                case 18:
                    return $"<path d=\"M53 28 L60 33 L67 28 M53 34 L60 39 L67 34\" fill=\"none\" stroke=\"{Gold}\" stroke-width=\"2.4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";
                case 19:
                    return $"<circle cx=\"60\" cy=\"34\" r=\"7\" fill=\"none\" stroke=\"{Gold}\" stroke-width=\"1.6\" stroke-dasharray=\"1.5 2.5\"/>" +
                           Petals(60, 34, 6, 3, 1.8, Gold) + "<circle cx=\"60\" cy=\"34\" r=\"1.8\" fill=\"#D63A4A\"/>";
                default:
                    return "";
            }
        }

        // ---- blush ----

        private static string Blush(int i, string color) => Cheek(41, i, color) + Cheek(79, i, color);

        private static string Cheek(double x, int i, string col)
        {
            const double y = 68;
            const string o = "fill-opacity=\"0.85\"";
            switch (i)
            {
                case 1:
                    return $"<ellipse cx=\"{N(x)}\" cy=\"{N(y)}\" rx=\"5\" ry=\"3\" fill=\"{col}\" {o}/>";
                case 2:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"4\" fill=\"{col}\" {o}/>";
                case 3:
                    return $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"6.5\" fill=\"{col}\" fill-opacity=\"0.55\"/>";
                case 4:
                    return $"<path d=\"M{N(x)} {N(y + 4)} C{N(x - 6)} {N(y)} {N(x - 4)} {N(y - 4)} {N(x)} {N(y - 1)} C{N(x + 4)} {N(y - 4)} {N(x + 6)} {N(y)} {N(x)} {N(y + 4)} Z\" fill=\"{col}\"/>";
                case 5:
                    return string.Concat(new[] { -4, 0, 4 }.Select(dx =>
                        $"<path d=\"M{N(x + dx - 1.5)} {N(y + 3)} L{N(x + dx + 1.5)} {N(y - 3)}\" stroke=\"{col}\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>"));
                case 6:
                    return $"<path d=\"M{N(x)} {N(y)} a1.2 1.2 0 1 1 1.2 1.2 a2.6 2.6 0 1 1 -2.6 -2.6 a4 4 0 1 1 4 4\" fill=\"none\" stroke=\"{col}\" stroke-width=\"1.5\" stroke-linecap=\"round\"/>";
                case 7:
                    return $"<path d=\"{Star(x, y, 4.5, 1.8, 5)}\" fill=\"{col}\"/>";
                case 8:
                    return $"<path d=\"M{N(x - 4)} {N(y + 3)} L{N(x)} {N(y - 4)} L{N(x + 4)} {N(y + 3)} Z\" fill=\"{col}\" {o}/>";
                case 9:
                    return $"<circle cx=\"{N(x - 2.5)}\" cy=\"{N(y)}\" r=\"2\" fill=\"{col}\"/><circle cx=\"{N(x + 2.5)}\" cy=\"{N(y)}\" r=\"2\" fill=\"{col}\"/>";
                case 10:
                    return $"<ellipse cx=\"{N(x)}\" cy=\"{N(y)}\" rx=\"5.5\" ry=\"2.8\" transform=\"rotate({(x < 60 ? 20 : -20)} {N(x)} {N(y)})\" fill=\"{col}\" {o}/>";
                case 11:
                    return $"<path d=\"M{N(x - 5)} {N(y - 1.5)} L{N(x + 5)} {N(y - 1.5)} M{N(x - 5)} {N(y + 1.5)} L{N(x + 5)} {N(y + 1.5)}\" stroke=\"{col}\" stroke-width=\"1.6\" stroke-linecap=\"round\"/>";
                case 12:
                    return $"<rect x=\"{N(x - 5)}\" y=\"{N(y - 3.5)}\" width=\"10\" height=\"7\" rx=\"2.5\" fill=\"{col}\" {o}/>";
                case 13:
                    return $"<path d=\"M{N(x)} {N(y - 4.5)} L{N(x + 4.5)} {N(y)} L{N(x)} {N(y + 4.5)} L{N(x - 4.5)} {N(y)} Z\" fill=\"{col}\" {o}/>";
                case 14:
                    return string.Concat(Enumerable.Range(0, 5).Select(j =>
                               $"<circle cx=\"{F1(x + 2.6 * Math.Cos(j * 1.2566))}\" cy=\"{F1(y + 2.6 * Math.Sin(j * 1.2566))}\" r=\"1.9\" fill=\"{col}\"/>"))
                           + $"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"1.3\" fill=\"{White}\"/>";
                case 15:
                    return $"<path d=\"M{N(x - 5)} {N(y - 1)} Q{N(x)} {N(y + 6)} {N(x + 5)} {N(y - 1)} Q{N(x)} {N(y + 2.5)} {N(x - 5)} {N(y - 1)} Z\" fill=\"{col}\"/>";
                case 16:
                    return $"<path d=\"M{N(x - 6)} {N(y)} Q{N(x - 3)} {N(y - 3)} {N(x)} {N(y)} Q{N(x + 3)} {N(y + 3)} {N(x + 6)} {N(y)}\" fill=\"none\" stroke=\"{col}\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>";
                case 17:
                    return $"<path d=\"{Star(x, y, 4.5, 1.3, 4)}\" fill=\"{col}\"/>";
                case 18:
                    return $"<circle cx=\"{N(x - 3)}\" cy=\"{N(y - 1)}\" r=\"1.2\" fill=\"{col}\"/><circle cx=\"{N(x + 1)}\" cy=\"{N(y - 2)}\" r=\"1.2\" fill=\"{col}\"/>" +
                           $"<circle cx=\"{N(x - 1)}\" cy=\"{N(y + 2)}\" r=\"1.2\" fill=\"{col}\"/><circle cx=\"{N(x + 3)}\" cy=\"{N(y + 1.5)}\" r=\"1.2\" fill=\"{col}\"/>";
                case 19:
                    return $"<path d=\"M{N(x - 3)} {N(y - 3)} L{N(x + 3)} {N(y + 3)} M{N(x + 3)} {N(y - 3)} L{N(x - 3)} {N(y + 3)}\" stroke=\"{col}\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>";
                default:
                    return "";
            }
        }

        // ---- hats: Thai ones first, then hats from around the world ----

        private static string Hat(int i)
        {
            switch (i)
            {
                case 1: // Ngob (Thai farmer hat)
                    return "<ellipse cx=\"60\" cy=\"22\" rx=\"46\" ry=\"9\" fill=\"#C99A4B\"/><path d=\"M22 22 C26 -2 94 -2 98 22 Z\" fill=\"#E2BC6E\"/>" +
                           "<path d=\"M30 16 L90 16 M36 9 L84 9 M46 3 L74 3\" stroke=\"#B9873A\" stroke-width=\"1.4\"/>" +
                           "<path d=\"M60 -3 L60 22 M42 0 L34 22 M78 0 L86 22\" stroke=\"#B9873A\" stroke-width=\"1.2\"/>" +
                           "<ellipse cx=\"60\" cy=\"22\" rx=\"46\" ry=\"9\" fill=\"none\" stroke=\"#A9772E\" stroke-width=\"1.6\"/>";
                case 2: // Chada (Thai classical dance crown)
                    return "<path d=\"M42 28 L78 28 L74 18 L68 4 L63 -14 L60 -28 L57 -14 L52 4 L46 18 Z\" fill=\"#E3A21A\"/>" +
                           "<path d=\"M42 28 L78 28 L77 23 L43 23 Z\" fill=\"#B47B0C\"/>" +
                           "<path d=\"M47 16 L73 16 M51 6 L69 6 M55 -4 L65 -4\" stroke=\"#B47B0C\" stroke-width=\"1.6\"/>" +
                           "<circle cx=\"60\" cy=\"20\" r=\"2.6\" fill=\"#D63A4A\"/><circle cx=\"60\" cy=\"10\" r=\"2\" fill=\"#2A9D8F\"/><circle cx=\"60\" cy=\"1\" r=\"1.6\" fill=\"#D63A4A\"/>" +
                           "<path d=\"M42 26 C34 28 30 36 32 44\" fill=\"none\" stroke=\"#E3A21A\" stroke-width=\"3\" stroke-linecap=\"round\"/>" +
                           "<path d=\"M78 26 C86 28 90 36 88 44\" fill=\"none\" stroke=\"#E3A21A\" stroke-width=\"3\" stroke-linecap=\"round\"/>";
                case 3: // Pha khao ma (Thai checkered cloth) headband
                    return "<path d=\"M30 30 C42 20 78 20 90 30 L90 38 C78 28 42 28 30 38 Z\" fill=\"#D63A4A\"/>" +
                           string.Concat(new[] { 36, 44, 52, 60, 68, 76, 84 }.Select(x => $"<path d=\"M{x} 24 L{x} 36\" stroke=\"#2F3C9E\" stroke-width=\"2.4\"/>")) +
                           "<path d=\"M31 33 C42 25 78 25 89 33\" fill=\"none\" stroke=\"#F2C14E\" stroke-width=\"1.6\"/>" +
                           "<path d=\"M90 32 L102 26 L100 40 Z\" fill=\"#D63A4A\"/><path d=\"M90 34 L104 40 L96 46 Z\" fill=\"#B82E3C\"/>";
                case 4: // Phuang malai (jasmine & rose garland crown)
                    return string.Concat(Enumerable.Range(0, 11).Select(j =>
                           {
                               var a = Math.PI + j * Math.PI / 10;
                               return $"<circle cx=\"{F1(60 + 30 * Math.Cos(a))}\" cy=\"{F1(30 + 14 * Math.Sin(a))}\" r=\"{(j % 3 == 0 ? "4.4" : "3.6")}\" " +
                                      $"fill=\"{(j % 3 == 0 ? "#E0485F" : White)}\" stroke=\"#E9DFCB\" stroke-width=\"0.8\"/>";
                           }))
                           + "<path d=\"M58 14 C56 8 64 8 62 14\" fill=\"#3FA06A\"/>";
                case 5: // Non la (Vietnam)
                    return "<path d=\"M16 28 L60 -16 L104 28 Q60 36 16 28 Z\" fill=\"#E8C77E\"/>" +
                           "<path d=\"M60 -16 L40 30 M60 -16 L60 32 M60 -16 L80 30\" stroke=\"#C9A257\" stroke-width=\"1.2\"/>" +
                           "<path d=\"M16 28 Q60 36 104 28\" fill=\"none\" stroke=\"#B88E44\" stroke-width=\"2\"/>";
                case 6: // Sombrero
                    return "<ellipse cx=\"60\" cy=\"22\" rx=\"58\" ry=\"11\" fill=\"#D9A441\"/><path d=\"M38 22 C38 -12 82 -12 82 22 Z\" fill=\"#E8BE5C\"/>" +
                           "<path d=\"M38 16 L82 16\" stroke=\"#D63A4A\" stroke-width=\"4\"/>" +
                           "<path d=\"M40 16 L44 12 L48 16 L52 12 L56 16 L60 12 L64 16 L68 12 L72 16 L76 12 L80 16\" fill=\"none\" stroke=\"#2A9D8F\" stroke-width=\"1.6\"/>" +
                           "<ellipse cx=\"60\" cy=\"22\" rx=\"58\" ry=\"11\" fill=\"none\" stroke=\"#B9842A\" stroke-width=\"2\" stroke-dasharray=\"3 3\"/>";
                case 7: // Fez
                    return "<path d=\"M45 26 L48 2 L72 2 L75 26 Z\" fill=\"#C8323C\"/><ellipse cx=\"60\" cy=\"2\" rx=\"12\" ry=\"3\" fill=\"#A82830\"/>" +
                           "<path d=\"M60 2 C68 4 74 10 74 20\" fill=\"none\" stroke=\"#1D2140\" stroke-width=\"1.6\"/><path d=\"M71 18 L74 26 L77 18 Z\" fill=\"#1D2140\"/>";
                case 8: // Beret
                    return "<ellipse cx=\"56\" cy=\"20\" rx=\"32\" ry=\"11\" transform=\"rotate(-10 56 20)\" fill=\"#2B2F45\"/>" +
                           "<ellipse cx=\"58\" cy=\"26\" rx=\"26\" ry=\"4\" fill=\"#1D2140\"/><path d=\"M58 9 L60 3\" stroke=\"#2B2F45\" stroke-width=\"3\" stroke-linecap=\"round\"/>";
                case 9: // Ushanka
                    return "<path d=\"M30 30 C30 4 90 4 90 30 Z\" fill=\"#7A5A44\"/><rect x=\"28\" y=\"22\" width=\"64\" height=\"12\" rx=\"6\" fill=\"#D9C8B4\"/>" +
                           "<path d=\"M28 28 C20 30 18 48 24 58 C30 54 32 40 32 30 Z\" fill=\"#D9C8B4\"/><path d=\"M92 28 C100 30 102 48 96 58 C90 54 88 40 88 30 Z\" fill=\"#D9C8B4\"/>" +
                           "<circle cx=\"60\" cy=\"16\" r=\"4\" fill=\"#E3A21A\"/><path d=\"M60 12 L61 15 L64 16 L61 17 L60 20 L59 17 L56 16 L59 15 Z\" fill=\"#D63A4A\"/>";
                case 10: // Kokoshnik
                    return "<path d=\"M30 30 C28 -4 92 -4 90 30 Z\" fill=\"#C8323C\"/><path d=\"M36 28 C36 4 84 4 84 28\" fill=\"none\" stroke=\"#E3A21A\" stroke-width=\"2.4\"/>" +
                           string.Concat(Enumerable.Range(0, 9).Select(j =>
                           {
                               var a = Math.PI + (j + 0.5) * Math.PI / 9;
                               return $"<circle cx=\"{F1(60 + 27 * Math.Cos(a))}\" cy=\"{F1(28 + 26 * Math.Sin(a))}\" r=\"1.8\" fill=\"{White}\"/>";
                           })) +
                           "<path d=\"M60 8 C54 14 54 20 60 24 C66 20 66 14 60 8 Z\" fill=\"#E3A21A\"/><circle cx=\"60\" cy=\"17\" r=\"2.4\" fill=\"#2A9D8F\"/>";
                case 11: // Tam o' shanter
                    return "<ellipse cx=\"60\" cy=\"18\" rx=\"36\" ry=\"12\" fill=\"#2A6B4F\"/>" +
                           "<path d=\"M26 18 L94 18 M30 12 L90 12 M30 24 L90 24\" stroke=\"#C8323C\" stroke-width=\"2\"/>" +
                           "<path d=\"M44 8 L44 28 M60 6 L60 30 M76 8 L76 28\" stroke=\"#E3C14E\" stroke-width=\"1.4\"/>" +
                           "<rect x=\"34\" y=\"24\" width=\"52\" height=\"6\" rx=\"3\" fill=\"#1D2140\"/><circle cx=\"60\" cy=\"5\" r=\"6\" fill=\"#C8323C\"/>";
                case 12: // Cowboy
                    return "<path d=\"M14 22 C20 32 100 32 106 22 C100 26 20 26 14 22 Z\" fill=\"#8B5A2B\"/>" +
                           "<path d=\"M14 22 C24 30 96 30 106 22 C104 16 98 16 94 22 L26 22 C22 16 16 16 14 22 Z\" fill=\"#A26A34\"/>" +
                           "<path d=\"M38 22 C36 2 46 -2 60 4 C74 -2 84 2 82 22 Z\" fill=\"#B57A3E\"/><path d=\"M38 18 L82 18\" stroke=\"#5A3A1C\" stroke-width=\"4\"/>";
                case 13: // Alpine hat with a feather
                    return "<ellipse cx=\"60\" cy=\"24\" rx=\"38\" ry=\"7\" fill=\"#3D6B45\"/><path d=\"M38 24 C38 2 82 2 82 24 Z\" fill=\"#4D8457\"/>" +
                           "<path d=\"M38 20 L82 20\" stroke=\"#8B5A2B\" stroke-width=\"3\"/>" +
                           "<path d=\"M76 20 C84 6 92 -2 100 -6 C94 4 88 12 78 20 Z\" fill=\"#C8323C\"/><path d=\"M78 18 C86 8 94 0 100 -6\" stroke=\"#7A1E26\" stroke-width=\"1\"/>";
                case 14: // Gat (Korea)
                    return "<ellipse cx=\"60\" cy=\"22\" rx=\"48\" ry=\"8\" fill=\"#1D2140\" fill-opacity=\"0.75\"/>" +
                           "<path d=\"M44 22 L46 -8 L74 -8 L76 22 Z\" fill=\"#1D2140\" fill-opacity=\"0.85\"/><ellipse cx=\"60\" cy=\"-8\" rx=\"14\" ry=\"3\" fill=\"#2B2F45\"/>" +
                           "<path d=\"M42 24 C40 44 44 60 46 72 M78 24 C80 44 76 60 74 72\" fill=\"none\" stroke=\"#1D2140\" stroke-width=\"1.2\" stroke-dasharray=\"2 2\"/>";
                case 15: // Hachimaki (Japan)
                    return "<path d=\"M30 28 C42 20 78 20 90 28 L90 35 C78 27 42 27 30 35 Z\" fill=\"#FFFFFF\" stroke=\"#E9DFCB\" stroke-width=\"1\"/>" +
                           "<circle cx=\"60\" cy=\"27\" r=\"4.2\" fill=\"#D63A4A\"/>" +
                           "<path d=\"M30 31 L18 24 L20 36 Z\" fill=\"#FFFFFF\" stroke=\"#E9DFCB\" stroke-width=\"1\"/>" +
                           "<path d=\"M30 33 L16 38 L24 44 Z\" fill=\"#FFFFFF\" stroke=\"#E9DFCB\" stroke-width=\"1\"/>";
                case 16: // Pagri (India)
                    return "<path d=\"M28 30 C24 2 96 2 92 30 C80 22 40 22 28 30 Z\" fill=\"#F08A24\"/>" +
                           "<path d=\"M32 22 C46 10 74 10 88 22 M30 26 C44 14 76 14 90 26 M40 12 C52 6 68 6 80 12\" fill=\"none\" stroke=\"#C96A10\" stroke-width=\"1.6\"/>" +
                           "<path d=\"M60 8 C56 14 56 18 60 22 C64 18 64 14 60 8 Z\" fill=\"#E3A21A\"/><circle cx=\"60\" cy=\"16\" r=\"2.2\" fill=\"#D63A4A\"/>" +
                           "<path d=\"M60 8 C58 2 62 -6 66 -10\" fill=\"none\" stroke=\"#FFFFFF\" stroke-width=\"2\" stroke-linecap=\"round\"/>";
                case 17: // Chullo (Peru)
                    return "<path d=\"M30 32 C28 2 92 2 90 32 Z\" fill=\"#C8323C\"/><path d=\"M32 22 L88 22\" stroke=\"#F2C14E\" stroke-width=\"4\"/>" +
                           "<path d=\"M34 22 L38 18 L42 22 L46 18 L50 22 L54 18 L58 22 L62 18 L66 22 L70 18 L74 22 L78 18 L82 22 L86 18\" fill=\"none\" stroke=\"#2A9D8F\" stroke-width=\"1.6\"/>" +
                           "<path d=\"M30 30 C26 40 26 50 30 56 L36 52 C34 44 34 38 36 30 Z\" fill=\"#C8323C\"/>" +
                           "<path d=\"M90 30 C94 40 94 50 90 56 L84 52 C86 44 86 38 84 30 Z\" fill=\"#C8323C\"/><circle cx=\"60\" cy=\"2\" r=\"5\" fill=\"#F2C14E\"/>";
                case 18: // Top hat (UK)
                    return "<ellipse cx=\"60\" cy=\"24\" rx=\"30\" ry=\"6\" fill=\"#1D2140\"/><rect x=\"42\" y=\"-14\" width=\"36\" height=\"38\" rx=\"3\" fill=\"#262A44\"/>" +
                           "<rect x=\"42\" y=\"14\" width=\"36\" height=\"6\" fill=\"#C8323C\"/><ellipse cx=\"60\" cy=\"-14\" rx=\"18\" ry=\"3\" fill=\"#2F3452\"/>";
                case 19: // Loovuz (Mongolia)
                    return "<path d=\"M34 26 C34 6 50 -6 60 -16 C70 -6 86 6 86 26 Z\" fill=\"#2F6FB5\"/><path d=\"M60 -16 L60 26\" stroke=\"#E3A21A\" stroke-width=\"1.4\"/>" +
                           "<path d=\"M60 -16 L62 -22\" stroke=\"#C8323C\" stroke-width=\"3\" stroke-linecap=\"round\"/>" +
                           "<rect x=\"28\" y=\"22\" width=\"64\" height=\"11\" rx=\"5.5\" fill=\"#6B4A2B\"/><path d=\"M38 14 Q60 6 82 14\" fill=\"none\" stroke=\"#E3A21A\" stroke-width=\"1.6\"/>";
                default:
                    return "";
            }
        }

        // ---- helpers ----

        /// <summary>The same shapes mirrored around the vertical axis of the head (x = 60).</summary>
        private static string Mirror(string markup) => $"<g transform=\"translate(120 0) scale(-1 1)\">{markup}</g>";

        private static string Star(double cx, double cy, double outer, double inner, int points)
        {
            var d = new StringBuilder();
            for (var j = 0; j < points * 2; j++)
            {
                var a = -Math.PI / 2 + j * Math.PI / points;
                var r = j % 2 == 1 ? inner : outer;
                d.Append(j == 0 ? 'M' : 'L').Append(F1(cx + r * Math.Cos(a))).Append(' ').Append(F1(cy + r * Math.Sin(a)));
            }

            return d.Append('Z').ToString();
        }

        private static string Petals(double cx, double cy, int n, double r, double w, string fill) =>
            string.Concat(Enumerable.Range(0, n).Select(j =>
                $"<ellipse cx=\"{N(cx)}\" cy=\"{N(cy - r)}\" rx=\"{N(w)}\" ry=\"{N(r)}\" transform=\"rotate({N(j * 360.0 / n)} {N(cx)} {N(cy)})\" fill=\"{fill}\"/>"));

        /// <summary>Darker (t &lt; 0) or lighter (t &gt; 0) color.</summary>
        private static string Shade(string hex, double t) => t < 0 ? Mix(hex, "#000000", -t) : Mix(hex, White, t);

        private static string Mix(string a, string b, double t)
        {
            var result = new StringBuilder("#");
            for (var i = 1; i < 7; i += 2)
            {
                var va = Convert.ToInt32(a.Substring(i, 2), 16);
                var vb = Convert.ToInt32(b.Substring(i, 2), 16);
                result.Append(((int)Math.Round(va + (vb - va) * t, MidpointRounding.AwayFromZero)).ToString("X2"));
            }

            return result.ToString();
        }

        private static string N(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        private static string F1(double v) => v.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
