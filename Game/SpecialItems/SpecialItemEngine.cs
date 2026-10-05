namespace TetrisWinForms.Game.SpecialItems;

public sealed record SpecialItemResult(int CellsCleared, SpecialItemDefinition Definition, int CenterRow, int CenterCol);

public static class SpecialItemEngine
{
    public static SpecialItemResult Apply(
        BoardModel board,
        SpecialItemType type,
        int row,
        int col)
    {
        int cleared = type switch
        {
            SpecialItemType.RuneBomb => ClearRectangle(board, row - 1, col - 1, 3, 3),
            SpecialItemType.DragonBreath => ClearRow(board, row),
            SpecialItemType.ThunderSpear => ClearColumn(board, col),
            SpecialItemType.VoidCross => ClearCross(board, row, col, 2),
            SpecialItemType.PrismRelic => ClearColor(board, board.IsInside(row, col) ? board.Cells[row, col] : 0),
            SpecialItemType.MeteorRelic => ClearRectangle(board, row - 1, col - 1, 4, 4),
            SpecialItemType.BlackHole => ClearDiamond(board, row, col, 3),
            SpecialItemType.PhoenixSigil => ClearDiagonals(board, row, col, 4),
            SpecialItemType.ChaosDice => ClearRandomOccupied(board, 18),
            _ => 0
        };

        return new SpecialItemResult(cleared, SpecialItemDefinition.All[type], row, col);
    }

    private static int ClearRow(BoardModel board, int row)
    {
        int count = 0;
        for (int col = 0; col < BoardModel.Columns; col++)
            if (board.ClearCell(row, col)) count++;
        return count;
    }

    private static int ClearColumn(BoardModel board, int col)
    {
        int count = 0;
        for (int row = 0; row < BoardModel.Rows; row++)
            if (board.ClearCell(row, col)) count++;
        return count;
    }

    private static int ClearCross(BoardModel board, int row, int col, int radius)
    {
        int count = 0;
        for (int i = -radius; i <= radius; i++)
        {
            if (board.ClearCell(row + i, col)) count++;
            if (i != 0 && board.ClearCell(row, col + i)) count++;
        }
        return count;
    }

    private static int ClearRectangle(BoardModel board, int startRow, int startCol, int height, int width)
    {
        int count = 0;
        for (int r = startRow; r < startRow + height; r++)
        for (int c = startCol; c < startCol + width; c++)
            if (board.ClearCell(r, c)) count++;
        return count;
    }

    private static int ClearDiamond(BoardModel board, int row, int col, int radius)
    {
        int count = 0;
        for (int dr = -radius; dr <= radius; dr++)
        for (int dc = -radius; dc <= radius; dc++)
        {
            if (Math.Abs(dr) + Math.Abs(dc) > radius) continue;
            if (board.ClearCell(row + dr, col + dc)) count++;
        }
        return count;
    }

    private static int ClearDiagonals(BoardModel board, int row, int col, int radius)
    {
        int count = 0;
        for (int i = -radius; i <= radius; i++)
        {
            if (board.ClearCell(row + i, col + i)) count++;
            if (i != 0 && board.ClearCell(row + i, col - i)) count++;
        }
        return count;
    }

    private static int ClearRandomOccupied(BoardModel board, int maxCells)
    {
        List<(int Row, int Col)> occupied = new();
        for (int r = 0; r < BoardModel.Rows; r++)
        for (int c = 0; c < BoardModel.Columns; c++)
            if (board.Cells[r, c] > 0) occupied.Add((r, c));

        int count = 0;
        foreach ((int r, int c) in occupied.OrderBy(_ => Random.Shared.Next()).Take(maxCells))
            if (board.ClearCell(r, c)) count++;
        return count;
    }

    private static int ClearColor(BoardModel board, int color)
    {
        if (color <= 0) color = board.GetMostCommonColor();
        if (color <= 0) return 0;

        int count = 0;
        for (int r = 0; r < BoardModel.Rows; r++)
        for (int c = 0; c < BoardModel.Columns; c++)
            if (board.Cells[r, c] == color && board.ClearCell(r, c)) count++;

        return count;
    }
}
