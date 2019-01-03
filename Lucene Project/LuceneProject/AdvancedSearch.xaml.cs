using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows.Controls;
using LuceneProject.LuceneFiles;
using System.Diagnostics;

namespace LuceneProject
{
    /// <summary>
    /// Interaction logic for AdvancedSearch.xaml
    /// </summary>
    public partial class AdvancedSearch : UserControl
    {
        public AdvancedSearch()
        {
            InitializeComponent();

            btnSearch.Click += btnSearch_Click;
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LuceneService lucene = new LuceneService(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\LuceneIndex\\");

            try
            {
                List<string> fields = new List<string>();
                if (ckAuthor.IsChecked == true)
                {
                    fields.Add("Author");
                }
                if (ckTitle.IsChecked == true)
                {
                    fields.Add("Title");
                }
                if (ckSummary.IsChecked == true)
                {
                    fields.Add("Summary");
                }

                var watch = Stopwatch.StartNew();                
                IEnumerable<Article> Results = lucene.AdvancedSearch(tbxTerms.Text.ToLower(), fields.ToArray());

                watch.Stop();
                String elapsedMs = watch.ElapsedMilliseconds.ToString();

                if (Results.Count() == 0)
                {
                    DatagridResults.Visibility = Visibility.Collapsed;
                    NoResults.Visibility = Visibility.Visible;
                }
                else
                {
                    NoResults.Visibility = Visibility.Collapsed;
                    DatagridResults.Visibility = Visibility.Visible;
                    DatagridResults.ItemsSource = Results;
                }
                DocumentsReturned.Text = "Documents Found: " + Results.Count();
                TimeElapsed.Text = "Time: " + elapsedMs + " ms";
            }
            catch (NullReferenceException)
            {
                var dialog = new ModernDialog
                {
                    Title = "Index Error",
                    Content = "Index does not exist"
                };

                dialog.ShowDialog();
            }


        }


        private void ViewResultsButtonClick(object sender, RoutedEventArgs e)
        {
            object ID = ((Button)sender).CommandParameter;

            List<Article> Articles = ArticleReader.ReadArticles(@"Data\cacm.all").ToList<Article>();

            Article OpenArticle = Articles.Find(x => x.Id == Convert.ToInt32(ID));
            string Authors = "No Authors.";

            int i = 0;
            foreach (var author in OpenArticle.Authors)
            {
                if (i == 0)
                {
                    Authors = author;
                    i++;
                }
            }

            string titleA = OpenArticle.Title.ToString() != String.Empty ? OpenArticle.Title.ToString() : "No Title";
            string summaryA = OpenArticle.Summary.ToString() != String.Empty ? OpenArticle.Summary.ToString() : "No Summary Available";

            var dialog = new ModernDialog
            {
                Title = titleA,
                Content = summaryA + System.Environment.NewLine + " Authors: " + Authors
            };
            
            dialog.ShowDialog();
        }
    }
}
