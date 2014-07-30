using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using SoundSyncGUI.Properties;
using System.Windows.Threading;

namespace SoundSyncGUI
{
    class SoundCloud
    {
        //Probably can rename these better later
        private static XmlDocument doc = new XmlDocument();
        private static XmlDocument doc2 = new XmlDocument();
        private static XmlDocument doc3 = new XmlDocument();
        private static Regex r = new Regex("[\\~#%&*{}/:<>?|\"-]");

        private string title, trackid, genre, artist, downloadurl, filename, folderstructure;
        public int count;
        public double progressIncrementValue;
        public double dblProgress;
        public List<string> lstUser;

        private static bool Download(string strUrl, string folderstructure, WebClient client)
        {
            int intFailedCount = 0;
            do
            {
                try
                {
                    client.DownloadFile(strUrl, folderstructure);
                    return true;

                }
                catch
                {
                    intFailedCount++;
                }
                //after ten attempts we'll just skip that track
            } while (intFailedCount < 10);
            return false;
        }

        //Go through each user in the list and download all the tracks of everyone they follow
        public void DownloadFollowers()
        {
            foreach (string strUsers in lstUser)
            {
                string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + strUsers + "/&client_id=" + Settings.Default.apiKey;

                doc3.Load(ResolveUrl);
                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrlFollowings = "http://api.soundcloud.com/users/" + userid + "/followings.xml?client_id=" + Settings.Default.apiKey;
                doc.Load(UserUrlFollowings);

                foreach (XmlNode child2 in doc.DocumentElement)
                {
                    string strUserID = child2.SelectSingleNode("id").InnerText;

                    foreach (XmlNode child in doc2.DocumentElement)
                    {
                        DownloadSingleUserTracks(strUsers);
                    }
                }
            }
        }

