using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Pglet.Protocol;

namespace Pglet
{
    public class PgletClient
    {
        // this constant is supposed to be patched during CI build
        const string PGLET_SERVER_VERSION = null;

        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

        // user permissions
        const int S_IRUSR = 0x100;
        const int S_IWUSR = 0x80;
        const int S_IXUSR = 0x40;

        // group permission
        const int S_IRGRP = 0x20;
        const int S_IWGRP = 0x10;
        const int S_IXGRP = 0x8;

        // other permissions
        const int S_IROTH = 0x4;
        const int S_IWOTH = 0x2;
        const int S_IXOTH = 0x1;

        public const string HOSTED_SERVICE_URL = "https://app.pglet.io";
        public const string DEFAULT_SERVER_PORT = "8550";
        public const string ZERO_SESSION = "0";

        private PgletClient() { }

        static PgletClient()
        {
            // subscribe to application exit/unload events
            Console.CancelKeyPress += delegate
            {
                OnExit();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                OnExit();
            };
        }

        public static async Task<Page> ConnectPage(string pageName = null, bool update = false, bool web = false,
            string serverUrl = null, string token = null, bool noWindow = false, string permissions = null,
            Func<Connection, string, string, string, Page> createPage = null, CancellationToken? cancellationToken = null)
        {
            var ct = cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None;
            var conn = await ConnectInternal(pageName, false, update, web, serverUrl, token, permissions, noWindow, null, createPage, ct);

            Page page = createPage != null ? createPage(conn, conn.PageUrl, conn.PageName, ZERO_SESSION) : new Page(conn, conn.PageUrl, conn.PageName, ZERO_SESSION);
            await page.LoadPageDetails();
            conn.Sessions[ZERO_SESSION] = page;
            return page;
        }

        public static async Task ServeApp(Func<Page, Task> sessionHandler, string pageName = null, bool web = false,
            string serverUrl = null, string token = null, bool noWindow = false, string permissions = null,
            Func<Connection, string, string, string, Page> createPage = null, Action<string> pageCreated = null, CancellationToken? cancellationToken = null)
        {
            var ct = cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None;
            var conn = await ConnectInternal(pageName, true, false, web, serverUrl, token, permissions, noWindow, sessionHandler, createPage, ct);

            pageCreated?.Invoke(conn.PageUrl);

            var semaphore = new SemaphoreSlim(0);
            using CancellationTokenRegistration ctr = ct.Register(() =>
            {
                semaphore.Release();
            });
            await semaphore.WaitAsync();
            conn.Close();
        }

        private static async Task<Connection> ConnectInternal(string pageName, bool isApp,
            bool update, bool web, string serverUrl, string token, string permissions, bool noWindow,
            Func<Page, Task> sessionHandler, Func<Connection, string, string, string, Page> createPage,
            CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(serverUrl))
            {
                if (web)
                {
                    serverUrl = HOSTED_SERVICE_URL;
                }
                else
                {
                    var serverPort = Environment.GetEnvironmentVariable("PGLET_SERVER_PORT");
                    serverUrl = "http://localhost:" + (string.IsNullOrEmpty(serverPort) ? DEFAULT_SERVER_PORT : serverPort);
                }
            }

            var wsUrl = GetWebSocketUrl(serverUrl);
            var ws = new ReconnectingWebSocket(wsUrl);
            var conn = new Connection(ws);
            conn.OnEvent = async (payload) =>
            {
                //Console.WriteLine("Event received: " + JsonUtility.Serialize(payload));
                if (conn.Sessions.TryGetValue(payload.SessionID, out Page page))
                {
                    await page.OnEvent(new Event
                    {
                        Target = payload.EventTarget,
                        Name = payload.EventName,
                        Data = payload.EventData
                    });

                    if (payload.EventTarget == "page" && payload.EventName == "close")
                    {
                        //Console.WriteLine("Session is closing: " + payload.SessionID);
                        conn.Sessions.TryRemove(payload.SessionID, out Page _);
                    }
                }
            };

