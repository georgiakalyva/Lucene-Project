using System;
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
using LuceneProject.Extensions;


namespace LuceneProject.LuceneFiles
{
    public class LuceneService
    {
        // Note there are many different types of Analyzer that may be used with Lucene, the exact one you use
        // will depend on your requirements
        private const Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_30;
        private Analyzer analyzer = new PorterAnalyzer();
        private Directory luceneIndexDirectory;
        private Directory luceneLeaderIndexDirectory;
        private Directory luceneIndexDirectoryFollowers;
        private IndexWriter writer;
        private IndexWriter writerLead;
        //private IndexWriter writerFollowers;
        public static string indexPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\LuceneIndex\\";
        public static string indexPathLead = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\LuceneLeadIndex\\";
        public static string indexPathFollower = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\LeceneFollowerIndex\\";

        public LuceneService(string indexPathfromUser)
        {
            //indexPath = indexPathfromUser;
        }

        //public void InitialiseLucene()
        //{
        //    if (System.IO.Directory.Exists(indexPath))
        //    {
        //        System.IO.Directory.Delete(indexPath, true);
        //    }

        //    luceneIndexDirectory = FSDirectory.Open(indexPath);
        //    writer = new IndexWriter(luceneIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
        //}

        //public void OpenConnection() 
        //{
        //    if (!System.IO.Directory.Exists(indexPath))
        //    {
        //        throw new NullReferenceException("File Not Exists");
        //    }
        //    else
        //    {
        //        luceneIndexDirectory = FSDirectory.Open(indexPath);
        //        writer = new IndexWriter(luceneIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
        //    }
        //}

        #region Question A
        public void BuildIndex(IEnumerable<Article> Articles)
        {
            //Έλεγχος αν υπάρχει ο κατάλογος, αν ναι να διαγραφεί
            if (System.IO.Directory.Exists(indexPath))
            {
                System.IO.Directory.Delete(indexPath, true);
            }
            //Αρχικοποιούμε τις μεταβλητές
            luceneIndexDirectory = FSDirectory.Open(indexPath);
            writer = new IndexWriter(luceneIndexDirectory, analyzer, true,
                IndexWriter.MaxFieldLength.UNLIMITED);
            //Για κάθε άρθρο εισάγουμε τα στοιχεία στο ευρετήριο
            foreach (var article in Articles)
            {
                //Προσθέτουμε το ID χωρίς να περνάει από Analyzer αυτούσιο
                Document doc = new Document();
                doc.Add(new Field("ID", article.Id.ToString(), Field.Store.YES,
                    Field.Index.NOT_ANALYZED));

                if (article.Title != null)
                {
                    //Προσθέτουμε τον Τίτλο αν υπάρχει αφού αφαιρέσουμε τα common words
                    string Title = ArticleReader.StringWordsRemove(article.Title);
                    //Προστίθονται όλες οι λέξεις με μικρά, περνόντας από τον Analyzer(Porter Stemmer)
                    doc.Add(new Field("Title", Title.ToLower(), Field.Store.YES, Field.Index.ANALYZED,
                        Field.TermVector.YES));
                }
                if (article.Summary != null)
                {
                    //Προσθέτουμε το Summary αν υπάρχει αφού αφαιρέσουμε τα common words
                    string Summary = ArticleReader.StringWordsRemove(article.Summary);
                    //Προστίθονται όλες οι λέξεις με μικρά, περνόντας από τον Analyzer(Porter Stemmer)
                    doc.Add(new Field("Summary", Summary.ToLowerInvariant(), Field.Store.YES,
                        Field.Index.ANALYZED, Field.TermVector.YES));
                }
                foreach (var author in article.Authors)
                {
                    if (author != null)
                    { //Προσθέτουμε τους Συγγραφείς αν υπάρχουν    
                        doc.Add(new Field("Author", author.ToLowerInvariant(), Field.Store.YES,
                            Field.Index.NOT_ANALYZED));
                    }
                }
                //Προσθέτουμε το έγγραφο στο ευρετήριο
                writer.AddDocument(doc);
            }
            //Optimization και απελευθέρωση από τη μνήμη τα αντικείμενα
            writer.Optimize();
            writer.Flush(true, true, true);
            writer.Dispose();
            luceneIndexDirectory.Dispose();
        }

