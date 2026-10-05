namespace TetrisWinForms.Game;

public sealed class PieceBag
{
    private readonly Random _random;
    private readonly Queue<TetrominoType> _queue = new();

    private static readonly TetrominoType[] ClassicPieces =
    [
        TetrominoType.I,
        TetrominoType.O,
        TetrominoType.T,
        TetrominoType.S,
        TetrominoType.Z,
        TetrominoType.J,
        TetrominoType.L
    ];

    private static readonly TetrominoType[] SpecialShapes =
    [
        TetrominoType.Plus,
        TetrominoType.U,
        TetrominoType.V,
        TetrominoType.P,
        TetrominoType.I5
    ];

    public PieceBag(Random random)
    {
        _random = random;
    }

    public TetrominoType Next()
    {
        if (_queue.Count == 0)
            Refill();

        return _queue.Dequeue();
    }

    private void Refill()
    {
        // Classic shapes stay dominant. Each classic piece appears twice while
        // each custom piece appears once: roughly 26% custom pieces overall.
        var bag = new List<TetrominoType>();
        foreach (TetrominoType type in ClassicPieces)
        {
            bag.Add(type);
            bag.Add(type);
        }

        bag.AddRange(SpecialShapes);

        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        foreach (TetrominoType type in bag)
            _queue.Enqueue(type);
    }
}
