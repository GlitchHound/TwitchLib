﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib;
using System.IO;
using System.Net;

namespace TwitchLibExample
{
    public partial class Form1 : Form
    {
        List<TwitchChatClient> chatClients = new List<TwitchChatClient>();
        List<TwitchWhisperClient> whisperClients = new List<TwitchWhisperClient>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(File.Exists("credentials.txt"))
            {
                StreamReader file = new StreamReader("credentials.txt");
                string twitchUser = file.ReadLine();
                string twitchOAuth = file.ReadLine();
                string twitchChannel = file.ReadLine();
                textBox4.Text = twitchUser;
                textBox6.Text = twitchUser;
                textBox5.Text = twitchOAuth;
                textBox7.Text = twitchOAuth;
                textBox8.Text = twitchChannel;
                textBox14.Text = twitchChannel;
                textBox15.Text = twitchOAuth;
            }
            this.Height = 640;
            MessageBox.Show("This application is intended to demonstrate basic functionality of TwitchLib.\n\n-swiftyspiffy");
        }

        private string TestReplace(MessageEmote caller)
        {
            string url = MessageEmote.SourceMatchingReplacementText(caller);
            if (!url.Equals(caller.Text)) return String.Format("<img src='{0}'/>", url);
            return url;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(ConnectionCredentials.ClientType.Chat, new TwitchIpAndPort(textBox8.Text, true),
                textBox4.Text, textBox5.Text);
            TwitchChatClient newClient = new TwitchChatClient(textBox8.Text, credentials, '!');
            //newClient.WillReplaceEmotes = true;
            //MessageEmote.ReplacementDelegate = TestReplace;
            //List<MessageEmote> ffz = new List<MessageEmote>();
            //MessageEmote catbag = new MessageEmote("25927", "CatBag", MessageEmote.EmoteSource.FrankerFaceZ);
            //MessageEmote yoohoo = new MessageEmote("6", "YooHoo", MessageEmote.EmoteSource.FrankerFaceZ);
            //MessageEmote zreknarf = new MessageEmote("27081", "ZreknarF", MessageEmote.EmoteSource.FrankerFaceZ);
            //ffz.Add(catbag);
            //ffz.Add(yoohoo);
            //ffz.Add(zreknarf);
            //newClient.ChannelEmotes.Merge(ffz);
            //newClient.ChannelEmotes.Remove(catbag);
            //newClient.OnMessageReceived += new EventHandler<TwitchChatClient.OnMessageReceivedArgs>(globalChatMessageReceivedEmote);
            newClient.OnMessageReceived += new EventHandler<TwitchChatClient.OnMessageReceivedArgs>(globalChatMessageReceived);
            newClient.OnCommandReceived += new EventHandler<TwitchChatClient.OnCommandReceivedArgs>(chatCommandReceived);
            newClient.OnIncorrectLogin += new EventHandler<TwitchChatClient.OnIncorrectLoginArgs>(incorrectChatLogin);
            newClient.OnConnected += new EventHandler<TwitchChatClient.OnConnectedArgs>(onChatConnected);
            newClient.Connect();
            chatClients.Add(newClient);
            ListViewItem lvi = new ListViewItem();
            lvi.Text = textBox4.Text;
            lvi.SubItems.Add("CHAT");
            lvi.SubItems.Add(textBox8.Text);
            listView1.Items.Add(lvi);

            if(!comboBox2.Items.Contains(textBox4.Text))
                comboBox2.Items.Add(textBox4.Text);


        }