        //This method is only used by DownloadFollowers, needed a way to only download tracks one user at a time
        private void DownloadSingleUserTracks(string strUsers)
        {
            string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + strUsers + "/&client_id=" + Settings.Default.apiKey;

            doc3.Load(ResolveUrl);
            string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

            string UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;

            doc.Load(UserUrl);
            foreach (XmlNode child in doc.DocumentElement)
            {
                string candownload = child.SelectSingleNode("downloadable").InnerText;

                if (candownload == "true")
                {
                    GetInfoForArtist(child);
                    folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                    if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                    if (!CheckFileExists())
                    {
                        WebClient client = new WebClient();
                        Download(downloadurl, folderstructure + @"\" + filename, client);
                        count--;

                        if (progressIncrementValue <= 100)
                        {
                            dblProgress += progressIncrementValue;
                        }

                        System.Threading.Thread.Sleep(20);
                    }
                }
            }
        }

        //Go through list and download said users tracks
        public void DownLoadTracks()
        {
            foreach (string strUsers in lstUser)
            {
                string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + strUsers + "/&client_id=" + Settings.Default.apiKey;

                doc3.Load(ResolveUrl);
                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrl = "http://api.soundcloud.com/users/" + userid + "/tracks.xml?client_id=" + Settings.Default.apiKey;

                doc.Load(UserUrl);
                foreach (XmlNode child in doc.DocumentElement)
                {
                    string candownload = child.SelectSingleNode("downloadable").InnerText;

                    if (candownload == "true")
                    {
                        GetInfoForArtist(child);
                        folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                        if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                        if (!CheckFileExists())
                        {
                            WebClient client = new WebClient();
                            Download(downloadurl, folderstructure + @"\" + filename, client);
                            count--;

                            if (progressIncrementValue <= 100)
                            {
                                dblProgress += progressIncrementValue;
                            }

                            System.Threading.Thread.Sleep(20);
                        }
                    }
                }
            }
        }

        //Go through list and download said users favorites/likes
        public void DownloadLikes()
        {
            foreach (string strUsers in lstUser)
            {
                string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + strUsers + "/&client_id=" + Settings.Default.apiKey;

                doc3.Load(ResolveUrl);
                string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                string UserUrl = "http://api.soundcloud.com/users/" + userid + "/favorites.xml?client_id=" + Settings.Default.apiKey;

                doc.Load(UserUrl);
                foreach (XmlNode child in doc.DocumentElement)
                {
                    string candownload = child.SelectSingleNode("downloadable").InnerText;

                    if (candownload == "true")
                    {
                        GetInfoForArtist(child);
                        folderstructure = Settings.Default.chooseDirectory + @"\SoundCloudMusic\" + artist;
                        if (!Directory.Exists(folderstructure)) { Directory.CreateDirectory(folderstructure); }
                        if (!CheckFileExists())
                        {
                            WebClient client = new WebClient();
                            Download(downloadurl, folderstructure + @"\" + filename, client);
                            count--;

                            if (progressIncrementValue <= 100)
                            {
                                dblProgress += progressIncrementValue;
                            }

                            System.Threading.Thread.Sleep(20);
                        }
                    }
                }
            }
        }

        //Make sure the username entered is valid
        private XmlDocument ValidateUsername(string strUser)
        {
            try
            {
                string ResolveUrl = "http://api.soundcloud.com/resolve.xml?url=http://soundcloud.com/" + strUser + "/&client_id=" + Settings.Default.apiKey;
                doc3.Load(ResolveUrl);

                //URL was resolved successfully, must be a valid user
                return doc3;
            }
            catch (Exception)
            {
                doc3 = null;
                return doc3;
            }
        }

        //Call this to figure out how many songs will be downloaded (mostly just for show)
        public bool CountResults(string strType)
        {
            //Just go through and make sure username is valid
            foreach (string users in lstUser)
            {
                ValidateUsername(users);

                //If document successfully loaded then we're good!
                if (doc3 != null)
                {
                    string userid = doc3.DocumentElement.SelectSingleNode("id").InnerText;

                    string UserUrl = "http://api.soundcloud.com/users/" + userid + "/" + strType + ".xml?client_id=" + Settings.Default.apiKey;
                    doc.Load(UserUrl);

                    if (strType == "followings")
                    {
                        foreach (XmlNode child2 in doc.DocumentElement)
                        {
                            string trackid = child2.SelectSingleNode("id").InnerText;
                            string UserUrlFollower = "http://api.soundcloud.com/users/" + trackid + "/tracks.xml?client_id=" + Settings.Default.apiKey;
                            doc2.Load(UserUrlFollower);

                            foreach (XmlNode child in doc2.DocumentElement)
                            {
                                string candownload = child.SelectSingleNode("downloadable").InnerText;

                                if (candownload == "true")
                                {
                                    count++;
                                }
                            }
                        }
                    }

                    foreach (XmlNode child in doc.DocumentElement)
                    {
                        string candownload = child.SelectSingleNode("downloadable").InnerText;

                        if (candownload == "true")
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            if (count > 0)
            {
                progressIncrementValue = 100 / count;
            }
            return true;
        }

        //Make sure file doesn't already exist
        private bool CheckFileExists()
        {
            if (File.Exists(folderstructure + @"\" + filename))
            {
                count--;

                if (progressIncrementValue <= 100)
                {
                    dblProgress += progressIncrementValue;
                }
                System.Threading.Thread.Sleep(20);
                return true;

            }
            else { return false; }
        }

        //Get all the basic info for an artist
        private void GetInfoForArtist(XmlNode xmlChild)
        {
            title = xmlChild.SelectSingleNode("title").InnerText;
            trackid = xmlChild.SelectSingleNode("id").InnerText;
            genre = xmlChild.SelectSingleNode("genre").InnerText;
            artist = xmlChild.SelectSingleNode("user").SelectSingleNode("username").InnerText;
            downloadurl = xmlChild.SelectSingleNode("download-url").InnerText + "?client_id=" + Settings.Default.apiKey;
            filename = MakeValidFileName(title + ".mp3");
            folderstructure = null;

            //Replace illegal folder characters with sanitized string
            if (r.IsMatch(artist))
            {
                artist = Regex.Replace(r.Replace(artist, " "), @"\s+", " ");
            }
        }

        //Basically need to make sure file names won't make the program explode
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidReStr, "_");
        }

    }
}
