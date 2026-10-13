using System;
using SAM.Picker;

internal static class UnlockAllProgressTests
{
    private static void Main()
    {
        var progress = new UnlockAllProgress(2, 3, false);
        progress.StartGame(1, 1);
        Assert(progress.GetStatus("Game").EndsWith("ETA Calculating..."), "initial estimate");
        Assert(progress.GetStatus("Game").Contains("Processing 1/2"), "one-based index");
        progress.CompleteGame(TimeSpan.FromSeconds(10));
        progress.StartGame(1, 2);
        Assert(IsNear(progress.GetRemainingDuration(), TimeSpan.FromSeconds(50)), "first-loop ETA");
        progress.CompleteGame(TimeSpan.FromSeconds(10));
        progress.StartGame(2, 1);
        Assert(IsNear(progress.GetRemainingDuration(), TimeSpan.FromSeconds(40)), "multi-loop continuity");
        Assert(UnlockAllProgress.FormatDuration(TimeSpan.FromHours(125) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3)) == "125h:02m:03s", "long duration formatting");

        var infinite = new UnlockAllProgress(1, -1, true);
        infinite.StartGame(7, 1);
        Assert(infinite.GetStatus("Game").EndsWith("ETA N/A (Infinite)"), "infinite status");
        Assert(new UnlockAllProgress(0, 1, false).GetRemainingDuration() == null, "zero games");

        var reset = new UnlockAllProgress(1, 1, false);
        reset.StartGame(1, 1);
        Assert(reset.GetStatus("Game").EndsWith("ETA Calculating..."), "new run resets estimate");
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition)
        {
            throw new InvalidOperationException(name);
        }
    }

    private static bool IsNear(TimeSpan? actual, TimeSpan expected)
    {
        return actual.HasValue && Math.Abs((actual.Value - expected).TotalSeconds) < 1;
    }
}
