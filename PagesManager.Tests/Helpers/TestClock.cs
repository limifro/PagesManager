using PagesManager.Core.Helpers;

namespace PagesManager.Tests.Helpers;

public class TestClock : IClock
{
    public DateTime UtcNow { get; set; }

    public TestClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }
}