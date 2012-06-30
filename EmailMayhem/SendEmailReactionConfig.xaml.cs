using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

namespace EmailMayhem {

    /// <summary>
    /// Interaction logic for SendEmailReactionConfig.xaml
    /// </summary>
    public partial class SendEmailReactionConfig : WpfConfiguration {

        public SendEmailReactionConfig(string hostname, int port, bool useSsl, string emailAddress, string password, string to, string subject, string body) {
            InitializeComponent();

            Hostname = hostname;
            Port = port;
            UseSsl = useSsl;
            EmailAddress = emailAddress;
            Password = password;
            To = to;
            Subject = subject;
            Body = body;
        }

        public string Hostname { get; private set; }

        public int Port { get; private set; }

        public bool UseSsl { get; private set; }

        public string EmailAddress { get; private set; }

        public string Password { get; private set; }

        public string To { get; private set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        public override void OnLoad() {
            base.OnLoad();

            hostname.Text = Hostname;
            port.Text = Port != 0 ? Port.ToString() : String.Empty;
            useSsl.IsChecked = UseSsl;
            emailAddress.Text = EmailAddress;
            password.Password = Password;
            to.Text = To;
            subject.Text = Subject;
            body.Text = Body;

            Validate();
        }

        public override string Title {
            get {
                return "Send Email";
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
                To = to.Text;
                Subject = subject.Text;
                Body = body.Text;

                using (SmtpClient client = new SmtpClient(Hostname, Port)) {
                    client.EnableSsl = UseSsl;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Credentials = new System.Net.NetworkCredential(EmailAddress, Password);

                    // I don't think this actually does any validation with this implementation unfortunately...
                    // Oh well. Validate the old fashioned way - are strings not empty
                }

                // Password could maybe be empty?
                if (string.IsNullOrWhiteSpace(Hostname) || string.IsNullOrWhiteSpace(EmailAddress) || string.IsNullOrWhiteSpace(To) || Port == 0) {
                    // Something that isn't optional is gone
                    Unvalidate();
                    return;
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

        private void emailAddress_TextChanged(object sender, TextChangedEventArgs e) {
            Unvalidate();
            if (emailAddress.Text.Contains("@gmail.com") || emailAddress.Text.Contains("@googlemail.com")) {
                if (string.IsNullOrEmpty(hostname.Text)) {
                    hostname.Text = "smtp.gmail.com";
                }
            } else if (emailAddress.Text.Contains("@hotmail.com") || emailAddress.Text.Contains("@live.com")) {
                if (string.IsNullOrEmpty(hostname.Text)) {
                    hostname.Text = "smtp.live.com";
                }
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e) {
            Unvalidate();
        }

        private void hostname_TextChanged(object sender, TextChangedEventArgs e) {
            Unvalidate();
            // Fill in some defaults, just to be nice :)
            if (hostname.Text == "smtp.gmail.com") {
                if (string.IsNullOrEmpty(port.Text)) {
                    port.Text = "587";
                    useSsl.IsChecked = true;
                }
            } else if (hostname.Text == "smtp.live.com") {
                if (string.IsNullOrEmpty(port.Text)) {
                    port.Text = "587";
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

        private void to_TextChanged(object sender, TextChangedEventArgs e) {
            To = to.Text;
            Unvalidate();
        }

        private void subject_TextChanged(object sender, TextChangedEventArgs e) {
            Subject = subject.Text;
        }

        private void body_TextChanged(object sender, TextChangedEventArgs e) {
            Body = body.Text;
        }
    }
}
