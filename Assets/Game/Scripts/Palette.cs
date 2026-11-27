using UnityEngine;

namespace JetpackJoe
{
    // the Catppuccin Mocha palette, one source for every colour in the game
    public static class Palette
    {
        public static readonly Color Base = Hex("#1e1e2e");
        public static readonly Color Mantle = Hex("#181825");
        public static readonly Color Crust = Hex("#11111b");
        public static readonly Color Surface0 = Hex("#313244");
        public static readonly Color Surface1 = Hex("#45475a");
        public static readonly Color Surface2 = Hex("#585b70");
        public static readonly Color Overlay0 = Hex("#6c7086");
        public static readonly Color Overlay1 = Hex("#7f849c");
        public static readonly Color Overlay2 = Hex("#9399b2");
        public static readonly Color Text = Hex("#cdd6f4");
        public static readonly Color Subtext1 = Hex("#bac2de");
        public static readonly Color Subtext0 = Hex("#a6adc8");
        public static readonly Color Rosewater = Hex("#f5e0dc");
        public static readonly Color Flamingo = Hex("#f2cdcd");
        public static readonly Color Pink = Hex("#f5c2e7");
        public static readonly Color Mauve = Hex("#cba6f7");
        public static readonly Color Red = Hex("#f38ba8");
        public static readonly Color Maroon = Hex("#eba0ac");
        public static readonly Color Peach = Hex("#fab387");
        public static readonly Color Yellow = Hex("#f9e2af");
        public static readonly Color Green = Hex("#a6e3a1");
        public static readonly Color Teal = Hex("#94e2d5");
        public static readonly Color Sky = Hex("#89dceb");
        public static readonly Color Sapphire = Hex("#74c7ec");
        public static readonly Color Blue = Hex("#89b4fa");
        public static readonly Color Lavender = Hex("#b4befe");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        public static Color A(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
