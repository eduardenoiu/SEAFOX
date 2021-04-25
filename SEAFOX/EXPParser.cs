using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace SEAFOX
{
    class EXPParser
    {
        public struct varData
        {
            public string name;
            public string type;
        }

        private bool end = false;
        private bool readingVar = false;

        public List<varData> readEXPDoc(List<varData> dataList, StreamReader file)
        {
            varData temp;
            string line;

            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("VAR_INPUT"))
                {
                    readingVar = true;
                }
                else if (line.Equals("END_VAR"))
                {
                    readingVar = false;
                }
                else if (readingVar)
                {
                    if (line != "")
                    {
                        string[] words = line.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            words[i] = words[i].Replace("\t", "");
                            words[i] = words[i].Replace("\n", "");
                            words[i] = words[i].Replace("\b", "");
                            words[i] = words[i].Replace(":", "");
                            words[i] = words[i].Replace(";", "");
                        }
                        //remove empty elements in array
                        words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        words = words.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                        temp.name = words[0];
                        temp.type = words[1];
                        dataList.Add(temp);
                    }
                }
            }
            return dataList;
        }
    }
}
