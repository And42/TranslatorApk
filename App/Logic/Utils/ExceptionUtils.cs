using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslatorApk.Logic.Utils
{
    public static class ExceptionUtils
    {
        public static string FlattenToString(this Exception exception)
        {
            var parts = new List<string>();

            while (exception != null)
            {
                parts.Add(exception.Message);
                exception = exception.InnerException;
            }

            return parts.Select(it => $"\"{it}\"").JoinStr(" <- ");
        }
    }
}
