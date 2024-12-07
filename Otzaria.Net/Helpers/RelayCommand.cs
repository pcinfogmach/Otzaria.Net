using System;
using System.Windows.Input;

namespace Otzaria.Net.Helpers
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action commandTask;
        public RelayCommand(Action action) { commandTask = action; }
        public virtual bool CanExecute(object parameter) { return true; }
        protected void OnCanExecuteChanged() { CanExecuteChanged?.Invoke(this, new EventArgs()); }
        public void Execute(object parameter) { commandTask(); }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

}
