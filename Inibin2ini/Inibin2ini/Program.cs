using Inibin2ini.IO.Inibin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Inibin2ini
{
    public class Program
    {
        public static string ProcessDir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName)));
        public static void Main(string[] args)
        {
            if (args.Any())
            {
                string ResourcesDir = $"{ProcessDir}\\Resources";
                string sectionPath = $"{ResourcesDir}\\Section.txt";
                string propertyPath = $"{ResourcesDir}\\Property.txt";

                List<string> Sections = new List<string>();
                List<string> Propertys = new List<string>();

                if (File.Exists(sectionPath))
                {
                    Sections.AddRange(File.ReadAllLines(sectionPath));
                }
                if (File.Exists(propertyPath))
                {
                    Propertys.AddRange(File.ReadAllLines(propertyPath));
                }


                Dictionary<uint, (string, string)> hashMap = new Dictionary<uint, (string, string)>();
                if (Sections.Any() && Propertys.Any())
                {
                    foreach (string section in Sections)
                    {
                        foreach (string key in Propertys)
                        {
                            uint hash = InibinFile.SectionHash(section, key);
                            if (!hashMap.ContainsKey(hash))
                            {
                                hashMap.Add(hash, (section, key));
                            }
                        }
                    }
                }
                foreach (string item in args)
                {
                    InibinFile inibin = new InibinFile(item);

                    Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>();

                    if (inibin.Sets.ContainsKey(InibinFlags.StringList))
                    {
                        foreach (KeyValuePair<uint, object> pairs in inibin.Sets[InibinFlags.StringList].Properties)
                        {
                            string key = pairs.Value.ToString();
                            if (!ini.ContainsKey(key))
                            {
                                ini.Add(key, new Dictionary<string, string>());
                                foreach (string property in Propertys)//key is header?
                                {
                                    uint hash = InibinFile.SectionHash(key, property);
                                    if (!hashMap.ContainsKey(hash))
                                    {
                                        hashMap.Add(hash, (key, property));
                                    }
                                }
                            }
                        }
                    }
                    Dictionary<string, string> UnknownList = new Dictionary<string, string>();
                    foreach (KeyValuePair<InibinFlags, InibinSet> keyValue in inibin.Sets)
                    {
                        foreach (KeyValuePair<uint, object> entry in keyValue.Value.Properties)
                        {
                            string value;

                            if (keyValue.Key == InibinFlags.Float32ListVec2 ||
                                keyValue.Key == InibinFlags.Float32ListVec3 ||
                                keyValue.Key == InibinFlags.Float32ListVec4 ||
                                keyValue.Key == InibinFlags.FixedPointFloatListVec2 ||
                                keyValue.Key == InibinFlags.FixedPointFloatListVec3 ||
                                keyValue.Key == InibinFlags.FixedPointFloatListVec4)
                            {
                                value = $"[{ string.Join("", from object z in (Array)entry.Value select $"{z},").TrimEnd(',')}]";
                            }
                            else
                            {
                                value = entry.Value.ToString();
                            }


                            if (hashMap.ContainsKey(entry.Key))
                            {
                                (string, string) section = hashMap[entry.Key];
                                if (!ini.ContainsKey(section.Item1))
                                {
                                    ini.Add(section.Item1, new Dictionary<string, string>());
                                }
                                ini[section.Item1].Add(section.Item2, value);
                            }
                            else
                            {
                                UnknownList.Add(entry.Key.ToString("X"), value);
                            }
                        }
                    }
                    StringBuilder sb = new StringBuilder();

                    if (ini.Any())
                    {
                        foreach (KeyValuePair<string, Dictionary<string, string>> pairs in ini)
                        {
                            if (pairs.Value.Any())//Count
                            {
                                sb.AppendLine($"[{pairs.Key}]");//header
                                foreach (KeyValuePair<string, string> keyValuePair in pairs.Value)
                                {
                                    sb.AppendLine($"{keyValuePair.Key}={keyValuePair.Value}");
                                }
                            }
                        }
                    }

                    if (UnknownList.Any())
                    {
                        sb.AppendLine($"[Unknown]");
                        foreach (KeyValuePair<string, string> entry in UnknownList)
                        {
                            sb.AppendLine($"{entry.Key}={entry.Value}");
                        }
                    }

                    string dir = Path.GetDirectoryName(item);
                    string fname = Path.GetFileNameWithoutExtension(item);
                    File.WriteAllText($"{dir}\\{fname}.ini", sb.ToString());
                }
            }
        }
    }
}
