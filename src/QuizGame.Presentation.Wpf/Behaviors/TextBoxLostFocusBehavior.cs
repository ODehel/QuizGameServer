using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuizGame.Presentation.Wpf.Behaviors;

public static class TextBoxLostFocusBehavior
{
    public static ICommand GetLostFocusCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(LostFocusCommandProperty);
    }

    public static void SetLostFocusCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(LostFocusCommandProperty, value);
    }

    public static readonly DependencyProperty LostFocusCommandProperty =
        DependencyProperty.RegisterAttached(
            "LostFocusCommand",
            typeof(ICommand),
            typeof(TextBoxLostFocusBehavior),
            new PropertyMetadata(null, OnLostFocusCommandChanged));

    private static void OnLostFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textBox = d as TextBox;
        if (textBox == null)
            return;

        if (e.NewValue is ICommand command)
        {
            textBox.PreviewLostKeyboardFocus += (s, args) =>
            {
                // Passer le DataContext du TextBox comme paramètre
                var parameter = textBox.DataContext;
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            };
        }
    }
}
