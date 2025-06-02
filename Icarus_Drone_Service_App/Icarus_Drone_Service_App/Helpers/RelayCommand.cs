using System;
using System.Windows.Input;

namespace Icarus_Drone_Service_App.Helpers
{
    /// <summary>
    /// A lightweight implementation of <see cref="ICommand"/> for MVVM binding.
    /// Encapsulates an action (<c>_execute</c>) and an optional predicate (<c>_canExecute</c>).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new <see cref="RelayCommand"/> that always can execute.
        /// </summary>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="RelayCommand"/> with execution and can‐execute logic.
        /// </summary>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        /// <param name="canExecute">The predicate to determine if the command can execute.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        /// <inheritdoc/>
        public void Execute(object parameter) =>
            _execute(parameter);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to re‐query <see cref="CanExecute"/>.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
