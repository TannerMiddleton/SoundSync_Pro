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

        private bool isDone = false;

        private void UpdateUI()
        {
            //Instantiate our class
            SoundCloud clsSound = new SoundCloud();
            try
            {
                while (!isDone)
                {
                    //Just loop this dispatcher and update the UI 
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        //If we're done then set all the necessary "finished" values
                        if (isDone)
                        {
                            clsSound.count = 0;
                            progressCounter.Value = 100;
                            txtCalculate.Text = Convert.ToString(clsSound.count) + " Songs";
                            EnableButtons();
                        }
                        else
                        {
                            progressCounter.Value = clsSound.dblProgress;
                            txtCalculate.Text = Convert.ToString(clsSound.count) + " Songs";
                        }
                    }));
                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                clsSound = null;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDownloadLikes_Click(object sender, RoutedEventArgs e)
        {
            SetUI();
            Task.Factory.StartNew(() =>
            {
                //Instantiate our class
                SoundCloud clsSound = new SoundCloud();
                try
                {
                    isDone = false;
                    clsSound.lstUser = null;
                    UpdateUI();

                    foreach (string users in listUsers.Items)
                    {
                        clsSound.lstUser.Add(users);
                    }

                    if(!clsSound.CountResults("favorites"))
                    {
                        return;
                    }

                    clsSound.DownloadLikes();
                    isDone = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    clsSound = null;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        EnableButtons();
                    }));
                }
            });
        }

        private void btnDownloadTracks_Click(object sender, RoutedEventArgs e)
        {
            SetUI();

            Task.Factory.StartNew(() =>
                {
                    SoundCloud clsSound = new SoundCloud();
                    try
                    {
                        isDone = false;
                        clsSound.lstUser = null;
                        UpdateUI();

                        foreach (string users in listUsers.Items)
                        {
                            clsSound.lstUser.Add(users);
                        }

                        if (!clsSound.CountResults("tracks"))
                        {
                            return;
                        }

                        clsSound.DownLoadTracks();
                        isDone = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        clsSound = null;
                        EnableButtons();
                    }
                });
        }

        private void btnDownloadFollowers_Click(object sender, RoutedEventArgs e)
        {
            SetUI();

            Task.Factory.StartNew(() =>
                {
                    SoundCloud clsSound = new SoundCloud();
                    try
                    {
                        isDone = false;
                        clsSound.lstUser = null;
                        UpdateUI();

                        foreach (string users in listUsers.Items)
                        {
                            clsSound.lstUser.Add(users);
                        }

                        if (!clsSound.CountResults("followings"))
                        {
                            return;
                        }

                        clsSound.DownloadFollowers();
                        isDone = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        clsSound = null;
                        EnableButtons();
                    }
                });
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

        private void SetUI()
        {
            txtCalculate.Text = "0";
            progressCounter.Value = 0;
            DisableButtons();
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                btnDownloadFollowers.IsEnabled = true;
                btnDownloadLikes.IsEnabled = true;
                btnDownloadTracks.IsEnabled = true;
                btnAdd.IsEnabled = true;
                btnRemove.IsEnabled = true;
                btnDirectory.IsEnabled = true;
            }));
        }
    }
}
     

