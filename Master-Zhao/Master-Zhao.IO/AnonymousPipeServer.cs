using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace Master_Zhao.IO
{
    public class AnonymousPipeServer
    {
        AnonymousPipeServerStream pipeServer;
        StreamWriter sw;

        public void StartServer(string processPath,string videoPath)
        {
            Process pipeClient = new Process();

            pipeClient.StartInfo.FileName = processPath;

            pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            pipeClient.StartInfo.ArgumentList.Add(pipeServer.GetClientHandleAsString());
            pipeClient.StartInfo.ArgumentList.Add(videoPath);
            pipeClient.Start();

            pipeServer.DisposeLocalCopyOfClientHandle();
            sw = new StreamWriter(pipeServer);
            sw.AutoFlush = true;
        }

        public void CloseServer()
        {
            if (sw != null)
                sw.Close();

            if (pipeServer != null)
                pipeServer.Close();
        }

        public async void SendMessage(string message)
        {
            if (pipeServer == null || pipeServer.IsConnected == false || pipeServer.CanWrite == false)
                return;

            if (sw == null)
                return;

            await sw.WriteLineAsync(message);

            //SYNC mode
            //pipeServer.WaitForPipeDrain(); 
        }
    }
}
