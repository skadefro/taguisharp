using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagui
{
    public class Instance : IDisposable
    {
        public static Instance Create(bool headless = false, bool nobrowser = false, bool quiet = false, bool preservelogfiles = false)
        {
            var result = new Instance();
            result.EnsureTagUI();
            end_processes(result);
            result.CreateProcess(headless, nobrowser, quiet, preservelogfiles);
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                try
                {
                    result.Dispose();
                }
                catch (Exception)
                {
                }
            };

            return result;
        }
        private System.Diagnostics.Process tagui;
        public event EventHandler<OutputEventArgs> onOutput;
        public void EnsureTagUI()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var taguifolder = System.IO.Path.Combine(home, "tagui");
            if (!System.IO.Directory.Exists(taguifolder))
            {
                var version = "v6.14.0";
                var tagui_zip_filename = "TagUI_Windows.zip";
                var tagui_zip_file = System.IO.Path.Combine(home, tagui_zip_filename);
                var tagui_zip_url = "https://github.com/kelaberetiv/TagUI/releases/download/" + version + "/" + tagui_zip_filename;
                if (!System.IO.File.Exists(tagui_zip_file))
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(tagui_zip_url, tagui_zip_file);
                    }
                System.IO.Compression.ZipFile.ExtractToDirectory(tagui_zip_file, home);
#if !Windows
                var tagerina = System.IO.Path.Combine(home, "tagui/src/erina");
                var taguiexe = System.IO.Path.Combine(home, "tagui/src/tagui");
                var taguiendexe = System.IO.Path.Combine(home, "tagui/src/tagui/src/end_processes");
                var permissions = "755";
                Task.Run(async () => {
                    await CliWrap.Cli.Wrap("/bin/bash")
                        .WithArguments(new[] { "-c", $"chmod {permissions} {tagerina}" })
                        .ExecuteAsync();
                })
                Task.Run(async () => {
                    await CliWrap.Cli.Wrap("/bin/bash")
                        .WithArguments(new[] { "-c", $"chmod {permissions} {taguiexe}" })
                        .ExecuteAsync();
                })
                Task.Run(async () => {
                    await CliWrap.Cli.Wrap("/bin/bash")
                        .WithArguments(new[] { "-c", $"chmod {permissions} {taguiendexe}" })
                        .ExecuteAsync();
                })
#endif
            }
        }
        public static void end_processes(Instance instance = null)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#if Windows
            var end_processes_exec = System.IO.Path.Combine(home, "tagui/src/end_processes.cmd");
#else
            var end_processes_exec = System.IO.Path.Combine(home, "tagui/src/end_processes");
