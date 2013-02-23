using SoundSyncGUI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml;

namespace SoundSyncGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork +=
                new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged +=
                new ProgressChangedEventHandler(bw_ProgressChanged);

            bw2.WorkerSupportsCancellation = true;
            bw2.WorkerReportsProgress = true;
            bw2.DoWork +=
                new DoWorkEventHandler(bw_DoWork2);
            bw2.ProgressChanged +=
                new ProgressChangedEventHandler(bw_ProgressChanged2);
        }

        private void bw_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        {
            if (done == true && !bw.IsBusy)
            {
                bw.RunWorkerAsync();
            }

            if (done == false)
            {
                txtCalculate.Text = Convert.ToString(countSongs + " Songs");
            }
        }

        BackgroundWorker bw = new BackgroundWorker();
        BackgroundWorker bw2 = new BackgroundWorker();

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (step == 1)
            {
                if (progressIncrementValue <= 100)
                {
                    progressCounter.Value += progressIncrementValue;
                }
                txtDownloading.Text += "File Exists: " + title + Environment.NewLine;
                if (countSongs > 0)
                {
                    countSongs--;
                }
                txtCalculate.Text = Convert.ToString(countSongs) + " Songs";
                txtDownloading.ScrollToEnd();
                step = 0;
            }

            if (step == 2)
            {
                progressCounter.Value += progressIncrementValue;
                txtDownloading.Text += "Downloading " + title + Environment.NewLine;
                
                if (countSongs > 0)
                {
                    countSongs--;
                }
                txtCalculate.Text = Convert.ToString(countSongs) + " Songs";
                txtDownloading.ScrollToEnd();
                step = 0;
            }

            if (step == 3)
            {            
                txtDownloading.Text += "Complete" + Environment.NewLine;
                txtDownloading.ScrollToEnd();
                step = 0;
            }

            if (step == 4)
            {
                txtDownloading.Text += "All Downloads Complete." + Environment.NewLine;
                countSongs = 0;
                progressCounter.Value = 100;
                txtCalculate.Text = Convert.ToString(countSongs) + " Songs";
                txtDownloading.ScrollToEnd();
                step = 0;
            }
        }



        public int step;
        public string title, trackid, trackid2, genre, artist, downloadurl, filename, folderstructure;
        public int optionChoice;
        public string UserUrlFollowings;

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //number = 0;
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(step = 0);
            

            XmlDocument doc = new XmlDocument();

            if (optionChoice == 1)
            {
                foreach (string users in userList)
                {
                    XmlDocument doc3 = new XmlDocument();
                    ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                    doc3.Load(ResolveUrl);
                    userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                    UserUrl = "http://api.soundcloud.com/users/" + userid + "/favorites.xml?client_id=" + Settings.Default.apiKey;

                    doc.Load(UserUrl);
                    foreach (XmlNode child in doc.DocumentElement)
                    {

                        string candownload = child.SelectSingleNode("downloadable").InnerText;

                        if (candownload == "true")
                        {
                            title = child.SelectSingleNode("title").InnerText;
                            trackid = child.SelectSingleNode("id").InnerText;
                            genre = child.SelectSingleNode("genre").InnerText;
                            artist = child.SelectSingleNode("user").SelectSingleNode("username").InnerText;
                            downloadurl = child.SelectSingleNode("download-url").InnerText + "?client_id=" + Settings.Default.apiKey;
                            filename = MakeValidFileName(title + ".mp3");
                            folderstructure = null;
                            folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                            if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                            if (File.Exists(folderstructure + @"\" + filename))
                            {
                                System.Threading.Thread.Sleep(100);
                                worker.ReportProgress(step = 1);
                            }
                            else
                            {
                                WebClient client = new WebClient();

                                worker.ReportProgress(step = 2);
                                client.DownloadFile(downloadurl, folderstructure + @"\" + filename);
                                System.Threading.Thread.Sleep(100);
                                //  worker.ReportProgress(number = 3);
                            }
                        }
                    }
                }
            }
            else if (optionChoice == 2)
            {
                foreach (string users in userList)
                {
                    XmlDocument doc3 = new XmlDocument();
                    ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                    doc3.Load(ResolveUrl);

                    string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                    doc.Load(UserUrlFollowings);
                    foreach (XmlNode child2 in doc.DocumentElement)
                    {

                        XmlDocument doc2 = new XmlDocument();
                        string trackid = child2.SelectSingleNode("id").InnerText;
                        UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                        doc2.Load(UserUrl);

                        foreach (XmlNode child in doc2.DocumentElement)
                        {
                            string candownload = child.SelectSingleNode("downloadable").InnerText;

                            if (candownload == "true")
                            {
                                title = child.SelectSingleNode("title").InnerText;
                                trackid2 = child.SelectSingleNode("id").InnerText;
                                genre = child.SelectSingleNode("genre").InnerText;
                                artist = child.SelectSingleNode("user").SelectSingleNode("username").InnerText;
                                downloadurl = child.SelectSingleNode("download-url").InnerText + "?client_id=" + Settings.Default.apiKey;
                                filename = MakeValidFileName(title + ".mp3");
                                folderstructure = null;
                                folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                                if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                                if (File.Exists(folderstructure + @"\" + filename))
                                {
                                    worker.ReportProgress(step = 1);
                                    System.Threading.Thread.Sleep(100);
                                }
                                else
                                {
                                    WebClient client = new WebClient();

                                    worker.ReportProgress(step = 2);
                                    client.DownloadFile(downloadurl, folderstructure + @"\" + filename);
                                    System.Threading.Thread.Sleep(100);
                                    //worker.ReportProgress(number = 3);
                                }
                            }
                        }
                    }
                }
            }
            else if (optionChoice == 3)
            {
                
                    foreach (string users in userList)
                    {
                        XmlDocument doc3 = new XmlDocument();
                        ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                        doc3.Load(ResolveUrl);
                        userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                        string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                        doc.Load(UserUrlFollowings);


                        foreach (XmlNode child2 in doc.DocumentElement)
                        {

                            XmlDocument doc2 = new XmlDocument();
                            string trackid = child2.SelectSingleNode("id").InnerText;
                            UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                            doc2.Load(UserUrl);

                            foreach (XmlNode child in doc2.DocumentElement)
                            {
                                string candownload = child.SelectSingleNode("downloadable").InnerText;

                                if (candownload == "true")
                                {
                                    title = child.SelectSingleNode("title").InnerText;
                                    trackid2 = child.SelectSingleNode("id").InnerText;
                                    genre = child.SelectSingleNode("genre").InnerText;
                                    artist = child.SelectSingleNode("user").SelectSingleNode("username").InnerText;
                                    downloadurl = child.SelectSingleNode("download-url").InnerText + "?client_id=" + Settings.Default.apiKey;
                                    filename = MakeValidFileName(title + ".mp3");
                                    folderstructure = null;
                                    folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                                    if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                                    if (File.Exists(folderstructure + @"\" + filename))
                                    {
                                        worker.ReportProgress(step = 1);
                                        System.Threading.Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        WebClient client = new WebClient();

                                        worker.ReportProgress(step = 2);
                                        client.DownloadFile(downloadurl, folderstructure + @"\" + filename);
                                        System.Threading.Thread.Sleep(100);
                                        //  worker.ReportProgress(number = 3);
                                    }
                                }
                            }
                        }

                    }
            }
            else if (optionChoice == 4)
            {
                foreach (string users in userList)
                {
                    XmlDocument doc3 = new XmlDocument();
                    ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                    doc3.Load(ResolveUrl);
                    userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                    UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;

                    doc.Load(UserUrl);
                    foreach (XmlNode child in doc.DocumentElement)
                    {

                        string candownload = child.SelectSingleNode("downloadable").InnerText;

                        if (candownload == "true")
                        {
                            title = child.SelectSingleNode("title").InnerText;
                            trackid = child.SelectSingleNode("id").InnerText;
                            genre = child.SelectSingleNode("genre").InnerText;
                            artist = child.SelectSingleNode("user").SelectSingleNode("username").InnerText;
                            downloadurl = child.SelectSingleNode("download-url").InnerText + "?client_id=" + Settings.Default.apiKey;
                            filename = MakeValidFileName(title + ".mp3");
                            folderstructure = null;
                            folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                            if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                            if (File.Exists(folderstructure + @"\" + filename))
                            {
                                System.Threading.Thread.Sleep(100);
                                worker.ReportProgress(step = 1);
                            }
                            else
                            {
                                WebClient client = new WebClient();

                                worker.ReportProgress(step = 2);
                                client.DownloadFile(downloadurl, folderstructure + @"\" + filename);
                                System.Threading.Thread.Sleep(100);
                                //  worker.ReportProgress(number = 3);
                            }
                        }
                    }
                }
            }
            worker.ReportProgress(step = 4);
            userList.Clear();
            bw.CancelAsync();
            bw.Dispose();
        }

        public double countSongs;

        private void btnDownloadLikes_Click(object sender, RoutedEventArgs e)
        {
            //DownloadSongs sync = new DownloadSongs();
            //ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + listUsers.SelectedValue + "/&client_id=" + Settings.Default.apiKey;
            changePath();
            likes = true;
            progress = true;
            username = Convert.ToString(listUsers.SelectedValue);
            downloadType = 2;
            optionChoice = 1;
            if (!bw2.IsBusy && !bw.IsBusy)
            {
                countSongs = 0;
                progressCounter.Value = 0;
                bw2.RunWorkerAsync();
            }
            if (!bw.IsBusy)
            {
                txtDownloading.Text = "";
            }
            
            //userList.Clear();
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            listUsers.Items.Add(txtAddUser.Text);
            txtAddUser.Text = "";

        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidReStr, "_");
        }

        public void changePath()
        {
            if (Settings.Default.chooseDirectory == "")
            {
                MessageBox.Show("Please Choose A Directory To Download Music To.");
                btnDirectory_Click(null, null);
            }
        }

        public string UserUrl;

        public string userid, userid2;
        public int downloadType;

        public string ResolveUrl;

        public List<string> userList = new List<string>();

        private void btnDownloadTracks_Click(object sender, RoutedEventArgs e)
        {
            changePath();
            tracks = true;
            downloadType = 1;
            progress = true;
          
           // username = Convert.ToString(listUsers.SelectedValue);
            optionChoice = 4;
            if (!bw2.IsBusy && !bw.IsBusy)
            {
                countSongs = 0;
                progressCounter.Value = 0;
                bw2.RunWorkerAsync();
               
            }
            if (!bw.IsBusy)
            {
                txtDownloading.Text = "";
            }
        }

        private void btnDownloadFollowers_Click(object sender, RoutedEventArgs e)
        {
           // ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + listUsers.SelectedValue + "/&client_id=" + Settings.Default.apiKey;
            changePath();
            followers = true;
            username = Convert.ToString(listUsers.SelectedValue);
            progress = true;
            
            downloadType = 3;
            optionChoice = 2;
            userList.Clear();
            if (!bw2.IsBusy && !bw.IsBusy)
            {
                progressCounter.Value = 0;
                countSongs = 0;
                bw2.RunWorkerAsync();
            }
            if (!bw.IsBusy)
            {
                txtDownloading.Text = "";
            }
            
        }

        public List<string> userNums = new List<string>();

        private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            progress = true;
            changePath();
            foreach (string stuff in listUsers.Items)
            {
                userList.Add(stuff);
            }
            all = true;
            downloadType = 4;
            optionChoice = 3;
            
            if (!bw2.IsBusy && !bw.IsBusy)
            {
                foreach (string stuff in listUsers.Items)
                {
                    userList.Add(stuff);
                }
                txtCalculate.Text = "";
                progressCounter.Value = 0;
                countSongs = 0;
                bw2.RunWorkerAsync();
            }
            if (!bw.IsBusy)
            {
                txtDownloading.Text = "";
            }
        }
        public bool progress = true;
        public double progressIncrementValue;
        public bool done;
        public bool likes, followers, tracks, all;
        public int number2;
        public string username;
        private void bw_DoWork2(object sender, DoWorkEventArgs e)
        {
            countSongs = 0;
            done = false;
            BackgroundWorker worker2 = sender as BackgroundWorker;

            while (progress == true)
            {
                if (all == true)
                {
                    userList.Clear();

                    foreach (string stuff in listUsers.Items)
                    {
                        userList.Add(stuff);
                    }
                    System.Threading.Thread.Sleep(100);
                    foreach (string users in userList)
                    {
                        XmlDocument doc3 = new XmlDocument();
                        try
                        {
                            ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                            doc3.Load(ResolveUrl);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The Username You Entered Is Invalid.");
                            done = true;
                        }
                        userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;
                        XmlDocument doc = new XmlDocument();
                        string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                        doc.Load(UserUrlFollowings);


                        foreach (XmlNode child2 in doc.DocumentElement)
                        {

                            XmlDocument doc2 = new XmlDocument();
                            string trackid = child2.SelectSingleNode("id").InnerText;
                            UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                            doc2.Load(UserUrl);

                            foreach (XmlNode child in doc2.DocumentElement)
                            {
                                string candownload = child.SelectSingleNode("downloadable").InnerText;

                                if (candownload == "true")
                                {
                                    countSongs++;
                                    System.Threading.Thread.Sleep(100);
                                    worker2.ReportProgress(number2++);
                                }
                            }
                        }
                    }
                }
                else if (likes == true)
                {
                    userList.Clear();

                    foreach (string stuff in listUsers.Items)
                    {
                        userList.Add(stuff);
                    }
                    System.Threading.Thread.Sleep(100);

                    foreach (string users in userList)
                    {
                        XmlDocument doc3 = new XmlDocument();
                        try
                        {
                            ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                            doc3.Load(ResolveUrl);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The Username You Entered Is Invalid.");
                            done = true;
                        }

                        userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                        XmlDocument doc = new XmlDocument();
                        UserUrl = "http://api.soundcloud.com/users/" + userid + "/favorites.xml?client_id=" + Settings.Default.apiKey;
                        doc.Load(UserUrl);

                        foreach (XmlNode child in doc.DocumentElement)
                        {

                            string candownload = child.SelectSingleNode("downloadable").InnerText;

                            if (candownload == "true")
                            {
                                countSongs++;
                                System.Threading.Thread.Sleep(100);
                                worker2.ReportProgress(number2++);
                            }
                        }
                    }
                }
                else if (tracks == true)
                {
                    userList.Clear();

                    foreach (string stuff in listUsers.Items)
                    {
                        userList.Add(stuff);
                    }
                    System.Threading.Thread.Sleep(100);
                    foreach (string users in userList)
                    {
                        XmlDocument doc3 = new XmlDocument();
                        try
                        {
                            ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                            doc3.Load(ResolveUrl);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The Username You Entered Is Invalid.");
                            done = true;
                        }

                        userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                        XmlDocument doc = new XmlDocument();


                        UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                        doc.Load(UserUrl);
                        foreach (XmlNode child in doc.DocumentElement)
                        {

                            string candownload = child.SelectSingleNode("downloadable").InnerText;

                            if (candownload == "true")
                            {
                                countSongs++;
                                System.Threading.Thread.Sleep(100);
                                worker2.ReportProgress(number2++);
                            }
                        }
                    }
                }
                else if (followers == true)
                {
                    userList.Clear();

                    foreach (string stuff in listUsers.Items)
                    {
                        userList.Add(stuff);
                    }
                    System.Threading.Thread.Sleep(100);
                    foreach (string users in userList)
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlDocument doc3 = new XmlDocument();
                        try
                        {
                            ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                            doc3.Load(ResolveUrl);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("The Username You Entered Is Invalid.");
                            done = true;
                        }
                        userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                        string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                        doc.Load(UserUrlFollowings);


                        foreach (XmlNode child2 in doc.DocumentElement)
                        {

                            XmlDocument doc2 = new XmlDocument();
                            string trackid = child2.SelectSingleNode("id").InnerText;
                            UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                            doc2.Load(UserUrl);

                            foreach (XmlNode child in doc2.DocumentElement)
                            {
                                string candownload = child.SelectSingleNode("downloadable").InnerText;

                                if (candownload == "true")
                                {
                                    countSongs++;
                                    System.Threading.Thread.Sleep(100);
                                    worker2.ReportProgress(number2++);
                                }
                            }
                        }
                    }
                }

                progressIncrementValue = 100 / countSongs;
                progress = false;
                done = true;
                likes = false;
                tracks = false;
                followers = false;
                all = false;
                username = "";
                worker2.ReportProgress(number2++);
                bw2.CancelAsync();
                bw2.Dispose();
                  
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            listUsers.Items.Remove(listUsers.SelectedItem);
        }

        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog pickDirectory = new System.Windows.Forms.FolderBrowserDialog();

            pickDirectory.ShowDialog();

            Settings.Default.chooseDirectory = pickDirectory.SelectedPath;
            Settings.Default.Save();
            txtDirectory.Text = Settings.Default.chooseDirectory;
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtDownloading.IsReadOnly = true;

            txtDirectory.Text = Settings.Default.chooseDirectory;
            changePath();
        }

        private void txtAddUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAdd_Click(null, null);
            }
        }  
    }
}
     

