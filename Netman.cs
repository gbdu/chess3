using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Gess{
    class Netman{
        private TcpClient Server;
        private IPEndPoint serverEndPoint;
        private ASCIIEncoding encoder;
        private Thread serverThread;
        
        private string Read(TcpClient Client){
            byte[] message = new byte[4096]; // Maximum data in every segment
            NetworkStream stream = Client.GetStream(); // Network stream to read from
            Regex rx = new Regex(@"^(\d+)\|(.*)$"); // Capture (length)|(data segment)
            
            int bytesRead = 0; // Bytes read on every call of stream.Read
            int bytesToRead = 0; // length captured in the regex
            string response = ""; // string to store the data in and return
            
            try {
                bytesRead = stream.Read(message, 0, 4096); // read from the stream
                
                Match match = rx.Match(encoder.GetString(message)); // match regex with the string from the stream
                
                bytesToRead = Convert.ToInt32(match.Groups[1].Value); // convert first match to int
                bytesRead -= match.Groups[1].Value.ToString().Length + 1; // reduce bytes read by length header's length
                
                string match_res = match.Groups[2].Value;
                for(int i = 0; i != 4096; i++){
                    if(match_res[i] != '\0') response += match_res[i];
                    else break;
                }
                                
                while(bytesRead != bytesToRead){
                    bytesRead += stream.Read(message, 0, 4096);
                    match_res = encoder.GetString(message);
                    for(int i = 0; i != 4096; i++){
                        if(match_res[i] != '\0') response += match_res[i];
                        else break;
                    }
                }
            }
            catch(SocketException se){
                MessageBox.Show("Socket exception: " + se.Message);
            }
            catch(IOException io){
                MessageBox.Show("IO Exception: " + io.Message);
            }
            return response;
        }
        
        private void Send(TcpClient Client, string command){
            byte[] message = encoder.GetBytes(command.Length.ToString() + "|" + command);
            NetworkStream stream = Client.GetStream();
            stream.Write(message, 0, message.Length);
            stream.Flush();
        }
        
        private void MoveFromNet(string str){
            Regex rx = new Regex(@"^(\d+),(\d+):(\d+),(\d+)$");
            Match match = rx.Match(str);
            
            int from_x = Convert.ToInt32(match.Groups[1].Value.ToString());
            int from_y = Convert.ToInt32(match.Groups[2].Value.ToString());
            int to_x = Convert.ToInt32(match.Groups[3].Value.ToString());
            int to_y = Convert.ToInt32(match.Groups[4].Value.ToString());
            
            Piece piece = resources.mf.GetPieceInSq(resources.mf.grid[from_x, from_y]);
            Square to = resources.mf.grid[to_x, to_y];
            
            if(piece == null){
                MessageBox.Show("WHAT" + str);
                resources.mf.grid[from_x, from_y].highlight = true;
                resources.mf.Invalidate();
                return;
            }
            if(to == null){
                MessageBox.Show(to_x.ToString() + " " + to_y.ToString());
            }
            
            piece.MoveTo(to);
        }
        
        public void MoveToNet(string str){
            Send(Server, "mv:" + str);
        }
        
        private void Command_up(string cmd){
            MoveFromNet(cmd);
            resources.mf.CurrentTurn = resources.mf.CurrentTurn == SuitType.white ? SuitType.black : SuitType.white;
            resources.mf.Invalidate();
        }
        
        private void server(){
            Server.Connect(serverEndPoint);
            string info = Read(Server);
            SuitType st = ParseSuit(info);
            resources.mf.Start(st);
            
            string res = "";
            string prefix;
            string cmd;
            
            while(res != "terminate"){
                res = Read(Server);
                prefix = res.Substring(0, 3);
                cmd = res.Substring(3);
                
                switch(prefix){
                    case "up:": // The other player made a move, update our grid
                        Command_up(cmd);
                    break;
                }
                
            }            
        }
        
        private SuitType ParseSuit(string info){
            // Format:
            // s-w or s-b (suit-white or suit-black)
            SuitType st ;
            switch(info[2]){
                case 'w':
                    st = SuitType.white;
                    break;
                default:
                case 'b':
                    st = SuitType.black;
                    break;
            }
            return st;
        }
       
        public Netman(string address, int port){
            serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            encoder = new ASCIIEncoding();
        }
        
        public void Start(){
            Server = new TcpClient();
            serverThread = new Thread(new ThreadStart(server));;
            serverThread.Start();
        }
    }
}
