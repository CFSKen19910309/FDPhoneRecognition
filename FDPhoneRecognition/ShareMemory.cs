using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class ShareMemory
    {
        string m_FlagName = "ws1";        
        public object GetShareMemory(string f_FlagName)
        {
            try
            {
                using (System.IO.MemoryMappedFiles.MemoryMappedFile mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting("Back"))
                {

                    System.Threading.Mutex mutex = System.Threading.Mutex.OpenExisting("test");
                    mutex.WaitOne();
                    int aa = 0;
                    using (System.IO.MemoryMappedFiles.MemoryMappedViewStream stream = mmf.CreateViewStream(0, 4))
                    {
                        System.IO.BinaryReader t_Reader = new System.IO.BinaryReader(stream);
                        byte[] rr = null;
                        rr  = t_Reader.ReadBytes(4);
                        int ads = BitConverter.ToInt32(rr, 0);
                        using (System.IO.MemoryMappedFiles.MemoryMappedViewStream streams = mmf.CreateViewStream(4, ads))
                        {
                            System.IO.BinaryReader t_Readera = new System.IO.BinaryReader(streams);
                            byte[] rsr = null;
                            rsr = t_Reader.ReadBytes(ads);
                            System.Drawing.Bitmap t_Bitmap = new System.Drawing.Bitmap(streams);
                            t_Bitmap.Save("2.Bmp");
                        }
                            


                    }
                    mutex.ReleaseMutex();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Memory-mapped file does not exist. Run Process A first.");
            }
            /*
            System.IO.MemoryMappedFiles.MemoryMappedFile t_MemoryMappedFile = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(f_FlagName, System.IO.MemoryMappedFiles.MemoryMappedFileRights.Read);
            System.Threading.Mutex t_Mutex = new System.Threading.Mutex(true, "GetSharedMemory");
            System.IO.MemoryMappedFiles.MemoryMappedViewStream t_MemoryMappedViewStream = t_MemoryMappedFile.CreateViewStream(0, 1);
            byte[] t_DataByte = new byte[4] { 0,0,0,0 };
            t_MemoryMappedViewStream.Read(t_DataByte, 0, 4);
            uint t_DataLength = BitConverter.ToUInt32(t_DataByte, 0);
            t_MemoryMappedViewStream.Close();

            t_MemoryMappedViewStream = t_MemoryMappedFile.CreateViewStream(1, t_DataLength);
            byte[] t_BitmapByte = null;
            //t_MemoryMappedViewStream.Read(t_BitmapByte, 2, (int)t_DataLength);
            System.Drawing.Bitmap t = new System.Drawing.Bitmap(t_MemoryMappedViewStream);
            */
            return null;
        }
        public struct MyColor
        {
            public short Red;
            public short Green;
            public short Blue;
            public short Alpha;

            // Make the view brigher.
            public void Brighten(short value)
            {
                Red = (short)Math.Min(short.MaxValue, (int)Red + value);
                Green = (short)Math.Min(short.MaxValue, (int)Green + value);
                Blue = (short)Math.Min(short.MaxValue, (int)Blue + value);
                Alpha = (short)Math.Min(short.MaxValue, (int)Alpha + value);
            }
        }
    }
}
