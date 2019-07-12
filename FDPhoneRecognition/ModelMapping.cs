using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class ModelMapping
    {
        string m_FilePath;
        Dictionary<string, string> t_ModelMappingDict = new Dictionary<string, string>();
        public ModelMapping(string f_Function)
        {
            if (f_Function.CompareTo("ISP") ==0)
            {
                m_FilePath = @".\FDData\FDPhoneRecognition\SystemFile\ModelMappingISP.json";
            }
            if (f_Function.CompareTo("PMP") ==0)
            {
                m_FilePath = @".\FDData\FDPhoneRecognition\SystemFile\ModelMappingPMP.json";
            }
        }
        public string DoMapModel(string f_Size, string f_Color)
        {
            string t_JsonContent = System.IO.File.ReadAllText(m_FilePath);
            t_ModelMappingDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(t_JsonContent);
            string t_ModelNameKey = f_Size + " " + f_Color;
            if(t_ModelMappingDict.ContainsKey(t_ModelNameKey) == true)
            {
                return t_ModelMappingDict[t_ModelNameKey];
            }
            else
            {
                return "No Found";
            }
        }

    }
}
