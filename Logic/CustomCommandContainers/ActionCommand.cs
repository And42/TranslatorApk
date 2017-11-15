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

        private static bool AlwaysTrueAction(object param)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ActionCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;
        private readonly Type _targetType = typeof(T);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly TypeConverter Converter = new TypeConverter();

        public ActionCommand(Action<T> executeAction, Func<T, bool> canExecuteFunc = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc ?? AlwaysTrueAction;
        }

        public void Execute(object parameter)
        {
            T param = (T)Converter.ConvertTo(parameter, _targetType);

            _executeAction(param);
        }

        public bool CanExecute(object parameter)
        {
            T param = (T)Converter.ConvertTo(parameter, _targetType);

            return _canExecuteFunc(param);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, null);
        }

        private static bool AlwaysTrueAction(T param)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
