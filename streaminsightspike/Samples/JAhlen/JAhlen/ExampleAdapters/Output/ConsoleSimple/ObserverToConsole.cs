// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.ComplexEventProcessing.Adapters.Observable;

namespace AdvantIQ.ExampleAdapters.Output.ConsoleSimple
{
    public class ObserverToConsole<T> : IObserver<T>
    {
        private readonly FieldInfo[] fieldInfos = typeof(T).GetFields();
        private readonly PropertyInfo[] propertyInfos = typeof(T).GetProperties();

        public ObserverToConsole()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException("ObservableToConsole.OnError()", error);
        }

        public void OnNext(T value)
        {
            var sep = "";
            foreach (var propertyInfo in propertyInfos)
            {
                Console.Write(sep);
                var v = propertyInfo.GetValue(value, null);
                if (v != null)
                    Console.Write(value.ToString());
                sep = ", ";
            }
            foreach (var fieldInfo in fieldInfos)
            {
                Console.Write(sep);
                Console.Write(fieldInfo.GetValue(value).ToString());
                sep = ", ";
            }
            Console.WriteLine();
        }
    }
}