        private void button3_Click(object sender, EventArgs e)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(ConnectionCredentials.ClientType.Whisper, new TwitchIpAndPort(true),
                textBox6.Text, textBox7.Text);
            TwitchWhisperClient newClient = new TwitchWhisperClient(credentials, '!');
            newClient.OnWhisperReceived += new EventHandler<TwitchWhisperClient.OnWhisperReceivedArgs>(globalWhisperReceived);
            newClient.OnCommandReceived += new EventHandler<TwitchWhisperClient.OnCommandReceivedArgs>(whisperCommandReceived);
            newClient.OnIncorrectLogin += new EventHandler<TwitchWhisperClient.OnIncorrectLoginArgs>(incorrectWhisperLogin);
            newClient.OnConnected += new EventHandler<TwitchWhisperClient.OnConnectedArgs>(onWhisperConnected);
            newClient.Connect();
            whisperClients.Add(newClient);
            ListViewItem lvi = new ListViewItem();
            lvi.Text = textBox6.Text;
            lvi.SubItems.Add("WHISPER");
            lvi.SubItems.Add("N/A");
            listView1.Items.Add(lvi);
            comboBox1.Items.Add(textBox6.Text);
        }

        public void onWhisperConnected(object sender, TwitchWhisperClient.OnConnectedArgs e)
        {
            MessageBox.Show("Connected to whisper group chat under username: " + e.Username);
        }

        public void onChatConnected(object sender, TwitchChatClient.OnConnectedArgs e)
        {
            MessageBox.Show("Connected to channel: " + e.Channel + "\nUnder username: " + e.Username);
        }

        public void incorrectChatLogin(object sender, TwitchChatClient.OnIncorrectLoginArgs e)
        {
            MessageBox.Show("Failed login as chat client!!!\nException: " + e.Exception + "\nUsername: " + e.Exception.Username);
        }

        public void incorrectWhisperLogin(object sender, TwitchWhisperClient.OnIncorrectLoginArgs e)
        {
            MessageBox.Show("Failed login as whisper client!!!\nException: " + e.Exception + "\nUsername: " + e.Exception.Username);
        }

        private void chatCommandReceived(object sender, TwitchChatClient.OnCommandReceivedArgs e)
        {
            listBox1.Items.Add(e.ChatMessage.Username + ": " + e.Command + "; args: " + e.ArgumentsAsString + ";");
            foreach(string arg in e.ArgumentsAsList)
            {
                Console.WriteLine("[chat] arg: " + arg);
            }
            Console.WriteLine("[chat] args as string: " + e.ArgumentsAsString);
        }

        private void whisperCommandReceived(object sender, TwitchWhisperClient.OnCommandReceivedArgs e)
        {
            listBox2.Items.Add(e.Username + ": " + e.Command + "; args: " + e.ArgumentsAsString + ";");
            foreach (string arg in e.ArgumentsAsList)
            {
                Console.WriteLine("[whisper] arg: " + arg);
            }
            Console.WriteLine("[whisper] args as string: " + e.ArgumentsAsString);
        }

        private void globalChatMessageReceived(object sender, TwitchChatClient.OnMessageReceivedArgs e)
        {
            //Don't do this in production
            CheckForIllegalCrossThreadCalls = false;

            richTextBox1.Text = String.Format("#{0} {1}[isSub: {2}]: {3}", e.ChatMessage.Channel, e.ChatMessage.DisplayName, e.ChatMessage.Subscriber, e.ChatMessage.Message) +
                "\n" + richTextBox1.Text;
        }

        private void globalChatMessageReceivedEmote(object sender, TwitchChatClient.OnMessageReceivedArgs e)
        {
            //Don't do this in production
            CheckForIllegalCrossThreadCalls = false;

            richTextBox1.Text = String.Format("#{0} {1}[isSub: {2}]: {3}", e.ChatMessage.Channel, e.ChatMessage.DisplayName, e.ChatMessage.Subscriber, e.ChatMessage.EmoteReplacedMessage) +
                "\n" + richTextBox1.Text;
        }

        private void globalWhisperReceived(object sender, TwitchWhisperClient.OnWhisperReceivedArgs e)
        {
            //Don't do this in production
            CheckForIllegalCrossThreadCalls = false;

            richTextBox2.Text = String.Format("{0} -> {1}: {2}", e.WhisperMessage.Username, e.WhisperMessage.BotUsername, e.WhisperMessage.Message) +
                "\n" + richTextBox2.Text;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();
            foreach (TwitchChatClient client in chatClients)
            {
                if(client.TwitchUsername.ToLower() == comboBox2.Text.ToLower()) {
                    comboBox3.Items.Add(client.Channel);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (TwitchChatClient client in chatClients)
            {
                if (client.TwitchUsername.ToLower() == comboBox2.Text.ToLower())
                {
                    if (client.Channel.ToLower() == comboBox3.Text.ToLower())
                    {
                        client.SendMessage(textBox3.Text);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (TwitchWhisperClient client in whisperClients)
            {
                if (client.TwitchUsername == comboBox1.Text.ToLower())
                {
                    client.SendWhisper(textBox1.Text, textBox2.Text);
                }
            }
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            try
            {
                TwitchChannel channel = await TwitchApi.GetTwitchChannel(textBox9.Text);
                MessageBox.Show(String.Format("Status: {0}\nBroadcaster Lang: {1}\nDisplay Name: {2}\nGame: {3}\nLanguage: {4}\nName: {5}\nCreated At: {6}\n" +
                "Updated At: {7}\nDelay: {8}\nLogo: {9}\nBackground: {10}\nProfile Banner: {11}\nMature: {12}\nPartner: {13}\nID: {14}\nViews: {15}\nFollowers: {16}",
                channel.Status, channel.BroadcasterLanguage, channel.DisplayName, channel.Game, channel.Language, channel.Name, channel.CreatedAt, channel.UpdatedAt,
                channel.Delay, channel.Logo, channel.Background, channel.ProfileBanner, channel.Mature, channel.Partner, channel.Id, channel.Views, channel.Followers));
            } catch (TwitchLib.Exceptions.InvalidChannelException)
            {
                MessageBox.Show(string.Format("The channel '{0}' is not a valid channel!", textBox9.Text));
            }
        }

        private async void button10_Click(object sender, EventArgs e)
        {
            List<Chatter> chatters = await TwitchApi.GetChatters(textBox10.Text);
            string messageContents = "";
            foreach(Chatter user in chatters)
            {
                if(messageContents == "")
                {
                    messageContents = String.Format("{0} ({1})", user.Username, user.UserType.ToString());
                } else
                {
                    messageContents += String.Format(", {0} ({1})", user.Username, user.UserType.ToString());
                }
            }
            MessageBox.Show(messageContents);
        }

        private async void button11_Click(object sender, EventArgs e)
        {
            if (await TwitchApi.UserFollowsChannel(textBox11.Text, textBox12.Text))
            {
                MessageBox.Show(String.Format("'{0}' follows the channel '{1}'!", textBox11.Text, textBox12.Text));
            } else
            {
                MessageBox.Show(String.Format("'{0}' does NOT follow the channel '{1}'!", textBox11.Text, textBox12.Text));
            }
        }

        private async void button12_Click(object sender, EventArgs e)
        {
            if(await TwitchApi.BroadcasterOnline(textBox13.Text))
            {
                MessageBox.Show(String.Format("'{0}' is ONLINE!", textBox13.Text));
            } else
            {
                MessageBox.Show(string.Format("'{0}' is OFFLINE!", textBox13.Text));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TwitchApi.UpdateStreamTitle(textBox16.Text, textBox14.Text, textBox15.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TwitchApi.UpdateStreamGame(textBox17.Text, textBox14.Text, textBox15.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TwitchApi.UpdateStreamTitleAndGame(textBox18.Text, textBox19.Text, textBox14.Text, textBox15.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            TwitchApi.RunCommercial(TwitchApi.CommercialLength.Seconds30, textBox14.Text, textBox15.Text);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            TwitchApi.RunCommercial(TwitchApi.CommercialLength.Seconds60, textBox14.Text, textBox15.Text);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            TwitchApi.RunCommercial(TwitchApi.CommercialLength.Seconds90, textBox14.Text, textBox15.Text);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            TwitchApi.ResetStreamKey(textBox14.Text, textBox15.Text);
        }

        private async void button16_Click(object sender, EventArgs e)
        {
            List<TwitchLib.TwitchAPIClasses.TwitchVideo> videos = await TwitchApi.GetChannelVideos(textBox20.Text);
            foreach(TwitchLib.TwitchAPIClasses.TwitchVideo vid in videos)
            {
                MessageBox.Show($"Title: {vid.Title}\nDescription: {vid.Description}\nStatus: {vid.Status}\nId: {vid.Id}\nTag List: {vid.TagList}\n Recorded At: {vid.RecordedAt}\n" +
                    $"Game: {vid.Game}\nPreview: {vid.Preview}\nBroadcast Id: {vid.BroadcastId}\nLength: {vid.Length}\nUrl: {vid.Url}\nViews: {vid.Views}\n");
            }
        }

        private async void button17_Click(object sender, EventArgs e)
        {
            List<string> hosts = await TwitchApi.GetChannelHosts(textBox21.Text);
            if (hosts.Count > 0)
                foreach (string host in hosts)
                    MessageBox.Show(host);
            else
                MessageBox.Show("No hosts for '" + textBox21.Text + "'");
        }

        private async void button18_Click(object sender, EventArgs e)
        {
            List<TwitchLib.TwitchAPIClasses.TwitchTeamMember> members = await TwitchApi.GetTeamMembers(textBox22.Text);
            foreach(TwitchLib.TwitchAPIClasses.TwitchTeamMember member in members)
            {
                MessageBox.Show($"Name: {member.Name}\nDescription: {member.Description}\nTitle: {member.Title}\nMeta Game: {member.MetaGame}\nDisplay Name: {member.DisplayName}\n" +
                    $"Link: {member.Link}\nFollower Count: {member.FollowerCount}\nTotal Views: {member.TotalViews}\nCurrent Views: {member.CurrentViews}");
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (TwitchApi.ChannelHasUserSubscribed(textBox23.Text, textBox14.Text, textBox15.Text).Result)
                MessageBox.Show("User is subscribed!");
            else
                MessageBox.Show("User is not subscribed!");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            List<TwitchLib.TwitchAPIClasses.TwitchFollower> followers = TwitchApi.GetTwitchFollowers(textBox24.Text).Result;
            foreach(TwitchLib.TwitchAPIClasses.TwitchFollower follower in followers)
            {
                MessageBox.Show(string.Format("notifications: {0}\ncreated at:{1}\n[user] name: {2}\n[user] display name: {3}\n[user] bio: {4}\n [user] logo: {5}\n[user] created at: {6}\n[user] updated at: {7}", follower.Notifications, follower.CreatedAt, follower.User.Name, follower.User.DisplayName, follower.User.Bio, follower.User.Logo, follower.User.CreatedAt, follower.User.UpdatedAt));
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            TwitchLib.TwitchAPIClasses.TwitchStream stream = TwitchApi.GetTwitchStream(textBox25.Text).Result;
            MessageBox.Show(string.Format("average fps: {0}\nchannel name: {1}\ncreated at: {2}\ndelay: {3}\ngame: {4}\nid: {5}\nplaylist: {6}\npreview large: {7}\nvideo height: {8}\n viewers: {9}",
                stream.AverageFps, stream.Channel.Name, stream.CreatedAt, stream.Delay, stream.Game, stream.Id, stream.IsPlaylist, stream.Preview.Large, stream.VideoHeight, stream.Viewers));
        }

        private void button22_Click(object sender, EventArgs e)
        {
            TimeSpan uptime = TwitchApi.GetUptime(textBox26.Text).Result;
            MessageBox.Show(string.Format("uptime: {0} days, {1} hours, {2} minutes, {3} seconds", uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds));
        }

        private void button23_Click(object sender, EventArgs e)
        {
            List<TwitchChannel> results = TwitchApi.SearchChannels(textBox27.Text).Result;
            if (results.Count > 0)
                foreach(TwitchChannel channel in results)
                    MessageBox.Show(String.Format("Status: {0}\nBroadcaster Lang: {1}\nDisplay Name: {2}\nGame: {3}\nLanguage: {4}\nName: {5}\nCreated At: {6}\n" +
                    "Updated At: {7}\nDelay: {8}\nLogo: {9}\nBackground: {10}\nProfile Banner: {11}\nMature: {12}\nPartner: {13}\nID: {14}\nViews: {15}\nFollowers: {16}",
                    channel.Status, channel.BroadcasterLanguage, channel.DisplayName, channel.Game, channel.Language, channel.Name, channel.CreatedAt, channel.UpdatedAt,
                    channel.Delay, channel.Logo, channel.Background, channel.ProfileBanner, channel.Mature, channel.Partner, channel.Id, channel.Views, channel.Followers));
            else
                MessageBox.Show("No results!");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            TwitchApi.UpdateStreamDelay(Decimal.ToInt32(numericUpDown1.Value), textBox14.Text, textBox15.Text);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Current sub count: " + TwitchApi.GetSubscriberCount(textBox14.Text, textBox15.Text).ToString());
        }
    }
}
