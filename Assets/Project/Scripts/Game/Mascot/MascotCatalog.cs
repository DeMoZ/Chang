using Chang.Profile;

namespace Chang.Mascot
{
    /// <summary>
    /// Options of the mascot constructor (9 parts × 20 options), the same as the Penpot page "Redesign · Mascot".
    /// Shape parts have names, color parts have hex colors. The art is drawn by <see cref="MascotSvg"/>.
    /// </summary>
    public static class MascotCatalog
    {
        private static readonly string[] HeadColors =
        {
            "#8E9BC9", "#A7A9AC", "#9CB4D8", "#7FB7B0", "#B9A3D6", "#E8A8B8", "#F2C18D", "#C7B299", "#8FBF8A", "#6E7A8A",
            "#D9D2C5", "#5D6B9E", "#F0D36B", "#E89B6C", "#9AD0E6", "#C98DB5", "#7C9C6B", "#B5B9C9", "#4E5A7A", "#E6E1F5"
        };

        private static readonly string[] TuskColors =
        {
            "#FFFFFF", "#FFF8E7", "#F5E6C8", "#EADBC0", "#F2C14E", "#D9A21B", "#C0C6D4", "#9EA7B8", "#F7C6D9", "#BDE8D3",
            "#BFD7F5", "#E3D1F7", "#FFE08A", "#F4A261", "#E76F51", "#2A9D8F", "#264653", "#1D2140", "#B08968", "#7F5539"
        };

        private static readonly string[] BlushColors =
        {
            "#F2B8C9", "#F7A1B5", "#FF8FA3", "#F4A6A6", "#FFB085", "#FFC6A5", "#F9D29D", "#E8A0BF", "#D291BC", "#C3A6E8",
            "#A0C4FF", "#9BF6FF", "#B9FBC0", "#FDFFB6", "#FFADAD", "#E07A5F", "#FF6B6B", "#C77DFF", "#F72585", "#FFFFFF"
        };

        private static readonly string[] EarNames =
        {
            "Classic", "Round", "Small", "Big", "Floppy", "Fan", "Leaf", "Heart", "Scallop", "Soft square",
            "Wide", "Tall", "Tilted up", "Tilted down", "Cloud", "Bean", "Half-moon", "Lotus petal", "Butterfly", "Folded"
        };

        private static readonly string[] EyeNames =
        {
            "Classic", "Big round", "Dots", "Happy", "Sleepy", "Wink", "Lashes", "Stars", "Hearts", "Sparkly",
            "Sunglasses", "Glasses", "Determined", "Surprised", "Side look", "Squint", "Teardrop", "Smiling", "Dizzy", "Brows"
        };

        private static readonly string[] TuskNames =
        {
            "None", "Classic", "Short", "Long", "Very long", "Curl", "Straight", "Thick", "Thin long", "Flared",
            "Hook", "Double", "Cone", "Stub", "Mammoth", "Nubs", "Broken one", "Gold cap", "Upward", "S-curve"
        };

        private static readonly string[] MarkNames =
        {
            "None", "Gold diamond", "Dot", "Lotus", "Flower", "Star", "Heart", "Moon", "Sun", "Three dots",
            "Gem drop", "Kranok flame", "Leaf", "Sparkle", "Royal regalia", "Jasmine band", "Thai flag", "Jewel band", "Chevrons", "Mandala"
        };

        private static readonly string[] BlushNames =
        {
            "None", "Oval", "Circle", "Big round", "Hearts", "Lines", "Swirl", "Stars", "Triangles", "Two dots",
            "Tilted", "Double line", "Soft square", "Diamond", "Flower", "Crescent", "Wave", "Sparkle", "Freckles", "Cross"
        };

        private static readonly string[] HatNames =
        {
            "None", "Ngob · Thailand", "Chada · Thailand", "Pha khao ma · Thailand", "Phuang malai · Thailand", "Nón lá · Vietnam",
            "Sombrero · Mexico", "Fez · Morocco", "Beret · France", "Ushanka · Russia", "Kokoshnik · Russia", "Tam · Scotland",
            "Cowboy · USA", "Alpine · Austria", "Gat · Korea", "Hachimaki · Japan", "Pagri · India", "Chullo · Peru", "Top hat · UK",
            "Loovuz · Mongolia"
        };

        public static string Label(MascotPart part) => part switch
        {
            MascotPart.Head => "Head colour",
            MascotPart.Ears => "Ears",
            MascotPart.Eyes => "Eyes",
            MascotPart.Tusks => "Tusks",
            MascotPart.TuskColor => "Tusk colour",
            MascotPart.Mark => "Forehead",
            MascotPart.Blush => "Blush",
            MascotPart.BlushColor => "Blush colour",
            _ => "Hat"
        };

        public static string ThaiLabel(MascotPart part) => part switch
        {
            MascotPart.Head => "สีหัว",
            MascotPart.Ears => "หู",
            MascotPart.Eyes => "ตา",
            MascotPart.Tusks => "งา",
            MascotPart.TuskColor => "สีงา",
            MascotPart.Mark => "เครื่องประดับ",
            MascotPart.Blush => "แก้ม",
            MascotPart.BlushColor => "สีแก้ม",
            _ => "หมวก"
        };

        public static bool IsColor(MascotPart part) => part is MascotPart.Head or MascotPart.TuskColor or MascotPart.BlushColor;

        /// <summary>Hex color of a color option.</summary>
        public static string Color(MascotPart part, int option) => part switch
        {
            MascotPart.Head => HeadColors[Clamp(option)],
            MascotPart.TuskColor => TuskColors[Clamp(option)],
            MascotPart.BlushColor => BlushColors[Clamp(option)],
            _ => null
        };

        /// <summary>Name of a shape option, or the hex value of a color option.</summary>
        public static string OptionName(MascotPart part, int option) => part switch
        {
            MascotPart.Ears => EarNames[Clamp(option)],
            MascotPart.Eyes => EyeNames[Clamp(option)],
            MascotPart.Tusks => TuskNames[Clamp(option)],
            MascotPart.Mark => MarkNames[Clamp(option)],
            MascotPart.Blush => BlushNames[Clamp(option)],
            MascotPart.Hat => HatNames[Clamp(option)],
            _ => Color(part, option)
        };

        /// <summary>
        /// The look drawn on an option tile: the current look with this option, without a hat (except on the hat tab),
        /// and with the colored part made visible (blush colors need a blush to be seen…), as in the design.
        /// </summary>
        public static MascotLook TileLook(MascotLook current, MascotPart part, int option)
        {
            var look = current.Clone();
            switch (part)
            {
                case MascotPart.TuskColor:
                    look.Tusks = 3;
                    break;
                case MascotPart.BlushColor:
                    look.Blush = 3;
                    break;
                case MascotPart.Blush:
                    look.BlushColor = 2;
                    break;
                case MascotPart.Tusks:
                    look.TuskColor = 0;
                    look.Head = 11;
                    break;
            }

            if (part != MascotPart.Hat)
            {
                look.Hat = 0;
            }

            look[part] = option;
            return look;
        }

        private static int Clamp(int option) => option < 0 ? 0 : option >= MascotLook.OptionCount ? MascotLook.OptionCount - 1 : option;
    }
}
