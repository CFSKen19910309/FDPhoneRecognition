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
            System.Drawing.Imaging.BitmapData t_BitmapData = t_Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, t_Bitmap.Width, t_Bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            IntPtr t_IntPtr = t_BitmapData.Scan0;
            int t_DataLength = Math.Abs(t_BitmapData.Stride) * t_Bitmap.Height;
            byte[] t_ImageData = new byte[t_DataLength];
            System.Runtime.InteropServices.Marshal.Copy(t_IntPtr, t_ImageData, 0, t_DataLength);
            t_Bitmap.UnlockBits(t_BitmapData);


            //MemoryStream t_MemoryStream = new MemoryStream();
            //t_Bitmap.Save(t_MemoryStream,  t_Bitmap.RawFormat);
            //byte[] tbyte = t_MemoryStream.ToArray();
            //int aa = tbyte.Length;
            //byte[] aa_byte = BitConverter.GetBytes(aa);
            //int temp = aa_byte.Length;
            byte[] t_HeightByte = BitConverter.GetBytes(t_Bitmap.Height);
            byte[] t_WidthByte = BitConverter.GetBytes(t_Bitmap.Width);
            int t_SharedMemoryLength = t_DataLength + 8;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("Back", t_SharedMemoryLength))
            {
                //bool mutexCreated;
                //Mutex mutex = new Mutex(true, "test", out mutexCreated);
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    //string tt = "ABC";
                    //byte[] aa = Encoding.ASCII.GetBytes(tt);
                    writer.Write(t_WidthByte);
                    writer.Write(t_HeightByte);
                    writer.Write(t_ImageData);
                    writer.Close();
                }
                //mutex.ReleaseMutex();

                Console.WriteLine("Start Process B and press ENTER to continue.");
                Console.ReadLine();

                //mutex.WaitOne();
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryReader reader = new BinaryReader(stream);
                    Console.WriteLine("Process A says: {0}", reader.ReadBoolean());
                    Console.WriteLine("Process B says: {0}", reader.ReadBoolean());
                }
               // mutex.ReleaseMutex();
            }

        }
    }
}
