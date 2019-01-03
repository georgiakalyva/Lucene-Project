using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneProject.LuceneFiles
{
    public interface ITermFrequencyMapper
    {
        string[] Terms { get; }
        int[] Frequencies { get; }
        int this[string term] { get; }
    }
}
