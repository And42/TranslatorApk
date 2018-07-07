using System;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// Gets the corresponding <see cref="PropertyInfo" /> from an <see cref="Expression" />.
        /// </summary>
        /// <param name="property">The expression that selects the property to get info on.</param>
        /// <returns>The property info collected from the expression.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The expression doesn't indicate a valid property."</exception>
        public static PropertyInfo GetPropertyInfo<T, P>(Expression<Func<T, P>> property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (property.Body is UnaryExpression unaryExp)
            {
                if (unaryExp.Operand is MemberExpression memberExp)
                {
                    return (PropertyInfo)memberExp.Member;
                }
            }
            else if (property.Body is MemberExpression memberExp)
            {
                return (PropertyInfo)memberExp.Member;
            }

            throw new ArgumentException($"The expression doesn't indicate a valid property. [ {property} ]");
        }
    }
}
