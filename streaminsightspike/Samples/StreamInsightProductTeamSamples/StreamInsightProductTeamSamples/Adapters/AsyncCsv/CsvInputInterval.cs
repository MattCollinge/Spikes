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

namespace StreamInsight.Samples.Adapters.AsyncCsv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    public class CsvInputInterval : IntervalInputAdapter, IInputAdapter<IntervalEvent>
    {
        private CsvInputAdapter<CsvInputInterval, IntervalEvent> inputAdapter;

        public CsvInputInterval(CsvInputConfig configInfo, CepEventType eventType)
        {
            this.inputAdapter = new CsvInputAdapter<CsvInputInterval, IntervalEvent>(configInfo, eventType, this);
        }

        public override void Start()
        {
            this.inputAdapter.Start();
        }

        public override void Resume()
        {
            this.inputAdapter.Resume();
        }
    }
}