using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using HmsPluginParser.Parsing;
using HmsPluginParser.Sending;
using HurricaneServer.Plugins;
using HurricaneServer.Plugins.Inbound;
using MimeKit;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Fluent;

namespace HmsPluginParser
{
    public class PluginMain : PluginBase, IInboundSMTPConnection
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<int, string> _connectionList = new ConcurrentDictionary<int, string>();
        private string _pluginDirectory;
        private readonly Dictionary<string, int> _domainsToParse =
            new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public override void OnLoad(Dictionary<string, object> options)
        {
            _pluginDirectory = Path.GetDirectoryName(this.FullPath);

            //Use built in settings
            Settings.SetSection("General");
            var domainList = Settings.Get("DomainsToParse", "").Split(',');
            foreach (var domain in domainList)
                _domainsToParse.Add(domain, 0);

        }

        public override void OnUnload(Dictionary<string, object> options)
        {
        }

        public InboundResponseAction OnRcptTo(int sessionId, string recipient, ref Dictionary<string, object> options, ref string response)
        {
            return InboundResponseAction.Success;
        }

        public InboundResponseAction OnAuth(int sessionId, AuthResult result, string account, string password,
            ref Dictionary<string, object> options, ref string response)
        {
            return InboundResponseAction.Success;
        }

        public InboundResponseAction OnRset(int sessionId, ref Dictionary<string, object> options, ref string response)
        {
            throw new NotImplementedException();
        }

        public InboundResponseAction OnConnect(int sessionId, string ip, ref Dictionary<string, object> options,
            ref string response)
        {
            _connectionList.TryAdd(sessionId, ip);
            return InboundResponseAction.Success;
        }

        public void OnConnectionClosed(int sessionId, ref Dictionary<string, object> options)
        {
            string ip;
            _connectionList.TryRemove(sessionId, out ip);
        }

        public InboundResponseAction OnData(int sessionId, ref Dictionary<string, object> options, ref string response)
        {
            return InboundResponseAction.Success;
        }

        public InboundResponseAction OnFrom(int sessionId, string from, ref Dictionary<string, object> options,
            ref string response)
        {
            return InboundResponseAction.Success;
        }

        public InboundResponseAction OnMessageComplete(int sessionId, ref Dictionary<string, object> options,
            ref string response)
        {
            return InboundResponseAction.Success;
        }

        public MessageResponseAction OnMessageRecieved(int sessionId, ref IInboundMessageEnvelope envelope,
            ref Dictionary<string, object> options)
        {
            var accountId = Convert.ToInt32(options["AccountId"]);
            string sessionSourceIpAddress = _connectionList[sessionId];

            //Only allow anonymous account 1000 inbound emails.  Any other account would mean the connection was authenticated.
            if (accountId != 1000)
                return MessageResponseAction.Accept;

            //Parse the SMTP recipients.  This only contains recipients that where added using the SMTP RCPT TO command.  
            var allRecipients = Parsers.ParseRecipientList(envelope.Recipients);

            //If a recipient domain is not in our domains to parse list we ignore it.
            foreach (var recipient in allRecipients)
                if (!_domainsToParse.ContainsKey(recipient.Domain))
                    return MessageResponseAction.Accept;

            try
            {
                //Parse the contents of the message.
                var result = Parsers.ParseMessage(envelope.GetMessage()); 

                //Here you can do what you want with the message.
                //This includes attachments, recipients from the Header, etc..
                var messageWithHeadersOnly = new MemoryStream();
                dynamic jsonObject = new ExpandoObject();
                jsonObject.Header = result.Headers.ToArray();
                var text = JsonConvert.SerializeObject(jsonObject);

            }
            catch(Exception e)
            {
                _logger.Error(e, $"Error parsing message. {envelope.SystemMessageId} {sessionSourceIpAddress}.");
            }

            //Discard the message no longer needed.
            return MessageResponseAction.Ignore;
        }
    }
}