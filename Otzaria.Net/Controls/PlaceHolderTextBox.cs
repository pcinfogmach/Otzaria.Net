using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Otzaria.Net.Controls
{
    public class PlaceHolderTextBox : Border
    {
        private readonly TextBox _textBox = new TextBox() { BorderThickness = new Thickness(0) };
        private readonly TextBlock _placeHolder = new TextBlock() { Opacity = 0.5, IsHitTestVisible = false, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3,0,3,0)};

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PlaceHolderTextBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PlaceHolderTextProperty =
            DependencyProperty.Register(nameof(PlaceHolderText), typeof(string), typeof(PlaceHolderTextBox), new PropertyMetadata(string.Empty));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public string PlaceHolderText { get => (string)GetValue(PlaceHolderTextProperty); set => SetValue(PlaceHolderTextProperty, value); }

        public PlaceHolderTextBox()
        {
            _textBox.TextChanged += (s, e) => _placeHolder.Visibility = string.IsNullOrEmpty(_textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            _textBox.SetBinding(TextBox.TextProperty, new Binding(nameof(Text)) { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            _placeHolder.SetBinding(TextBlock.TextProperty, new Binding(nameof(PlaceHolderText)) { Source = this });

            Grid grid = new Grid();
            grid.Children.Add(_textBox);
            grid.Children.Add(_placeHolder);
            this.Child = grid;

            _textBox.PreviewKeyDown += _textBox_PreviewKeyDown;
            _textBox.GotKeyboardFocus += _textBox_GotKeyboardFocus;
        }

        private void _textBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { _textBox.Text = string.Empty; e.Handled = true; }
        }

        private void _textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _textBox.Dispatcher.BeginInvoke(
                new Action(delegate
                {
                    _textBox.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }
}
