using System;

namespace Chang.Profile
{
    /// <summary>Parts of the mascot the player can change, in the order of the editor tabs.</summary>
    public enum MascotPart
    {
        Head,
        Ears,
        Eyes,
        Tusks,
        TuskColor,
        Mark,
        Blush,
        BlushColor,
        Hat
    }

    /// <summary>
    /// The player's mascot: an option index (0…<see cref="OptionCount"/>-1) per part.
    /// The options themselves (shapes, colors, names) are in <c>Chang.Mascot.MascotCatalog</c>.
    /// </summary>
    [Serializable]
    public class MascotLook
    {
        public const int OptionCount = 20;

        public static readonly MascotPart[] Parts = (MascotPart[])Enum.GetValues(typeof(MascotPart));

        public int Head;
        public int Ears;
        public int Eyes;
        public int Tusks = 1;
        public int TuskColor;
        public int Mark = 1;
        public int Blush = 1;
        public int BlushColor;
        public int Hat;

        public int this[MascotPart part]
        {
            get => part switch
            {
                MascotPart.Head => Head,
                MascotPart.Ears => Ears,
                MascotPart.Eyes => Eyes,
                MascotPart.Tusks => Tusks,
                MascotPart.TuskColor => TuskColor,
                MascotPart.Mark => Mark,
                MascotPart.Blush => Blush,
                MascotPart.BlushColor => BlushColor,
                MascotPart.Hat => Hat,
                _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
            };
            set
            {
                value = Math.Clamp(value, 0, OptionCount - 1);
                switch (part)
                {
                    case MascotPart.Head: Head = value; break;
                    case MascotPart.Ears: Ears = value; break;
                    case MascotPart.Eyes: Eyes = value; break;
                    case MascotPart.Tusks: Tusks = value; break;
                    case MascotPart.TuskColor: TuskColor = value; break;
                    case MascotPart.Mark: Mark = value; break;
                    case MascotPart.Blush: Blush = value; break;
                    case MascotPart.BlushColor: BlushColor = value; break;
                    case MascotPart.Hat: Hat = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(part), part, null);
                }
            }
        }

        public MascotLook Clone() => (MascotLook)MemberwiseClone();

        /// <summary>A full random combination (the Random button of the editor).</summary>
        public static MascotLook Random(Random random)
        {
            var look = new MascotLook();
            foreach (var part in Parts)
            {
                look[part] = random.Next(OptionCount);
            }

            return look;
        }
    }
}
