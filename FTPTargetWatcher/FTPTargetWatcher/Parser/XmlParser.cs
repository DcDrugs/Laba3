using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Xml
{
    class Parser
    {
        public static Document Load(StreamReader input)
        {
            var number = Split(Split(input.ReadLine(), "\"").Item2, "\"").Item1;
            if (int.Parse(number[0].ToString()) == 1)
                return new Document(FirstLoadNode(input));
            else
                return new Document(SecondLoadNode(input));
        }
        private static (string, string) Split(string line, string by = "\n")
        {
            int pos = line.IndexOf(by);
            if (pos == -1)
                pos = line.Length;
            string left = line.Substring(0, pos);

            if (pos < line.Length && pos + 1 < line.Length)
            {
                return (left, line.Substring(pos + by.Length));
            }
            else
            {
                return (left, string.Empty);
            }
        }

        private static string Lstrip(ref string line)
        {
            while (line != string.Empty && char.IsWhiteSpace(line[0]))
            {
                line = line.Remove(0,1);
            }
            return line;
        }

        private static string Unquote(string value)
        {
            if (value != string.Empty && value.First() == '"')
            {
                value = value.Remove(0, 1);
            }
            if (value != string.Empty && value.Last() == '"')
            {
                value.Remove(value.Length - 2);
            }
            return value;
        }

        private static Node SecondLoadNode(StreamReader input)
        {
            string rootName = input.ReadLine();

            var root = new Node(rootName.Substring(1, rootName.Length - 2), new Dictionary<string, string>());
            for (string line; (line = input.ReadLine()) != string.Empty && Lstrip(ref line)[1] != '/';)
            {
                var(nodeName, attrs) = Split(Lstrip(ref line), " ");
                attrs = Split(attrs, ">").Item1;
                var nodeAttrs = new Dictionary<string, string>();
                while (attrs != string.Empty)
                {
                    var (head, tail) = Split(attrs, "\" ");
                    var (name, value) = Split(head, "=");
                    if (name != string.Empty && value != string.Empty)
                    {
                        nodeAttrs[Unquote(name)] = Unquote(value);
                    }
                    attrs = tail;
                }

                root.AddChild(new Node(nodeName.Substring(1), nodeAttrs));
            }
            return root;
        }

        private static Node FirstLoadNode(StreamReader input, string rootName = "")
        {
            if(rootName == string.Empty)
                rootName = input.ReadLine();

            var root = new Node(rootName.Substring(1, rootName.Length - 2), new Dictionary<string, string>());
            var nodeAttrs = root.GetDictionary();
            for (string line; (line = input.ReadLine()) != string.Empty && Lstrip(ref line)[1] != '/';)
            {
                if (line == string.Concat("</", rootName.Substring(1)))
                    break;
                if (line == Split(line, "</").Item1)
                {
                    root.AddChild(FirstLoadNode(input, line));
                    continue;
                }
                var (name, closeAction) = Split(Lstrip(ref line), "</");
                var (nodeName, value) = Split(name, ">");
                nodeName = nodeName.Substring(1, nodeName.Length - 1);
                if (nodeName != closeAction.Substring(0, closeAction.LastIndexOf('>')))
                    throw new ArgumentException("Error open/close tag:"
                        + nodeName + "!=" + closeAction.Substring(0, closeAction.Length - 2));
                if (name != string.Empty && value != string.Empty)
                {
                    nodeAttrs[nodeName] = value;
                }
            }
            return root;
        }
    }

    class Node
    {
        private string _name;
        private List<Node> _children;
        private Dictionary<string, string> _attrs;

        class FindNodeClass
        {
            private string _str;

            public FindNodeClass(string str)  { _str = str; }
            public bool FindNode(Node lhs)
            {
                return lhs._name == _str;
            }
        }

        public Node(string name, Dictionary<string, string> attrs)
        {
            _children = new List<Node>();
            _name = name;
            _attrs = attrs;
        }

        public static Predicate<Node> Find(string name)
        {
            var find = new FindNodeClass(name);
            return find.FindNode;
        }

        public ref List<Node> Children()
        {
            return ref _children;
        }

        public ref Dictionary<string, string> GetDictionary()
        {
            return ref _attrs;
        }
        public void AddChild(Node node)
        {
            _children.Add(node);
        }
        public string Name()
        {
            return _name;
        }

        public T AttributeValue<T>(string name)
        {
            return (T)Convert.ChangeType(_attrs[name], typeof(T));
        }
    };

    class Document
    {
        private readonly Node _root;
        public Document(Node root)
        {
            _root = root;
        }

        public Node GetRoot()
        {
            return _root;
        }

    };

}