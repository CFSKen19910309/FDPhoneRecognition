using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WriteToMemory
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Drawing.Bitmap t_Bitmap = new System.Drawing.Bitmap(@"C:\Users\CFSKe\Desktop\FDPhoneRecognition\WriteToMemory\bin\Debug\1.bmp");
            MemoryStream t_MemoryStream = new MemoryStream();
            t_Bitmap.Save(t_MemoryStream, t_Bitmap.RawFormat);
            byte[] tbyte = t_MemoryStream.ToArray();
            int aa = tbyte.Length;
            byte[] aa_byte = BitConverter.GetBytes(aa);
            int temp = aa_byte.Length;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("Back", aa+4))
            {
                bool mutexCreated;
                Mutex mutex = new Mutex(true, "test", out mutexCreated);
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    string tt = "ABC";
                    //byte[] aa = Encoding.ASCII.GetBytes(tt);
                    writer.Write(aa_byte);
                    writer.Write(tbyte);
                }
                mutex.ReleaseMutex();

                Console.WriteLine("Start Process B and press ENTER to continue.");
                Console.ReadLine();

                mutex.WaitOne();
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryReader reader = new BinaryReader(stream);
                    Console.WriteLine("Process A says: {0}", reader.ReadBoolean());
                    Console.WriteLine("Process B says: {0}", reader.ReadBoolean());
                }
                mutex.ReleaseMutex();
            }

        }
    }
}
