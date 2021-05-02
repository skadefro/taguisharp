using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagui
{
    //
    // Summary:
    //     Provides data for the System.IO.FileSystemWatcher.Error event.
    public class OutputEventArgs : EventArgs
    {
        public string Output { get; private set; }
        public OutputEventArgs(string Output)
        {
            this.Output = Output;
        }
    }

}
