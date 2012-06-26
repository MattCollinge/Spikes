// *********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// *********************************************************

namespace StreamInsight.Samples.SequenceIntegration.HitchhikersGuide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Used to define a sample query. The attributed method should take a single Application
    /// argument and return an IObservable query sink.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DemoQueryAttribute : Attribute
    {
        readonly int _ordinal;
        readonly string _name;
        readonly string _description;

        public DemoQueryAttribute(int ordinal, string name, string description)
        {
            _ordinal = ordinal;
            _name = name;
            _description = description;
        }

        public int Ordinal { get { return _ordinal; } }
        public string Name { get { return _name; } }
        public string Description { get { return _description; } }
    }

    public sealed class DemoQuery
    {
        readonly DemoQueryAttribute _attribute;
        readonly Func<Application, dynamic> _run;

        private DemoQuery(MethodInfo method, DemoQueryAttribute attribute)
        {
            _run = a => method.Invoke(null, new[] { a });
            _attribute = attribute;
        }

        public int Ordinal { get { return _attribute.Ordinal; } }
        public string Name { get { return _attribute.Name; } }
        public string Description { get { return _attribute.Description; } }

        public dynamic Run(Application application)
        {
            return _run(application);
        }

        /// <summary>
        /// Finds all query implementations on the given type.
        /// </summary>
        public static IList<DemoQuery> FindQueries(Type queriesType)
        {
            if (null == queriesType)
            {
                throw new ArgumentNullException("queriesType");
            }

            var queries = from m in queriesType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                          let a = m.SingleOrDefault<DemoQueryAttribute>()
                          where a != null && m.ReturnType == typeof(object)
                          let p = m.GetParameters()
                          where p.Length == 1 && p[0].ParameterType == typeof(Application)
                          select new DemoQuery(m, a) into q
                          orderby q.Ordinal
                          select q;

            return queries.ToList().AsReadOnly();
        }
    }

}
