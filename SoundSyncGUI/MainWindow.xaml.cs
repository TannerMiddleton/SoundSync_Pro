using SoundSyncGUI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidReStr, "_");
        }

        private static bool Download(string strUrl, string folderstructure, WebClient client)
        {
            try
            {
                client.DownloadFile(strUrl, folderstructure);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static XmlDocument doc = new XmlDocument();
        private static XmlDocument doc2 = new XmlDocument();
        private static XmlDocument doc3 = new XmlDocument();
        private static Regex r = new Regex("[\\~#%&*{}/:<>?|\"-]");

        private string title, trackid, trackid2, genre, artist, downloadurl, filename, folderstructure;
        private int count;
        private double progressIncrementValue;

        private void btnDownloadLikes_Click(object sender, RoutedEventArgs e)
        {
            txtDownloading.Text = "";
            progressCounter.Value = 0;
            DisableButtons();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!CountLikes())
                    {
                        return;
                    }

                    foreach (string users in listUsers.Items)
                    {
                        string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                        doc3.Load(ResolveUrl);
                        string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                        string UserUrl = "http://api.soundcloud.com/users/" + userid + "/favorites.xml?client_id=" + Settings.Default.apiKey;

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

                                //Replace illegal folder characters with sanitized string
                                if (r.IsMatch(artist))
                                {
                                    artist = Regex.Replace(r.Replace(artist, " "), @"\s+", " ");
                                }

                                folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                                if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                                if (File.Exists(folderstructure + @"\" + filename))
                                {
                                    count--;
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                    {
                                        txtDownloading.Text += "File Exists: " + title + Environment.NewLine;
                                        txtCalculate.Text = count.ToString();
                                        if (progressIncrementValue <= 100)
                                        {
                                            progressCounter.Value += progressIncrementValue;
                                        }
                                    }));
                                    System.Threading.Thread.Sleep(20);
                                }
                                else
                                {
                                    WebClient client = new WebClient();

                                    int intFailedCount = 0;
                                    bool isDownloaded = false;
                                    do
                                    {

                                        isDownloaded = Download(downloadurl, folderstructure + @"\" + filename, client);

                                        if (!isDownloaded && intFailedCount < 10)
                                        {
                                            intFailedCount++;
                                        }
                                        else if (intFailedCount >= 10)
                                        {
                                            break;
                                        }

                                    } while (!isDownloaded);

                                    count--;

                                    //Use this to update the UI
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                    {
                                        txtCalculate.Text = Convert.ToString(count + " Songs");

                                        if (progressIncrementValue <= 100)
                                        {
                                            progressCounter.Value += progressIncrementValue;
                                        }

                                        txtDownloading.Text += "Downloading " + title + Environment.NewLine;

                                        txtDownloading.ScrollToEnd();

                                    }));

                                    System.Threading.Thread.Sleep(20);
                                }
                            }
                        }
                    }
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        txtDownloading.Text += "All Downloads Complete." + Environment.NewLine;
                        count = 0;
                        progressCounter.Value = 100;
                        txtCalculate.Text = Convert.ToString(count) + " Songs";
                        txtDownloading.ScrollToEnd();
                        EnableButtons();
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        EnableButtons();
                    }));
                }
            });
        }

        private void btnDownloadTracks_Click(object sender, RoutedEventArgs e)
        {
            txtDownloading.Text = "";
            txtCalculate.Text = "0";
            progressCounter.Value = 0;
            DisableButtons();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (!CountTracks())
                        {
                            return;
                        }

                        foreach (string users in listUsers.Items)
                        {
                            string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                            doc3.Load(ResolveUrl);
                            string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                            string UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;

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

                                    //Replace illegal folder characters with sanitized string
                                    if (r.IsMatch(artist))
                                    {
                                        artist = Regex.Replace(r.Replace(artist, " "), @"\s+", " ");
                                    }

                                    folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                                    if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                                    if (File.Exists(folderstructure + @"\" + filename))
                                    {
                                        count--;
                                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                        {
                                            txtDownloading.Text += "File Exists: " + title + Environment.NewLine;
                                            txtCalculate.Text = count.ToString();
                                            if (progressIncrementValue <= 100)
                                            {
                                                progressCounter.Value += progressIncrementValue;
                                            }
                                        }));

                                        System.Threading.Thread.Sleep(20);

                                    }
                                    else
                                    {
                                        WebClient client = new WebClient();

                                        int intFailedCount = 0;
                                        bool isDownloaded = false;
                                        do
                                        {

                                            isDownloaded = Download(downloadurl, folderstructure + @"\" + filename, client);

                                            if (!isDownloaded && intFailedCount < 10)
                                            {
                                                intFailedCount++;
                                            }
                                            else if (intFailedCount >= 10)
                                            {
                                                break;
                                            }

                                        } while (!isDownloaded);

                                        count--;

                                        //Use this to update the UI
                                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                        {
                                            txtCalculate.Text = Convert.ToString(count + " Songs");

                                            if (progressIncrementValue <= 100)
                                            {
                                                progressCounter.Value += progressIncrementValue;
                                            }

                                            txtDownloading.Text += "Downloading " + title + Environment.NewLine;

                                            txtDownloading.ScrollToEnd();

                                        }));

                                        System.Threading.Thread.Sleep(20);
                                    }
                                }
                            }
                        }
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                           {
                               txtDownloading.Text += "All Downloads Complete." + Environment.NewLine;
                               count = 0;
                               progressCounter.Value = 100;
                               txtCalculate.Text = Convert.ToString(count) + " Songs";
                               txtDownloading.ScrollToEnd();
                               EnableButtons();
                           }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            EnableButtons();
                        }));
                    }
                });
        }

        private void btnDownloadFollowers_Click(object sender, RoutedEventArgs e)
        {
            txtDownloading.Text = "";
            txtCalculate.Text = "0";
            progressCounter.Value = 0;
            DisableButtons();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (!CountFollowersTracks())
                        {
                            return;
                        }

                        foreach (string users in listUsers.Items)
                        {
                            string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;

                            doc3.Load(ResolveUrl);
                            string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                            string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                            doc.Load(UserUrlFollowings);

                            foreach (XmlNode child2 in doc.DocumentElement)
                            {
                                string trackid = child2.SelectSingleNode("id").InnerText;
                                string UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
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

                                        //Replace illegal folder characters with sanitized string
                                        if (r.IsMatch(artist))
                                        {
                                            artist = Regex.Replace(r.Replace(artist, " "), @"\s+", " ");
                                        }

                                        folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                                        if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                                        if (File.Exists(folderstructure + @"\" + filename))
                                        {
                                            count--;
                                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                            {
                                                txtDownloading.Text += "File Exists: " + title + Environment.NewLine;
                                                txtCalculate.Text = count.ToString();
                                                if (progressIncrementValue <= 100)
                                                {
                                                    progressCounter.Value += progressIncrementValue;
                                                }
                                            }));

                                            System.Threading.Thread.Sleep(20);
                                        }
                                        else
                                        {
                                            WebClient client = new WebClient();

                                            int intFailedCount = 0;
                                            bool isDownloaded = false;
                                            do
                                            {

                                                isDownloaded = Download(downloadurl, folderstructure + @"\" + filename, client);

                                                if (!isDownloaded && intFailedCount < 10)
                                                {
                                                    intFailedCount++;
                                                }
                                                else if (intFailedCount >= 10)
                                                {
                                                    break;
                                                }

                                            } while (!isDownloaded);

                                            count--;
                                            //Use this to update the UI
                                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                            {
                                                txtCalculate.Text = Convert.ToString(count + " Songs");

                                                if (progressIncrementValue <= 100)
                                                {
                                                    progressCounter.Value += progressIncrementValue;
                                                }

                                                txtDownloading.Text += "Downloading " + title + Environment.NewLine;

                                                txtDownloading.ScrollToEnd();

                                            }));

                                            System.Threading.Thread.Sleep(20);
                                        }
                                    }
                                }
                            }
                        }
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            txtDownloading.Text += "All Downloads Complete." + Environment.NewLine;
                            count = 0;
                            progressCounter.Value = 100;
                            txtCalculate.Text = Convert.ToString(count) + " Songs";
                            txtDownloading.ScrollToEnd();
                            EnableButtons();
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            EnableButtons();
                        }));
                    }
                });
        }

        private bool CountLikes()
        {
            foreach (string users in listUsers.Items)
            {
                try
                {
                    string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                    doc3.Load(ResolveUrl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The Username You Entered Is Invalid.");

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        EnableButtons();
                    }));
                    return false;
                }

                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrl = "http://api.soundcloud.com/users/" + userid + "/favorites.xml?client_id=" + Settings.Default.apiKey;
                doc.Load(UserUrl);

                foreach (XmlNode child in doc.DocumentElement)
                {

                    string candownload = child.SelectSingleNode("downloadable").InnerText;

                    if (candownload == "true")
                    {
                        count++;

                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            txtCalculate.Text = count.ToString();
                        }));
                    }
                }
            }
            if (count > 0)
            {
                progressIncrementValue = 100 / count;
            }
            return true;
        }

        private bool CountTracks()
        {
            foreach (string users in listUsers.Items)
            {
                try
                {
                    string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                    doc3.Load(ResolveUrl);
                }
                catch (Exception)
                {
                    MessageBox.Show("The Username You Entered Is Invalid.");

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        EnableButtons();
                    }));
                    return false;
                }

                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                doc.Load(UserUrl);
                foreach (XmlNode child in doc.DocumentElement)
                {
                    string candownload = child.SelectSingleNode("downloadable").InnerText;

                    if (candownload == "true")
                    {
                        count++;

                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            txtCalculate.Text = count.ToString();
                        }));
                    }
                }
            }
            if (count > 0)
            {
                progressIncrementValue = 100 / count;
            }
            return true;
        }

        private bool CountFollowersTracks()
        {
            foreach (string users in listUsers.Items)
            {
                try
                {
                    string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + users + "/&client_id=" + Settings.Default.apiKey;
                    doc3.Load(ResolveUrl);
                }
                catch (Exception)
                {
                    MessageBox.Show("The Username You Entered Is Invalid.");

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        EnableButtons();
                    }));
                    return false;
                }

                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                doc.Load(UserUrlFollowings);

                foreach (XmlNode child2 in doc.DocumentElement)
                {
                    string trackid = child2.SelectSingleNode("id").InnerText;
                    string UserUrl = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                    doc2.Load(UserUrl);

                    foreach (XmlNode child in doc2.DocumentElement)
                    {
                        string candownload = child.SelectSingleNode("downloadable").InnerText;

                        if (candownload == "true")
                        {
                            count++;

                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                txtCalculate.Text = count.ToString();
                            }));
                        }
                    }
                }
            }
            if (count > 0)
            {
                progressIncrementValue = 100 / count;
            }
            return true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtAddUser.Text))
            {
                listUsers.Items.Add(txtAddUser.Text);
                txtAddUser.Text = ""; 
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            listUsers.Items.Remove(listUsers.SelectedItem);
            listUsers.SelectedIndex = 0;
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtDownloading.IsReadOnly = true;

            txtDirectory.Text = Settings.Default.chooseDirectory;
            changePath();
        }

        public void changePath()
        {
            if (Settings.Default.chooseDirectory == "")
            {
                MessageBox.Show("Please Choose A Directory To Download Music To.");
                btnDirectory_Click(null, null);
            }
        }

        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog pickDirectory = new System.Windows.Forms.FolderBrowserDialog();

            pickDirectory.ShowDialog();

            Settings.Default.chooseDirectory = pickDirectory.SelectedPath;
            Settings.Default.Save();
            txtDirectory.Text = Settings.Default.chooseDirectory;
        }

        private void txtAddUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAdd_Click(null, null);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            mainWindow_Closing(null, null);
        }

        private void DisableButtons()
        {
            btnDownloadFollowers.IsEnabled = false;
            btnDownloadLikes.IsEnabled = false;
            btnDownloadTracks.IsEnabled = false;
            btnAdd.IsEnabled = false;
            btnRemove.IsEnabled = false;
            btnDirectory.IsEnabled = false;
        }

        private void EnableButtons()
        {
            btnDownloadFollowers.IsEnabled = true;
            btnDownloadLikes.IsEnabled = true;
            btnDownloadTracks.IsEnabled = true;
            btnAdd.IsEnabled = true;
            btnRemove.IsEnabled = true;
            btnDirectory.IsEnabled = true;
        }
    }
}
     

