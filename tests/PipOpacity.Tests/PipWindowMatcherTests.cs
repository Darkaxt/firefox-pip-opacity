namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class PipWindowMatcherTests
{
    [Theory]
    [InlineData("chrome.exe")]
    [InlineData("helium.exe")]
    [InlineData("msedge.exe")]
    [InlineData("brave.exe")]
    [InlineData("opera.exe")]
    [InlineData("vivaldi.exe")]
    [InlineData("chromium.exe")]
    [InlineData("CHROME.EXE")]
    public void ChromiumVideoPipMatchesSupportedBrowser(string executable)
    {
        var window = NewWindow(processPath: @"C:\Browsers\" + executable,
            title: "Picture in picture", className: "Chrome_WidgetWin_1");

        Assert.True(PipWindowMatcher.IsBrowserPictureInPicture(window));
        Assert.True(PipWindowMatcher.HasPictureInPictureShape(window with { ProcessPath = string.Empty }));
    }

    [Theory]
    [InlineData("chrome.exe", "YouTube - Google Chrome", "Chrome_WidgetWin_1")]
    [InlineData("chrome.exe", "Picture in picture - Google Chrome", "Chrome_WidgetWin_1")]
    [InlineData("chrome.exe", "Picture in picture", "Chrome_WidgetWin_0")]
    [InlineData("msedgewebview2.exe", "Picture in picture", "Chrome_WidgetWin_1")]
    [InlineData("ChatGPT.exe", "Picture in picture", "Chrome_WidgetWin_1")]
    [InlineData("unknown.exe", "Picture in picture", "Chrome_WidgetWin_1")]
    [InlineData("firefox.exe", "Picture in picture", "Chrome_WidgetWin_1")]
    [InlineData("chrome.exe", "Picture-in-Picture", "MozillaDialogClass")]
    public void NormalWindowsAndMismatchedBrowserFamiliesAreRejected(string executable, string title, string className)
    {
        Assert.False(PipWindowMatcher.IsBrowserPictureInPicture(
            NewWindow(processPath: @"C:\Browsers\" + executable, title: title, className: className)));
    }

    [Theory]
    [InlineData(false, 300, 200, 0)]
    [InlineData(true, 0, 200, 0)]
    [InlineData(true, 300, 0, 0)]
    [InlineData(true, 300, 200, 123)]
    public void ChromiumPipRequiresVisiblePositiveSizeUnownedWindow(bool visible, int width, int height, int owner)
    {
        Assert.False(PipWindowMatcher.IsBrowserPictureInPicture(NewWindow(
            processPath: @"C:\Browsers\chrome.exe", title: "Picture in picture",
            className: "Chrome_WidgetWin_1", isVisible: visible, width: width, height: height, owner: owner)));
    }

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
        Assert.True(PipWindowMatcher.IsBrowserPictureInPicture(window));
    }

    [Theory]
    [MemberData(nameof(NonPipWindows))]
    public void NonPictureInPictureWindowsDoNotMatch(WindowSnapshot window)
    {
        Assert.False(PipWindowMatcher.IsFirefoxPictureInPicture(window));
        Assert.False(PipWindowMatcher.IsBrowserPictureInPicture(window));
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
