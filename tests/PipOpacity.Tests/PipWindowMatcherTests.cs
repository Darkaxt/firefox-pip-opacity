namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class PipWindowMatcherTests
{
    [Fact]
    public void FirefoxPictureInPictureWindowMatches()
    {
        var window = new WindowSnapshot(
            Handle: 0x1234,
            ProcessPath: @"C:\Program Files\Mozilla Firefox\firefox.exe",
            Title: "Picture-in-Picture",
            ClassName: "MozillaDialogClass",
            IsVisible: true,
            Width: 672,
            Height: 378,
            Owner: 0);

        Assert.True(PipWindowMatcher.IsFirefoxPictureInPicture(window));
    }

    [Theory]
    [MemberData(nameof(NonPipWindows))]
    public void NonPictureInPictureWindowsDoNotMatch(WindowSnapshot window)
    {
        Assert.False(PipWindowMatcher.IsFirefoxPictureInPicture(window));
    }

    [Fact]
    public void PictureInPictureShapeMatchesBeforeProcessPathIsKnown()
    {
        var window = NewWindow(processPath: string.Empty);

        Assert.True(PipWindowMatcher.HasPictureInPictureShape(window));
    }

    [Theory]
    [MemberData(nameof(NonPipShapes))]
    public void NonPictureInPictureShapesDoNotNeedProcessPathInspection(WindowSnapshot window)
    {
        Assert.False(PipWindowMatcher.HasPictureInPictureShape(window with { ProcessPath = string.Empty }));
    }

    public static TheoryData<WindowSnapshot> NonPipWindows()
    {
        return new TheoryData<WindowSnapshot>
        {
            NewWindow(title: "Queue - Mozilla Firefox", className: "MozillaWindowClass"),
            NewWindow(processPath: @"C:\Program Files\Other\firefox-helper.exe"),
            NewWindow(title: "picture-in-picture"),
            NewWindow(className: "MozillaWindowClass"),
            NewWindow(isVisible: false),
            NewWindow(width: 0),
            NewWindow(height: 0),
            NewWindow(owner: 0x9999),
        };
    }

    public static TheoryData<WindowSnapshot> NonPipShapes()
    {
        return new TheoryData<WindowSnapshot>
        {
            NewWindow(title: "Queue - Mozilla Firefox", className: "MozillaWindowClass"),
            NewWindow(title: "picture-in-picture"),
            NewWindow(className: "MozillaWindowClass"),
            NewWindow(isVisible: false),
            NewWindow(width: 0),
            NewWindow(height: 0),
            NewWindow(owner: 0x9999),
        };
    }

    private static WindowSnapshot NewWindow(
        string processPath = @"C:\Program Files\Mozilla Firefox\firefox.exe",
        string title = "Picture-in-Picture",
        string className = "MozillaDialogClass",
        bool isVisible = true,
        int width = 672,
        int height = 378,
        nint owner = 0)
    {
        return new WindowSnapshot(
            Handle: 0x1234,
            ProcessPath: processPath,
            Title: title,
            ClassName: className,
            IsVisible: isVisible,
            Width: width,
            Height: height,
            Owner: owner);
    }
}
