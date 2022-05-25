using System;
using System.Windows.Input;

namespace Thomsen.WpfTools.Mvvm {
    public class CommandHandler : ICommand {
        #region Private Fields
        private readonly Action<object?> _action;
        private readonly Func<bool> _canExecute;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates instance of the command handler
        /// </summary>
        /// <param name="action">Action to be executed by the command</param>
        /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
        public CommandHandler(Action<object?> action, Func<bool> canExecute) {
            _action = action;
            _canExecute = canExecute;
        }
        #endregion Constructors

        #region Events
        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion Events

        #region Public Methods
        /// <summary>
        /// Forces checking if execute is allowed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object? parameter) {
            return _canExecute.Invoke();
        }

        /// <summary>
        /// Wires Action on command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object? parameter) {
            _action(parameter);
        }
        #endregion Public Methods
    }
}
