using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Text.Font
{
    public struct GameFontSpriteCharacter
    {
        public int SpriteNumber;
        public char AsciiCharacter;
        public int Width;

        public GameFontSpriteCharacter(int spriteNumber, char character, int width)
        {
            // Initialize fields.
            this.SpriteNumber = spriteNumber;
            this.AsciiCharacter = character;
            this.Width = width;
        }
    }

    public abstract class GameFontSpriteSheet
    {
        /// <summary>
        /// Name of the font.
        /// </summary>
        public string FontName { get; protected set; }

        /// <summary>
        /// Dictionary of character entries key'd by the ascii character.
        /// </summary>
        public Dictionary<char, GameFontSpriteCharacter> CharacterTable { get; protected set; }
    }
}
