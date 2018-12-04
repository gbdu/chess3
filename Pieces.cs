using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gess{
    public class Pieces {
        private Piece[] pieces;
        private Grid grid;
        private SuitType suit;
        
        private Piece[] Init(){
            Piece[] Suit = new Piece[16];
            int r = suit == SuitType.black ? 0 : 7;
            Suit[0] = new Piece(grid[0, r], RockType.rook, suit);
            Suit[1] = new Piece(grid[1, r], RockType.knight, suit);
            Suit[2] = new Piece(grid[2, r], RockType.bishop, suit);
            Suit[3] = new Piece(grid[3, r], RockType.queen, suit);
            Suit[4] = new Piece(grid[4, r], RockType.king, suit);
            Suit[5] = new Piece(grid[5, r], RockType.bishop, suit);
            Suit[6] = new Piece(grid[6, r], RockType.knight, suit);
            Suit[7] = new Piece(grid[7, r], RockType.rook, suit);
            for(int i = 8; i != 16; i++){
                Suit[i] = new Piece(grid[i-8, suit == SuitType.black ? 1 : 6], RockType.pawn, suit);
            }
            return Suit;
        }
        
        public Piece this[int i]{
            get {
                return pieces[i];
            }
            set {
                Monitor.Enter(pieces[i]);
                pieces[i] = value;
                Monitor.Exit(pieces[i]);
            }
        }
        
        public Pieces(Grid inGrid, SuitType Suit){
            suit = Suit;
            grid = inGrid;
            pieces = Init();
        }
    }
}
