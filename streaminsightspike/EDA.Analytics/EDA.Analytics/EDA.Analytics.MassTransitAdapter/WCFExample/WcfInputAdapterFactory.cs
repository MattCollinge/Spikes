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

namespace StreamInsight.Samples.Adapters.Wcf
{
    using System;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    public sealed class WcfInputAdapterFactory : IInputAdapterFactory<Uri>
    {
        public InputAdapterBase Create(Uri configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            InputAdapterBase inputAdapter;

            switch (eventShape)
            {
                case EventShape.Point:
                    {
                        inputAdapter = new WcfPointInputAdapter(cepEventType, configInfo);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return inputAdapter;
        }

        public void Dispose()
        {
        }
    }
}
