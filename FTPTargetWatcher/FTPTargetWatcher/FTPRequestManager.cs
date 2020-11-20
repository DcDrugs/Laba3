using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Text;

namespace FTPTargetWatcher
{
    public partial class FTPRequestManager : ServiceBase
    {
        private Logger _logger;
        private string _sourcePath;
        private string _targetPath;
        Dictionary<ArchiveCryptMode, string> mods;

        public FTPRequestManager()
        {
        }

        protected override void OnStart(string[] args)
        {
            var parser = new Parser.ParserManager();
            parser.MakeParsed(args[0]);
            _sourcePath = parser.SourcetPath;
            _targetPath = parser.TargetPath;
            if (_sourcePath == null || _targetPath == null)
                throw new NullReferenceException("Error path");
            mods = parser.GetMods();

            _logger = new Logger(_targetPath, _sourcePath, mods);
            Thread loggerThread = new Thread(new ThreadStart(_logger.Start));
            loggerThread.Start();
        }

        protected override void OnStop()
        {
            _logger.Stop();
            Thread.Sleep(1000);
        }
    }
    public enum ArchiveCryptMode
    {
        Archive,
        Dearchive,
        Encrypt,
        ArchiveAndEcrypt,
        Decrypt
    }

    delegate void ProcessHandler(string directoryName);
    delegate void AddDirectoryNameHandler(string name);
    class Logger
    {
        private FileSystemWatcher _watcher;
        private static CryptoManager _crypto = new CryptoManager();
        private string _sourcePath;
        private bool _enabled = true;
        private static Options.ArchiveCryptManager _manager = new Options.ArchiveCryptManager(ref _crypto);
        private event ProcessHandler _Processed;
        private static readonly Dictionary<ArchiveCryptMode, AddDirectoryNameHandler> _addDirectoryName
            = new Dictionary<ArchiveCryptMode, AddDirectoryNameHandler>
            {
                { ArchiveCryptMode.Archive, _manager.archivator.SetArchiveName},
                { ArchiveCryptMode.Dearchive,  _manager.archivator.SetDearchiveName},
                { ArchiveCryptMode.Encrypt,  _manager.cryptor.SetEncryptName },
                { ArchiveCryptMode.ArchiveAndEcrypt,  _manager.archiveCrypt.SetArchiveAndEcryptName },
                { ArchiveCryptMode.Decrypt, _manager.cryptor.SetDecryptName }
            };
        private static readonly Dictionary<ArchiveCryptMode, ProcessHandler> _operation
            = new Dictionary<ArchiveCryptMode, ProcessHandler>
            {
                { ArchiveCryptMode.Archive,  _manager.ProcessCompress},
                { ArchiveCryptMode.Dearchive,  _manager.ProcessDecompress},
                { ArchiveCryptMode.Encrypt,  _manager.ProcessEncrypt},
                { ArchiveCryptMode.Decrypt, _manager.ProcessDecrypt},
                { ArchiveCryptMode.ArchiveAndEcrypt,  _manager.ProcessArchiveAndEcrypt}
            };

        public Logger(string targetPath, string sourcePath, Dictionary<ArchiveCryptMode, string> mods)
        {
            _sourcePath = sourcePath;
            _watcher = new FileSystemWatcher(targetPath);
            _watcher.Created += WatcherCreated;
            _manager.path.MakePath(targetPath, sourcePath);
            foreach (var mod in mods)
            {
                _Processed += _operation[mod.Key];
                _addDirectoryName[mod.Key](mod.Value);
            }
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
            while (_enabled)
            {
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _enabled = false;
        }
        private void WatcherCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                var name = new Options.FileOption(_Processed, e.Name);
                Thread loggerThread = new Thread(new ThreadStart(name.Process));
                loggerThread.Start();
            }
            catch (Exception ex)
            {
                using (var file = new FileStream(Path.Combine(_sourcePath, e.Name + "_" + DateTime.Now + ".txt"), FileMode.Create))
                {
                    file.Write(Encoding.ASCII.GetBytes(ex.Message), 0, ex.Message.Length);
                }
            }
        }

    }
}