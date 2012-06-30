using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Text;

using MayhemCore;
using MayhemWpf.ModuleTypes;
using MayhemWpf.UserControls;

using OpenPop.Mime.Header;
using OpenPop.Pop3;

namespace EmailMayhem {

    [DataContract]
    [MayhemModule("Email Received", "Triggers when an email is received")]
    public class EmailReceiveEvent : EventBase, IWpfConfigurable {

        [DataMember]
        private string Hostname { get; set; }

        [DataMember]
        private int Port { get; set; }

        [DataMember]
        private bool UseSsl { get; set; }

        [DataMember]
        private string EmailAddress { get; set; }

        [DataMember]
        private string Password { get; set; }

        [DataMember]
        private string Subject { get; set; }

        [DataMember]
        private string From { get; set; }

        private bool Done;

        private Thread PollingThread;

        private object Lock;

        protected override void OnLoadDefaults() {
            base.OnLoadDefaults();

            Hostname = String.Empty;
            Port = 995;
            UseSsl = true;
            EmailAddress = String.Empty;
            Password = String.Empty;
            Subject = String.Empty;
            From = String.Empty;
        }

        protected override void OnLoadFromSaved() {
            base.OnLoadFromSaved();
        }

        protected override void OnAfterLoad() {
            base.OnAfterLoad();
            Lock = new object();
            Done = false;
        }

        protected override void OnDeleted() {
            base.OnDeleted();

            lock (Lock) {
                Done = true;
            }
            if (PollingThread != null) {
                PollingThread.Join();
                PollingThread = null;
            }
        }

        protected override void OnEnabling(EnablingEventArgs e) {
            base.OnEnabling(e);
            lock (Lock) {
                Done = false;
            }

            // Start up the polling thread
            PollingThread = new Thread(EmailReceiveThread);
            PollingThread.Start();
        }

        protected override void OnDisabled(DisabledEventArgs e) {
            base.OnDisabled(e);
            lock (Lock) {
                Done = true;
            }
            if (PollingThread != null) {
                PollingThread.Join();
                PollingThread = null;
            }
        }

        public string GetConfigString() {
            lock (Lock) {
                return EmailAddress;
            }
        }

        public WpfConfiguration ConfigurationControl {
            get {
                EmailReceiveEventConfig config;
                lock (Lock) {
                    config = new EmailReceiveEventConfig(Hostname, Port, UseSsl, EmailAddress, Password, Subject, From);
                }
                return config;
            }
        }

        public void OnSaved(WpfConfiguration configurationControl) {
            EmailReceiveEventConfig config = configurationControl as EmailReceiveEventConfig;
            lock (Lock) {
                Hostname = config.Hostname;
                Port = config.Port;
                UseSsl = config.UseSsl;
                EmailAddress = config.EmailAddress;
                Password = config.Password;
                Subject = config.Subject.Trim();
                From = config.From.Trim();
            }
        }

        private Pop3Client InitiateClient() {
            Pop3Client client = new Pop3Client();
            try {
                client.Connect(Hostname, Port, UseSsl);
                client.Authenticate(EmailAddress, Password);
            } catch {
                // Ignore exceptions - let them be raised in the thread
                // There's not really a good way to debug this anyway if something goes wrong at this point.
            }
            return client;
        }

        private void EmailReceiveThread() {

            Pop3Client client = null;
            int messageCount = 0;
            using (client = InitiateClient()) {
                messageCount = client.GetMessageCount();
            }
            
            int pollingDelay = 15000;

            bool done = false;
            while (!done) {

                lock (Lock) {
                    done = Done;

                    using (client = InitiateClient()) {
                        int newMessageCount = client.GetMessageCount();
                        // Try to get each message between the previous latest message and the current latest message
                        // If we find a message thats valid and new, we trigger
                        bool trigger = false;
                        for (int message = messageCount + 1; message <= newMessageCount; message++) {
                            // The message might be deleted, so a try catch is needed
                            try {
                                MessageHeader header = client.GetMessageHeaders(message);

                                // Match the subject
                                bool subjectPasses = false;
                                if (!string.IsNullOrEmpty(Subject)) {
                                    if (header.Subject.ToLower().Contains(Subject.ToLower())) {
                                        subjectPasses = true;
                                    }
                                } else {
                                    subjectPasses = true;
                                }

                                // Match the sender
                                bool fromPasses = false;
                                if (!string.IsNullOrEmpty(From)) {
                                    // Compare against the email address
                                    if (!string.IsNullOrEmpty(header.Sender.Address)) {
                                        if (header.Sender.Address.ToLower().Contains(From.ToLower())) {
                                            fromPasses = true;
                                        }
                                    // Then try to compare against the display name
                                    } else if (!string.IsNullOrEmpty(header.Sender.DisplayName)) {
                                        if (header.Sender.DisplayName.ToLower().Contains(From.ToLower())) {
                                            fromPasses = true;
                                        }
                                    }
                                    // If neither of those existed, then who knows who this is from, but it isn't one of the people you asked for
                                } else {
                                    fromPasses = true;
                                }

                                // If we've either already triggered from some other message, or if this message's subject and from checks passes, then we need to trigger as soon as we finish
                                trigger = trigger || (subjectPasses && fromPasses);
                            } catch {
                                // Just move along to the next message
                            }
                        }

                        if (trigger) {
                            Trigger();
                        }

                        messageCount = newMessageCount;
                    }
                }

                if (!done) {
                    // Sleep till the next time we should check things
                    Thread.Sleep(pollingDelay);
                }
            }
        }
    }
}
