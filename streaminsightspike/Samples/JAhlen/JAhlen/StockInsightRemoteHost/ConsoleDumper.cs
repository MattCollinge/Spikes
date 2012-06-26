// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace AdvantIQ.StockInsight
{
    [ServiceContract]
    public interface IConsoleDumper
    {
        [OperationContract]
        void Dump(string msg);
    }

    public class ConsoleDumper : IConsoleDumper
    {
        public void Dump(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
