namespace TetrisWinForms.Game;

public sealed class BoardModel
{
    public const int Rows = 20;
    public const int Columns = 10;
    public const int GarbageColorValue = 13;
    public int[,] Cells { get; } = new int[Rows, Columns];

    public bool IsInside(int row, int col)
        => row >= 0 && row < Rows && col >= 0 && col < Columns;

    public bool IsEmpty(int row, int col)
        => IsInside(row, col) && Cells[row, col] == 0;

    public void Clear()
        => Array.Clear(Cells, 0, Cells.Length);

    public bool CanPlace(ActivePiece piece)
    {
        foreach ((int row, int col) in piece.Cells())
        {
            if (!IsInside(row, col) || Cells[row, col] != 0)
                return false;
        }

        return true;
    }

    public void LockPiece(ActivePiece piece)
    {
        foreach ((int row, int col) in piece.Cells())
        {
            if (IsInside(row, col))
                Cells[row, col] = piece.ColorValue;
        }
    }

    public bool ClearCell(int row, int col)
    {
        if (!IsInside(row, col) || Cells[row, col] == 0) return false;
        Cells[row, col] = 0;
        return true;
    }

    public int ClearCompletedLines(GravityDirection gravity)
        => gravity is GravityDirection.Down or GravityDirection.Up
            ? ClearFullRows(gravity)
            : ClearFullColumns(gravity);

    private int ClearFullRows(GravityDirection gravity)
    {
        int cleared = 0;

        if (gravity == GravityDirection.Down)
        {
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (!IsRowFull(row)) continue;
                RemoveRowDown(row);
                cleared++;
                row++;
            }
        }
        else
        {
            for (int row = 0; row < Rows; row++)
            {
                if (!IsRowFull(row)) continue;
                RemoveRowUp(row);
                cleared++;
                row--;
            }
        }

        return cleared;
    }

    private int ClearFullColumns(GravityDirection gravity)
    {
        int cleared = 0;

        if (gravity == GravityDirection.Right)
        {
            for (int col = Columns - 1; col >= 0; col--)
            {
                if (!IsColumnFull(col)) continue;
                RemoveColumnRight(col);
                cleared++;
                col++;
            }
        }
        else
        {
            for (int col = 0; col < Columns; col++)
            {
                if (!IsColumnFull(col)) continue;
                RemoveColumnLeft(col);
                cleared++;
                col--;
            }
        }

        return cleared;
    }

    private bool IsRowFull(int row)
    {
        for (int col = 0; col < Columns; col++)
            if (Cells[row, col] == 0) return false;
        return true;
    }

    private bool IsColumnFull(int col)
    {
        for (int row = 0; row < Rows; row++)
            if (Cells[row, col] == 0) return false;
        return true;
    }

    private void RemoveRowDown(int row)
    {
        for (int r = row; r > 0; r--)
        for (int c = 0; c < Columns; c++)
            Cells[r, c] = Cells[r - 1, c];

        for (int c = 0; c < Columns; c++)
            Cells[0, c] = 0;
    }

    private void RemoveRowUp(int row)
    {
        for (int r = row; r < Rows - 1; r++)
        for (int c = 0; c < Columns; c++)
            Cells[r, c] = Cells[r + 1, c];

        for (int c = 0; c < Columns; c++)
            Cells[Rows - 1, c] = 0;
    }

    private void RemoveColumnRight(int col)
    {
        for (int c = col; c > 0; c--)
        for (int r = 0; r < Rows; r++)
            Cells[r, c] = Cells[r, c - 1];

        for (int r = 0; r < Rows; r++)
            Cells[r, 0] = 0;
    }

    private void RemoveColumnLeft(int col)
    {
        for (int c = col; c < Columns - 1; c++)
        for (int r = 0; r < Rows; r++)
            Cells[r, c] = Cells[r, c + 1];

        for (int r = 0; r < Rows; r++)
            Cells[r, Columns - 1] = 0;
    }

    public void Compact(GravityDirection gravity)
    {
        switch (gravity)
        {
            case GravityDirection.Down:
                CompactDown();
                break;
            case GravityDirection.Up:
                CompactUp();
                break;
            case GravityDirection.Right:
                CompactRight();
                break;
            case GravityDirection.Left:
                CompactLeft();
                break;
        }
    }

    private void CompactDown()
    {
        for (int col = 0; col < Columns; col++)
        {
            int write = Rows - 1;
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (Cells[row, col] == 0) continue;
                int value = Cells[row, col];
                Cells[row, col] = 0;
                Cells[write--, col] = value;
            }
        }
    }

    private void CompactUp()
    {
        for (int col = 0; col < Columns; col++)
        {
            int write = 0;
            for (int row = 0; row < Rows; row++)
            {
                if (Cells[row, col] == 0) continue;
                int value = Cells[row, col];
                Cells[row, col] = 0;
                Cells[write++, col] = value;
            }
        }
    }

    private void CompactRight()
    {
        for (int row = 0; row < Rows; row++)
        {
            int write = Columns - 1;
            for (int col = Columns - 1; col >= 0; col--)
            {
                if (Cells[row, col] == 0) continue;
                int value = Cells[row, col];
                Cells[row, col] = 0;
                Cells[row, write--] = value;
            }
        }
    }

    private void CompactLeft()
    {
        for (int row = 0; row < Rows; row++)
        {
            int write = 0;
            for (int col = 0; col < Columns; col++)
            {
                if (Cells[row, col] == 0) continue;
                int value = Cells[row, col];
                Cells[row, col] = 0;
                Cells[row, write++] = value;
            }
        }
    }

    public bool RiseGarbageRow(Random random)
    {
        bool overflow = false;
        for (int col = 0; col < Columns; col++)
            overflow |= Cells[0, col] != 0;

        for (int row = 0; row < Rows - 1; row++)
        for (int col = 0; col < Columns; col++)
            Cells[row, col] = Cells[row + 1, col];

        int hole = random.Next(Columns);
        for (int col = 0; col < Columns; col++)
            Cells[Rows - 1, col] = col == hole ? 0 : GarbageColorValue;

        return overflow;
    }

    public int GetMostCommonColor()
    {
        return Cells.Cast<int>()
            .Where(x => x > 0)
            .GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }
}
