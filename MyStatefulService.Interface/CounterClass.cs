using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStatefulService.Interface
{
    public class CounterClass
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public long Count { get; set; }
    }
}
