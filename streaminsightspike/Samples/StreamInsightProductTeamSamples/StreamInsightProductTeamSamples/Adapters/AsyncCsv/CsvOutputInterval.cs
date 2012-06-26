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
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;
    using System.Threading;

    public class CsvOutputInterval : IntervalOutputAdapter, IOutputAdapter<IntervalEvent>
    {
        private CsvOutputAdapter<CsvOutputInterval, IntervalEvent> outputAdapter;

        public CsvOutputInterval(CsvOutputConfig configInfo, CepEventType eventType)
        {
            this.outputAdapter = new CsvOutputAdapter<CsvOutputInterval, IntervalEvent>(configInfo, eventType);
        }

        public override void Start()
        {
            new Thread(() => this.outputAdapter.ConsumeEvents(this)).Start();
        }

        public override void Resume()
        {
            new Thread(() => this.outputAdapter.ConsumeEvents(this)).Start();
        }
    }
}

