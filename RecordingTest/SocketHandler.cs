using Microsoft.Web.WebSockets;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RecordingTest
{
    public class SocketHandler : WebSocketHandler
    {
        private static readonly WebSocketCollection Sockets = new WebSocketCollection();
        private string currentCommand = "stop";
        private byte[] audioStream = null;
        private readonly string _sessionId;

        public SocketHandler(string sessionId)
        {
            _sessionId = sessionId;
        }

        public override void OnOpen()
        {
            Sockets.Add(this);
        }

        public override void OnClose()
        {
            Sockets.Remove(this);
        }

        public override void OnError()
        {
            Sockets.Remove(this);
        }

        public override void OnMessage(byte[] message)
        {
            if (currentCommand.Equals("start"))
            {
                try
                {
                    audioStream = audioStream.Concat(message).ToArray();
                }
                catch (Exception e)
                {

                }
            }
        }

        public override void OnMessage(string message)
        {
            if (message.Equals("start"))
            {
                currentCommand = "start";
                audioStream = new byte[0];
            }
            else if (message.Contains("stop"))
            {
                if (audioStream == null)
                {
                    return;
                }

                try
                {
                    currentCommand = "stop";
                    var tempStream = audioStream;
                    audioStream = null;
                    var sampleRate = Convert.ToInt32(message.Substring(5));

                    var audio = Encoding.Default.GetBytes(new char[4] { 'R', 'I', 'F', 'F' });
                    var temp = BitConverter.GetBytes((int)(tempStream.Length + 44 - 8));
                    audio = audio.Concat(temp).ToArray();
                    temp = Encoding.Default.GetBytes(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((int)16);
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((short)1);
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((short)1);
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes(sampleRate);
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((int)(sampleRate * ((16 * 1) / 8)));
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((short)((16 * 1) / 8));
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes((short)16);
                    audio = audio.Concat(temp).ToArray();
                    temp = Encoding.Default.GetBytes(new char[4] { 'd', 'a', 't', 'a' });
                    audio = audio.Concat(temp).ToArray();
                    temp = BitConverter.GetBytes(tempStream.Length);
                    audio = audio.Concat(temp).ToArray();
                    audio = audio.Concat(tempStream).ToArray();

                    FileStream fs = new FileStream(@"D://Recording.wav", FileMode.Create);
                    fs.Write(audio, 0, audio.Length);
                    fs.Close();
                    audioStream = null;
                    Send(@"Recording.wav");
                }
                catch
                {

                }
            }
        }
    }
}