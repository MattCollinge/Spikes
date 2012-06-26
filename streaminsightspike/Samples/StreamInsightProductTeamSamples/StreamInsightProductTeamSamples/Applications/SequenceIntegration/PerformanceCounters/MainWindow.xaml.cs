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
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Server _server;
        readonly Microsoft.ComplexEventProcessing.Application _application;
        ThresholdConfiguration _configuration;

        public MainWindow()
        {
            InitializeComponent();

            _configuration = GetConfiguration();

            _server = Server.Create("Default");
            Closing += (s, e) =>
            {
                _processor.Clear();
                _threshold.Clear();
                _server.Dispose();
            };
            _application = _server.CreateApplication("Performance Counters");

            var configurationSource = CreateConfigurationObservable(); 

            _processor.SetSource(Queries.ProcessorUtilizationPerCore(_application));
            _threshold.SetSource(Queries.ProcessorUtilizationSustainedThreshold(_application, configurationSource));
        }

        /// <summary>
        /// Translates WPF control events into an observable source.
        /// </summary>
        /// <returns></returns>
        IObservable<ThresholdConfiguration> CreateConfigurationObservable()
        {
            return Utility.CreateObservable<ThresholdConfiguration>(observer =>
            {
                // immediately output last read configuration to seed the configuration
                // stream
                Dispatcher.Invoke((Func<ThresholdConfiguration>)GetConfiguration);
                observer.OnNext(_configuration);

                // whenever the user modifies the threshold or duration, output
                // a new event
                RoutedPropertyChangedEventHandler<double> thresholdAction = (sender, args) =>
                    observer.OnNext(GetConfiguration());
                TextChangedEventHandler durationAction = (sender, args) =>
                    observer.OnNext(GetConfiguration());
                _thresholdSlider.ValueChanged += thresholdAction;
                _durationTextBox.TextChanged += durationAction;
                
                // when the subscription is disposed, remove event delegates as well
                return Utility.CreateDisposable(() =>
                {
                    _thresholdSlider.ValueChanged -= thresholdAction;
                    _durationTextBox.TextChanged -= durationAction;
                });
            });
        }

        ThresholdConfiguration GetConfiguration()
        {
            double seconds;
            if (!double.TryParse(_durationTextBox.Text, out seconds) ||
                seconds <= 1.0 / TimeSpan.TicksPerSecond)
            {
                seconds = _configuration.Duration.TotalSeconds;
            }
            _configuration = new ThresholdConfiguration
            {
                Threshold = (float)_thresholdSlider.Value,
                Duration = TimeSpan.FromSeconds(seconds),
            };
            return _configuration;
        }
    }
}
