using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class VideoCapture
    {
        static int _frame_number = 0;
        static System.Threading.EventWaitHandle _evt = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        static System.Threading.EventWaitHandle _evt2 = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        public static void run(System.Threading.EventWaitHandle _quitEvent)
        {
            Program.logIt("VideoCapture::run: ++");
            string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
            System.IO.Directory.CreateDirectory(root);
            Emgu.CV.VideoCapture vc = new Emgu.CV.VideoCapture(0);
            bool b = vc.SetCaptureProperty(CapProp.Mode, 0);
            b = vc.SetCaptureProperty(CapProp.FrameHeight, 1944);
            b = vc.SetCaptureProperty(CapProp.FrameWidth, 2592);
            //double d = vc.GetCaptureProperty(CapProp.Buffersuze);
            //b = vc.SetCaptureProperty(CapProp.Buffersuze, 0);
            System.Threading.EventWaitHandle[] evts = new System.Threading.EventWaitHandle[] 
            {
                _evt,
                _quitEvent,
            };
            while (true)
            {
                int r = System.Threading.EventWaitHandle.WaitAny(evts);
                if (r == 0)
                {
                    // capture frame event set
                    Emgu.CV.Mat cm = new Emgu.CV.Mat();
                    vc.Read(cm);
                    _frame_number += 1;
                    cm.Save(System.IO.Path.Combine(root, $"frame_{_frame_number:D5}.jpg"));
                    _evt2.Set();
                }
                else if (r == 1)
                {
                    // _quit event set
                    break;
                }
            }
            Program.logIt("VideoCapture::run: --");
        }
        public static Tuple<bool,string> captre_frame(int timeout = 10*1000)
        {
            bool ret = false;
            string rets = "";
            _evt.Set();
            if(_evt2.WaitOne(timeout))
            {
                ret = true;
                rets = $"frame_{_frame_number:D5}.jpg";
            }
            return new Tuple<bool, string>(ret,rets);
        }
    }
}
