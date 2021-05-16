namespace WebCharts.Services.Enums
{
    public enum WrapMode
    {
        Clamp = 4, // The texture or gradient is not tiled.
        Tile = 0, // Tiles the gradient or texture.
        TileFlipX = 1, // Reverses the texture or gradient horizontally and then tiles the texture or gradient.
        TileFlipXY = 3, // Reverses the texture or gradient horizontally and vertically and then tiles the texture or gradient.
        TileFlipY = 2, // Reverses the texture or gradient vertically and then tiles the texture or gradient.
    }
}
