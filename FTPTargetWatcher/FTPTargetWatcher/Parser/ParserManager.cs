using System.IO;
using System;
using System.Collections.Generic;

namespace Parser
{
    class ParserManager
    {
        private Dictionary<FTPTargetWatcher.ArchiveCryptMode, string> _mods = new Dictionary<FTPTargetWatcher.ArchiveCryptMode, string>();
        public string TargetPath { get; private set; }
        public string SourcetPath { get; private set; }

        private bool isParsed = false;

        public Dictionary<FTPTargetWatcher.ArchiveCryptMode, string> GetMods()
        {
            return _mods;
        }

        public bool IsParsed()
        {
            return isParsed;
        }
        public void MakeParsed(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var stream = new StreamReader(file))
                {
                    if (path.Substring(path.LastIndexOf(".") + 1).ToLower() == "xml")
                        TryParse(Xml.Parser.Load(stream));
                    else
                        Parse(Json.Parser.Load(stream));
                }
            }
        }
        private void TryParse(Xml.Document xmlDoc)
        {
            try
            {
                ParseFirstType(xmlDoc);

            }
            catch(Exception)
            {
                ParseSecondType(xmlDoc);
            }
        }

        private void ParseFirstType(Xml.Document xmlDoc)
        {
            TargetPath = xmlDoc.GetRoot().AttributeValue<string>("TargetPath");
            SourcetPath = xmlDoc.GetRoot().AttributeValue<string>("SourcetPath");
            foreach (var mod in xmlDoc.GetRoot().Children().Find(Xml.Node.Find("Mods")).GetDictionary())
            {
                _mods[(FTPTargetWatcher.ArchiveCryptMode)Enum.Parse(typeof(FTPTargetWatcher.ArchiveCryptMode), mod.Key)] = mod.Value;
            }
            isParsed = true;
        }

        private void ParseSecondType(Xml.Document xmlDoc)
        {
            TargetPath = xmlDoc.GetRoot().Children()[0].AttributeValue<string>("TargetPath");
            SourcetPath = xmlDoc.GetRoot().Children()[0].AttributeValue<string>("SourcetPath");
            foreach (var mod in xmlDoc.GetRoot().Children()[1].GetDictionary())
            {
                _mods[(FTPTargetWatcher.ArchiveCryptMode)Enum.Parse(typeof(FTPTargetWatcher.ArchiveCryptMode), mod.Key)] = mod.Value;
            }
            isParsed = true;
        }

        private void Parse(Json.Document jsonDoc)
        {
            TargetPath = jsonDoc.GetRoot().AsDictionary()["TargetPath"].AsString();
            SourcetPath = jsonDoc.GetRoot().AsDictionary()["SourcetPath"].AsString();
            foreach (var mod in jsonDoc.GetRoot().AsDictionary()["Mods"].AsDictionary())
            {
                _mods[(FTPTargetWatcher.ArchiveCryptMode)Enum.Parse(typeof(FTPTargetWatcher.ArchiveCryptMode), mod.Key)] = mod.Value.AsString();
            }
            isParsed = true;
        }

    }
}
