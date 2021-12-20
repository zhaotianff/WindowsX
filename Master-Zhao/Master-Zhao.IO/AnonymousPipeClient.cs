using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.IO
{
    public class AnonymousPipeClient
    {
        PipeStream pipeClient;
        StreamReader sr;
        private Action<string> receiveMessageHandler;
        private bool receiveFlag = true;


        public bool StartClient(string handleAsString)
        {
            try
            {
                pipeClient = new AnonymousPipeClientStream(PipeDirection.In, handleAsString);
                sr = new StreamReader(pipeClient);
                new System.Threading.Thread(Receive) { IsBackground = true }.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CloseClient()
        {
            receiveFlag = false;
            pipeClient?.Close();
            sr?.Close();
        }

        public void SetReceiveMessageHandler(Action<string> action)
        {
            receiveMessageHandler = action;
        }

        private void  Receive()
        { 
            while(receiveFlag)
            {
                var message =  sr.ReadLine();
                receiveMessageHandler?.Invoke(message);
            }
        }
    }
}
