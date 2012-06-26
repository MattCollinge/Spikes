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

namespace StreamInsight.Samples.SequenceIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary>
    /// Helper methods supporting 
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns an IObservable based on a Subscribe implementation.
        /// </summary>
        public static IObservable<T> CreateObservable<T>(Func<IObserver<T>, IDisposable> subscribe)
        {
            return new AnonymousObservable<T>(subscribe);
        }

        /// <summary>
        /// Returns an IDisposable based on a Dispose implementation.
        /// </summary>
        public static IDisposable CreateDisposable(Action dispose)
        {
            return new AnonymousDisposable(dispose);
        }

        /// <summary>
        /// Returns an IObserver based on OnNext, OnError and OnCompleted implementations.
        /// </summary>
        public static IObserver<T> CreateObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return new AnonymousObserver<T>(onNext, onError, onCompleted);
        }

        /// <summary>
        /// Gets the instance of custom attribute on member. Throws if more than one instance
        /// of the attribute exists.
        /// </summary>
        public static TAttribute SingleOrDefault<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            if (null == member)
            {
                throw new ArgumentNullException("member");
            }

            return member.GetCustomAttributes(typeof(TAttribute), true)
                .Cast<TAttribute>()
                .SingleOrDefault();
        }

        sealed class AnonymousObserver<T> : IObserver<T>
        {
            readonly Action<T> _onNext;
            readonly Action<Exception> _onError;
            readonly Action _onCompleted;

            public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
            {
                _onNext = onNext;
                _onError = onError;
                _onCompleted = onCompleted;
            }

            public void OnCompleted()
            {
                if (_onCompleted != null)
                {
                    _onCompleted();
                }
            }

            public void OnError(Exception error)
            {
                if (_onError != null)
                {
                    _onError(error);
                }
            }

            public void OnNext(T value)
            {
                if (_onNext != null)
                {
                    _onNext(value);
                }
            }
        }

        sealed class AnonymousObservable<T> : IObservable<T>
        {
            readonly Func<IObserver<T>, IDisposable> _subscribe;

            public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
            {
                _subscribe = subscribe;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                if (null == observer)
                {
                    throw new ArgumentNullException("observer");
                }
                return null != _subscribe 
                    ? _subscribe(observer) 
                    : new AnonymousDisposable(null);
            }
        }

        sealed class AnonymousDisposable : IDisposable
        {
            readonly Action _dispose;

            public AnonymousDisposable(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (null != _dispose)
                {
                    _dispose();
                }
            }
        }
    }
}
