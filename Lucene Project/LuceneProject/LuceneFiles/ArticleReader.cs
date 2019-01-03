using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneProject.LuceneFiles
{
    public class ArticleReader
    {
        #region Public methods
        public static string StringWordsRemove(string stringToClean)
        {
            return string.Join(" ", stringToClean
                 .Split(new[] { ' ', ',', '.', '?', '!', '-', '\r', '\n', '"', ')', '(' }, StringSplitOptions.RemoveEmptyEntries)
                 .Except(ArticleReader.CommonWords(@"Data\common_words")));
        }


        public static IEnumerable<string> CommonWords(string file)
        {
            List<string> CWList = new List<string>();

            using (StreamReader reader = new StreamReader(file))
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    CWList.Add(line.ToString());
                }
            }
            return CWList;
        }
        public static IEnumerable<Article> ReadArticles(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException("file");
            };

            using (StreamReader reader = new StreamReader(file))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    Article article = new Article
                    {
                        Id = int.Parse(line.Split(' ')[1])
                    };

                    line = reader.ReadLine();

                    while (line != null && !line.StartsWith(".I"))
                    {
                        if (line.Equals(".T"))
                        {
                            StringBuilder titleBuilder = new StringBuilder();

                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line))
                            {
                                titleBuilder.AppendLine(line);
                            }

                            article.Title = titleBuilder.ToString();
                            continue;
                        }

                        if (line.Equals(".W"))
                        {
                            StringBuilder summaryBuilder = new StringBuilder();

                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line))
                            {
                                summaryBuilder.AppendLine(line);
                            }

                            article.Summary = summaryBuilder.ToString();
                            continue;
                        }

                        if (line.Equals(".B"))
                        {
                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line)) { }
                            continue;
                        }

                        if (line.Equals(".A"))
                        {
                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line))
                            {
                                line = line.Split(',')[0];
                                article.Authors.Add(line);
                            }
                            continue;
                        }

                        if (line.Equals(".N"))
                        {
                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line)) { }
                            continue;
                        }

                        if (line.Equals(".X"))
                        {
                            while ((line = reader.ReadLine()) != null && !IsKeyLine(line)) { }
                            continue;
                        }
                    }

                    yield return article;
                }
            }
        }

        #endregion

        #region Private methods

        private static bool IsKeyLine(string line)
        {
            return line.StartsWith(".I") || line.Equals(".T") || line.Equals(".W") || line.Equals(".B") || line.Equals(".A") || line.Equals(".N") || line.Equals(".X") || line.Equals(".C");
        }

        #endregion

    }
}