#endif
            if (instance != null && instance.tagui != null)
            {
                try
                {
                    if (!instance.tagui.HasExited) instance.tagui.Kill();
                }
                catch (Exception)
                {
                }
                instance.tagui.Dispose();
                instance.tagui = null;
            }
            var ps = new System.Diagnostics.Process();
            ps.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = false;
            ps.StartInfo.FileName = end_processes_exec;
            ps.StartInfo.UseShellExecute = false;
            if (instance != null) ps.OutputDataReceived += instance.Tagui_OutputDataReceived;
            ps.Start();
            ps.WaitForExit(10000);
        }
        public void CreateProcess(bool headless, bool nobrowser, bool quiet, bool preservelogfiles)
        {
            isReady = false;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#if Windows
            var tagui_exec = System.IO.Path.Combine(home, "tagui/src/tagui.cmd");
#else
            var tagui_exec = System.IO.Path.Combine(home, "tagui/src/tagui");
#endif
            System.IO.File.WriteAllText(System.IO.Path.Combine(home, "live.tag"), "live" + Environment.NewLine + "// mouse_xy^(^)"); // Start live mode
            if (preservelogfiles)
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(home, "tagui/src/tagui_logging"), ""); // preserve log files
            }
            else
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(home, "tagui/src/tagui_logging"))) System.IO.File.Delete(System.IO.Path.Combine(home, "tagui/src/tagui_logging"));
            }
            tagui = new System.Diagnostics.Process();
            tagui.EnableRaisingEvents = true;
            tagui.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            tagui.StartInfo.CreateNoWindow = true;
            tagui.StartInfo.RedirectStandardInput = true;
            tagui.StartInfo.RedirectStandardOutput = true;
            tagui.StartInfo.RedirectStandardError = true;
            tagui.StartInfo.FileName = tagui_exec;
            tagui.StartInfo.Arguments = "live";
            if (headless) tagui.StartInfo.Arguments += " -headless";
            if (nobrowser) tagui.StartInfo.Arguments += " -nobrowser";
            if (quiet) tagui.StartInfo.Arguments += " -quiet";
            tagui.StartInfo.UseShellExecute = false;
            //tagui.StartInfo.WorkingDirectory = home; // No, we depend on the rpa_csharp.txt to be in "current" folder
            tagui.OutputDataReceived += Tagui_OutputDataReceived;
            tagui.ErrorDataReceived += Tagui_OutputDataReceived;
            tagui.Exited += Tagui_Exited;
            tagui.EnableRaisingEvents = true;
            tagui.Start();
            tagui.BeginOutputReadLine();
        }
        public Instance Web(string URL)
        {
            WaitIsReady();
            Send(URL);
            return Noop();
        }
        public Instance Click(string Element)
        {
            WaitIsReady();
            Send("click " + Element);
            return Noop();
        }
        public Instance Click(int X, int Y)
        {
            WaitIsReady();
            Send("click (" + X + "," + Y + ")");
            return Noop();
        }
        public Instance RightClick(string Element)
        {
            WaitIsReady();
            Send("rclick " + Element);
            return Noop();
        }
        public Instance RightClick(int X, int Y)
        {
            WaitIsReady();
            Send("rclick (" + X + "," + Y + ")");
            return Noop();
        }
        public Instance DoubleClick(string Element)
        {
            WaitIsReady();
            Send("dclick " + Element);
            return Noop();
        }
        public Instance DoubleClick(int X, int Y)
        {
            WaitIsReady();
            Send("dclick (" + X + "," + Y + ")");
            return Noop();
        }
        public Instance Hover(string Element)
        {
            WaitIsReady();
            Send("hover " + Element);
            return Noop();
        }
        public Instance Hover(int X, int Y)
        {
            WaitIsReady();
            Send("hover (" + X + "," + Y + ")");
            return Noop();
        }
        public Instance Type(string Element, string Input)
        {
            WaitIsReady();
            Send("type " + Element + " as " + Input);
            return Noop();
        }
        public Instance Type(string Element, int X, int Y)
        {
            WaitIsReady();
            Send("type " + Element + " as (" + X + "," + Y + ")");
            return Noop();
        }
        public Instance Keyboard(string Input)
        {
            WaitIsReady();
            Send("keyboard " + Input);
            return Noop();
        }
        public Instance MouseDown()
        {
            WaitIsReady();
            Send("mouse down");
            return Noop();
        }
        public Instance MouseUp()
        {
            WaitIsReady();
            Send("mouse up");
            return Noop();
        }
        public Instance Select(string Element, string OptionValue)
        {
            WaitIsReady();
            Send("select " + Element + " as " + OptionValue);
            return Noop();
        }
        public string Table(string XPath, string csvfilename = "sharprpa.csv")
        {
            if (System.IO.File.Exists(csvfilename)) System.IO.File.Delete(csvfilename);
            Send("table " + XPath + " to " + csvfilename);
            Noop();
            if (System.IO.File.Exists(csvfilename))
            {
                // Should we delete ? should we delete if NOT sharprpa.csv ? add an option ?
                return System.IO.File.ReadAllText("sharprpa.csv");
            }
            return null;
        }
        public Instance WaitFor(string Element)
        {
            WaitIsReady();
            if (Exist(Element)) return this;
            throw new Exception("Failed locating " + Element);
        }
        public bool Exist(string Element)
        {
            WaitIsReady();
            if (string.IsNullOrEmpty(Dump("exist(\"" + Element + "\")"))) return false;
            return true;
        }
        public bool Present(string Element)
        {
            WaitIsReady();
            if (string.IsNullOrEmpty(Dump("present('" + Element.Replace("'", "\"") + "')"))) return false;
            return true;
        }
        public Instance Mouse(out int X, out int Y)
        {
            WaitIsReady();
            X = -1; Y = -1;
            var result = Dump("mouse_xy()");
            if (result.Contains(","))
            {
                X = int.Parse(result.Replace("(", "").Split(',')[0]);
                Y = int.Parse(result.Replace(")", "").Split(',')[1]);
            }
            return this;
        }
        public Instance MouseX(out int X)
        {
            WaitIsReady();
            X = int.Parse(Dump("mouse_x()"));
            return this;
        }
        public Instance MouseY(out int Y)
        {
            WaitIsReady();
            Y = int.Parse(Dump("mouse_y()"));
            return this;
        }
        public Instance Popup(string Element)
        {
            WaitIsReady();
            Send("popup " + Element);
            return Noop();
        }
        public Instance Frame(string Element)
        {
            WaitIsReady();
            Send("frame " + Element);
            return Noop();
        }
        public Instance Download(string URL, string Filename)
        {
            WaitIsReady();
            Send("download " + URL + " to " + Filename);
            return Noop();
        }
        public Instance Upload(string Element, string Filename)
        {
            WaitIsReady();
            Send("upload " + Element + " to " + Filename);
            return Noop();
        }
        public string API(string URL)
        {
            throw new NotSupportedException("API seem to not be supported in LIVE mode");
            //WaitIsReady();
            //Send("api " + URL);
            //return Dump("api_result");
        }
        public Instance Echo(string Expression)
        {
            WaitIsReady();
            Send("echo " + Expression);
            return Noop();
        }
        public string Read(string Element)
        {
            WaitIsReady();
            Send("read " + Element + " to dummyvar");
            return Dump("dummyvar");
        }
        public string Read(int X, int Y, int Width, int Height)
        {
            WaitIsReady();
            Send(string.Format("read ({0},{1})-({2},{3}) to taguisharp", X, Y, X + Width, Y + Height));
            return Dump("taguisharp");
        }
        public bool Check(string Element)
        {
            throw new NotSupportedException("step not supported in live mode, there is no conditions language parser");
        }
        public string URL()
        {
            return Dump("url()");
        }
        public string Clipboard()
        {
            return Dump("clipboard()");
        }
        public Instance Snap(string Element, string Filename)
        {
            WaitIsReady();
            Send("snap " + Element + " to " + Filename);
            return Noop();
        }
        public string Dump(string command)
        {
            WaitIsReady();
            outbuffer = "";
            if (System.IO.File.Exists("rpa_csharp.txt")) System.IO.File.Delete("rpa_csharp.txt");
            Send("dump `" + command + "` to rpa_csharp.txt");
            while (outbuffer.Length == 0 && !System.IO.File.Exists("rpa_csharp.txt")) System.Threading.Thread.Sleep(500);
            outbuffer = "";
            if (System.IO.File.Exists("rpa_csharp.txt"))
            {
                var result = System.IO.File.ReadAllText("rpa_csharp.txt");
                System.IO.File.Delete("rpa_csharp.txt");
                if (string.IsNullOrEmpty(result) || result == "null" || result == "null" + Environment.NewLine) return null;
                return result;
            }
            return null;
        }
        public Instance Wait(int seconds)
        {
            System.Threading.Thread.Sleep(seconds * 1000);
            return this;
        }
        public Instance Noop()
        {
            Send("dummyvar=\"OpenIAP\"");
            Dump("dummyvar");
            outbuffer = "";
            return this;
        }
        private bool isReady = false;
        public void WaitIsReady()
        {
            if (isReady) return;
            while (!outbuffer.Contains("LIVE MODE") && !outbuffer.Contains("ERROR")) System.Threading.Thread.Sleep(500);
            if (outbuffer.Contains("ERROR")) throw new Exception(outbuffer);
            isReady = true;
        }
        private string outbuffer = "";
        public void Send(string command)
        {
            tagui.StandardInput.WriteLine(command);
            if (EchoSendCommand) onOutput?.Invoke(this, new OutputEventArgs(command));
        }
        private void Tagui_Exited(object sender, EventArgs e)
        {
            isReady = false;
            try
            {
                onOutput?.Invoke(this, new OutputEventArgs("EXITED WITH " + tagui?.ExitCode));
            }
            catch (Exception)
            {
            }
        }
        public bool OutputSkipBlank { get; set; } = true;
        public bool EchoSendCommand { get; set; } = false;
        private void Tagui_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            outbuffer += e.Data;
            if (OutputSkipBlank && (string.IsNullOrEmpty(e.Data) || e.Data == Environment.NewLine)) return;
            onOutput?.Invoke(this, new OutputEventArgs(e.Data));
        }
        public void Dispose()
        {
            if (tagui != null)
            {
                try
                {
                    if (!tagui.HasExited)
                    {
                        Send("done");
                        tagui.WaitForExit(3000);
                        if (!tagui.HasExited) tagui.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                tagui.Dispose();
                tagui = null;
            }
        }
    }
}
