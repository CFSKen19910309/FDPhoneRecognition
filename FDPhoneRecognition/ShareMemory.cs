using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class ShareMemory
    {
        string m_FlagName = "";
        /*
           using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("Map1"))
           {
               bool mutexCreated;
               System.Threading.Mutex mutex = new System.Threading.Mutex(true, "testmapmutex", out mutexCreated);
               using (MemoryMappedViewStream stream = mmf.CreateViewStream(0, 256))
               {
                   StreamReader tr = new StreamReader(stream);
                   char[] ss = new char[256];
                   int vt = tr.Read(ss, 0, 255);
                   string aa =  ss.ToString();

                   BinaryWriter writer = new BinaryWriter(stream);
                   writer.Write(1);
               }
               mutex.ReleaseMutex();
           }
           */
    }
}
