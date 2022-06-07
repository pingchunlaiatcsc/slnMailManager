using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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
using System.Windows.Threading;
using System.Xml;

namespace prjMailManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MailContent tmpMailContent;
        public MainWindow()
        {
            InitializeComponent();
            //rndRow();
            InitListView();


        }
        public void InitListView()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string FolderPath;
            Settings settings = new Settings();
            tb_MailFolderPath.Text = settings.MailFolderPath;
            tb_MoveToFolderName.Text = settings.MoveToFolderName;
            FolderPath = settings.MailFolderPath;


            List<MailContent> mailContentList = new List<MailContent>();


            string[] PathEntries = SearchAllInDirectory(FolderPath);
            List<string> SubjectList = new List<string>();
            List<string> SendDateList = new List<string>();
            List<string> FolderNameList = new List<string>();
            int j = 0;
            string tmpPath = "";
            string[] err_tmpSubjectArray = { };
            try
            {
                foreach (var item in PathEntries)
                {
                    string[] tmpSubjectArray = item.Split('\\');
                    string tmpSubjectString = tmpSubjectArray[tmpSubjectArray.Length - 1];
                    string tmpSendDateString = $"{tmpSubjectString.Split('-')[0].Substring(0, 4)}/{tmpSubjectString.Split('-')[0].Substring(4, 2)}/{tmpSubjectString.Split('-')[0].Substring(6, 2)}";
                    FolderNameList.Add(tmpSubjectString);

                    //tmpSubjectString.Split('-').Length == 2 代表此資料夾沒有主旨
                    if (tmpSubjectString.Split('-').Length == 2)
                    {
                        SubjectList.Add("");
                    }
                    else
                    {
                        SubjectList.Add(tmpSubjectString.Split('-')[2]);
                    }
                    SendDateList.Add(tmpSendDateString);

                    err_tmpSubjectArray = tmpSubjectArray;
                    tmpPath = item;
                    j++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"index {j} : {tmpPath} ");
                MessageBox.Show($"err_tmpSubjectArray.Length  : {err_tmpSubjectArray.Length} ");
                foreach (var item in err_tmpSubjectArray)
                {
                    MessageBox.Show($"item  : {item} ");
                }
                MessageBox.Show($"err_tmpSubjectArray[4]  : {err_tmpSubjectArray[4]} ");
                MessageBox.Show(ex.Message);
            }



            List<string> ContentEntries = new List<string>();
            foreach (var item in PathEntries)
            {
                ContentEntries.Add(readTxt(item));
            }

            for (int i = 0; i < ContentEntries.Count; i++)
            {
                var Id = ContentEntries.ElementAt(i).Substring(7, 6);
                var Sender = ContentEntries.ElementAt(i).Substring(14, 3);
                mailContentList.Add(new MailContent() { Sender = Sender, Id = Id, Subject = SubjectList[i], SendDate = SendDateList[i], FolderPath = PathEntries[i], FolderName = FolderNameList[i] });
            }

            List<MailContent> tmpMailContentList = new List<MailContent>();
            for (int i = mailContentList.Count - 1; i >= 0; i--)
            {
                tmpMailContentList.Add(mailContentList[i]);
            }
            mailContentList = tmpMailContentList;


            listViewMails.ItemsSource = mailContentList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listViewMails.ItemsSource);
            view.Filter = ContentFilter;
            //view.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
        }
        private bool ContentFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
            {
                var judge = ((item as MailContent).Sender.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || ((item as MailContent).Id.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || ((item as MailContent).Subject.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || ((item as MailContent).SendDate.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                return judge;
            }
        }
        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(listViewMails.ItemsSource).Refresh();
        }
        public string readTxt(string FolderPath)
        {

            string text = "";
            string MailPath = FolderPath + @"\本文.html";

            using (var fs = new FileStream(MailPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.GetEncoding("big5")))
            {
                text = sr.ReadToEnd();
            }
            return text;
        }
        public void showHtml(string Html)
        {
            myWebBrowser.NavigateToString(Html);
        }
        public string[] SearchAllInDirectory(string Path)
        {
            try
            {
                string[] entries = Directory.GetDirectories(Path);
                return entries;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public void moveFolder(string FolderName)
        {
            if (!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }
            foreach (MailContent item in listViewMails.SelectedItems)
            {
                var yy = item.FolderPath;
                Directory.Move(yy, $"./{FolderName}/{item.FolderName}");
            }
            InitListView();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            moveFolder(tb_MoveToFolderName.Text);
            #region old function to get all checkbox
            //SearchAllInDirectory();
            //foreach (var item in mainStackPanel.Children)
            //{
            //    if (item.GetType().FullName == "System.Windows.Controls.WrapPanel")
            //    {
            //        WrapPanel myWrapPanel = (WrapPanel)item;
            //        foreach (var obj in myWrapPanel.Children)
            //        {
            //            if (obj.GetType().FullName == "System.Windows.Controls.CheckBox")
            //            {
            //                CheckBox checkBox = (CheckBox)obj;
            //                if (checkBox.IsChecked == true)
            //                {
            //                    MessageBox.Show($"{checkBox.Name} is checked");

            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
        }


        public class Settings
        {
            public string MailFolderPath { get; set; }
            public string MoveToFolderName { get; set; }

            public Settings()
            {
                using (ReadINI oTINI = new ReadINI("./Config.ini"))
                {
                    string sResult = oTINI.getKeyValue("Path", "MailFolderPath"); //Section name=PostData；Key name=Value
                    MailFolderPath = sResult;
                    sResult = oTINI.getKeyValue("Path", "MoveToFolderName"); //Section name=PostData；Key name=Value
                    MoveToFolderName = sResult;
                }

            }
        }
        private void Setting_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (ReadINI oTINI = new ReadINI("./Config.ini"))
            {
                oTINI.setKeyValue("Path", "MailFolderPath", tb_MailFolderPath.Text);
                oTINI.setKeyValue("Path", "MoveToFolderName", tb_MoveToFolderName.Text);
            }
        }


        public class MailContent
        {
            public string Sender { get; set; }
            public string Id { get; set; }
            public string Subject { get; set; }
            public string SendDate { get; set; }
            public bool IsChecked { get; set; }
            public string FolderPath { get; set; }
            public string FolderName { get; set; }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var yy = sender as MailContent;
            var zz = yy.FolderPath;
        }

        private void ListViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tmpMailContent = ((ListViewItem)sender).Content as MailContent;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", tmpMailContent.FolderPath);
            tmpMailContent = null;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showHtml(readTxt(tmpMailContent.FolderPath));
            tmpMailContent = null;

        }

        private void Click_SelectAll(object sender, RoutedEventArgs e)
        {
            var CheckBoxAll = (CheckBox)sender as CheckBox;

            bool allcheckbox = (SelectAll.IsChecked == true);

            if (CheckBoxAll.IsChecked == true)
            {
                foreach (MailContent item in listViewMails.ItemsSource)
                {
                    item.IsChecked = true;
                    listViewMails.SelectedItems.Add(item);

                }

            }
            else
            {
                foreach (MailContent item in listViewMails.ItemsSource)
                {
                    item.IsChecked = false;
                    listViewMails.SelectedItems.Clear();
                }
            }
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            InitListView();
        }
    }
}
