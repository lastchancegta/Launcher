using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.IO.Compression;

/*
Copyright 2019 Sushi

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

namespace Launcher
{
    public partial class Launcher : Form
    {
        private static string _version = "0.0.7";
        public static string localVersion = "0.0.0";
        public static string rageDirectory = Properties.Settings.Default.rage_directory;
        public static string gameDirectory = "\\client_resources\\game.lastchance.wtf_22005\\";
        public static string masterListDirectory = "\\client_resources\\144.76.164.23_22005\\";

        public Launcher()
        {
            InitializeComponent();
        }

        public void UpdateGame()
        {
            if (Directory.Exists(rageDirectory + gameDirectory))
            {
                Directory.Delete(rageDirectory + gameDirectory, true);
            }
            Thread thread = new Thread(() =>
            {
                Directory.CreateDirectory(rageDirectory + gameDirectory);
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri("https://lastchance.wtf/launcher/current.zip"), @rageDirectory + gameDirectory + "current.zip");
            });
            thread.Start();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                progressBar1.Show();
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                ZipFile.ExtractToDirectory(rageDirectory + gameDirectory + "current.zip", rageDirectory + gameDirectory);

                if (Directory.Exists(rageDirectory + masterListDirectory))
                {
                    Directory.Delete(rageDirectory + masterListDirectory, true);
                };
                Directory.CreateDirectory(rageDirectory + masterListDirectory);
                ZipFile.ExtractToDirectory(rageDirectory + gameDirectory + "current.zip", rageDirectory + masterListDirectory);


                File.Delete(rageDirectory + gameDirectory + "current.zip");

                MessageBox.Show("Update abgeschlossen.", "LastChance Launcher");
                progressBar1.Hide();
                button1.Enabled = true;
                Connect.Enabled = true;
                Settings.Enabled = true;
                UpdateCheck.Enabled = true;
                Connect.Enabled = true;
                button2.Enabled = true;
            });
        }

        public void UpdateLauncher()
        {
            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(updater_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri("https://lastchance.wtf/launcher/launcher.exe"), Directory.GetCurrentDirectory() + "\\launcher.exe");
            });
            thread.Start();
        }

        void updater_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                File.Move(Directory.GetCurrentDirectory() + "\\LastChance Launcher.exe", Directory.GetCurrentDirectory() + "\\LastChance Launcher.bak");
                File.Move(Directory.GetCurrentDirectory() + "\\launcher.exe", Directory.GetCurrentDirectory() + "\\LastChance Launcher.exe");
                MessageBox.Show("Update abgeschlossen. Restarte Launcher.", "LastChance Launcher");
                Application.Restart();
            });
        }

        public void SetRageDirectory()
        {
            MessageBox.Show("Bitte wähle das Installationsverzeichnis von RageMP aus.", "LastChance Launcher");
            bool legitPath = false;
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            while (!legitPath)
            {
                if (browserDialog.ShowDialog() == DialogResult.OK)
                {
                    string rageExe = Directory.GetFiles(browserDialog.SelectedPath, "ragemp_v.exe").FirstOrDefault();
                    if (rageExe != null && rageExe != "")
                    {
                        legitPath = true;
                        Properties.Settings.Default["rage_directory"] = browserDialog.SelectedPath;
                        Properties.Settings.Default.Save();
                        rageDirectory = browserDialog.SelectedPath;
                        CheckAndUpdate();
                    }
                    else
                    {
                        MessageBox.Show("Falsches Verzeichnis gewählt. RAGEMP wurde nicht gefunden.", "Fehler");
                    }
                }
            }
        }

        public bool CheckAndUpdate()
        {
            using (var client = new WebClient())
            {
                string gameserverVersion = client.DownloadString("https://lastchance.wtf/launcher/server.txt");
                localVersion = GetInstalledVersion();
                if (gameserverVersion != localVersion)
                {
                    MessageBox.Show("Update verfügbar.", "Update");
                    button1.Enabled = false;
                    Connect.Enabled = false;
                    Settings.Enabled = false;
                    UpdateCheck.Enabled = false;
                    Connect.Enabled = false;
                    button2.Enabled = false;

                    UpdateGame();
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public static void ConnectToServer()
        {
            try
            {
                Process[] runningProcesses = Process.GetProcessesByName("ragemp_v");
                if (runningProcesses.Length == 0)
                {
                    Process RAGEMP = new Process();
                    RAGEMP.StartInfo.WorkingDirectory = rageDirectory;
                    RAGEMP.StartInfo.FileName = rageDirectory + "\\ragemp_v.exe";
                    RAGEMP.StartInfo.Arguments = " rage://v/connect?ip=game.lastchance.wtf:22005";
                    RAGEMP.Start();
                }
                else
                {
                    MessageBox.Show("RAGE läuft bereits.", "Fehler");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Konnte RAGE nicht starten.", "Fehler");
            }
        }

        public static string GetInstalledVersion()
        {
            string fullPath = rageDirectory + gameDirectory;
            string versionPath = fullPath + "version.txt";

            if (File.Exists(versionPath))
            {
                string version = File.ReadAllText(versionPath);
                if (version != null && version != "")
                {
                    return version;
                }
                else
                {
                    return "0.0.0";
                }
            }
            else
            {
                return "0.0.0";
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

            if (!CheckAndUpdate())
            {
                MessageBox.Show("Kein Update verfügbar.", "Update");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetRageDirectory();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            using (var client = new WebClient())
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\LastChance Launcher.bak"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + "\\LastChance Launcher.bak");
                }
                string launcherVersion = client.DownloadString("https://lastchance.wtf/launcher/launcher.txt");
                if (launcherVersion != _version)
                {
                    MessageBox.Show("Launcher Update verfügbar. Update startet.", "LastChance Launcher");
                    UpdateLauncher();
                }
                else
                {
                    if (rageDirectory != null && rageDirectory != "")
                    {
                        CheckAndUpdate();
                    }
                    else
                    {
                        SetRageDirectory();
                    }
                }
            }
        }
        private void Connect_Click(object sender, EventArgs e)
        {
            if (!CheckAndUpdate())
            {
                ConnectToServer();
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Die aktuellen Spieldateien werden gelöscht und müssen neu heruntergeladen werden. Fortfahren?", "Cache leeren", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (Directory.Exists(rageDirectory + gameDirectory))
                {
                    Directory.Delete(rageDirectory + gameDirectory, true);
                    if (Directory.Exists(rageDirectory + masterListDirectory))
                    {
                        Directory.Delete(rageDirectory + masterListDirectory, true);
                    }
                    MessageBox.Show("Servercache geleert.", "Erfolgreich");
                }
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        public static void InstallSavegame()
        {
            string gtaFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Rockstar Games\\GTA V\\Profiles\\";
            if (!Directory.Exists(gtaFolder))
            {
                MessageBox.Show("Bevor du das 100% Savegame installieren kannst musst du GTA ein mal im Singleplayer starten und speichern lassen!", "Fehler");
                return;
            }
            string profileFolder = Directory.GetDirectories(gtaFolder).FirstOrDefault();
            if (!string.IsNullOrEmpty(profileFolder))
            {
                string oldSavegame = Directory.GetFiles(profileFolder).Where(i => i.Contains(".bak")).FirstOrDefault();
                if (!string.IsNullOrEmpty(oldSavegame))
                {
                    WebClient client = new WebClient();
                    client.DownloadFile("https://lastchance.wtf/launcher/savegame/SGTA50015", oldSavegame.Substring(oldSavegame.Length-4));
                    client.DownloadFile("https://lastchance.wtf/launcher/savegame/SGTA50015.bak", oldSavegame);
                    client.DownloadFile("https://lastchance.wtf/launcher/savegame/SGTA50015", "SGTA50015");
                    client.DownloadFile("https://lastchance.wtf/launcher/savegame/SGTA50015.bak", "SGTA50015" + ".bak");
                    MessageBox.Show("Das Savegame wurde erfolgreich installiert. Viel Spaß!", "Erfolgreich");
                }
                else
                {
                    MessageBox.Show("Bevor du das 100% Savegame installieren kannst musst du GTA ein mal im Singleplayer starten und speichern lassen!", "Fehler");
                }
            }
            else
            {
                MessageBox.Show("Bevor du das 100% Savegame installieren kannst musst du GTA ein mal im Singleplayer starten und speichern lassen!", "Fehler");

            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Möchtest du das Savegame wirklich installieren? Dein Singleplayer-Spielstand wird dabei überschrieben.", "Savegame", MessageBoxButtons.YesNo);
            if(dialog == DialogResult.Yes)
            {
                InstallSavegame();
            }
            

        }
    }
}
