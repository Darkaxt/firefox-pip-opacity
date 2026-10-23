namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class HotkeyCommandTests
{
    [Theory]
    [InlineData(HotkeyIds.IncreaseOpacityNumpad, HotkeyCommand.IncreaseOpacity)]
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

    [Fact]
    public void RemovedTopmostHotkeyIdReturnsNone()
    {
        Assert.Equal(HotkeyCommand.None, HotkeyCommandMapper.FromId(107));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(102)]
    public void RemovedMainKeyboardOpacityHotkeyIdsReturnNone(int id)
    {
        Assert.Equal(HotkeyCommand.None, HotkeyCommandMapper.FromId(id));
    }
}
