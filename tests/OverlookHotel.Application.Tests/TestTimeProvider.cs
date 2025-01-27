namespace OverlookHotel.Application.Tests;

using System;

public class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;
}