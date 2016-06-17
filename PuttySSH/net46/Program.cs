using PuttySSHnet46;
using System;
using PuttySSHnet46.Enums;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing...");
            
            var sftp = new SshSftp("81.174.224.221", "Kefron", "AB48Kd");
            sftp.LoadDirectory("/IWT/CHRONO_INPUT/imported/");

            if (sftp.Exists("1645d0ea-8374-4f5a-9949-bea70608a1f7.pdf", SftpPathType.FilePath))
            {
                sftp.Download("1645d0ea-8374-4f5a-9949-bea70608a1f7.pdf");
            }
            
            //var process = sftp.ProccessSession;

            //var inputStream = process.StandardInput;
            //inputStream.WriteLine("cd /IWT/CHRONO_INPUT/imported");
            //inputStream.WriteLine("ls *1645d0ea-8374-4f5a-9949-bea70608a1f7.pdf");

            //var manualResetEventSlim = new ManualResetEventSlimProvider();
            //var outputBuilder = new StringBuilder();

            //process.BeginOutputReadLine();
            //process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            //{
            //    outputBuilder.AppendLine(e.Data);
            //    Console.WriteLine(e.Data);
            //};

            //manualResetEventSlim.Wait(5000);

            //Console.WriteLine(outputBuilder);
            Console.ReadLine();
        }
    }
}
