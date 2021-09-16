﻿using Pglet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pglet
{
    public class PgletClient
    {
        public const string HOSTED_SERVICE_URL = "https://console.pglet.io";
        public const string ZERO_SESSION = "0";

        ReconnectingWebSocket _ws;
        ConnectionWS _conn;
        string _hostClientId;
        string _pageName;
        string _pageUrl;
        ConcurrentDictionary<string, Page> _sessions = new ConcurrentDictionary<string, Page>();

        public async Task<Page> ConnectPage(string pageName = null,
            string serverUrl = null, string token = null, bool noWindow = false, string permissions = null,
            Func<ConnectionWS, string, string, Page> createPage = null, CancellationToken? cancellationToken = null)
        {
            await ConnectInternal(pageName, false, serverUrl, token, permissions, cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None);

            Page page = createPage != null ? createPage(_conn, _pageName, ZERO_SESSION) : new Page(_conn, _pageName, ZERO_SESSION);
            await page.LoadHash();
            _sessions[ZERO_SESSION] = page;
            return page;
        }

        public async Task ServeApp(Func<Page, Task> sessionHandler, string pageName = null,
            string serverUrl = null, string token = null, bool noWindow = false, string permissions = null,
            Func<ConnectionWS, string, string, Page> createPage = null, Action<string> pageCreated = null, CancellationToken? cancellationToken = null)
        {
            await ConnectInternal(pageName, true, serverUrl, token, permissions, cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None);

            pageCreated?.Invoke(_pageUrl);

            // new session handler
            _conn.OnSessionCreated = async (payload) =>
            {
                Console.WriteLine("Session created: " + JsonUtility.Serialize(payload));
                Page page = createPage != null ? createPage(_conn, _pageName, payload.SessionID) : new Page(_conn, _pageName, payload.SessionID);
                await page.LoadHash();
                _sessions[payload.SessionID] = page;

                var h = sessionHandler(page).ContinueWith(async t =>
                {
                    if (t.IsFaulted)
                    {
                        await page.ErrorAsync("There was an error while processing your request: " + (t.Exception as AggregateException).InnerException.Message);
                    }
                });
            };

            var tcs = new TaskCompletionSource<bool>();
            if (cancellationToken.HasValue)
            {
                using CancellationTokenRegistration ctr = cancellationToken.Value.Register(() => {
                    tcs.SetCanceled();
                });
            }

            await tcs.Task;
        }

        private async Task ConnectInternal(string pageName, bool isApp, string serverUrl, string token, string permissions, CancellationToken cancellationToken)
        {
            _ws = new ReconnectingWebSocket(GetWebSocketUrl(serverUrl ?? HOSTED_SERVICE_URL));
            await _ws.Connect(cancellationToken);
            _conn = new ConnectionWS(_ws);
            _conn.OnEvent = OnPageEvent;

            var resp = await _conn.RegisterHostClient(pageName, isApp, token, permissions, cancellationToken);
            _hostClientId = resp.HostClientID;
            _pageName = resp.PageName;
            _pageUrl = GetPageUrl(serverUrl, _pageName).ToString();
        }

        private Task OnPageEvent(PageEventPayload payload)
        {
            //Console.WriteLine("Event received: " + JsonUtility.Serialize(payload));
            if (_sessions.TryGetValue(payload.SessionID, out Page page))
            {
                page.OnEvent(new Event
                {
                    Target = payload.EventTarget,
                    Name = payload.EventName,
                    Data = payload.EventData
                });

                if (payload.EventTarget == "page" && payload.EventName == "close")
                {
                    _sessions.TryRemove(payload.SessionID, out Page _);
                }
            }
            return Task.CompletedTask;
        }

        private Uri GetPageUrl(string serverUrl, string pageName)
        {
            var pageUri = new UriBuilder(serverUrl);
            pageUri.Path = "/" + pageName;
            return pageUri.Uri;
        }

        private Uri GetWebSocketUrl(string serverUrl)
        {
            var wssUri = new UriBuilder(serverUrl);
            wssUri.Scheme = wssUri.Scheme == "https" ? "wss" : "ws";
            wssUri.Path = "/ws";
            return wssUri.Uri;
        }
    }
}
