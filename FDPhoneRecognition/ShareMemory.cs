using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class ShareMemory
    {
        string m_FlagName = string.Empty;
        string m_targetFilename = string.Empty;
        public ShareMemory(string f_FlagName, string targetfilename)
        {
            m_FlagName = f_FlagName;
            m_targetFilename = targetfilename;
        }
        public void SyncGetMemory()
        {
            System.Threading.Thread t_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetShareMemory));
            t_Thread.Start();
            //Call Back
            //t_Thread.Join();
        }
        public void GetShareMemory()
        {
            try
            {
                using (System.IO.MemoryMappedFiles.MemoryMappedFile t_MMF = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(m_FlagName))
                {
                    System.IO.BinaryReader t_BinaryReader = null;
                    System.IO.MemoryMappedFiles.MemoryMappedViewStream t_MMViewstream = null;

                    t_MMViewstream = t_MMF.CreateViewStream(0, 8);
                    t_BinaryReader = new System.IO.BinaryReader(t_MMViewstream);
                    byte[] t_Data = null;
                    t_Data = t_BinaryReader.ReadBytes(4);
                    int t_Width = BitConverter.ToInt32(t_Data, 0);
                    t_Data = t_BinaryReader.ReadBytes(4);
                    int t_Height = BitConverter.ToInt32(t_Data, 0);
                    t_BinaryReader.Close();

                    System.Drawing.Bitmap t_Bitmap = new System.Drawing.Bitmap(t_Width, t_Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    System.Drawing.Imaging.ColorPalette t_ColorPalette = t_Bitmap.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        t_ColorPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                    }
                    t_Bitmap.Palette = t_ColorPalette;
                    //System.Drawing.Bitmap.p
                    System.Drawing.Imaging.BitmapData t_BitmapData = t_Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, t_Width, t_Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    
                    t_MMViewstream = t_MMF.CreateViewStream(8, t_BitmapData.Stride * t_Height);
                    t_BinaryReader = new System.IO.BinaryReader(t_MMViewstream);
                    t_Data = t_BinaryReader.ReadBytes(t_BitmapData.Stride * t_Height);

                    System.Runtime.InteropServices.Marshal.Copy(t_Data, 0, t_BitmapData.Scan0, t_BitmapData.Stride * t_Height);

                    //IntPtr t_DataIntPtr = Marshal.AllocHGlobal(t_Data.Length);
                    //Marshal.Copy(t_Data, 0, t_DataIntPtr, t_Data.Length);

                    // Call unmanaged code
                    //Marshal.FreeHGlobal(t_DataIntPtr);
                    //t_BitmapData.Scan0 = t_DataIntPtr;
                    t_Bitmap.UnlockBits(t_BitmapData);
                    t_Bitmap.Save($@"{System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\GetBitmap.bmp");
                    //t_Bitmap.Save(m_targetFilename);
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
            return;
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
