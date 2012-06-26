// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace AdvantIQ.ExampleAdapters.Output.WinFormStacker
{
    [ServiceContract]
    public interface IStackerCtl
    {
        [OperationContract]
        void Push(string message);
    }
}
