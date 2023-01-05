//using ImageClassification_ConsoleApp2;
using Microsoft.VisualBasic.ApplicationServices;
using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;
using S7.Net;
using System;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ImageClassification = ImageClassification_ConsoleApp2.ImageClassification;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Collections.Generic;

namespace VPJCountDetection
{
    public partial class VPJCountDetection : Form

    {
        bool RFRC = false;
        bool INTERLOC = true;
        public static bool runstate = true;
        public static int Bountlecount = 0;
        public static int Badcount;
        string Format;
        public long FGcode;
        public long FF;
        int TCount;

        List<double> FGS50 = new List<double>();
        List<double> FGS100 = new List<double>();
        List<double> FGS250 = new List<double>();
        List<double> FGS450 = new List<double>();

        int clk = 1;
        public static Bitmap smpimg;
        string logo = (@"C:\VPJCD\Program Files\pngwing.png");
        private const string urlToCamera = "rtsp://admin:Unilever.123@10.10.71.252/Streaming/Channels/101/?transportmode=unicast";
        private const int streamWidth = 1280;//2688;//240;
        private const int streamHeight = 720;//1520;//160;

        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static int _imageNumber = 1;
        private static readonly FrameDecoder FrameDecoder = new FrameDecoder();
        private static readonly FrameTransformer FrameTransformer = new FrameTransformer(streamWidth, streamHeight);

        public VPJCountDetection()
        {








            InitializeComponent();
            pictureBox3.Image = Image.FromFile(logo);

            if (RFRC == true)
            {
                button8.BackColor = Color.Green;

                if (runstate == true)
                {
                    buttonrun.Visible = false;
                    buttonStop.Visible = true;
                }

                if (runstate == false)
                {
                    buttonrun.Visible = true;
                    buttonStop.Visible = false;
                }
            }

            else
            {
                button7.BackColor = Color.Green;
                runstate = true;
                buttonrun.Visible = false;
                buttonStop.Visible = false;
            }

            if (INTERLOC == true)
            {
                button4.BackColor = Color.Green;
            }

            else
            {
                button6.BackColor = Color.Green;
            }

        }




