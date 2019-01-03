using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Index;

namespace LuceneProject.LuceneFiles
{
    public class CustomTermVectorMapper : TermVectorMapper, ITermFrequencyMapper
    {
        #region Private fields

        // Λεξικό που διατηρεί τους όρους και τη συχνότητα τους
        private readonly IDictionary<string, int> termFrequencies;

        #endregion

        #region Constructors

        public CustomTermVectorMapper() :
            this(true, true)
        { }

        public CustomTermVectorMapper(bool ignoringPositions, bool ignoringOffsets) :
            base(ignoringPositions, ignoringOffsets)
        {
            this.termFrequencies = new Dictionary<string, int>();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Οι όροι που βρίσκονται στο έγγραφο.
        /// </summary>
        public string[] Terms
        {
            get { return this.termFrequencies.Keys.ToArray(); }
        }

        /// <summary>
        /// Οι συχότητες εμφάνισης των όρων.
        /// </summary>
        public int[] Frequencies
        {
            get { return this.termFrequencies.Values.ToArray(); }
        }

        /// <summary>
        /// Εύρεση της συχνότητας για έναν συγκεκριμένο όρο.
        /// </summary>
        /// <param name="term">Ο όρος του οποίου τη συχνότητα επιθυμούμε να ανακτήσουμε.</param>
        /// <returns>Τη συχνότητα του όρου που ζητήθηκε.</returns>
        public int this[string term]
        {
            get { return this.termFrequencies[term]; }
        }

        #endregion

        // Μέθοδος που καλεί η Lucene. Την κάνουμε override για να εκμεταλευτούμε την πληροφορία που μας παρέχει και να την αποθηκεύσουμε
        // στο λεξικό.
        public override void Map(string term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions)
        {
            if (this.termFrequencies.ContainsKey(term))
            {
                this.termFrequencies[term] += frequency;
            }
            else
            {
                this.termFrequencies.Add(term, frequency);
            }
        }

        public override void SetExpectations(string field, int numTerms, bool storeOffsets, bool storePositions)
        {
            // We do not use this method in our implementation.
        }
    }
}
