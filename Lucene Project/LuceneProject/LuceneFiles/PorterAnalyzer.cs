using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace LuceneProject.LuceneFiles
{
    public sealed class PorterAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            TokenStream tok = new PorterStemFilter(new LowerCaseTokenizer(reader));
            return tok;
        }

        //public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        //     {
        //         var tokenizer = (Tokenizer) PreviousTokenStream;
        //         if (tokenizer == null)
        //         {
        //             tokenizer = new LowerCaseTokenizer(reader);
        //             PreviousTokenStream = tokenizer;
        //         }
        //        else
        //           tokenizer.Reset(reader);
        //         return tokenizer;
        //     }
        
    }
}
