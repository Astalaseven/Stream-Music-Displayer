﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Serialization;

namespace SMD
{
    public partial class Main : Form
    {

        PlayerList Plist;
        Player currentPlayer;
        Song currentSong;

        Finder f = new Finder();

        public Main()
        {
            if (!File.Exists("Players.xml"))
            {
                using (StreamWriter outfile = new StreamWriter("Players.xml"))
                {
                    outfile.Write(Properties.Resources.Players);
                }
            }

            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            toolStripSeparator2.Visible = false;
            currentSonglb.Visible = false;
            currentsongToolStripMenuItem.Visible = false;

            timer.Interval = Properties.Settings.Default.RefeshRate * 1000;
            UpdateList();

            if (PlayerSelect.FindString(Properties.Settings.Default.LastPlayer) > -1)
                PlayerSelect.SelectedIndex = PlayerSelect.FindString(Properties.Settings.Default.LastPlayer);

            PlayerSelect_SelectionChangeCommitted(PlayerSelect, new EventArgs());

            foreach (Player Plr in Plist.Players)
            {
                if (Properties.Settings.Default.LastPlayer == Plr.Name)
                    currentPlayer = Plr;
            }
            
        }

        bool notify_st = true;
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
            if (notify_st)
            {
                notify("Stream Music Displayer was minimised to system tray, click the icon to restore it.");
                notify_st = false;
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options o = new Options();
            o.ShowDialog();
            UpdateList();
            timer.Interval = Properties.Settings.Default.RefeshRate * 1000;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void toggle_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                currentSong = null;

                toolStripSeparator2.Visible = false;
                currentSonglb.Visible = false;
                currentsongToolStripMenuItem.Visible = false;

                timer.Enabled = false;
                toggle.Text = "Start";
                File.WriteAllText(Properties.Settings.Default.OutputFile, "");
            }
            else
            {
                toolStripSeparator2.Visible = true;
                currentSonglb.Visible = true;
                currentsongToolStripMenuItem.Visible = true;

                timer.Enabled = true;
                toggle.Text = "Stop";
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (currentPlayer != null)
            {
                Song s = f.get(currentPlayer.Type);

                if (currentSong == null || s.Title != currentSong.Title)
                {
                    currentSong = s;

                    if (String.IsNullOrEmpty(s.Album))
                    {
                        currentSonglb.Text = String.Format("Current Song: {0} by {1} ", s.Title, s.Artist);
                        currentsongToolStripMenuItem.Text = String.Format("Current Song: {0} by {1}", s.Title, s.Artist);
                    }
                    else if (String.IsNullOrEmpty(s.Artist))
                    {
                        currentSonglb.Text = String.Format("Current Song: {0}", s.Title);
                        currentsongToolStripMenuItem.Text = String.Format("Current Song: {0}", s.Title);
                    }
                    else
                    {
                        currentSonglb.Text = String.Format("Current Song: {0} by {1} (Album: {2})", s.Title, s.Artist, s.Album);
                        currentsongToolStripMenuItem.Text = String.Format("Current Song: {0} by {1} (Album: {2})", s.Title, s.Artist, s.Album);
                    }

                    write_song(s);
                }
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.Visible)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                }
            }
        }

        private void PlayerSelect_SelectionChangeCommitted(object sender, EventArgs e)
        {
            foreach (Player Plr in Plist.Players)
            {
                if (PlayerSelect.SelectedItem.ToString() == Plr.Name)
                    currentPlayer = Plr;
            }

            Properties.Settings.Default.LastPlayer = PlayerSelect.SelectedItem.ToString();
            Properties.Settings.Default.Save();

            switch (currentPlayer.Type)
            {
                case MusicPlayers.Foobar2000:
                    infolb.Text = "Shows song playing in Foobar2000.";
                    playerLink.Text = "Open Foobar2000";
                    break;
                case MusicPlayers.AIMP3:
                    infolb.Text = "Shows song playing in AIMP3.";
                    playerLink.Text = "Open AIMP3";
                    break;
                case MusicPlayers.MediaMonkey:
                    infolb.Text = "Shows song playing in Media Monkey.";
                    playerLink.Text = "Open MediaMonkey";
                    break;
                case MusicPlayers.MPC:
                    infolb.Text = "Shows song playing in Media Player Classic Home Cinema (mpc-hc).";
                    playerLink.Text = "Open mpc-hc";
                    break;
                case MusicPlayers.Winamp:
                    infolb.Text = "Shows song playing in Winamp.";
                    playerLink.Text = "Open Winamp";
                    break;
                case MusicPlayers.VLC:
                    infolb.Text = "Shows song playing in VLC.";
                    playerLink.Text = "Open VLC";
                    break;
                case MusicPlayers.Spotify:
                    infolb.Text = "Shows song playing in Spotify.";
                    playerLink.Text = "Open Spotify";
                    break;
                case MusicPlayers.Youtube:
                    infolb.Text = "Shows song playing in Youtube. Must be active tab in Chrome or Firefox and requires applet to run correctly.";
                    playerLink.Text = "Open Youtube";
                    break;
            }
        }

        private void playerLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
            
            switch (currentPlayer.Type)
            {
                case MusicPlayers.Foobar2000:
                    Process.Start("Foobar2000");
                    break;
                case MusicPlayers.AIMP3:
                    Process.Start("AIMP3");
                    break;
                case MusicPlayers.MediaMonkey:
                    Process.Start("MediaMonkey");
                    break;
                case MusicPlayers.MPC:
                    Process.Start("mpc-hc");
                    break;
                case MusicPlayers.Winamp:
                    Process.Start("Winamp");
                    break;
                case MusicPlayers.VLC:
                    Process.Start(@"C:\Program Files\VideoLAN\VLC\vlc.exe");
                    break;
                case MusicPlayers.Spotify:
                    Process.Start("spotify");
                    break;
                case MusicPlayers.Grooveshark:
                    Process.Start("http://grooveshark.com/");
                    break;
                case MusicPlayers.Youtube:
                    Process.Start("https://www.youtube.com/");
                    break;
            }
            }
            catch (Exception ex) {MessageBox.Show("Could not start " + currentPlayer.Type.ToString()); }
        }

        private void helplink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://obsproject.com/forum/resources/stream-music-displayer.107/");
            }
            catch (Exception ex) { }
        }

        private void notify(string text)
        {
            notifyIcon.BalloonTipTitle = "Stream Music Displayer";
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(1000);
        }

        private void write_song(Song song)
        {
            if (song.Title != "None")
            {
                string ifreg = @"(?<=\[if\()(.*?)\)\](.*?)\[\/if\]";
                string output = Properties.Settings.Default.OutFormat;

                for (int i = 0; i < 4; i++)
                {
                    Regex r = new Regex(ifreg, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    Match m = r.Match(output);

                    if (m.Success)
                    {
                        if (m.Groups[1].ToString().Contains("[Artist]"))
                        {
                            if (String.IsNullOrEmpty(song.Artist))
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", "");
                            else
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", m.Groups[2].ToString().Replace("[Artist]", song.Artist));
                        }
                        else if (m.Groups[1].ToString().Contains("[Title]"))
                        {
                            if (String.IsNullOrEmpty(song.Title))
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", "");
                            else
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", m.Groups[2].ToString().Replace("[Title]", song.Title));
                        }
                        else if (m.Groups[1].ToString().Contains("[Album]"))
                        {
                            if (String.IsNullOrEmpty(song.Album))
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", "");
                            else
                                output = output.Replace("[if(" + m.Groups[1].ToString() + ")]" + m.Groups[2].ToString() + "[/if]", m.Groups[2].ToString().Replace("[Album]", song.Album));
                        }
                    }
                }

                output = output.Replace("[Artist]", song.Artist);
                output = output.Replace("[Title]", song.Title);
                output = output.Replace("[Album]", song.Album);
                output = output.Replace("  ", " ");
                output = output.Replace("<br/>", "\n\r");

                try
                {
                    File.WriteAllText(Properties.Settings.Default.OutputFile, output);
                }
                catch (Exception ex) { }
            }
            else
            {
                try
                {
                    File.WriteAllText(Properties.Settings.Default.OutputFile, "");
                }
                catch (Exception ex) { }
            }
        }

        private void UpdateList()
        {
            Plist = null;
            using (Stream s = File.OpenRead("Players.xml"))
            {
                Plist = (PlayerList)new XmlSerializer(typeof(PlayerList)).Deserialize(s);
            }

            PlayerSelect.Items.Clear();

            foreach (Player p in Plist.Players)
            {
                if(p.Enabled)
                PlayerSelect.Items.Add(p.Name);
            }
        }
    }
}
