using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace WindowsX.IO
{
    public class AnonymousPipeServer
    {
        AnonymousPipeServerStream pipeServer;
        StreamWriter sw;

        public bool StartServer(string processPath,string videoPath)
        {
            try
            {
                Process pipeClient = new Process();

                pipeClient.StartInfo.FileName = processPath;

                pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
                pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString() + " " + videoPath;
                pipeClient.Start();

                pipeServer.DisposeLocalCopyOfClientHandle();
                sw = new StreamWriter(pipeServer);
                sw.AutoFlush = true;
                return true;
            }
            catch           
            {
                return false;
            }
        }

        public bool CloseServer()
        {
            try
            {
                if (sw != null)
                    sw.Close();

                if (pipeServer != null)
                    pipeServer.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SendMessage(string message)
        {
            if (pipeServer == null || pipeServer.IsConnected == false || pipeServer.CanWrite == false)
                return;

            if (sw == null)
                return;

            sw.WriteLine(message);

            //SYNC mode
            //pipeServer.WaitForPipeDrain(); 
        }
    }
}