            if (sessionHandler != null)
            {
                conn.OnSessionCreated = async (payload) =>
                {
                    Trace.TraceInformation("Session created: " + JsonUtility.Serialize(payload));
                    Page page = createPage != null ? createPage(conn, conn.PageUrl, conn.PageName, payload.SessionID) : new Page(conn, conn.PageUrl, conn.PageName, payload.SessionID);
                    await page.LoadPageDetails();
                    conn.Sessions[payload.SessionID] = page;

                    var h = sessionHandler(page).ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            await page.ErrorAsync("There was an error while processing your request: " + (t.Exception as AggregateException).InnerException.Message);
                        }
                    });
                };
            }

            ws.OnFailedConnect = async () =>
            {
                if (wsUrl.Host == "localhost")
                {
                    await StartPgletServer();
                }
            };
            ws.OnReconnected = async () =>
            {
                await conn.RegisterHostClient(pageName, isApp, update, token, permissions, cancellationToken);
            };

            await ws.Connect(cancellationToken);

            var resp = await conn.RegisterHostClient(pageName, isApp, update, token, permissions, cancellationToken);
            conn.PageName = resp.PageName;
            conn.PageUrl = GetPageUrl(serverUrl, conn.PageName).ToString();

            if (!noWindow)
            {
                OpenBrowser(conn.PageUrl);
            }

            return conn;
        }

        private static Uri GetPageUrl(string serverUrl, string pageName)
        {
            var pageUri = new UriBuilder(serverUrl ?? HOSTED_SERVICE_URL);
            pageUri.Path = "/" + pageName;
            return pageUri.Uri;
        }

        private static Uri GetWebSocketUrl(string serverUrl)
        {
            var wssUri = new UriBuilder(serverUrl ?? HOSTED_SERVICE_URL);
            wssUri.Scheme = wssUri.Scheme == "https" ? "wss" : "ws";
            wssUri.Path = "/ws";
            return wssUri.Uri;
        }

        private static async Task StartPgletServer()
        {
            Trace.TraceInformation("Starting Pglet Server in local mode");

            var pgletExe = RuntimeInfo.IsWindows ? "pglet-server.exe" : "pglet";

            var platform = "win";
            if (RuntimeInfo.IsLinux)
            {
                platform = "linux";
            }
            else if (RuntimeInfo.IsMac)
            {
                platform = "osx";
            }

            // check for executable in "runtimes" directory
            var pgletPath = Path.Combine(GetApplicationDirectory(), "runtimes", $"{platform}-{RuntimeInfo.Architecture}", pgletExe);
            if (File.Exists(pgletPath))
            {
                Trace.TraceInformation("Pglet Server found in {0}", pgletPath);
            }
            else
            {
                // check if executable is in PATH
                pgletPath = FindExecutablePath(pgletExe);
                if (pgletPath != null)
                {
                    Trace.TraceInformation("Pglet Server found in PATH at {0}", pgletPath);
                }
                else
                {
                    // download Pglet Server executable to $HOME/.pglet/bin
                    pgletPath = await DownloadPgletServer();
                }
            }

            if (RuntimeInfo.IsLinux || RuntimeInfo.IsMac)
            {
                // set chmod
                const int _0755 = S_IRUSR | S_IXUSR | S_IWUSR | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH;
                chmod(pgletPath, (int)_0755);
            }

            Trace.TraceInformation("Pglet executable path: {0}", pgletPath);

            var psi = new ProcessStartInfo
            {
                FileName = pgletPath,
                Arguments = "server --background",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }

        private static async Task<string> DownloadPgletServer()
        {
            var platform = "windows";
            if (RuntimeInfo.IsLinux)
            {
                platform = "linux";
            }
            else if (RuntimeInfo.IsMac)
            {
                platform = "darwin";
            }

            var arch = RuntimeInfo.Architecture;
            if (RuntimeInfo.Architecture == "x64")
            {
                arch = "amd64";
            }

            var ext = RuntimeInfo.IsWindows ? ".exe" : "";

            var homeDir = RuntimeInfo.IsWindows ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            var pgletDir = Path.Combine(homeDir, ".pglet", "bin");
            var pgletPath = Path.Combine(pgletDir, $"pglet{ext}");

            if (File.Exists(pgletPath))
            {
                Trace.TraceInformation("Pglet Server already downloaded into {0}", pgletPath);
                return pgletPath;
            }

            var client = new HttpClient();
            var productValue = new ProductInfoHeaderValue("Pglet", "1.0");
            client.DefaultRequestHeaders.UserAgent.Add(productValue);

            string ver = PGLET_SERVER_VERSION;
            if (ver == null)
            {
                Trace.TraceInformation("Checking Pglet releases for the latest version");
                using (var response = await client.GetAsync("https://api.github.com/repos/pglet/pglet/releases"))
                {
                    response.EnsureSuccessStatusCode();
                    using (var content = response.Content)
                    {
                        var releasesJson = await content.ReadAsStringAsync();
                        var releases = JsonUtility.Deserialize<JObject[]>(releasesJson);
                        ver = releases[0].Value<string>("name").Substring(1);
                    }
                }
            }

            var url = $"https://github.com/pglet/pglet/releases/download/v{ver}/pglet-{ver}-{platform}-{arch}{ext}";

            if (!Directory.Exists(pgletDir))
            {
                Directory.CreateDirectory(pgletDir);
            }

            Trace.TraceInformation("Downloading Pglet v{0} from {1} to {2}...", ver, url, pgletPath);
            using (var response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var content = response.Content)
                {
                    var stream = await content.ReadAsStreamAsync();
                    using (var file = File.Create(pgletPath))
                    {
                        await stream.CopyToAsync(file);
                    }
                }
            }

            using (var wc = new System.Net.WebClient())
            {
                await wc.DownloadFileTaskAsync(url, pgletPath);
            }
            return pgletPath;
        }

        private static void OpenBrowser(string url)
        {
            string procVer = "/proc/version";
            bool wsl = (RuntimeInfo.IsLinux && File.Exists(procVer) && File.ReadAllText(procVer).ToLowerInvariant().Contains("microsoft"));
            if (RuntimeInfo.IsWindows || wsl)
            {
                Process.Start("explorer.exe", url);
            }
            else if (RuntimeInfo.IsMac)
            {
                Process.Start("open", url);
            }
        }

        private static string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string FindExecutablePath(string exeFileName)
        {
            foreach (var path in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
            {
                string exePath = Path.Combine(path, exeFileName);
                if (File.Exists(exePath))
                {
                    return exePath;
                }
            }
            return null;
        }

        private static void OnExit()
        {
            Trace.TraceInformation("Exiting from program...");
            Connection.CloseAllConnections();
        }
    }
}
