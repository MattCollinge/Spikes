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
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary>
    /// Simple control allowing visualization of IObservable content.
    /// </summary>
    /// <remarks>
    /// The SetSource overloads support configuring IObservable event sources for the grid.
    /// Specializations for StreamInsight event types implement the following functionality:
    /// flattening control fields (StartTime, EventKind, etc.) and payload fields, and;
    /// avoiding accesses of properties when they are unavailable. For instance, the Payload
    /// property of PointEvent cannot be accessed for CTI events.
    /// </remarks>
    public sealed partial class ObservableDataGridControl : UserControl, IObservableControl
    {
        IObservableCollectionSubject _collection;

        public ObservableDataGridControl()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            // clear any existing data context settings
            if (null != _collection)
            {
                _collection.Stop();
                _collection = null;
            }

            // reset UI
            _errorTextBox.Text = null;
            _grid.DataContext = null;
            _grid.Columns.Clear();
            _playButton.IsEnabled = false;
            _stopButton.IsEnabled = false;
        }

        public void SetSource<T>(IObservable<T> source)
        {
            InternalSetSource(source);

            AddFields<T>(false);
        }

        public void SetSource<T>(ICepObservable<PointEvent<T>> source)
        {
            InternalSetSource(source.Select(p => p.EventKind == EventKind.Insert
                ? new { p.EventKind, p.StartTime, p.Payload }
                : new { p.EventKind, p.StartTime, Payload = default(T) }));
            
            AddEventFields<PointEvent<T>, T>();
        }

        public void SetSource<T>(ICepObservable<IntervalEvent<T>> source)
        {
            InternalSetSource(source.Select(i => i.EventKind == EventKind.Insert
                ? new { i.EventKind, i.StartTime, EndTime = (DateTimeOffset?)i.EndTime, i.Payload }
                : new { i.EventKind, i.StartTime, EndTime = (DateTimeOffset?)null, Payload = default(T) }));

            AddEventFields<IntervalEvent<T>, T>();
        }

        public void SetSource<T>(ICepObservable<EdgeEvent<T>> source)
        {
            InternalSetSource(source.Select(e => e.EventKind == EventKind.Insert
                ? e.EdgeType == EdgeType.End
                ? new 
                { 
                    EventKind = e.EventKind,
                    EdgeType = (EdgeType?)e.EdgeType, 
                    StartTime = e.StartTime, 
                    EndTime = (DateTimeOffset?)e.EndTime, 
                    Payload = e.Payload,
                }
                : new
                {
                    EventKind = e.EventKind,
                    EdgeType = (EdgeType?)e.EdgeType, 
                    StartTime = e.StartTime, 
                    EndTime = (DateTimeOffset?)null, 
                    Payload = e.Payload,
                }
                : new
                {
                    EventKind = e.EventKind,
                    EdgeType = (EdgeType?)null,
                    StartTime = e.StartTime,
                    EndTime = (DateTimeOffset?)null,
                    Payload = default(T),
                }
                ));

            AddEventFields<EdgeEvent<T>, T>();
        }

        void InternalSetSource<T>(IObservable<T> source)
        {
            Clear();

            // attempt to create an observer collection
            if (null != source)
            {
                _collection = new ObservableCollectionSubject<T>(source, this);
                _grid.DataContext = _collection;
                _playButton.IsEnabled = true;
            }
        }

        void AddEventFields<TEvent, TPayload>()
            where TEvent : TypedEvent<TPayload>
        {
            // add system event fields (StartTime, EventKind, etc.)
            AddFields<TEvent>(false);

            // remove Event.Payload field to avoid redundant data
            _grid.Columns.Remove(_grid.Columns.Single(c => "Payload".Equals(c.Header)));

            // add payload fields as top-level fields
            AddFields<TPayload>(true);
        }

        void AddFields<T>(bool arePayloadFields)
        {
            foreach (var property in from p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     where p.CanRead && p.GetIndexParameters().Length == 0
                                     select p)
            {
                AddField<T>(arePayloadFields, property.Name, property.PropertyType);
            }

            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                AddField<T>(arePayloadFields, field.Name, field.FieldType);
            }
        }

        void AddField<T>(bool isPayloadField, string name, Type type)
        {
            string path = isPayloadField
                ? "Payload." + name
                : name;

            DataGridColumn column = new DataGridTextColumn
            {
                Header = name,
                Binding = new Binding(path) { Mode = BindingMode.OneWay },
            };

            _grid.Columns.Add(column);
        }

        void PlayClick(object sender, RoutedEventArgs e)
        {
            if (_collection != null)
            {
                _collection.Play();
                _playButton.IsEnabled = false;
                _stopButton.IsEnabled = true;
            }
        }

        void StopClick(object sender, RoutedEventArgs e)
        {
            _collection.Stop();
            Stop();
        }

        void Invoke(Action action)
        {
            Dispatcher.Invoke(action);
        }

        void IObservableControl.OnNext(object value)
        {
            if (!(_lockScrollBarCheckBox.IsChecked ?? false))
            {
                _grid.ScrollIntoView(_collection[_collection.Count - 1]);
            }
        }

        void IObservableControl.OnCompleted()
        {
            Stop();
        }

        void IObservableControl.OnError(Exception error)
        {
            _errorTextBox.Text = error.ToString();
            Stop();
        }

        void Stop()
        {
            _playButton.IsEnabled = _collection != null;
            _stopButton.IsEnabled = false;
        }
    }
}
