using LuceneProject.LuceneFiles;
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

namespace LuceneProject
{
    /// <summary>
    /// Interaction logic for BuildIndex.xaml
    /// </summary>
    public partial class BuildIndex : UserControl
    {
        IEnumerable<Article> articles = ArticleReader.ReadArticles(@"Data\cacm.all");
        public BuildIndex()
        {
            InitializeComponent();
            tbxPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + "\\LuceneIndex\\";
            tbxPathCluster.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + "\\LuceneLeadIndex\\";

            btnBuild.Click += btnBuild_Click;
            btnBuildCluster.Click += btnBuildCluster_Click;


        }
        void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            
            string path = tbxPath.Text;
            LuceneService lucene = new LuceneService(tbxPath.Text.ToString());
            // lucene.GetTermFrequency();
            //lucene.InitialiseLucene();
            lucene.BuildIndex(articles);
            // lucene.BuildLeaderIndex(articles);
            // lucene.IndexClustering(articles);
            spSuccess.Visibility = Visibility.Visible;
        }

        void btnBuildCluster_Click(object sender, RoutedEventArgs e)
        {
            string path = tbxPathCluster.Text;
            LuceneService lucene = new LuceneService(tbxPathCluster.Text.ToString());
            //lucene.GetTermFrequency();
            //lucene.InitialiseLucene();
            //lucene.BuildIndex(articles);
            // lucene.BuildLeaderIndex(articles);
            lucene.IndexClustering(articles);
            spSuccessCluster.Visibility = Visibility.Visible;
        }
    }
}
