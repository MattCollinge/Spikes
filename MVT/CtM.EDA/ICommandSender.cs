using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CtM.EDA
{

    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;

    }
    
}
