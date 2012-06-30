using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

using MayhemCore;
using MayhemWpf.ModuleTypes;
using MayhemWpf.UserControls;

namespace EmailMayhem {

    [DataContract]
    [MayhemModule("Send Email", "Sends an email")]
    public class SendEmailReaction : ReactionBase, IWpfConfigurable {

        [DataMember]
        private string Hostname { get; set; }

        [DataMember]
        private int Port { get; set; }

        [DataMember]
        private bool UseSsl { get; set; }

        [DataMember]
        private string EmailAddress { get; set; }

        //[DataMember]
        private string Password { get; set; }

        [DataMember]
        private string To { get; set; }

        [DataMember]
        private string Subject { get; set; }

        [DataMember]
        private string Body { get; set; }

        protected override void OnLoadDefaults() {
            base.OnLoadDefaults();

            Hostname = String.Empty;
            Port = 0;
            UseSsl = true;
            EmailAddress = String.Empty;
            Password = String.Empty;
            To = String.Empty;
            Subject = String.Empty;
            Body = String.Empty;
        }

        protected override void OnLoadFromSaved() {
            base.OnLoadFromSaved();

            Password = String.Empty;
            MessageBox.Show("Please re-enter your password for your email account", "Send Email Reaction", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        protected override void OnAfterLoad() {
            base.OnAfterLoad();
        }

        protected override void OnDeleted() {
            base.OnDeleted();
        }

        protected override void OnEnabling(EnablingEventArgs e) {
            base.OnEnabling(e);
        }

        protected override void OnDisabled(DisabledEventArgs e) {
            base.OnDisabled(e);
        }

        public override void Perform() {
            using (SmtpClient client = new SmtpClient(Hostname, Port)) {
                client.EnableSsl = UseSsl;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new System.Net.NetworkCredential(EmailAddress, Password);

                MailMessage email = new MailMessage(EmailAddress, To, Subject, Body);
                try {
                    client.Send(email);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    // There isn't really anything that can be done here to make sure its working...
                }
            }
        }

        public string GetConfigString() {
            return string.Format("Email {0}", To);
        }

        public WpfConfiguration ConfigurationControl {
            get {
                return new SendEmailReactionConfig(Hostname, Port, UseSsl, EmailAddress, Password, To, Subject, Body);;
            }
        }

        public void OnSaved(WpfConfiguration configurationControl) {
            SendEmailReactionConfig config = configurationControl as SendEmailReactionConfig;
            Hostname = config.Hostname;
            Port = config.Port;
            UseSsl = config.UseSsl;
            EmailAddress = config.EmailAddress;
            Password = config.Password;
            To = config.To;
            Subject = config.Subject;
            Body = config.Body;
        }
    }
}
