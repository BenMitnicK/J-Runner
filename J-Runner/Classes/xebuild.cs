﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace JRunner.Classes
{

    class xebuild
    {
        public enum hacktypes
        {
            nothing = 0,
            retail = 1,
            glitch = 2,
            jtag = 3,
            glitch2 = 4,
            glitch2m = 5
        }
        public enum XebuildError
        {
            none = 0,
            nocpukey,
            nofile,
            filemissing,
            nobinfile,
            nodash,
            noconsole,
            noinis,
            nobootloaders,
            wrongcpukey
        }

        private string _cpukey;
        private hacktypes _ttype;
        private int _dash;
        private int _customCB = 0;
        private consoles _ctype;
        private bool _audclamp;
        private bool _CR4;
        private bool _altoptions;
        private bool _DLpatches;
        private bool _includeLaunch;
        private bool _rjtag;
        private bool _nowrite;
        private bool _noava;
        private bool _clean;
        private bool _noreeb;
        private Nand.PrivateN _nand;
        private List<String> _patches;

        public void loadvariables(string cpukey, hacktypes ttype, int dash, consoles ctype, List<String> patches, Nand.PrivateN nand)
        {
            loadvariables(cpukey, ttype, dash, ctype, patches, nand, false, false, false, false, false, false);
        }
        public void loadvariables(string cpukey, hacktypes ttype, int dash, consoles ctype, List<String> patches, Nand.PrivateN nand, bool altoptions, bool DLpatches, bool includeLaunch, bool audclamp, bool rjtag, bool cr4)
        {
            this._cpukey = cpukey;
            this._ttype = ttype;
            this._dash = dash;
            this._ctype = ctype;
            this._patches = patches;
            this._nand = nand;
            this._altoptions = altoptions;
            this._DLpatches = DLpatches;
            this._includeLaunch = includeLaunch;
            this._audclamp = audclamp;
            this._rjtag = rjtag;
            this._CR4 = cr4;
        }

        public List<int> getCBs()
        {
            List<int> cbs = new List<int>();
            string[] ommit = { "version", "security", "flashfs" };
            foreach (string s in parse_ini.getlabels(Path.Combine(variables.pathforit, @"xeBuild\" + _dash + @"\_" + _ttype + ".ini")))
            {
                if (!ommit.Contains(s) && s.Contains(_ctype.Ini))
                {
                    int cb;
                    if (int.TryParse(s.Replace(_ctype.Ini + "bl_", ""), out cb)) cbs.Add(cb);
                }
            }
            //int cba;
            //string normalc = (parse_ini.parselabel(Path.Combine(variables.pathforit, @"xeBuild\" + _dash + @"\_" + _ttype + ".ini"), _ctype.Ini + "bl")[0]);
            //string normalcb = normalc.Substring(normalc.IndexOf("_") + 1, normalc.IndexOf(".") - normalc.IndexOf("_") - 1);
            //if (int.TryParse(normalcb, out cba)) cbs.Add(cba);

            return cbs;
        }
        public void customCB()
        {
            Forms.customCB ccb = new Forms.customCB(getCBs());
            ccb.ShowDialog();
            _customCB = ccb.selected;
        }

        void copySMC()
        {
            if (_ttype == hacktypes.jtag && !_nand.checkifhackedSMC() && !File.Exists(Path.Combine(variables.xePath, "SMC.bin")))
            {
                if (_ctype.ID == 2)
                {
                    if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                    else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);

                }
                else if (_ctype.ID == 3)
                {
                    if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                    else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 8)
                {
                    File.Copy(variables.xePath + "SMCx.bin", variables.xePath + "SMC.bin", true);
                }
                else
                {
                    if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                    else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);
                }

                if (_rjtag && _ctype.ID != 8)
                {
                    File.WriteAllBytes(variables.xePath + "SMC.bin", Nand.Nand.patch_SMC((File.ReadAllBytes(variables.xePath + "SMC.bin"))));
                }
            }
            else if (_ttype == hacktypes.glitch2 && _CR4 && !File.Exists(Path.Combine(variables.xePath, "SMC.bin")))
            {
                if (_ctype.ID == 1)
                {
                    File.Copy(variables.xePath + "TRINITY_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 2 || _ctype.ID == 9)
                {
                    File.Copy(variables.xePath + "FALCON_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 4 || _ctype.ID == 5 || _ctype.ID == 6 || _ctype.ID == 7)
                {
                    File.Copy(variables.xePath + "JASPER_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 10 || _ctype.ID == 11)
                {
                    File.Copy(variables.xePath + "CORONA_CR4.bin", variables.xePath + "SMC.bin", true);
                }
            }
        }
        void copySMCcustom()
        {
            if (_ttype == hacktypes.jtag)
            {
                if (File.Exists(Path.Combine(variables.xePath, "SMC.bin")) && (!Nand.Nand.checkifhackedSMC(File.ReadAllBytes(Path.Combine(variables.xePath, "SMC.bin")))))
                {

                    if (_ctype.ID == 2)
                    {
                        if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                        else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);

                    }
                    else if (_ctype.ID == 3)
                    {
                        if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                        else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);
                    }
                    else if (_ctype.ID == 8)
                    {
                        File.Copy(variables.xePath + "SMCx.bin", variables.xePath + "SMC.bin", true);
                    }
                    else
                    {
                        if (_audclamp) File.Copy(variables.xePath + "SMCaud.bin", variables.xePath + "SMC.bin", true);
                        else File.Copy(variables.xePath + "SMCfzj.bin", variables.xePath + "SMC.bin", true);
                    }

                    if (_rjtag && _ctype.ID != 8)
                    {
                        File.WriteAllBytes(variables.xePath + "SMC.bin", Nand.Nand.patch_SMC((File.ReadAllBytes(variables.xePath + "SMC.bin"))));
                    }
                }
            }
            else if (_CR4)
            {
                if (_ctype.ID == 1)
                {
                    File.Copy(variables.xePath + "TRINITY_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 2 || _ctype.ID == 9)
                {
                    File.Copy(variables.xePath + "FALCON_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 4 || _ctype.ID == 5 || _ctype.ID == 6 || _ctype.ID == 7)
                {
                    File.Copy(variables.xePath + "JASPER_CR4.bin", variables.xePath + "SMC.bin", true);
                }
                else if (_ctype.ID == 10 || _ctype.ID == 11)
                {
                    File.Copy(variables.xePath + "CORONA_CR4.bin", variables.xePath + "SMC.bin", true);
                }
            }
        }

        void checkDashLaunch()
        {
            if (_DLpatches && _includeLaunch)
            {
                if (!File.Exists(Path.Combine(variables.launchpath, _dash + @"\launch.ini")))
                {
                    if (File.Exists(Path.Combine(variables.launchpath, "launch.ini")))
                        System.IO.File.Copy(Path.Combine(variables.launchpath, "launch.ini"), Path.Combine(variables.launchpath, _dash + @"\launch.ini"), true);
                    else if (File.Exists(Path.Combine(variables.launchpath, "launch_default.ini")))
                        System.IO.File.Copy(Path.Combine(variables.launchpath, @"launch_default.ini"), Path.Combine(variables.launchpath, _dash + @"\launch.ini"), true);
                }
            }
            edittheini();
        }
        void edittheini()
        {
            Console.WriteLine(_dash);
            foreach (hacktypes type in Enum.GetValues(typeof(hacktypes)))
            {
                if (type == hacktypes.retail || type == hacktypes.nothing) continue;
                string file = Path.Combine(variables.pathforit, @"xeBuild\" + _dash + @"\_" + type + ".ini");
                string[] writepatches = { @"..\launch.xex", @"..\lhelper.xex", @"..\launch.ini" };
                string[] writepatches2 = { @"..\launch.xex", @"..\lhelper.xex" };
                string[] empty = { };

                if (File.Exists(file))
                {
                    if (_DLpatches)
                    {
                        parse_ini.edit_ini(file, _includeLaunch ? writepatches : writepatches2, empty);
                    }
                    else
                    {
                        parse_ini.edit_ini(file, empty, writepatches);
                    }
                }
                else Console.WriteLine("Couldn't add dashlaunch patches to {0}", file);
            }
        }
        void savekvinfo(string savefile)
        {
            try
            {
                if (!_nand.ok) return;
                TextWriter tw = new StreamWriter(savefile);
                tw.WriteLine("*******************************************");
                tw.WriteLine("*******************************************");
                string console_type = _ctype.Text;
                tw.WriteLine("Console Type: {0}", console_type);
                tw.WriteLine("");
                tw.WriteLine("Cpu Key: {0}", _cpukey);
                tw.WriteLine("");
                tw.WriteLine("KV Type: {0}", _nand.ki.kvtype.Replace("0", ""));
                tw.WriteLine("");
                tw.WriteLine("MFR Date: {0}", _nand.ki.mfdate);
                tw.WriteLine("");
                tw.WriteLine("Console ID: {0}", _nand.ki.consoleid);
                tw.WriteLine("");
                tw.WriteLine("Serial: {0}", _nand.ki.serial);
                tw.WriteLine("");
                string region = "";
                if (_nand.ki.region == "02FE") region = "PAL/EU";
                else if (_nand.ki.region == "00FF") region = "NTSC/US";
                else if (_nand.ki.region == "01FE") region = "NTSC/JAP";
                else if (_nand.ki.region == "01FF") region = "NTSC/JAP";
                else if (_nand.ki.region == "01FC") region = "NTSC/KOR";
                else if (_nand.ki.region == "0101") region = "NTSC/HK";
                else if (_nand.ki.region == "0201") region = "PAL/AUS";
                else if (_nand.ki.region == "7FFF") region = "DEVKIT";
                tw.WriteLine("Region: {0} | {1}", _nand.ki.region, region);
                tw.WriteLine("");
                tw.WriteLine("Osig: {0}", _nand.ki.osig);
                tw.WriteLine("");
                tw.WriteLine("DVD Key: {0}", _nand.ki.dvdkey);
                tw.WriteLine("");
                tw.WriteLine("*******************************************");
                tw.WriteLine("*******************************************");
                tw.Close();
                Console.WriteLine("KV Info saved to file");
            }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); Console.WriteLine("Failed"); }
        }

        XebuildError doSomeChecks()
        {
            if (String.IsNullOrEmpty(_cpukey)) return XebuildError.nocpukey;

            if (_ctype.ID == -1) return XebuildError.noconsole;
            if (_dash == 0) return XebuildError.nodash;
            string ini = (variables.launchpath + @"\" + _dash + @"\_" + _ttype + ".ini");
            if (!File.Exists(ini)) return XebuildError.noinis;
            if (!parse_ini.getlabels(ini).Contains(_ctype.Ini + "bl")) return XebuildError.nobootloaders;

            return XebuildError.none;
        }
        void moveXell()
        {
            if (_ttype != hacktypes.retail)
            {
                File.Copy(Path.Combine(variables.pathforit, @"common\xell\xell-2f.bin"), Path.Combine(variables.pathforit, @"xebuild\data\xell-2f.bin"), true);
                File.Copy(Path.Combine(variables.pathforit, @"common\xell\xell-gggggg.bin"), Path.Combine(variables.pathforit, @"xebuild\data\xell-gggggg.bin"), true);
            }
        }
        void moveOptions()
        {
            if (_altoptions)
            {
                Console.WriteLine("Using edited settings");
                File.Copy(Path.Combine(variables.pathforit, @"xebuild\options_edited.ini"), Path.Combine(variables.pathforit, @"xebuild\data\options.ini"), true);
            }
            else
            {
                File.Copy(Path.Combine(variables.pathforit, @"xebuild\options.ini"), Path.Combine(variables.pathforit, @"xebuild\data\options.ini"), true);
            }
        }

        public XebuildError createxebuild(bool custom)
        {
            XebuildError result = XebuildError.none;
            result = doSomeChecks();
            if (result != XebuildError.none) return result;
            moveXell();
            moveOptions();
            if (variables.changeldv != 0)
            {
                string cfldv = "cfldv=" + variables.highldv.ToString();
                string[] edit = { cfldv };
                string[] delete = { };
                parse_ini.edit_ini(Path.Combine(variables.pathforit, @"xeBuild\data\options.ini"), edit, delete);
            }

            Console.WriteLine("Load Files Initiliazation Finished");
            if (!custom) copySMC();
            else copySMCcustom();
            checkDashLaunch();

            if (variables.changeldv != 0)
            {
                string cfldv = "cfldv=" + variables.highldv.ToString();
                string[] edit = { cfldv };
                string[] delete = { };
                parse_ini.edit_ini(Path.Combine(variables.pathforit, @"xeBuild\data\options.ini"), edit, delete);
            }

            Console.WriteLine("Started Creation of the {0} xebuild image", _dash);


            variables.xefolder = Path.Combine(Directory.GetParent(variables.outfolder).FullName, _nand.ki.serial);
            if (variables.debugme) Console.WriteLine("outfolder: {0}", variables.xefolder);
            if (!Directory.Exists(variables.xefolder)) Directory.CreateDirectory(variables.xefolder);
            System.IO.File.WriteAllText(System.IO.Path.Combine(variables.xefolder, variables.cpukeypath), _cpukey);
            savekvinfo(Path.Combine(variables.xefolder, "KV_Info.txt"));
            if (variables.changeldv != 0) variables.changeldv = 2;

            return result;
        }

        public void build()
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = variables.pathforit + @"\xeBuild\xeBuild.exe";
            string arguments = "";
            arguments = "-t " + _ttype;
            arguments += " -c " + _ctype.XeBuild;
            foreach (String patch in _patches)
            {
                arguments += " " + patch;
            }
            if (variables.debugme) arguments += " -v";
            arguments += " -noenter";
            arguments += " -f " + _dash;
            arguments += " -d data";
            arguments += " \"" + variables.xefolder + "\\" + variables.nandflash + "\" ";

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            arguments = regex.Replace(arguments, @" ");

            if (variables.debugme) Console.WriteLine(variables.pathforit);
            if (variables.debugme) Console.WriteLine("---" + variables.pathforit + @"\xeBuild\xeBuild.exe");
            if (variables.debugme) Console.WriteLine(arguments);
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WorkingDirectory = variables.pathforit;
            pProcess.StartInfo.RedirectStandardInput = true;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Exited += new EventHandler(xeExit);
            //pProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(DataReceived);
            //pProcess.Exited += new EventHandler(xe_Exited);
            //pProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            try
            {
                AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                pProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine(e.Data);
                        if (e.Data != null && e.Data.Contains("image built")) { variables.xefinished = true; }
                    }
                };
                pProcess.Start();
                pProcess.BeginOutputReadLine();
                pProcess.StandardInput.WriteLine("enter");
                pProcess.WaitForExit();

                if (pProcess.HasExited)
                {
                    pProcess.CancelOutputRead();
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }


        }

        public void build(string arguments)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = variables.pathforit + @"\xeBuild\xeBuild.exe";

            if (variables.debugme) Console.WriteLine(variables.pathforit);
            if (variables.debugme) Console.WriteLine("---" + variables.pathforit + @"\xeBuild\xeBuild.exe");
            if (variables.debugme) Console.WriteLine(arguments);
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WorkingDirectory = variables.pathforit;
            pProcess.StartInfo.RedirectStandardInput = true;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Exited += new EventHandler(xeExit);
            //pProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(DataReceived);
            //pProcess.Exited += new EventHandler(xe_Exited);
            //pProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            try
            {
                AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                pProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine(e.Data);
                        if (e.Data != null && e.Data.Contains("image built")) { variables.xefinished = true; }
                    }
                };
                pProcess.Start();
                pProcess.BeginOutputReadLine();
                pProcess.StandardInput.WriteLine("enter");
                pProcess.WaitForExit();

                if (pProcess.HasExited)
                {
                    pProcess.CancelOutputRead();
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }


        }


        ////////////////////////////////////////////////


        public void Uloadvariables(int dash, hacktypes ttype, List<String> patches, bool altoptions, bool nowrite, bool noava, bool clean, bool noreeb, bool DLpatches, bool includeLaunch)
        {
            this._dash = dash;
            this._ttype = ttype;
            this._patches = patches;
            this._nowrite = nowrite;
            this._noava = noava;
            this._clean = clean;
            this._altoptions = altoptions;
            this._noreeb = noreeb;
            this._DLpatches = DLpatches;
            this._includeLaunch = includeLaunch;
        }

        public XebuildError createxebuild()
        {
            XebuildError result = XebuildError.none;
            if (_dash == 0) result = XebuildError.nodash;
            if (result != XebuildError.none) return result;
            //File.Copy(Path.Combine(variables.pathforit, @"common\xell\xell-2f.bin"), Path.Combine(variables.pathforit, @"xeBuild\xell-2f.bin"), true);
            //File.Copy(Path.Combine(variables.pathforit, @"common\xell\xell-gggggg.bin"), Path.Combine(variables.pathforit, @"xeBuild\xell-gggggg.bin"), true);
            moveOptions();
           
            Console.WriteLine("Load Files Initiliazation Finished");
            checkDashLaunch();

            Console.WriteLine("Started Updating Console to {0}", _dash);
            variables.xefolder = variables.outfolder;

            return result;
        }

        public void update()
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = variables.pathforit + @"\xeBuild\xeBuild.exe";
            string arguments = "update ";
            foreach (String patch in _patches)
            {
                arguments += " " + patch;
            }
            if (variables.debugme) arguments += " -v";
            if (_noava) arguments += " -noava";
            if (_nowrite) arguments += " -nowrite";
            if (_clean) arguments += " -clean";
            if (_noreeb) arguments += " -noreeb";
            arguments += " -noenter";
            arguments += " -f " + _dash;
            arguments += " -d ";
            arguments += "\"" + variables.outfolder + "\"";

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            arguments = regex.Replace(arguments, @" ");

            if (variables.debugme) Console.WriteLine(variables.pathforit);
            if (variables.debugme) Console.WriteLine("---" + variables.pathforit + @"\xeBuild\xeBuild.exe");
            if (variables.debugme) Console.WriteLine(arguments);
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WorkingDirectory = variables.pathforit;
            pProcess.StartInfo.RedirectStandardInput = true;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Exited += new EventHandler(xeExit);
            try
            {
                AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                pProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine(e.Data);
                        if (e.Data != null && e.Data.Contains("image built")) { variables.xefinished = true; }
                    }
                };
                pProcess.Start();
                pProcess.BeginOutputReadLine();
                pProcess.WaitForExit();

                if (pProcess.HasExited)
                {
                    pProcess.CancelOutputRead();
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }


        }

        public void client(string args)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = variables.pathforit + @"\xeBuild\xeBuild.exe";
            string arguments = "client ";
            arguments += args;
            if (variables.debugme) arguments += " -v";
            arguments += " -noenter";

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            arguments = regex.Replace(arguments, @" ");

            if (variables.debugme) Console.WriteLine(variables.pathforit);
            if (variables.debugme) Console.WriteLine("---" + variables.pathforit + @"\xeBuild\xeBuild.exe");
            if (variables.debugme) Console.WriteLine(arguments);
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WorkingDirectory = variables.pathforit;
            pProcess.StartInfo.RedirectStandardInput = true;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            //pProcess.Exited += new EventHandler(xeExit);
            try
            {
                AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                pProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                pProcess.Start();
                pProcess.BeginOutputReadLine();
                pProcess.WaitForExit();

                if (pProcess.HasExited)
                {
                    pProcess.CancelOutputRead();
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }


        }

        public delegate void xeExited(object sender, EventArgs e);
        public event xeExited xeExit;


    }
}