        public IEnumerable<Article> Search(string searchTerm, string[] fields)
        {  //Έλεγχος αν υπάρχει το ευρετήριο αν όχι πέτα το μήνυμα
            if (!System.IO.Directory.Exists(indexPath))
            {
                throw new NullReferenceException("Index Does Not Exist");
            }
            //Αρχικοποίηση μεταβλητών
            luceneIndexDirectory = FSDirectory.Open(indexPath);
            List<Article> CompleteResults = new List<Article>();
            if (searchTerm != "" && fields.Length != 0)
            {
                //Αρχικοποίηση μεταβλητών
                IndexSearcher searcher = new IndexSearcher(luceneIndexDirectory);
                //Δημιουργία Searcher κειμένου
                MultiFieldQueryParser allFieldsSearcher =
                    new MultiFieldQueryParser(LuceneVersion, fields, analyzer);

                //Parce το όρο αναζήτησης
                Query query = allFieldsSearcher.Parse(searchTerm);
                //Δημιουργία collector που θα φέρει τα 100 πρώτα αποτελέσματα
                TopScoreDocCollector topScoreDocCollector = TopScoreDocCollector.Create(3200, true);
                //Πραγματοποίηση αναζήτησης
                searcher.Search(query, topScoreDocCollector);
                //Προσθήκη αποτελεσμάτων σε λίστα
                ScoreDoc[] hits = topScoreDocCollector.TopDocs().ScoreDocs;
                List<Article> results = new List<Article>();
                //Ανατρέχουμε τη λίστα αποτελεσμάτων με τη λίστα των άρθρων για να 
                //επιστρέψουμε στο χρήστη τα ολόκληρα τα άρθρα.
                foreach (ScoreDoc hit in hits)
                {
                    Article art = new Article();
                    int docId = hit.Doc;
                    float score = hit.Score;

                    Document document = searcher.Doc(docId);

                    art.Score = Convert.ToDouble(score.ToString("0.0000"));
                    art.Id = Convert.ToInt32(document.Get("ID"));

                    results.Add(art);
                }

                IEnumerable<Article> Articles = ArticleReader.ReadArticles(@"Data\cacm.all");
                //Προσθέτουμε τα άρθρα στα αποτελέσματα και τα scor του κάθε άρθρου
                foreach (Article item in results)
                {
                    foreach (Article article in Articles)
                    {
                        if (article.Id == item.Id)
                        {
                            Article art = new Article();
                            art = article;
                            art.Score = item.Score;
                            CompleteResults.Add(art);
                            break;
                        }
                    }
                }
                luceneIndexDirectory.Dispose();
                //Επιστρέφουμε τα αποτελέσματα στο χρήστη                
                return CompleteResults.OrderByDescending(x=> x.Score);
            }
            else
            {
                return CompleteResults;
            }

        }

        #endregion

