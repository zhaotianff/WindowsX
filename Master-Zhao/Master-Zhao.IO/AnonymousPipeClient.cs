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


        public void StartClient(string handleAsString)
        {
            pipeClient = new AnonymousPipeClientStream(PipeDirection.In, handleAsString);
            sr = new StreamReader(pipeClient);
            new System.Threading.Thread(Receive) { IsBackground = true }.Start();
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
