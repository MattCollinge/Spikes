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
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Control interface for ObservableCollectionSubject.
    /// </summary>
    interface IObservableCollectionSubject : IList
    {
        void Play();
        void Stop();
    }

    /// <summary>
    /// Interface that must be implemented by control leveraging ObservableCollectionSubject.
    /// </summary>
    public interface IObservableControl
    {
        Dispatcher Dispatcher { get; }
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(object value);
    }

    /// <summary>
    /// An ObservableCollection implementation backed by IObservable source.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public sealed class ObservableCollectionSubject<T> : ObservableCollection<T>, IObservableCollectionSubject, IObserver<T>
    {
        readonly IObservableControl _control;
        readonly IObservable<T> _observable;
        readonly object _sync = new object();
        bool _playing;
        IDisposable _subscription;

        /// <summary>
        /// Initializes subject using the given observable source and parent control.
        /// </summary>
        /// <param name="observable">Event source.</param>
        /// <param name="control">Observable control.</param>
        public ObservableCollectionSubject(IObservable<T> observable, IObservableControl control)
        {
            if (null == observable)
            {
                throw new ArgumentNullException("observable");
            }
            if (null == control)
            {
                throw new ArgumentNullException("control");
            }

            _control = control;
            _observable = observable;
        }

        void IObserver<T>.OnCompleted()
        {
            _control.Dispatcher.Invoke((Action)_control.OnCompleted);
            Stop();
        }

        void IObserver<T>.OnError(Exception error)
        {
            _control.Dispatcher.Invoke((Action<Exception>)_control.OnError, error);
            Stop();
        }

        void IObserver<T>.OnNext(T value)
        {
            _control.Dispatcher.Invoke((Action<T>)Add, value);
            _control.Dispatcher.Invoke((Action<object>)_control.OnNext, value);
        }

        /// <summary>
        /// If not currently playing, subscribes to the observable event source.
        /// </summary>
        public void Play()
        {
            lock (_sync)
            {
                if (!_playing)
                {
                    Clear();
                    _playing = true;

                    // Subscribe on a new thread to avoid blocking.
                    new Thread(AsyncPlay).Start();
                }
            }
        }

        void AsyncPlay()
        {
            IDisposable subscription = _observable.Subscribe(this);
            lock (_sync)
            {
                // If we have stopped already, dispose the subscription immediately.
                // Otherwise, remember it so that we can dispose of it later.
                if (!_playing)
                {
                    subscription.Dispose();
                }
                else
                {
                    _subscription = subscription;
                }
            }
        }
        
        /// <summary>
        /// If currently playing, disposes subscription to the observable event source.
        /// </summary>
        public void Stop()
        {
            lock (_sync)
            {
                if (_playing)
                {
                    _playing = false;
                }
                if (null != _subscription)
                {
                    _subscription.Dispose();
                    _subscription = null;
                }
            }
        }
    }
}
