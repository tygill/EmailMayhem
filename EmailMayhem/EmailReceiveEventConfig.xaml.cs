using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MayhemCore;
using MayhemWpf.UserControls;

using OpenPop.Pop3;

namespace EmailMayhem {
    /// <summary>
    /// Interaction logic for EmailReceiveEventConfig.xaml
    /// </summary>
    public partial class EmailReceiveEventConfig : WpfConfiguration {

        public EmailReceiveEventConfig(string hostname, int port, bool useSsl, string emailAddress, string password, string subject, string from) {
            InitializeComponent();

            Hostname = hostname;
            Port = port;
            UseSsl = useSsl;
            EmailAddress = emailAddress;
            Password = password;
            Subject = subject;
            From = from;
        }

        public string Hostname { get; private set; }

        public int Port { get; private set; }

        public bool UseSsl { get; private set; }

        public string EmailAddress { get; private set; }

        public string Password { get; private set; }

        public string Subject { get; private set; }

        public string From { get; private set; }

        public override void OnLoad() {
            base.OnLoad();

            hostname.Text = Hostname;
            port.Text = Port != 0 ? Port.ToString() : String.Empty;
            useSsl.IsChecked = UseSsl;
            emailAddress.Text = EmailAddress;
            password.Password = Password;
            subject.Text = Subject;
            from.Text = From;

            Validate();
        }

        public override string Title {
            get {
                return "Receive Email";
            }
        }

        private void Unvalidate() {
            CanSave = false;
            checkButton.IsEnabled = true;
        }

        private void Validate() {
            // If everything is valid and we can make a connection, then set CanSave to true
            try {
                Hostname = hostname.Text;
                Port = int.Parse(port.Text);
                UseSsl = useSsl.IsChecked.GetValueOrDefault();
                EmailAddress = emailAddress.Text;
                // This is a fun line to have...I almost feel like I should send it in to the daily wtf
                Password = password.Password;
                Subject = subject.Text;
                From = from.Text;

                using (Pop3Client client = new Pop3Client()) {
                    client.Connect(Hostname, Port, UseSsl);

                    if (!client.Connected) {
                        CanSave = false;
                        return;
                    }

                    client.Authenticate(EmailAddress, Password);
                }

                // We got this far, this must be good
                CanSave = true;
                checkButton.IsEnabled = false;

            } catch {
                // Something failed to validate, so we can't save yet
                CanSave = false;
            }
        }

        private void checkButton_Click(object sender, RoutedEventArgs e) {
            Validate();
        }

        // The subject and from text fields have no callbacks for a reason - it doesn't matter what value they have
        // They're always valid!

        private void emailAddress_TextChanged(object sender, TextChangedEventArgs e) {
            Unvalidate();
            if (emailAddress.Text.Contains("@gmail.com") || emailAddress.Text.Contains("@googlemail.com")) {
                if (string.IsNullOrEmpty(hostname.Text)) {
                    hostname.Text = "pop.gmail.com";
                }
            } else if (emailAddress.Text.Contains("@hotmail.com") || emailAddress.Text.Contains("@live.com")) {
                if (string.IsNullOrEmpty(hostname.Text)) {
                    hostname.Text = "pop3.live.com";
                }
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e) {
            Unvalidate();
        }

        private void hostname_TextChanged(object sender, TextChangedEventArgs e) {
            Unvalidate();
            // Fill in some defaults, just to be nice :)
            if (hostname.Text == "pop.gmail.com") {
                if (string.IsNullOrEmpty(port.Text)) {
                    port.Text = "995";
                    useSsl.IsChecked = true;
                }
            } else if (hostname.Text == "pop3.live.com") {
                if (string.IsNullOrEmpty(port.Text)) {
                    port.Text = "995";
                    useSsl.IsChecked = true;
                }
            }
        }

        private void port_TextChanged(object sender, TextChangedEventArgs e) {
            port.Text = Regex.Replace(port.Text, "[^0-9]", "", RegexOptions.Compiled);
            Unvalidate();
        }

        private void useSsl_Checked(object sender, RoutedEventArgs e) {
            Unvalidate();
        }

        private void useSsl_Unchecked(object sender, RoutedEventArgs e) {
            Unvalidate();
        }

        private void subject_TextChanged(object sender, TextChangedEventArgs e) {
            Subject = subject.Text;
        }

        private void from_TextChanged(object sender, TextChangedEventArgs e) {
            From = from.Text;
        }
    }
}
