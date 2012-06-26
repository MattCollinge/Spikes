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

namespace StreamInsight.Samples.SequenceIntegration.PerformanceCounters
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// An observable source based on a local machine performance counter.
    /// </summary>
    public sealed class PerformanceCounterObservable : IObservable<PerformanceCounterSample>
    {
        readonly Func<PerformanceCounter> _createCounter;
        readonly TimeSpan _pollingInterval;

        public PerformanceCounterObservable(string categoryName, string counterName, string instanceName, TimeSpan pollingInterval)
        {
            // create a new performance counter for every subscription
            _createCounter = () => new PerformanceCounter(categoryName, counterName, instanceName, true);
            _pollingInterval = pollingInterval;
        }

        public IDisposable Subscribe(IObserver<PerformanceCounterSample> observer)
        {
            return new Subscription(this, observer);
        }

        sealed class Subscription : IDisposable
        {
            readonly PerformanceCounter _counter;
            readonly TimeSpan _pollingInterval;
            readonly IObserver<PerformanceCounterSample> _observer;
            readonly Timer _timer;
            readonly object _sync = new object();
            CounterSample _previousSample;
            bool _isDisposed;

            public Subscription(PerformanceCounterObservable observable, IObserver<PerformanceCounterSample> observer)
            {
                // create a new counter for this subscription
                _counter = observable._createCounter();
                _pollingInterval = observable._pollingInterval;
                _observer = observer;
                
                // seed previous sample to support computation
                _previousSample = _counter.NextSample();

                // create a timer to support polling counter at an interval
                _timer = new Timer(Sample);
                _timer.Change(_pollingInterval.Milliseconds, -1);
            }

            void Sample(object state)
            {
                lock (_sync)
                {
                    if (!_isDisposed)
                    {
                        DateTime startTime = DateTime.UtcNow;
                        CounterSample currentSample = _counter.NextSample();
                        float value = CounterSample.Calculate(_previousSample, currentSample);
                        _observer.OnNext(new PerformanceCounterSample { StartTime = startTime, Value = value });
                        _previousSample = currentSample;
                        _timer.Change(_pollingInterval.Milliseconds, -1);
                    }
                }
            }

            public void Dispose()
            {
                lock (_sync)
                {
                    if (!_isDisposed)
                    {
                        _isDisposed = true;
                        _timer.Dispose();
                        _counter.Dispose();
                    }
                }
            }
        }
    }
}