        #region Question B
        public void BuildLeaderIndex(IEnumerable<Article> Articles)
        {
            int leadersNumber = (int)Math.Ceiling(Math.Sqrt(Articles.Count()));

            IEnumerable<Article> LeaderArticles = Articles.OrderBy(i => i.GetHashCode()).Take(leadersNumber);

            //Έλεγχος αν υπάρχει ο κατάλογος, αν ναι να διαγραφεί
            if (System.IO.Directory.Exists(indexPathLead))
            {
                System.IO.Directory.Delete(indexPathLead, true);
            }
            //Αρχικοποιούμε τις μεταβλητές
            luceneLeaderIndexDirectory = FSDirectory.Open(indexPathLead);
            writerLead = new IndexWriter(luceneLeaderIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            //Για κάθε άρθρο εισάγουμε τα στοιχεία στο ευρετήριο
            foreach (var article in LeaderArticles)
            {
                Document doc = new Document();
                //Προσθέτουμε το ID χωρίς να περνάει από Analyzer αυτούσιο
                doc.Add(new Field("ID", article.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                if (article.Title != null)
                {
                    //Προσθέτουμε τον Τίτλο αν υπάρχει αφού αφαιρέσουμε τα common words
                    string Title = ArticleReader.StringWordsRemove(article.Title);
                    //Προστίθονται όλες οι λέξεις με μικρά, περνόντας από τον Analyzer(Porter Stemmer)
                    doc.Add(new Field("Title", Title.ToLower(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                }
                if (article.Summary != null)
                {
                    //Προσθέτουμε το Summary αν υπάρχει αφού αφαιρέσουμε τα common words
                    string Summary = ArticleReader.StringWordsRemove(article.Summary);
                    //Προστίθονται όλες οι λέξεις με μικρά, περνόντας από τον Analyzer(Porter Stemmer)
                    doc.Add(new Field("Summary", Summary.ToLower(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                }

                foreach (var author in article.Authors)
                {
                    if (author != null)
                    {
                        //Προσθέτουμε τους Συγγραφείς αν υπάρχουν  
                        doc.Add(new Field("Author", author.ToLowerInvariant(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                }
                //Προσθέτουμε το έγγραφο στο ευρετήριο
                writerLead.AddDocument(doc);
            }
            //Optimization και απελευθέρωση από τη μνήμη τα αντικείμενα
            writerLead.Optimize();
            writerLead.Flush(true, true, true);
            writerLead.Dispose();
            luceneLeaderIndexDirectory.Dispose();

        }

        //public IEnumerable<Article> LeaderResults(int DocID, List<DocumentClusters> Clusters) 
        //{
        //    IEnumerable<Article> Articles = ArticleReader.ReadArticles(@"Data\cacm.all");
        //    List<Article> Results = new List<Article>();

        ////    foreach (DocumentClusters cluster in Clusters)
        ////    {
        ////        if (cluster.LeaderDocumentID == DocID)
        ////            {
        ////                foreach (DocumentRank ClusteredDoc in cluster)
        ////                {
        ////                    foreach (Article article in Articles)
        ////                    {
        ////                        if (ClusteredDoc.DocumentID == article.Id)
        ////                        {
        ////                            Results.Add(article);
        ////                        }
        ////                    }
        ////                    //return Results;
        ////                }
        ////            }

        ////    }
        //    return Results;
        //}

        public IEnumerable<TermData> GetTermFrequency()
        {
            List<TermData> termlist = new List<TermData>();
            if (System.IO.Directory.Exists(indexPath))
            {
                luceneIndexDirectory = FSDirectory.Open(indexPath);
                // writer = new IndexWriter(luceneIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                //IndexReader reader = writer.GetReader();


                IndexReader reader = IndexReader.Open(luceneIndexDirectory, true);
                TermEnum terms = reader.Terms();
                while (terms.Next())
                {
                    TermData td = new TermData();
                    Term term = terms.Term;
                    td.TermText = term.Text;
                    td.TermFrequency = reader.DocFreq(term);
                    termlist.Add(td);
                }
                reader.Dispose();
                return termlist;
            }
            else
            {
                throw new NullReferenceException("Index does not exist.");
            }
        }

        /// <summary>
        /// Ανακτά τις πληροφορίες για τη συχνότητα των όρων σε κάποιο ζητούμενο έγγραφο.
        /// </summary>
        /// <param name="documentId">Το id του εγγράφου του οποίου θέλουμε τη συχνότητα των όρων.</param>
        /// <returns>Επιστρέψει πληροφορία σχετικά με τους όρους του  κειμένου καθώς και τη συχνότητα εμφάνισης τους.</returns>
        public ITermFrequencyMapper GetTermFrequency(int documentId)
        {
            if (System.IO.Directory.Exists(indexPath))
            {
                luceneIndexDirectory = FSDirectory.Open(indexPath);

                using (IndexReader reader = IndexReader.Open(luceneIndexDirectory, true))
                {
                    // 
                    CustomTermVectorMapper customTermVectorMapper = new CustomTermVectorMapper();
                    reader.GetTermFreqVector(documentId, customTermVectorMapper);
                    return customTermVectorMapper;
                }
            }
            else
            {
                throw new NullReferenceException("Index does not exist.");
            }
        }

        public void IndexClustering(IEnumerable<Article> Articles)
        {
            if (!System.IO.Directory.Exists(indexPath))
            {
                throw new NullReferenceException("Index Does Not Exist");
            }

            // Ο αριθμός των leaders. Ισούται με ρίζα N. Το στρογγυλοποιούμε στον πλησιέστερο μεγαλύτερο ακέραιο.
            int leadersNumber = (int)Math.Ceiling(Math.Sqrt(Articles.Count()));
            IEnumerable<int> leaderArticleIds = Articles.OrderBy(i => i.GetHashCode()).Take(leadersNumber).Select(document => document.Id);
            IEnumerable<int> followerArticleIds = Articles.Select(article => article.Id).Except(leaderArticleIds);

            IEnumerable<Tuple<Document, ITermFrequencyMapper>> leaders = GetIndexedDocuments(leaderArticleIds);
            IEnumerable<Tuple<Document, ITermFrequencyMapper>> followers = GetIndexedDocuments(followerArticleIds);

            IDictionary<Document, IList<Document>> clusters = GenerateClusters(leaders, followers);

            BuildClusteredIndex(clusters);
        }

        private void BuildClusteredIndex(IDictionary<Document, IList<Document>> clusters)
        {
            if (clusters == null)
            {
                throw new ArgumentNullException("clusters");
            }

            // Έλεγχος αν υπάρχει ο κατάλογος, αν ναι να διαγραφεί
            if (System.IO.Directory.Exists(indexPathLead))
            {
                System.IO.Directory.Delete(indexPathLead, true);
            }

            // Έλεγχος αν υπάρχει ο κατάλογος, αν ναι να διαγραφεί
            if (System.IO.Directory.Exists(indexPathFollower))
            {
                System.IO.Directory.Delete(indexPathFollower, true);
            }

            using (Directory luceneLeaderIndexDirectory = FSDirectory.Open(indexPathLead))
            {
                using (IndexWriter indexWriter = new IndexWriter(luceneLeaderIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // Για κάθε συστάδα, εκτέλεσε τα παρακάτω
                    foreach (KeyValuePair<Document, IList<Document>> kvp in clusters)
                    {
                        // Πάρε τον ηγέτη της συστάδας
                        Document leader = kvp.Key;

                        // Πάρε τα id's των ακολούθων του
                        IEnumerable<string> followers = kvp.Value.Select(i => i.Get("ID"));

                        // Δημιούργησε με τα id's των ακολούθων ένα αλφαριθμητικό που θα περιέχει τα id's αυτά χωρισμένα με ','
                        string followerString = followers != null && followers.Count() != 0 ? followers.Aggregate((x, y) => x + "," + y) : string.Empty;

                        // Δημιούργησε ένα νεό πεδίο στο Document. Το πεδίο αυτό περιέχει τα id's των ακολούθων.
                        // Αποθήκευση των id's αυτών σε ένα νέο πεδίο "Followers"
                        leader.Add(new Field("Followers", followerString, Field.Store.YES, Field.Index.NO, Field.TermVector.NO));

                        // Εισαγωγή του Document στο ευρετήριο των ηγετών
                        indexWriter.AddDocument(leader);
                    }

                    indexWriter.Flush(true, true, true);
                    indexWriter.Optimize();
                }
            }

            // Για τους ακολούθους
            using (Directory luceneFollowerIndexDirectory = FSDirectory.Open(indexPathFollower))
            {
                using (IndexWriter indexWriter = new IndexWriter(luceneFollowerIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // Για κάθε συστάδα, εκτέλεσε τα παρακάτω
                    foreach (KeyValuePair<Document, IList<Document>> kvp in clusters)
                    {
                        // Για κάθε ακόλουθου της συγκεκριμένης συστάδας
                        foreach (Document follower in kvp.Value)
                        {
                            // Αναζήτησε το id του ηγέτη της συστάδας και εισήγαγε το σε ένα νέο πεδίο "Leader"
                            follower.Add(new Field("Leader", kvp.Key.Get("ID"), Field.Store.YES, Field.Index.NOT_ANALYZED));

                            // Εισαγωγή του Document
                            indexWriter.AddDocument(follower);
                        }
                    }

                    indexWriter.Flush(true, true, true);
                    indexWriter.Optimize();
                }
            }
        }

        private IEnumerable<string> GetIndexTerms()
        {
            List<string> termsList = new List<string>();

            using (Directory luceneIndexDirectory = FSDirectory.Open(indexPath))
            {
                using (IndexReader reader = IndexReader.Open(luceneIndexDirectory, true))
                {
                    TermEnum terms = reader.Terms();

                    while (terms.Next())
                    {
                        Term term = terms.Term;
                        string termText = term.Text;
                        termsList.Add(termText);
                    }

                    return termsList;
                }
            }
        }

        public IEnumerable<Tuple<Document, ITermFrequencyMapper>> GetIndexedDocuments(IEnumerable<int> documentIds)
        {
            if (documentIds == null)
            {
                throw new ArgumentNullException("documentIds");
            }

            if (!System.IO.Directory.Exists(indexPath))
            {
                throw new NullReferenceException("Index Does Not Exist");
            }

            int[] docsIds = documentIds.ToArray();

            using (Directory luceneIndexDirectory = FSDirectory.Open(indexPath))
            {
                using (IndexSearcher searcher = new IndexSearcher(luceneIndexDirectory))
                {
                    IEnumerable<Tuple<Document, ITermFrequencyMapper>> documents = ReadIndexedDocuments(searcher);
                    IEnumerable<Tuple<Document, ITermFrequencyMapper>> filteredDocuments = documents.Where(document => docsIds.Contains(int.Parse(document.Item1.Get("ID"))));
                    return filteredDocuments;
                }
            }
        }

        public IDictionary<Document, IList<Document>> GenerateClusters(IEnumerable<Tuple<Document, ITermFrequencyMapper>> leaders, IEnumerable<Tuple<Document, ITermFrequencyMapper>> followers)
        {
            if (leaders == null)
            {
                throw new ArgumentNullException("leaders");
            }

            if (followers == null)
            {
                throw new ArgumentNullException("followers");
            }

            IDictionary<Document, IList<Document>> clusters = new Dictionary<Document, IList<Document>>();
            foreach (Tuple<Document, ITermFrequencyMapper> leader in leaders)
            {
                clusters.Add(leader.Item1, new List<Document>());
            }

            // Για κάθε ακόλουθο, βρες τον πλησιέστερο ηγέτη του.
            foreach (Tuple<Document, ITermFrequencyMapper> follower in followers)
            {
                double cosineSimilarity = double.MinValue;
                Document currentLeader = null;

                // Για κάθε ηγέτη, υπολόγισε τους ακολούθους του.
                foreach (Tuple<Document, ITermFrequencyMapper> leader in leaders)
                {
                    int[] leaderFrequencies;
                    int[] followerFrequencies;
                    GenerateCommonTermArrays(leader.Item2, follower.Item2, out leaderFrequencies, out followerFrequencies);
                    double currentCosineSimilarity = followerFrequencies.CosineSimilarity(leaderFrequencies);

                    // Εάν έχουμε υπολογίσει μεγαλύτερη τιμή, έχουμε βρει πλησιέστερο έγγραφο.
                    if (currentCosineSimilarity >= cosineSimilarity)
                    {
                        cosineSimilarity = currentCosineSimilarity;
                        currentLeader = leader.Item1;
                    }
                }
                clusters[currentLeader].Add(follower.Item1);
            }

            return clusters;
        }

        private void GenerateCommonTermArrays(ITermFrequencyMapper leader, ITermFrequencyMapper follower, out int[] leaderFrequencies, out int[] followerFrequencies)
        {
            if (leader == null)
            {
                throw new ArgumentNullException("leader");
            }

            if (follower == null)
            {
                throw new ArgumentNullException("follower");
            }

            IEnumerable<string> terms = leader.Terms.Union(follower.Terms);

            IDictionary<string, int> leaderFreqs = terms.ToDictionary(key => key, value => default(int));
            IDictionary<string, int> followerFreqs = terms.ToDictionary(key => key, value => default(int));

            for (int i = 0; i < leader.Terms.Length; i++)
            {
                string term = leader.Terms[i];
                leaderFreqs[term] = leader.Frequencies[i];
            }

            for (int i = 0; i < follower.Terms.Length; i++)
            {
                string term = follower.Terms[i];
                followerFreqs[term] = follower.Frequencies[i];
            }

            leaderFrequencies = leaderFreqs.Values.ToArray();
            followerFrequencies = followerFreqs.Values.ToArray();
        }

        private IEnumerable<Tuple<Document, ITermFrequencyMapper>> ReadIndexedDocuments(IndexSearcher searcher)
        {
            if (searcher == null)
            {
                throw new ArgumentNullException("searcher");
            }

            IList<Tuple<Document, ITermFrequencyMapper>> documents = new List<Tuple<Document, ITermFrequencyMapper>>();
            for (int i = 0; i < searcher.MaxDoc; i++)
            {
                Tuple<Document, ITermFrequencyMapper> record = new Tuple<Document, ITermFrequencyMapper>(searcher.Doc(i), GetTermFrequency(i));
                documents.Add(record);
            }

            return documents;
        }

        public IEnumerable<Article> AdvancedSearch(string searchTerm, string[] fields)
        {  //Έλεγχος αν υπάρχει το ευρετήριο αν όχι πέτα το μήνυμα
            if (!System.IO.Directory.Exists(indexPathLead))
            {
                throw new NullReferenceException("Index Does Not Exist");
            }
            //Αρχικοποίηση μεταβλητών
            luceneLeaderIndexDirectory = FSDirectory.Open(indexPathLead);
            List<Article> results = new List<Article>();
            List<Article> CompleteResults2 = new List<Article>();
            if (searchTerm != "" && fields.Length != 0)
            {
                //Αρχικοποίηση μεταβλητών
                IndexSearcher searcher = new IndexSearcher(luceneLeaderIndexDirectory);
                //Δημιουργία Searcher κειμένου
                MultiFieldQueryParser allFieldsSearcher =
                    new MultiFieldQueryParser(LuceneVersion, fields, analyzer);

                //Parce το όρο αναζήτησης
                Query query = allFieldsSearcher.Parse(searchTerm);
                //Δημιουργία collector που θα φέρει τον leader
                TopScoreDocCollector topScoreDocCollector = TopScoreDocCollector.Create(1, true);
                //Πραγματοποίηση αναζήτησης
                searcher.Search(query, topScoreDocCollector);
                //Προσθήκη αποτελεσμάτων σε λίστα
                ScoreDoc[] hits = topScoreDocCollector.TopDocs().ScoreDocs;
                
                //Απομονώνουμε τον Leader
                Article leader = new Article();
                int docId = hits[0].Doc;
                float score = hits[0].Score;
                Document document = searcher.Doc(docId);
                leader.Score = Convert.ToDouble(score.ToString("0.0000"));
                leader.Id = Convert.ToInt32(document.Get("ID"));
                results.Add(leader);

                //Έλεγχος αν υπάρχει το ευρετήριο αν όχι πέτα το μήνυμα
                if (!System.IO.Directory.Exists(indexPathFollower))
                {
                    throw new NullReferenceException("Index Does Not Exist");
                }
                //Αρχικοποίηση μεταβλητών
                luceneIndexDirectoryFollowers = FSDirectory.Open(indexPathFollower);

                //Αρχικοποίηση μεταβλητών
                IndexSearcher searcherFollowers = new IndexSearcher(luceneIndexDirectoryFollowers);
                //Δημιουργία Searcher κειμένου
                MultiFieldQueryParser allFieldsSearcherFollowers =
                    new MultiFieldQueryParser(LuceneVersion, fields, analyzer);
                //Filter filter = //new FieldValueFilter("Leader", new[] { leader.Id.ToString() });
                //new QueryWrapperFilter(new TermQuery(new Term("Leader", leader.Id.ToString())));
                //    //QueryWrapperFilter(new WildcardQuery(new Term("Leader", leader.Id.ToString())));
                ////FieldRangeFilter("Leader", leader.Id.ToString(), leader.Id.ToString(), true, true);

                //Parce το όρο αναζήτησης
                Query queryFollowers = allFieldsSearcherFollowers.Parse(searchTerm);
                //Δημιουργία collector που θα φέρει τα πρώτα 1000 αποτελέσματα
                TopScoreDocCollector topScoreDocCollectorFollowers = TopScoreDocCollector.Create(3200, true);
                //Πραγματοποίηση αναζήτησης
                searcherFollowers.Search(queryFollowers, topScoreDocCollectorFollowers);
                //Προσθήκη αποτελεσμάτων σε λίστα
                ScoreDoc[] Followershits = topScoreDocCollectorFollowers.TopDocs().ScoreDocs;

                foreach (ScoreDoc hitFollow in Followershits)
                {
                    
                        Article art = new Article();
                        int docIdFollower = hitFollow.Doc;
                        float scoreFollower = hitFollow.Score;

                        Document documentFollower = searcherFollowers.Doc(docIdFollower);

                        art.Score = Convert.ToDouble(scoreFollower.ToString("0.0000"));
                        art.Id = Convert.ToInt32(documentFollower.Get("ID"));
                        int leaderID = Convert.ToInt32(documentFollower.Get("Leader"));

                        if (leaderID == leader.Id)
                    {
                        results.Add(art);
                    }
                }

                IEnumerable<Article> Articles = ArticleReader.ReadArticles(@"Data\cacm.all");
                //Προσθέτουμε τα άρθρα στα αποτελέσματα και τα scor του κάθε άρθρου
                foreach (Article res in results)
                {
                    foreach (Article article in Articles)
                    {
                        if (article.Id.ToString() == res.Id.ToString())
                        {
                            Article art = new Article();
                            art = article;
                            art.Score = res.Score;
                            CompleteResults2.Add(art);
                            //break;
                        }
                    }
                }
                //Επιστρέφουμε τα αποτελέσματα στο χρήστη      
                luceneLeaderIndexDirectory.Dispose();
                luceneIndexDirectoryFollowers.Dispose();
                return CompleteResults2;
            }
            else
            {
                return CompleteResults2;
            }

        }
        #endregion
    }
}

