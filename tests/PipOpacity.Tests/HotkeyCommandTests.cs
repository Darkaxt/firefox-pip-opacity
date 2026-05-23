namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class HotkeyCommandTests
{
    [Theory]
    [InlineData(HotkeyIds.IncreaseOpacity, HotkeyCommand.IncreaseOpacity)]
    [InlineData(HotkeyIds.IncreaseOpacityNumpad, HotkeyCommand.IncreaseOpacity)]
    [InlineData(HotkeyIds.DecreaseOpacity, HotkeyCommand.DecreaseOpacity)]
    [InlineData(HotkeyIds.DecreaseOpacityNumpad, HotkeyCommand.DecreaseOpacity)]
    [InlineData(HotkeyIds.Reset, HotkeyCommand.Reset)]
    [InlineData(HotkeyIds.ToggleClickThrough, HotkeyCommand.ToggleClickThrough)]
    public void KnownHotkeyIdsMapToCommands(int id, HotkeyCommand expected)
    {
        Assert.Equal(expected, HotkeyCommandMapper.FromId(id));
    }

    [Fact]
    public void UnknownHotkeyIdReturnsNone()
    {
        Assert.Equal(HotkeyCommand.None, HotkeyCommandMapper.FromId(999));
    }
}
