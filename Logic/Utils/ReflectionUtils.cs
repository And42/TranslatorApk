using System;

namespace TranslatorApk.Logic.Utils
{
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Создаёт делегат указанного метода
        /// </summary>
        /// <typeparam name="T">Тип делегата</typeparam>
        /// <param name="obj">Объект, содержащий метод</param>
        /// <param name="method">Метод</param>
        public static T CreateDelegate<T>(object obj, string method) where T : class 
        {
            return Delegate.CreateDelegate(typeof(T), obj, method) as T;
        }

        /// <summary>
        /// Выполняет метод в объекте, используя рефлексию
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="type">Тип, в котором выполняется поиск метода</param>
        /// <param name="obj">Объект, у которого вызывается метод</param>
        /// <param name="name">Название метода</param>
        /// <param name="parameters">Параметры метода</param>
        public static T ExecRefl<T>(Type type, object obj, string name, params object[] parameters)
        {
            // ReSharper disable once PossibleNullReferenceException
            return type.GetMethod(name).Invoke(obj, parameters).As<T>();
        }
    }
}
