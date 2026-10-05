namespace TetrisWinForms.Effects;

public sealed class ScreenShakeService
{
    private CancellationTokenSource? _cts;
    private readonly Random _random = new();

    public async Task ShakeAsync(
        Control target,
        int intensity = 6,
        int durationMs = 220,
        ShakeAxis axis = ShakeAxis.Both)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        Point origin = target.Location;

        try
        {
            int frameMs = 18;
            int frames = Math.Max(1, durationMs / frameMs);

            for (int i = 0; i < frames; i++)
            {
                token.ThrowIfCancellationRequested();

                int dx = axis == ShakeAxis.Vertical ? 0 : _random.Next(-intensity, intensity + 1);
                int dy = axis == ShakeAxis.Horizontal ? 0 : _random.Next(-intensity, intensity + 1);
                target.Location = new Point(origin.X + dx, origin.Y + dy);

                await Task.Delay(frameMs, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            target.Location = origin;
        }
    }
}