        public void AddStatus(string status, string MessageType)  //Add status to list view in athread safe manner
        {
            string[] row = { status, DateTime.Now.ToString(), MessageType };
            var listViewItem = new ListViewItem(row);
            if (MessageType == "Error")
                listViewItem.ForeColor = Color.Red;
            else listViewItem.ForeColor = Color.Blue;

            if (StatusListView.InvokeRequired)
            {
                StatusListView.Invoke(new MethodInvoker(delegate
                {
                    StatusListView.Items.Add(listViewItem);
                    StatusListView.Items[StatusListView.Items.Count - 1].EnsureVisible();

                }));
            }
            else
            {
                StatusListView.Items.Add(listViewItem);
                StatusListView.Items[StatusListView.Items.Count - 1].EnsureVisible();
            }

            DateTime status3 = (DateTime.UtcNow.AddHours(1)).AddMinutes(-10);
            string Datestr = status3.ToString("dd-MMMM-yyyy");
            string filepath = @"C:\OperationLog\" + Datestr + ".txt";
          
            try
            {
                string path = filepath;
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString() + '\t' + status + '\t' + MessageType);
                }
            }
            catch { }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            panel1set.Visible = true;
            panel2hm.Visible = false;


        }



        private void button3_Click(object sender, EventArgs e)
        {
            runstate = true;
            buttonrun.Visible = false;
            buttonStop.Visible = true;
            AddStatus("System started Manually ", "Information");

            if (runstate == true)
            {

                AddStatus("Initializing the System ", "Information");
                Thread trdsnp = new Thread(snapshooter);
                trdsnp.Start();
            }


        }






        public void Form1_Load(object sender, EventArgs e)
        {

         

            textBox2.Text = "0";
            textBox1.Text = Badcount.ToString();
            textBox4.Text = TCount.ToString();


            AddStatus("Initializing the System ", "Information");
            Thread trdsnp = new Thread(snapshooter);
            trdsnp.Start();

            Thread trdidsp = new Thread(initialdisplayrunner);

            trdidsp.Start();


        }


        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBox2.Text = value;
        }

        public void AppendTextBox4(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox4), new object[] { value });
                return;
            }
            textBox4.Text = value;
        }

        public void AppendTextBox5(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox5), new object[] { value });
                return;
            }
            textBox5.Text = value;
        }

        public void AppendTextBox6(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox6), new object[] { value });
                return;
            }
            textBox1.Text = value;
        }




        public void InvokeButtonvisibilityOC()
        {
            if (InvokeRequired)
            {
                this.Invoke(new System.Action(InvokeButtonvisibilityOC), new object[] { });
                return;
            }
            buttonrun.Visible = true;
            buttonStop.Visible = false;
        }


        public void InvokeButtonvisibilityFC2()
        {
            if (InvokeRequired)
            {
                this.Invoke(new System.Action(InvokeButtonvisibilityFC2), new object[] { });
                return;
            }


            buttonrun.Visible = false;
            buttonStop.Visible = true;

        }




        public void InvokeButtonvisibilityFC()
        {
            if (InvokeRequired)
            {
                this.Invoke(new System.Action(InvokeButtonvisibilityFC), new object[] { });
                return;
            }
            buttonrun.Visible = false;
            buttonStop.Visible = false;
        }




        public void displayrunner()
        {


            { pictureBox2.Image = Engine.smpimg2; }



            string Boxnm1 = Engine.Boxnm.ToString();

            AppendTextBox(Boxnm1);
            Thread.Sleep(1);


            AppendTextBox4(TCount.ToString());
            Thread.Sleep(1);


            AppendTextBox5(Format);
            Thread.Sleep(1);

            AppendTextBox6(Badcount.ToString());



        }

        public void initialdisplayrunner()
        {

            { pictureBox1.Image = smpimg; }

        }





        public void snapshooter(object obj)

        {
            if (clk == 1|| clk == 50)
            {
                readExcel50();
                readExcel100();
                readExcel250();
                readExcel450();
               


                if (clk == 100)
                  { clk = 0; }



            }

            clk = clk + 1;
           

            PartnerPODataPLCClass poplc = new PartnerPODataPLCClass();
            //  Console.WriteLine($"Platform {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");

            var serverUri = new Uri(urlToCamera);
            var connectionParameters = new ConnectionParameters(serverUri/*, credentials*/);

           
                try
                {
                    //hardcoded logic

                    var MachinePLC = new Plc(poplc.cpu, poplc.PLCIPAddress, poplc.rackNumber, poplc.slotNumber);
                    MachinePLC.Open();

                    FGcode = Convert.ToInt64(MachinePLC.Read(DataType.DataBlock, poplc.PLCDBNumber, poplc.FGCodeAdr, VarType.DInt, 1));
                    AddStatus("PLC communication successful, Line 5 Ronchi Filler ", "Information");
                    AddStatus("FGcode retrived, Line 5 Ronchi Filler ", "Information");
                    MachinePLC.Close();
                }

                catch (Exception)

                {

                    AddStatus("PLC communication Failed, Line 5 Ronchi Filler ", "Error");

                    if (RFRC == false)
                    {
                        // Thread trdsnp = new Thread(snapshooter);
                        // trdsnp.Start();

                    }


                }


            if (FGS50.Contains(FGcode))
            {
                Bountlecount = 12;
                AddStatus("FGcode loeaded for 50ml Jar & boundle count is 12", "Information");
                Format = " 50ml Jar & boundle count is 12";
            }


            if (FGS100.Contains(FGcode))
            {
                Bountlecount = 12;
                AddStatus("FGcode loeaded for 100ml Jar & boundle count is 12", "Information");
                Format = " 100ml Jar & boundle count is 12";
            }

            if (FGS250.Contains(FGcode))
            {
                Bountlecount = 6;
                AddStatus("FGcode loeaded for 250ml Jar & boundle count is 6", "Information");
                Format = " 250ml Jar & boundle count is 6";
            }


            if (FGS450.Contains(FGcode))
            {
                Bountlecount = 6;
                AddStatus("FGcode loeaded for 450ml Jar & boundle count is 6", "Information");
                Format = " 450ml Jar & boundle count is 6";
            }

            if (FGS50.Contains(FGcode) || FGS100.Contains(FGcode) || FGS250.Contains(FGcode) || FGS450.Contains(FGcode))
            { }
            else
            {
                AddStatus("FGCode unidentified / Not Available in Excel FGCodelist.xlsx " + FGcode, "Error");
                AddStatus("Please add the new FGcode in FGCodelist.xlsx " + FGcode, "Information");
                Format = "Un-Identified";
                runstate = false;

            }

            

            if (runstate == true)
            {
                if (RFRC == true)
                {
                    InvokeButtonvisibilityFC2();
                    // buttonrun.Visible = false;
                    //  buttonStop.Visible = true;

                }
            }


            else
            {
                if (RFRC == false)
                {

                    InvokeButtonvisibilityFC();
                    // buttonrun.Visible = false;
                    // buttonStop.Visible = false;
                }
                else
                {
                    InvokeButtonvisibilityOC();
                    // buttonrun.Visible = true;
                    // buttonStop.Visible = false;

                }


            }



            // bool Run = true;
            while (runstate == true)
            {


                try
                {
                    SaveOnePicture(connectionParameters).Wait();
                    Thread trdidsp = new Thread(initialdisplayrunner);
                    trdidsp.Start();
                    // var sampleData = new MLModel2_ConsoleApp1.MLModel2.ModelInput()


                    var sampleData1 = new ImageClassification.ModelInput()
                    {
                        ImageSource = @"C:\VPJCD\ImageMLProjects\RTSPTrials\RTSPClientNetCore\image.jpg",
                    };

               



                    //Load model and predict output
                    var result1 = ImageClassification.Predict(sampleData1);


                    // Image Clasification Result Checking
                    if (result1.Prediction == "good")
                    {
                        // AddStatus("Good Boundle Image acquired" , "Information");
                        bool looper = true;
                        if (looper = true)
                        {
                            SaveOnePicture(connectionParameters).Wait();

                            var sampleData = new ImageClassification.ModelInput()
                            {
                                ImageSource = @"C:\VPJCD\ImageMLProjects\RTSPTrials\RTSPClientNetCore\image.jpg",
                            };

                            var result = ImageClassification.Predict(sampleData);


                            if (result.Prediction == "good")
                            {
                                Engine Eng = new Engine();
                                Engine.Run();

                                TCount = TCount + 1;


                                if (Engine.Boxnm == VPJCountDetection.Bountlecount)
                                {
                                    InterlockPLCClass interlockplc = new InterlockPLCClass();
                                    var MachinePLC = new Plc(interlockplc.cpu, interlockplc.PLCIPAddress, interlockplc.rackNumber, interlockplc.slotNumber);
                                    MachinePLC.Open();
                                    MachinePLC.WriteBit(DataType.DataBlock, interlockplc.PLCDBNumber, interlockplc.VPJCDInterlock, interlockplc.VPJCDInterlock, 0);
                                    MachinePLC.Close();
                                }
                                else
                                {
                                    InterlockPLCClass interlockplc = new InterlockPLCClass();




                                    var fileName = $"image{_imageNumber++}.jpg";
                                    Engine.testImage.Save(Path.Combine(@"C:\VPJCD\ImageMLProjects\BadSampleHistory\", fileName), ImageFormat.Jpeg);

                                    AddStatus("Bad Boundle Found ", "Error");
                                    AddStatus("requesting shrink wrapper to Interlocking the line  ", "Error");

                                    try
                                    {


                                        if (INTERLOC == true)
                                        {

                                            var MachinePLC = new Plc(interlockplc.cpu, interlockplc.PLCIPAddress, interlockplc.rackNumber, interlockplc.slotNumber);
                                            MachinePLC.Open();

                                            if (Format == " 450ml Jar & boundle count is 6")
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            //450ml (used to allow first bundle to pass)

                                            if (Format == " 100ml Jar & boundle count is 12")
                                            {
                                                Thread.Sleep(3000);
                                            }
                                            //100ml (used to allow first bundle to pass)

                                            if (Format == " 50ml Jar & boundle count is 12")
                                            {
                                                Thread.Sleep(3000);
                                            }
                                            //50ml (used to allow first bundle to pass)


                                            if (Format == " 250ml Jar & boundle count is 6")
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            //250ml (used to allow first bundle to pass)

                                            MachinePLC.WriteBit(DataType.DataBlock, interlockplc.PLCDBNumber, interlockplc.VPJCDInterlock, interlockplc.VPJCDInterlock, 0);   //
                                            Thread.Sleep(2000);
                                            MachinePLC.WriteBit(DataType.DataBlock, interlockplc.PLCDBNumber, interlockplc.VPJCDInterlock, interlockplc.VPJCDInterlock, 0);
                                            MachinePLC.Close();
                                            AddStatus("PLC Interlocking successful, Line 5 Shrink Wrapper ", "Information");


                                        }
                                        else
                                        {
                                            AddStatus("PLC Interlock is turned OFF, Application ", "Information");

                                        }






                                    }

                                    catch (Exception)

                                    {

                                        AddStatus("PLC Interlocking Failed, Line 5 Shrink Wrapper ", "Error");




                                    }



                                }




                            }




















                            if (RFRC == true)

                            {
                                runstate = false;
                                InvokeButtonvisibilityOC();
                               
                            }
                            else
                            {
                                runstate = true;
                                

                            }





                        }







                    }


                }
                catch (Exception) 
                { }


                Thread trddsp = new Thread(displayrunner);

                trddsp.Start();



            }





        }

        private void readExcel50()
        {

            string filepath = @"C:\VPJCD\FGCodelist.xlsx";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb;
            Worksheet ws;

            wb = excel.Workbooks.Open(filepath);
            ws = wb.Worksheets[1];
            

            Range cell = ws.Range["B3:B34"];
            

            foreach (double Result in cell.Value)
            {
                // FGS.Add(Convert.ToDouble(Result));
                if (Result != null && Result != 0)
                { FGS50.Add(Result); }
                


            }



            wb.Close();


        }


        private void readExcel100()
        {


            string filepath = @"C:\VPJCD\FGCodelist.xlsx";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb;
            Worksheet ws;


            wb = excel.Workbooks.Open(filepath);
            ws = wb.Worksheets[2];


            Range cell = ws.Range["B3:B34"];


            foreach (double Result in cell.Value)
            {
                // FGS.Add(Convert.ToDouble(Result));
                if (Result != null && Result != 0)
                { FGS100.Add(Result); }



            }




            wb.Close();





        }

        private void readExcel250()
        {


            string filepath = @"C:\VPJCD\FGCodelist.xlsx";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb;
            Worksheet ws;


            wb = excel.Workbooks.Open(filepath);
            ws = wb.Worksheets[3];


            Range cell = ws.Range["B3:B34"];


            foreach (double Result in cell.Value)
            {
                // FGS.Add(Convert.ToDouble(Result));
                if (Result != null && Result != 0)
                { FGS250.Add(Result); }



            }




            wb.Close();





        }



        private void readExcel450()
        {


            string filepath = @"C:\VPJCD\FGCodelist.xlsx";
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb;
            Worksheet ws;


            wb = excel.Workbooks.Open(filepath);
            ws = wb.Worksheets[4];


            Range cell = ws.Range["B3:B34"];


            foreach (double Result in cell.Value)
            {
                // FGS.Add(Convert.ToDouble(Result));
                if (Result != null && Result != 0)
                { FGS450.Add(Result); }



            }




            wb.Close();





        }









        public  async Task SaveOnePicture(ConnectionParameters connectionParameters)
        {
          
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                
                using var rtspClient = new RtspClient(connectionParameters);
                rtspClient.FrameReceived += delegate (object o, RawFrame rawFrame)
                {
                    if (!(rawFrame is RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame))
                        return;
                   
                    var decodedFrame = FrameDecoder.TryDecode(rawVideoFrame);
                   
                    if (decodedFrame == null) return;

                    smpimg = FrameTransformer.TransformToBitmap(decodedFrame);
                    cancellationTokenSource.Cancel();
                  
                };


                await rtspClient.ConnectAsync(cancellationTokenSource.Token);
                await rtspClient.ReceiveAsync(cancellationTokenSource.Token);
                

            }
            catch (OperationCanceledException)
            {
            }
            
            smpimg?.Save(Path.Combine(@"C:\VPJCD\ImageMLProjects\RTSPTrials\RTSPClientNetCore", "image.jpg"), ImageFormat.Jpeg);

           

        }


        private static async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        {
            try
            {
                var delay = TimeSpan.FromSeconds(5);

                using (var rtspClient = new RtspClient(connectionParameters))
                {
                    rtspClient.FrameReceived += RtspClient_FrameReceived;

                    while (true)
                    {
                        try
                        {

                            await rtspClient.ConnectAsync(token);
                            await rtspClient.ReceiveAsync(token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (RtspClientSharpCore.Rtsp.RtspClientException e)
                        {
                            Console.WriteLine(e.ToString());
                            await Task.Delay(delay, token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static void RtspClient_FrameReceived(object sender, RtspClientSharpCore.RawFrames.RawFrame rawFrame)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!(rawFrame is RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame))
                return;

            var decodedFrame = FrameDecoder.TryDecode(rawVideoFrame);

            if (decodedFrame == null)
                return;

            var bitmap2 = FrameTransformer.TransformToBitmap(decodedFrame);

            var fileName = $"image{_imageNumber++}.jpg";

            var frameType = rawFrame is RtspClientSharpCore.RawFrames.Video.RawH264IFrame ? "IFrame" : "PFrame";
            sw.Stop();
          
        }


        private void button5_Click(object sender, EventArgs e)
        {
            panel2hm.Visible = true;
            panel1set.Visible = false;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.BackColor = Color.Green;
            button4.BackColor = Color.Gray;
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            runstate = false;
            AddStatus("System Stoped Manually ", "Information");
            buttonStop.Visible = false;
            buttonrun.Visible = true;


        }

        private void button7_Click(object sender, EventArgs e)
        {
            RFRC = false;
            button7.BackColor = Color.Green;
            button8.BackColor = Color.Gray;
            buttonrun.Visible = false;
            buttonStop.Visible = false;

           
        }

        private void button8_Click(object sender, EventArgs e)
        {
            RFRC = true;
            button8.BackColor = Color.Green;
            button7.BackColor = Color.Gray;
            if (runstate == true)
            {
                buttonrun.Visible = false;
                buttonStop.Visible = true;
            }

            if (runstate == false)
            {
                buttonrun.Visible = true;
                buttonStop.Visible = false;
            }

            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.BackColor = Color.Green;
            button6.BackColor = Color.Gray;
        }

    }


    public class PartnerPODataPLCClass
    {
        public string PLCIPAddress = "10.10.71.126";
        public int PLCDBNumber = 9000;
        public short rackNumber = 0;
        public short slotNumber = 1;
        public CpuType cpu = CpuType.S71500;


        public int POAddr = 1028;
        public int FGCodeAdr = 674;

        public int roPOAdr = 1074;
        public int roFGAdr = 1078;

        public int goodCounterAdr = 692;
        public int MachineSpeedAdr = 688;

        public int liquidTotalizerAdr = 1036;
        public int TankDataByteAdr = 1040;

        public int t1BulkCodeAdr = 1058;
        public int t2BulkCodeAdr = 1062;

    }

    public class InterlockPLCClass
    {
        public string PLCIPAddress = "10.10.71.141";
        public int PLCDBNumber = 9000;
        public short rackNumber = 0;
        public short slotNumber = 2;
        public CpuType cpu = CpuType.S7300;


        public int VPJCDInterlock = 0;
        
    }


    





}
