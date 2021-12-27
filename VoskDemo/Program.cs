using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Text;
using Vosk;

namespace VoskDemo
{
    public class Result
    {
        public string partial;
        public string text;
    }

    public class Program
    {
        private static VoskRecognizer _recognizer;
        private static WaveInEvent _microphoneWaveIn;
        private static StringBuilder _textBuilder = new StringBuilder();
        private static int _lineCount = 1;


        static void OpenMicrophone()
        {
            _microphoneWaveIn = new WaveInEvent();
            _microphoneWaveIn.WaveFormat = new WaveFormat(16000, 1);
            _microphoneWaveIn.DataAvailable += WaveIn_DataAvailable;
            _microphoneWaveIn.StartRecording();
        }

        static void StopMicrophone()
        {
            _microphoneWaveIn.DataAvailable -= WaveIn_DataAvailable;
            _microphoneWaveIn.StopRecording();
            _microphoneWaveIn.Dispose();
            _microphoneWaveIn = null;
        }

        static void Main(string[] args)
        {
            Vosk.Vosk.SetLogLevel(0);

            Model model = new Model("model_TR");
            _recognizer = new VoskRecognizer(model, 16000);


            while (true)
            {
                OpenMicrophone();

                Console.WriteLine("Please Speak. To stop press enter");
                Console.ReadLine();

                StopMicrophone();

                _recognizer.FinalResult();

                _recognizer.Reset();

                Console.WriteLine("To speak again please type restart");

                string command = Console.ReadLine();

                if (command == "restart")
                {
                    _textBuilder.Clear();
                    _lineCount = 1;
                }
                else
                {
                    break;
                }
            }
        }

        private static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_recognizer.AcceptWaveform(e.Buffer, e.Buffer.Length))
            {
                string result = _recognizer.Result();
                WriteToTheConsole(result);
            }
            else
            {
                string result = _recognizer.PartialResult();
                WriteToTheConsole(result);
            }
        }



        private static void WriteToTheConsole(string text)
        {
            var result = JsonConvert.DeserializeObject<Result>(text);
            if (!string.IsNullOrEmpty(result.partial))
            {
                Console.WriteLine($"Partial : {result.partial}");
            }
            if (!string.IsNullOrEmpty(result.text))
            {
                _textBuilder.AppendLine($"Sentence - {_lineCount++} : {result.text}");
                Console.Clear();
                Console.WriteLine("Please Speak. To stop press enter");
                Console.WriteLine(_textBuilder);
            }
        }
    }
}