using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace Satoshi_City_launcher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
        downloadingAvailable
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;

        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Content = "PLAY";
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = "UPDATE FAILED - RETRY";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = "DOWNLOADING GAME";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = "DOWNLOADING UPDATE";
                        break;
                    case LauncherStatus.downloadingAvailable:
                        PlayButton.Content = "DOWNLOAD AVAILABLE";
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SetActu();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Windows.zip");
            gameExe = Path.Combine(rootPath, "Windows", "War_Thugz_V0_ToP.exe");
        }

        public void SetActu()
        {
            
            string url = "https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161890&authkey=ANbGfDk7Db8qnNk";
            using(WebClient client = new WebClient()) 
            {
                string actu = client.DownloadString(url);
                if (actu != String.Empty)
                {
                    DescriptionActu.Text = actu;
                }
            }
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                Versiontext.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161889&authkey=AInUgaQtd8HnUT4"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        Status = LauncherStatus.downloadingAvailable;

                        /**InstallGameFiles(true, onlineVersion);*/
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex) 
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error Checking for game updates: {ex}");
                }
            }
            else
            {
                Status = LauncherStatus.downloadingAvailable;

                /**InstallGameFiles(false, Version.zero);*/
            }
        }

        //récuperation des sauvegarde de jeu
        public static void CopyDirectory(string sourceDir, string destDir)
        {
                // Crée le répertoire de destination s'il n'existe pas
                DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (dir.Exists)
            {



                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(destDir);

                // Obtient les fichiers dans le répertoire et les copie dans le répertoire de destination
                foreach (FileInfo file in dir.GetFiles())
                {
                    string tempPath = Path.Combine(destDir, file.Name);
                    file.CopyTo(tempPath, true);
                }

                // Si le répertoire source a des sous-dossiers, les copie récursivement
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subdir.Name);
                    CopyDirectory(subdir.FullName, tempPath);
                }
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate) 
                {
                    Status = LauncherStatus.downloadingUpdate;
                    CopyDirectory("Windows\\War_Thugz_V0_ToP\\Saved\\SaveGames", "SaveGames");
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161889&authkey=AInUgaQtd8HnUT4"));
                }
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCalback);
                webClient.DownloadFileAsync(new Uri("https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161891&authkey=AGfX65DqGpvP7Ko"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error Installing the game : {ex}");
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void DownloadGameCompletedCalback(object sender, AsyncCompletedEventArgs e)
        {
            try 
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);
                

                File.WriteAllText(versionFile, onlineVersion);

                Versiontext.Text = onlineVersion;
                CopyDirectory("SaveGames" , "Windows\\War_Thugz_V0_ToP\\Saved\\SaveGames");
                Status = LauncherStatus.ready;
            }
            catch(Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates(); 
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            Version onlineVersion = new Version(webClient.DownloadString("https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161889&authkey=AInUgaQtd8HnUT4"));
            if (File.Exists(gameExe) && Status == LauncherStatus.ready) 
            {
                ProcessStartInfo startInfo= new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory= Path.Combine(rootPath, "Windows");
                Process.Start(startInfo);

                Close();
            }
            else if(Status == LauncherStatus.failed) 
            {
                CheckForUpdates();
            }
            else if (File.Exists(versionFile) && Status == LauncherStatus.downloadingAvailable){
                InstallGameFiles(true, onlineVersion);
            }
            else if (Status == LauncherStatus.downloadingAvailable)
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void Mint_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://mint.thugz.life/")
            {
                UseShellExecute = true
            });
        }

        private void magicEden_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://magiceden.io/marketplace/thugzlife")
            {
                UseShellExecute = true
            });
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://discord.com/invite/pJ86xvC8NA")
            {
                UseShellExecute = true
            });
        }

        private void Facebook_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.facebook.com/Thugz.life.NFT/")
            {
                UseShellExecute = true
            });
        }

        private void Instagram_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.instagram.com/thugz.life.nft/")
            {
                UseShellExecute = true
            });
        }

        private void Twitch_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.twitch.tv/thugz_life")
            {
                UseShellExecute = true
            });
        }

        private void Twitter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://twitter.com/Thugz_NFT")
            {
                UseShellExecute = true
            });
        }

        private void Youtube_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.youtube.com/@Thugz_Life")
            {
                UseShellExecute = true
            });
        }

        private void Site_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.thugz.life/")
            {
                UseShellExecute = true
            });
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short SubMinor;

        internal Version(short _major, short _minor, short _SubMinor)
        {
            major = _major;
            minor = _minor;
            SubMinor = _SubMinor;

        }

        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');
            if (_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                SubMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            SubMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (SubMinor != _otherVersion.SubMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{SubMinor}";
        }
    }
}
