using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DibrisBike
{
    class accumuloSald
    {
        public accumuloSald()
        {
        }

        public void setAccumuloSald(ConcurrentQueue<string[]> _queueLC1, ConcurrentQueue<string[]> _queueLC2, ConcurrentQueue<string[]> _queueLC3, AutoResetEvent _signalLC)
        {
            while(true)
            {
                _signalLC.WaitOne();
                //TODO: problem: how to differentiate the queue?
            }
        }
    }
}
