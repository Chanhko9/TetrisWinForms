namespace TetrisWinForms.Game;

public sealed class ActivePiece
{
    public TetrominoType Type { get; }
    public int Rotation { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int ColorValue => TetrominoShapes.ColorValue(Type);

    public ActivePiece(TetrominoType type, int row, int col, int rotation = 0)
    {
        Type = type;
        Row = row;
        Col = col;
        Rotation = rotation;
    }

    public IEnumerable<(int Row, int Col)> Cells()
    {
        foreach (BlockOffset offset in TetrominoShapes.Get(Type, Rotation))
            yield return (Row + offset.Row, Col + offset.Col);
    }

    public ActivePiece Copy()
        => new(Type, Row, Col, Rotation);
}
