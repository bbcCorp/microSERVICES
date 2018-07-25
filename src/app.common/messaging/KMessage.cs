using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace app.common.messaging
{
    public class KMessage
    {
        public string Topic { get; set;}

        public int Partition { get; set; }

        public long Offset { get; set;}

        public  string Message { get; set; }

    }

    public class KMessage<T>
    {
        public string Topic { get; set;}

        public int Partition { get; set; }

        public long Offset { get; set;}

        public  T Message { get; set; }

    }
}    