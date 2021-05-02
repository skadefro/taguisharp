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
            result.end_processes();
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
            }
        }
        public void end_processes()
        {
            isReady = false;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var end_processes_exec = System.IO.Path.Combine(home, "tagui/src/end_processes");
            if (tagui != null)
            {
                try
                {
                    if (!tagui.HasExited) tagui.Kill();
                }
                catch (Exception)
                {
                }
                tagui.Dispose();
                tagui = null;
            }
            var ps = new System.Diagnostics.Process();
            ps.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = false;
            ps.StartInfo.FileName = "cmd.exe";
            ps.StartInfo.Arguments = "/c " + end_processes_exec;
            ps.StartInfo.UseShellExecute = false;
            ps.OutputDataReceived += Tagui_OutputDataReceived;
            ps.Start();
            ps.WaitForExit(10000);
        }
        public void CreateProcess(bool headless, bool nobrowser, bool quiet, bool preservelogfiles)
        {
            isReady = false;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var tagui_exec = System.IO.Path.Combine(home, "tagui/src/tagui.cmd");
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
            //tagui.StartInfo.FileName = "cmd.exe";
            //tagui.StartInfo.Arguments = "/c " + tagui_exec + " live";
            tagui.StartInfo.UseShellExecute = false;
            tagui.StartInfo.WorkingDirectory = @"C:\openiap\tagui\testtagui\bin\Debug";
            tagui.OutputDataReceived += Tagui_OutputDataReceived;
            tagui.ErrorDataReceived += Tagui_OutputDataReceived;
            tagui.Exited += Tagui_Exited;
            tagui.EnableRaisingEvents = true;
            tagui.Start();
            tagui.BeginOutputReadLine();
        }
        public Instance OpenURL(string URL)
        {
            WaitIsReady();
            tagui.StandardInput.WriteLine(URL);
            return Noop();
        }
        public Instance Click(string element)
        {
            WaitIsReady();
            tagui.StandardInput.WriteLine("click " + element);
            return Noop();
        }
        public bool Exist(string element)
        {
            WaitIsReady();
            if (string.IsNullOrEmpty(Dump("present(\"" + element + "\""))) return false;
            return true;
        }
        public Instance Type(string Element, string Input)
        {
            WaitIsReady();
            tagui.StandardInput.WriteLine("type " + Element + " as " + Input);
            return Noop();
        }
        public bool Present(string element)
        {
            WaitIsReady();
            if (string.IsNullOrEmpty(Dump("present('" + element.Replace("'", "\"") + "')"))) return false;
            return true;
        }
        public Instance Snap(string Element, string Filename)
        {
            WaitIsReady();
            tagui.StandardInput.WriteLine("snap " + Element + " to " + Filename);
            return Noop();
        }
        public string Dump(string command)
        {
            WaitIsReady();
            outbuffer = "";
            if (System.IO.File.Exists("rpa_csharp.txt")) System.IO.File.Delete("rpa_csharp.txt");
            tagui.StandardInput.WriteLine("dump `" + command + "` to rpa_csharp.txt");
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
            tagui.StandardInput.WriteLine("dummyvar=\"OpenIAP\"");
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
        private void Tagui_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            outbuffer += e.Data;
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
                        tagui.StandardInput.WriteLine("done");
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
