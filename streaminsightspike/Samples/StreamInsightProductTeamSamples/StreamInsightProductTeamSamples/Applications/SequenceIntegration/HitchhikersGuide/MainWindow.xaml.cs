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

        public MainWindow()
        {
            InitializeComponent();

            _server = Server.Create("Default");
            Closing += (s, e) =>
            {
                _results.Clear();
                _server.Dispose();
            };
            _application = _server.CreateApplication("Hitchhiker's Guide");
            _queriesListView.DataContext = DemoQuery.FindQueries(typeof(Queries));
        }

        private void QuerySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DemoQuery query = _queriesListView.SelectedItem as DemoQuery;
            if (null != query)
            {
                _results.SetSource(query.Run(_application));
            }
        }
    }
}
