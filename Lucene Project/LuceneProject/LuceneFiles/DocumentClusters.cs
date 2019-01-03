using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneProject.LuceneFiles
{
    public class DocumentClusters
    {
        public int LeaderDocumentID { get; set; }
        public List<DocumentRank> Cluster { get; set; }
    }

    public class DocumentRank
    {
        public int DocumentID {get;set;}
        public int Rank {get;set;}
    }
}
