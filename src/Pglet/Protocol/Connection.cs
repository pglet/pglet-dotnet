﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pglet.Protocol
{
    public class Connection
    {
        private static Dictionary<Connection, bool> _allConnections = new();

        ReconnectingWebSocket _ws;
        ConcurrentDictionary<string, TaskCompletionSource<JObject>> _wsCallbacks = new ConcurrentDictionary<string, TaskCompletionSource<JObject>>();
        Func<PageEventPayload, Task> _onEvent;
        Func<PageSessionCreatedPayload, Task> _onSessionCreated;

        public Func<PageEventPayload, Task> OnEvent
        {
            set { _onEvent = value; }
        }

        public Func<PageSessionCreatedPayload, Task> OnSessionCreated
        {
            set { _onSessionCreated = value; }
        }

        public string HostClientId { get; set; }
        public string PageName { get; set; }
        public string PageUrl { get; set; }
        public ConcurrentDictionary<string, Page> Sessions { get; set; } = new();

        public Connection(ReconnectingWebSocket ws)
        {
            _ws = ws;
            _ws.OnMessage = OnMessage;
            _allConnections[this] = true;
        }

        private async Task OnMessage(byte[] message)
        {
            var j = Encoding.UTF8.GetString(message);
            var m = JsonUtility.Deserialize<Protocol.Message>(j);

            //Console.WriteLine($"OnMessage: {m.Payload}");

            if (m.Payload == null)
            {
                throw new Exception("Invalid message received by a WebSocket");
            }

            if (!String.IsNullOrEmpty(m.Id))
            {
                // it's a callback
                if (_wsCallbacks.TryRemove(m.Id, out TaskCompletionSource<JObject> tcs))
                {
                    tcs.SetResult(m.Payload as JObject);
                }
            }
            else if (m.Action == Actions.PageEventToHost)
            {
                if (_onEvent != null)
                {
                    // page event
                    await _onEvent(JsonUtility.Deserialize<PageEventPayload>(m.Payload as JObject)).ConfigureAwait(false);
                }
            }
            else if (m.Action == Actions.SessionCreated)
            {
                if (_onSessionCreated != null)
                {
                    // new session started
                    await _onSessionCreated(JsonUtility.Deserialize<PageSessionCreatedPayload>(m.Payload as JObject)).ConfigureAwait(false);
                }
            }
            else
            {
                // something else
                // TODO - throw?
                Trace.TraceWarning(m.Payload.ToString());
            }
        }

        public async Task<RegisterHostClientResponsePayload> RegisterHostClient(string pageName, bool isApp, bool update, string authToken, string permissions, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("Connection: RegisterHostClient");
            var payload = new RegisterHostClientRequestPayload
            {
                HostClientID = this.HostClientId,
                PageName = String.IsNullOrEmpty(pageName) ? "*" : pageName,
                IsApp = isApp,
                Update = update,
                AuthToken = authToken,
                Permissions = permissions
            };

            var respPayload = await SendMessageWithResult(Actions.RegisterHostClient, payload, cancellationToken);
            Trace.TraceInformation("Connection: RegisterHostClient response: {0}", respPayload);

            var result = JsonUtility.Deserialize<RegisterHostClientResponsePayload>(respPayload);
            if (!String.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }
            this.HostClientId = result.HostClientID;
            return result;
        }

        public async Task<PageCommandResponsePayload> SendCommand(string pageName, string sessionId, Command command, CancellationToken cancellationToken)
        {
            var payload = new PageCommandRequestPayload
            {
                PageName = pageName,
                SessionID = sessionId,
                Command = command
            };

            var respPayload = await SendMessageWithResult(Actions.PageCommandFromHost, payload, cancellationToken);
            var result = JsonUtility.Deserialize<PageCommandResponsePayload>(respPayload);
            if (!String.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }
            return result;
        }

        public async Task<PageCommandsBatchResponsePayload> SendCommands(string pageName, string sessionId, List<Command> commands, CancellationToken cancellationToken)
        {
            var payload = new PageCommandsBatchRequestPayload
            {
                PageName = pageName,
                SessionID = sessionId,
                Commands = commands
            };

            var respPayload = await SendMessageWithResult(Actions.PageCommandsBatchFromHost, payload, cancellationToken);
            var result = JsonUtility.Deserialize<PageCommandsBatchResponsePayload>(respPayload);
            if (!String.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }
            return result;
        }

        private Task<JObject> SendMessageWithResult(string actionName, object payload, CancellationToken cancellationToken)
        {
            return SendMessageInternal(Guid.NewGuid().ToString("N"), actionName, payload, cancellationToken);
        }

        private async Task<JObject> SendMessageInternal(string messageId, string actionName, object payload, CancellationToken cancellationToken)
        {
            // send request
            var msg = new Protocol.Message
            {
                Id = messageId,
                Action = actionName,
                Payload = payload
            };

            var j = JsonUtility.Serialize(msg);
            var jb = Encoding.UTF8.GetBytes(j);
            await _ws.SendMessage(jb, cancellationToken);

            if (messageId != null)
            {
                // register TSC for response
                var tcs = new TaskCompletionSource<JObject>();
                _wsCallbacks.TryAdd(msg.Id, tcs);

                using CancellationTokenRegistration ctr = cancellationToken.Register(() =>
                {
                    if (_wsCallbacks.TryRemove(msg.Id, out TaskCompletionSource<JObject> tcs))
                    {
                        tcs.SetCanceled();
                    }
                });

                return await tcs.Task;
            }
            else
            {
                // shoot and forget
                return null;
            }
        }

        public static void CloseAllConnections()
        {
            Trace.TraceInformation($"Closing {_allConnections.Count} active connection(s)");

            foreach (var conn in _allConnections.Keys.ToArray())
            {
                conn.Close();
            }
        }

        public void Close()
        {
            _allConnections.Remove(this);

            if (_ws != null)
            {
                _ws.CloseAsync().Wait();
                _ws = null;
            }
        }
    }
}
