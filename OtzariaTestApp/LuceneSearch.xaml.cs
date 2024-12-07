using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OtzariaTestApp
{
    /// <summary>
    /// Interaction logic for LuceneSearch.xaml
    /// </summary>
    public partial class LuceneSearch : UserControl
    {
        LuceneService _luceneService = new LuceneService(@".\LuceneIndex");
        SQLiteFTS4Service _sqliteService = new SQLiteFTS4Service();
        public LuceneSearch()
        {
            InitializeComponent();
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");
        }

        private async void IndexFolderButton_Click(object sender, RoutedEventArgs e)
        {
            //C:\אוצריא\אוצריא\תנך\תורה
            await _sqliteService.IndexFolder("C:\\אוצריא\\אוצריא");
            Console.WriteLine("Indexing Complete!");
            //_luceneService.PrintAllIndexEntries();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResultsList.Items.Clear();
            string queryText = SearchBox.Text;

            if (string.IsNullOrWhiteSpace(queryText)) return;

            try
            {
                var results =  _sqliteService.Search(queryText);
                foreach (var result in results)
                {
                    ResultsList.Items.Add(result);
                }
            }
            catch (Exception ex) {MessageBox.Show(ex.Message); }
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsList.SelectedItem is SearchResult result)
            {
                string[] lines = File.ReadAllLines(result.FilePath);
                string relevantContent = string.Join(Environment.NewLine, lines.Skip(result.Start).Take(result.End - result.Start + 1));
                FileContentBox.Text = relevantContent;
            }
        }
    }
}
