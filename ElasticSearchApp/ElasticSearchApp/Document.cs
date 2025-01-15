using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApp
{
    public class Document
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }  // Content extracted from PDF
        public DateTime CreatedDate { get; set; }
    }
}
