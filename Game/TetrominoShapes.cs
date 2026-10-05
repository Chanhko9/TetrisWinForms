namespace TetrisWinForms.Game;

public static class TetrominoShapes
{
    private static readonly IReadOnlyDictionary<TetrominoType, BlockOffset[][]> Shapes =
        new Dictionary<TetrominoType, BlockOffset[][]>
        {
            [TetrominoType.I] =
            [
                [new(1, 0), new(1, 1), new(1, 2), new(1, 3)],
                [new(0, 2), new(1, 2), new(2, 2), new(3, 2)],
                [new(2, 0), new(2, 1), new(2, 2), new(2, 3)],
                [new(0, 1), new(1, 1), new(2, 1), new(3, 1)]
            ],
            [TetrominoType.O] =
            [
                [new(0, 1), new(0, 2), new(1, 1), new(1, 2)],
                [new(0, 1), new(0, 2), new(1, 1), new(1, 2)],
                [new(0, 1), new(0, 2), new(1, 1), new(1, 2)],
                [new(0, 1), new(0, 2), new(1, 1), new(1, 2)]
            ],
            [TetrominoType.T] =
            [
                [new(0, 1), new(1, 0), new(1, 1), new(1, 2)],
                [new(0, 1), new(1, 1), new(1, 2), new(2, 1)],
                [new(1, 0), new(1, 1), new(1, 2), new(2, 1)],
                [new(0, 1), new(1, 0), new(1, 1), new(2, 1)]
            ],
            [TetrominoType.S] =
            [
                [new(0, 1), new(0, 2), new(1, 0), new(1, 1)],
                [new(0, 1), new(1, 1), new(1, 2), new(2, 2)],
                [new(1, 1), new(1, 2), new(2, 0), new(2, 1)],
                [new(0, 0), new(1, 0), new(1, 1), new(2, 1)]
            ],
            [TetrominoType.Z] =
            [
                [new(0, 0), new(0, 1), new(1, 1), new(1, 2)],
                [new(0, 2), new(1, 1), new(1, 2), new(2, 1)],
                [new(1, 0), new(1, 1), new(2, 1), new(2, 2)],
                [new(0, 1), new(1, 0), new(1, 1), new(2, 0)]
            ],
            [TetrominoType.J] =
            [
                [new(0, 0), new(1, 0), new(1, 1), new(1, 2)],
                [new(0, 1), new(0, 2), new(1, 1), new(2, 1)],
                [new(1, 0), new(1, 1), new(1, 2), new(2, 2)],
                [new(0, 1), new(1, 1), new(2, 0), new(2, 1)]
            ],
            [TetrominoType.L] =
            [
                [new(0, 2), new(1, 0), new(1, 1), new(1, 2)],
                [new(0, 1), new(1, 1), new(2, 1), new(2, 2)],
                [new(1, 0), new(1, 1), new(1, 2), new(2, 0)],
                [new(0, 0), new(0, 1), new(1, 1), new(2, 1)]
            ],
            [TetrominoType.Plus] =
            [
                [new(0, 1), new(1, 0), new(1, 1), new(1, 2), new(2, 1)],
                [new(0, 1), new(1, 0), new(1, 1), new(1, 2), new(2, 1)],
                [new(0, 1), new(1, 0), new(1, 1), new(1, 2), new(2, 1)],
                [new(0, 1), new(1, 0), new(1, 1), new(1, 2), new(2, 1)]
            ],
            [TetrominoType.U] =
            [
                [new(0, 0), new(0, 2), new(1, 0), new(1, 1), new(1, 2)],
                [new(0, 0), new(0, 1), new(1, 0), new(2, 0), new(2, 1)],
                [new(0, 0), new(0, 1), new(0, 2), new(1, 0), new(1, 2)],
                [new(0, 0), new(0, 1), new(1, 1), new(2, 0), new(2, 1)]
            ],
            [TetrominoType.V] =
            [
                [new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2)],
                [new(0, 0), new(0, 1), new(0, 2), new(1, 0), new(2, 0)],
                [new(0, 0), new(0, 1), new(0, 2), new(1, 2), new(2, 2)],
                [new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2)]
            ],
            [TetrominoType.P] =
            [
                [new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 0)],
                [new(0, 0), new(0, 1), new(0, 2), new(1, 1), new(1, 2)],
                [new(0, 1), new(1, 0), new(1, 1), new(2, 0), new(2, 1)],
                [new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(1, 2)]
            ],
            [TetrominoType.I5] =
            [
                [new(1, 0), new(1, 1), new(1, 2), new(1, 3), new(1, 4)],
                [new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2)],
                [new(2, 0), new(2, 1), new(2, 2), new(2, 3), new(2, 4)],
                [new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2)]
            ]
        };

    public static IReadOnlyList<BlockOffset> Get(TetrominoType type, int rotation)
        => Shapes[type][((rotation % 4) + 4) % 4];

    public static int ColorValue(TetrominoType type) => (int)type + 1;
}
