using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TranslatorApk.Logic.CustomCommandContainers
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecuteFunc;

        public ActionCommand(Action<object> executeAction, Func<object, bool> canExecuteFunc = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc ?? AlwaysTrueAction;
        }

        public ActionCommand(Action executeAction, Func<bool> canExecuteFunc = null)
        {
            _executeAction = executeAction != null ? new Action<object>(obj => executeAction()) : throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc != null ? new Func<object, bool>(obj => canExecuteFunc()) : AlwaysTrueAction;
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, null);
        }

        private static bool AlwaysTrueAction(object param) => true;

        public event EventHandler CanExecuteChanged;
    }

    public class ActionCommand<T> : ICommand
    {
        private static readonly Type TargetType = typeof(T);
        private static readonly bool IsClass = default(T) == null;

        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;
        
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TypeConverter Converter = new TypeConverter();

        public ActionCommand(Action<T> executeAction, Func<T, bool> canExecuteFunc = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc ?? AlwaysTrueAction;
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _executeAction(Convert(parameter));
        }

        public bool CanExecute(object parameter) => _canExecuteFunc(Convert(parameter));

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, null);
        }

        private static T Convert(object obj)
        {
            if (obj == null && IsClass)
                return default;

            return obj is T
                ? (T)obj
                : (T)Converter.ConvertTo(obj, TargetType);
        }

        private static bool AlwaysTrueAction(T param)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
