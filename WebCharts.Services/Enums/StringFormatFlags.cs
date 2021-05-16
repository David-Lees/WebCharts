using System;

namespace WebCharts.Services.Enums
{
    [Flags]
    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1, // Text is displayed from right to left.
        DirectionVertical = 2, // Text is vertically aligned.
        DisplayFormatControl = 32, // Control characters such as the left-to-right mark are shown in the output with a representative glyph.
        FitBlackBox = 4,//Parts of characters are allowed to overhang the string's layout rectangle. By default, characters are repositioned to avoid any overhang.
        LineLimit = 8192, // Only entire lines are laid out in the formatting rectangle. By default layout continues until the end of the text, or until no more lines are visible as a result of clipping, whichever comes first. Note that the default settings allow the last line to be partially obscured by a formatting rectangle that is not a whole multiple of the line height. To ensure that only whole lines are seen, specify this value and be careful to provide a formatting rectangle at least as tall as the height of one line.
        MeasureTrailingSpaces = 2048, // Includes the trailing space at the end of each line. By default the boundary rectangle returned by the MeasureString method excludes the space at the end of each line. Set this flag to include that space in measurement.
        NoClip = 16384,// Overhanging parts of glyphs, and unwrapped text reaching outside the formatting rectangle are allowed to show. By default all text and glyph parts reaching outside the formatting rectangle are clipped.
        NoFontFallback = 1024, // Fallback to alternate fonts for characters not supported in the requested font is disabled. Any missing characters are displayed with the fonts missing glyph, usually an open square.
        NoWrap = 4096, // Text wrapping between lines when formatting within a rectangle is disabled. This flag is implied when a point is passed instead of a rectangle, or when the specified rectangle has a zero line length.
    }
}
