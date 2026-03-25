using System.Windows;
using System.Windows.Input;

namespace Hybrid.Native.Windows;

public static class MouseEventArgsExtensions
{
    public static WebMouseData ToWebMouseData(this MouseButtonEventArgs self, Window window)
    {
        ushort buttons = self.ChangedButton switch
        {
            MouseButton.Left => 1,
            MouseButton.Right => 2,
            MouseButton.Middle => 4,
            _ => 0,
        };

        var position = self.GetPosition(window);

        return new WebMouseData(buttons, (float)position.X, (float)position.Y, null, null);
    }

    public static WebMouseData ToWebMouseData(this MouseEventArgs self, Window window)
    {
        var position = self.GetPosition(window);
        return new WebMouseData(0, (float)position.X, (float)position.Y, null, null);
    }

    public static WebMouseData ToWebMouseData(this MouseWheelEventArgs self, Window window)
    {
        var position = self.GetPosition(window);
        return new WebMouseData(0, (float)position.X, (float)position.Y, null, self.Delta);
    }
}