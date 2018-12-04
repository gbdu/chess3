using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gess{
    public class Grid {
        private Square[,] data;
        public Grid(){
            data = Init();
        }
        public Square this[int x, int y]{
            get {
                return data[x, y];
            }
            set {
                Monitor.Enter(data[x,y]);
                data[x,y] = value;
                Monitor.Exit(data[x,y]);
            }
        }
        private Square[,] Init(){
            Square[,] grid = new Square[8, 8] ;
            // Set the original grid
            for(int r = 0; r != 8; r++){
                for(int c = 0; c != 8; c++){
                    grid[r, c] = new Square(r, c);
                }
            }
            return grid;
        }
    }
}
