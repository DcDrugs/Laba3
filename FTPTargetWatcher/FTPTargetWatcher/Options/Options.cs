using System;
using System.IO;
using FTPTargetWatcher;
using System.Threading;

namespace Options
{
    class FileOption
    {
        private string _name;
        private ProcessHandler _proc;
        public FileOption(ProcessHandler proc, string name)
        {
            _proc = proc;
            _name = name;
        }

        public void Process()
        {
            _proc(_name);
        }
    }

    class PathOption
    {
        public string TargetPath { get; private set; }
        public string SourcePath { get; private set; }

        private static PathOption instance;
        private bool isEmpty = true;

        public static PathOption GetInstance()
        {
            if (instance == null)
                instance = new PathOption();
            return instance;
        }
        private PathOption() { }

        public void MakePath(string targetPath, string sourcePath)
        {
            if (isEmpty == true)
            {
                TargetPath = targetPath;
                SourcePath = sourcePath;
            }
            isEmpty = false;
        }
    }

    delegate void SetHandler(string name);

    class ArchiveOption
    {
        public string ArchiveName { get; private set; }
        public string DearchiveName { get; private set; }

        private PathOption _path;

        public ArchiveOption()
        {
            _path = PathOption.GetInstance();
        }

        public void SetArchiveName(string name)
        {
            ArchiveName = name;
            Directory.CreateDirectory(Path.Combine(_path.SourcePath, name));
        }

        public void SetDearchiveName(string name)
        {
            DearchiveName = name;
            Directory.CreateDirectory(Path.Combine(_path.SourcePath, name));
        }
    }

    class CryptOption
    {
        public string EncryptName { get; private set; }
        public string DecryptName { get; private set; }
        public CryptoManager Crypto { get; set; }

        private PathOption _path;

        public CryptOption()
        {
            _path = PathOption.GetInstance();
        }

        public void SetEncryptName(string name)
        {
            EncryptName = name;
            Directory.CreateDirectory(Path.Combine(_path.SourcePath, name));
        }

        public void SetDecryptName(string name)
        {
            DecryptName = name;
            Directory.CreateDirectory(Path.Combine(_path.SourcePath, name));
        }
    }

    class ArchiveCryptOption
    {
        public string ArchiveAndEncryptName { get; private set; }

        private PathOption _path;

        public ArchiveCryptOption()
        {
            _path = PathOption.GetInstance();
        }

        public void SetArchiveAndEcryptName(string name)
        {
            ArchiveAndEncryptName = name;
            Directory.CreateDirectory(Path.Combine(_path.SourcePath, name));
        }
    }


    class ArchiveCryptManager
    {
        public PathOption path;
        public ArchiveOption archivator;
        public CryptOption cryptor;
        public ArchiveCryptOption archiveCrypt;

        public ArchiveCryptManager(ref CryptoManager crypt)
        {
            path = PathOption.GetInstance();
            cryptor = new CryptOption();
            archivator = new ArchiveOption();
            archiveCrypt = new ArchiveCryptOption();
            cryptor.Crypto = crypt;
        }

        public void ProcessCompress(string name)
        {
            Process(name);
            Archivetor.Compress(Path.Combine(path.TargetPath, name), Path.Combine(path.SourcePath, archivator.ArchiveName, name + ".gz"));
        }

        public void ProcessDecompress(string name)
        {
            PredProcess(name + ".gz", archivator.SetArchiveName, archivator.ArchiveName);
            Archivetor.Decompress(Path.Combine(path.SourcePath, archivator.ArchiveName, name + ".gz"),
                     Path.Combine(path.SourcePath, archivator.DearchiveName, name));
        }

        public void ProcessEncrypt(string name)
        {
            Process(name);
            cryptor.Crypto.EncryptFile(Path.Combine(path.TargetPath, name), Path.Combine(path.SourcePath, cryptor.EncryptName, name));
        }

        public void ProcessDecrypt(string name)
        {
            PredProcess(name, cryptor.SetEncryptName, cryptor.EncryptName);
            cryptor.Crypto.DecryptFile(Path.Combine(path.SourcePath, cryptor.EncryptName, name), Path.Combine(path.SourcePath, cryptor.DecryptName, name));
        }

        public void ProcessArchiveAndEcrypt(string name)
        {
            PredProcess(name, cryptor.SetEncryptName, cryptor.EncryptName);
            Archivetor.Compress(Path.Combine(path.SourcePath, cryptor.EncryptName, name),
                   Path.Combine(path.SourcePath, archiveCrypt.ArchiveAndEncryptName, name + ".gz"));
        }

        public bool IsFileLocked(string file)
        {
            try
            {
                using (var stream = File.OpenRead(file)) { }
                return false;
            }
            catch (IOException)
            {
                Thread.Sleep(1000);
                Console.WriteLine(file);
                return true;
            }
        }
        public void Process(string name)
        {
            int i = 0;
            while (IsFileLocked(Path.Combine(path.TargetPath, name)))
            {
                i++;
                if (i == 1000)
                    throw new FileLoadException("Error to open file: " + name);
            }
        }

        private void CheckDirectory(SetHandler Set, string directoryName, ProcessHandler process, string fileName)
        {
            if (!Directory.Exists(Path.Combine(path.SourcePath, directoryName)))
            {
                Set("ErrorDirectory");
                process(fileName);
            }
        }

        private void PredProcess(string name, SetHandler set, string path)
        {
            CheckDirectory(set, path, ProcessEncrypt, name);
            while (IsFileLocked(Path.Combine(this.path.SourcePath, path, name))) ;
        }
    }
}
