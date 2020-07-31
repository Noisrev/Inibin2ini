using Fantome.Libraries.League.IO.Inibin;
using Inibin2ini.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Inibin_To_ini
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { @"E:\2020.1.13.bak\source\repos\twq.troybin" };
            if (args != null)
            {
                if (args.Length > 0)
                {
                    if (args[0] != null && args[0] != "" && args[0] != string.Empty)
                    {
                        InibinFile ini = new InibinFile(args[0]);
                        Dictionary<uint, string> headHash = new Dictionary<uint, string>();
                        for (int i = 0; i < 20; i++)
                        {
                            headHash.Add(SectionHash("System", "GroupPart" + i.ToString()), "GroupPart" + i.ToString());
                        }
                        Dictionary<string, Dictionary<string, string>> sets = ini.Sets.ContainsKey(InibinFlags.StringList) ? (from KeyValuePair<uint, object> i in ini.Sets[InibinFlags.StringList].Properties where headHash.ContainsKey(i.Key) select i.Value.ToString()).ToDictionary(key => key, value => new Dictionary<string, string>()) : new Dictionary<string, Dictionary<string, string>>();
                        //Add Head Hash
                        sets.Add("default", new Dictionary<string, string>());
                        sets.Add("System", new Dictionary<string, string>());
                        //Load Hashtable
                        string[] keys = System.Text.Encoding.Default.GetString(Resources.inibin2).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        // head
                        Dictionary<uint, string> keyHead = new Dictionary<uint, string>();
                        Dictionary<uint, string> hashMap = new Dictionary<uint, string>();
                        foreach (var head in sets.Keys)
                        {
                            foreach (var key in keys)
                            {
                                uint hash = SectionHash(head, key);
                                if (!keyHead.ContainsKey(hash))
                                {
                                    keyHead.Add(hash, head);
                                }
                                if (!hashMap.ContainsKey(hash))
                                {
                                    hashMap.Add(hash, key);
                                }
                            }
                        }
                        sets.Add("Unknown", new Dictionary<string, string>());
                        foreach (var flag in ini.Sets.Keys)
                        {
                            foreach (var i in ini.Sets[flag].Properties)
                            {
                                string headstr = keyHead.ContainsKey(i.Key) ? keyHead[i.Key] : "Unknown";
                                string value = string.Empty;
                                if (flag == InibinFlags.FixedPointFloatListVec2 || flag == InibinFlags.FixedPointFloatListVec3 || flag == InibinFlags.FixedPointFloatListVec4 || flag == InibinFlags.Float32ListVec2 || flag == InibinFlags.Float32ListVec3 || flag == InibinFlags.Float32ListVec4)
                                {
                                    value = "[";
                                    foreach (var c in (Array)i.Value)
                                    {
                                        value += c.ToString() + ",";
                                    }
                                    value = value.Substring(0, value.Length - 1) + "]";
                                }
                                else
                                {
                                    value = i.Value.ToString();
                                }
                                sets[headstr].Add(hashMap.TryGetValue(i.Key, out string result) ? result : i.Key.ToString(), value);
                            }
                        }
                        List<string> lines = new List<string>();
                        foreach (var item in sets)
                        {
                            if (item.Value.Count != 0)
                            {
                                lines.Add($"[{item.Key}]");
                                foreach (var keyValuePair in item.Value)
                                {
                                    lines.Add($"{keyValuePair.Key}={keyValuePair.Value}");
                                }
                            }
                        }
                        if (args.Length > 1)
                        {
                            var path = string.Empty;
                            path = args[1].Replace(@"\", "/");
                            path = path.Substring(0, path.Length - (path.Split('/').Last().Length + 1));
                            Directory.CreateDirectory(path);
                            File.WriteAllLines(args[1], lines);
                            Console.WriteLine("Ouput :" + args[1]);
                            Console.WriteLine();
                            Console.WriteLine("press any key to exit......");
                            Console.ReadKey(true);
                        }
                        else
                        {
                            Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace(@"\", "/") + "/out");
                            File.WriteAllLines(System.IO.Directory.GetCurrentDirectory().Replace(@"\", "/") + "/out/" + Path.GetFileNameWithoutExtension(args[0]) + ".ini", lines);
                            Console.WriteLine("Ouput :" + Directory.GetCurrentDirectory().Replace(@"\", "/") + "/out/" + Path.GetFileNameWithoutExtension(args[0]) + ".ini");
                            Console.WriteLine();
                            Console.WriteLine("press any key to exit......");
                            Console.ReadKey(true);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No file import!");
                    Console.WriteLine();
                    Console.WriteLine("usage: inibin2ini <InputFilePath> <OuputFilePath>");
                    Console.WriteLine("And you can drag the file to open , Ouput File: out/InputFileName.ini");
                    Console.WriteLine();
                    Console.WriteLine("press any key to exit......");
                    Console.ReadKey(true);
                }
            }
        }
        public static UInt32 SectionHash(string section, string property)
        {
            UInt32 hash = 0;
            section = section.ToLower();
            property = property.ToLower();
            for (int i = 0; i < section.Length; i++)
            {
                hash = section[i] + 65599 * hash;
            }
            hash = (65599 * hash + 42);
            for (int i = 0; i < property.Length; i++)
            {
                hash = property[i] + 65599 * hash;
            }
            return hash;
        }
    }
}
