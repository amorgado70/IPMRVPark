using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Models.View
{
    public class SelectionOption
    {
        public long ID { get; set; }
        public string Label { get; set; }
        public SelectionOption(long id, string label)
        {
            ID = id;
            Label = label;
        }
    }
}
