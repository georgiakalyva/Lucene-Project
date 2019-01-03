using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneProject.LuceneFiles
{
    public class Article
    {
        #region Constructors

        public Article()
        {
            Authors = new List<string>();
            this.Title = string.Empty;
            this.Summary = string.Empty;
            this.Score = 0;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Το μοναδικό αναγνωριστικό (πεδίο .I) του άρθρου.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ο τίτλος (πεδίο .T) του άρθρου.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Η σύνοψη (πεδίο .W) του άρθρου.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Το σκορ του άρθρου
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Οι συγγραφείς (πεδίο .A) του άρθρου.
        /// </summary>
        public List<string> Authors { get; set; }

        #endregion

        #region Public methods

        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + this.Id.GetHashCode();
                hash = hash * 23 + this.Title.GetHashCode();
                hash = this.Summary != null ? hash * 23 + this.Summary.GetHashCode() : hash;

                // Fix implementation for each author!
                hash = this.Authors != null ? hash * 23 + this.Authors.GetHashCode() : hash;

                return hash;
            }
        }

        #endregion
    }
}
