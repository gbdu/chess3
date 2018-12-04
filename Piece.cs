using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace Gess{
    public class Piece{
        private RockType Type;
        private SuitType Suit;
        public Square sq;
        public bool dead;
        
        public Square currentSquare { get { return sq; }}
        public SuitType suit { get { return Suit; } }
        public bool IsDead { get { return dead; } }
        
        public RockType type {
            set {
                Type = value;
            }
            get {
                return Type;
            }
        }

        public void Die(){
            dead = !dead;
            //sq = null;
        }
        
        public Piece(Square Square, RockType InType, SuitType inSuit){
            Type = InType;
            Suit = inSuit;
            sq = Square;
        }
                
        private void MovePieceTo(Square NewSq){
            Piece TargetPiece = resources.mf.GetPieceInSq(NewSq);
            if(TargetPiece != null){
                TargetPiece.Die();
            }
            sq = NewSq;
        }
        
        private void GetPawnMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(16);
            killing_moves = new List<Square>(16);
            
            int nRow = Suit == SuitType.black ? 1 : -1;
            int mRow = Suit == SuitType.black ? 1 : 6;
            int oRow = Suit == SuitType.black ? 2 : -2;
            int xRow = Suit == SuitType.black ? 7 : 0;
            
            Piece nullPiece = new Piece(new Square(0,0), RockType.pawn, SuitType.white);
            
            // Piece one square ahead, if any

            Piece piece1 = sq.row+nRow <= 7 && sq.row+nRow >= 0 ? 
                resources.mf.GetPieceInSq(resources.mf.grid[sq.col, sq.row+nRow]) : nullPiece;
            // Piece two squares ahead, if any
            Piece piece2 = sq.row+oRow <= 7 && sq.row+oRow >= 0 ?
                resources.mf.GetPieceInSq(resources.mf.grid[sq.col, sq.row+oRow]) : nullPiece;
            
            // Piece forward-right
            if(sq.row+nRow <= 7 && sq.row+nRow >= 0 && sq.col-nRow <= 7 && sq.col-nRow >= 0){
                Square Sqr = resources.mf.grid[sq.col-nRow, sq.row+nRow] ;
                Piece piece = resources.mf.GetPieceInSq(Sqr);
                
                if(piece != null && Sqr != null && piece.suit != Suit && SafeForKing(Sqr)){
                    killing_moves.Add(Sqr);
                }
            }
            
            // Piece forward-left
            if(sq.row+nRow <= 7 && sq.row+nRow >= 0 && sq.col+nRow <= 7 && sq.col+nRow >= 0){
                Square Sqr = resources.mf.grid[sq.col+nRow, sq.row+nRow];
                Piece piece = resources.mf.GetPieceInSq(Sqr);
                
                if(piece != null && Sqr != null && piece.suit != Suit && SafeForKing(Sqr)){
                    killing_moves.Add(Sqr);
                }
            }
            
            if(sq.row == xRow){
                type = RockType.queen;
                return ;
            }
            
            // One square ahead. Only if there's no piece occupying that square.
            if(piece2 != nullPiece && piece1 == null && SafeForKing(resources.mf.grid[sq.col, sq.row+nRow])) {
                moving_moves.Add(resources.mf.grid[sq.col, sq.row+nRow]);
            }
            
            // Two squares ahead. Only if there's no piece occupying that square.
            if(piece2 != nullPiece && piece2 == null && sq.row == mRow && SafeForKing(resources.mf.grid[sq.col, sq.row+oRow])){
                moving_moves.Add(resources.mf.grid[sq.col, sq.row+oRow]);
            }
        }
        
        void GetKnightMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(16);
            killing_moves = new List<Square>(16);
            
            int nOne = Suit == SuitType.black ? 1 : -1;
            int nTwo = Suit == SuitType.black ? 2 : -2;
            Piece piece;
            // Two squares up, one square left
            if(sq.col+nOne <= 7 && sq.row+nTwo <= 7 && sq.col+nOne >= 0 && sq.row+nTwo >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col+nOne, sq.row+nTwo]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col+nOne, sq.row+nTwo])){
                        killing_moves.Add(resources.mf.grid[sq.col+nOne, sq.row+nTwo]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col+nOne, sq.row+nTwo])){
                    moving_moves.Add(resources.mf.grid[sq.col+nOne, sq.row+nTwo]);
                }
            }
            
            // Two squares up, one square right
            if(sq.col-nOne <= 7 && sq.row+nTwo <= 7 && sq.col-nOne >= 0 && sq.row+nTwo >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col-nOne, sq.row+nTwo]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col-nOne, sq.row+nTwo]) ){
                        killing_moves.Add(resources.mf.grid[sq.col-nOne, sq.row+nTwo]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col-nOne, sq.row+nTwo])){
                    moving_moves.Add(resources.mf.grid[sq.col-nOne, sq.row+nTwo]);
                }
            }
            
            // Two squares right, one square up
            if(sq.col-nTwo <= 7 && sq.row+nOne <= 7 && sq.col-nTwo >= 0 && sq.row+nOne >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col-nTwo, sq.row+nOne]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col-nTwo, sq.row+nOne])){
                        killing_moves.Add(resources.mf.grid[sq.col-nTwo, sq.row+nOne]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col-nTwo, sq.row+nOne])) {
                    moving_moves.Add(resources.mf.grid[sq.col-nTwo, sq.row+nOne]);
                }
            }
            
            // Two squares right, one square down
            if(sq.col-nTwo <= 7 && sq.row-nOne <= 7 && sq.col-nTwo >= 0 && sq.row-nOne >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col-nTwo, sq.row-nOne]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col-nTwo, sq.row-nOne])) {
                        killing_moves.Add(resources.mf.grid[sq.col-nTwo, sq.row-nOne]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col-nTwo, sq.row-nOne])){
                    moving_moves.Add(resources.mf.grid[sq.col-nTwo, sq.row-nOne]);
                }
            }
            
            // Two squares left, one square up
            if(sq.col+nTwo <= 7 && sq.row+nOne <= 7 && sq.col+nTwo >= 0 && sq.row+nOne >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col+nTwo, sq.row+nOne]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col+nTwo, sq.row+nOne])){
                        killing_moves.Add(resources.mf.grid[sq.col+nTwo, sq.row+nOne]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col+nTwo, sq.row+nOne])){
                    moving_moves.Add(resources.mf.grid[sq.col+nTwo, sq.row+nOne]);
                }
            }
            
            // Two squares left, one square down
            if(sq.col+nTwo <= 7 && sq.row-nOne <= 7 && sq.col+nTwo >= 0 && sq.row-nOne >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col+nTwo, sq.row-nOne]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col+nTwo, sq.row-nOne])) {
                        killing_moves.Add(resources.mf.grid[sq.col+nTwo, sq.row-nOne]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col+nTwo, sq.row-nOne])){
                    moving_moves.Add(resources.mf.grid[sq.col+nTwo, sq.row-nOne]);
                }
            }
            
            // Two squares down, one square left
            if(sq.col+nOne <= 7 && sq.row-nTwo <= 7 && sq.col+nOne >= 0 && sq.row-nTwo >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col+nOne, sq.row-nTwo]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col+nOne, sq.row-nTwo])){
                        killing_moves.Add(resources.mf.grid[sq.col+nOne, sq.row-nTwo]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col+nOne, sq.row-nTwo])) {
                    moving_moves.Add(resources.mf.grid[sq.col+nOne, sq.row-nTwo]);
                }
            }
            
            // Two squares down, one square right
            if(sq.col-nOne <= 7 && sq.row-nTwo <= 7 && sq.col-nOne >= 0 && sq.row-nTwo >= 0) {
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col-nOne, sq.row-nTwo]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col-nOne, sq.row-nTwo])){
                        killing_moves.Add(resources.mf.grid[sq.col-nOne, sq.row-nTwo]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col-nOne, sq.row-nTwo])) {
                    moving_moves.Add(resources.mf.grid[sq.col-nOne, sq.row-nTwo]);
                }
            }
        }
        
        void GetBishopMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(16);
            killing_moves = new List<Square>(16);
            int nOne = Suit == SuitType.black ? 1 : -1;
            Piece piece;
                      
            // Up-left
            for(int x = sq.col+nOne, y = sq.row+nOne; x >= 0 && y >= 0 && x <= 7 && y <= 7; x+=nOne, y+=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, y])) {
                        killing_moves.Add(resources.mf.grid[x, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, y])) moving_moves.Add(resources.mf.grid[x, y]);
            }
            
            // Up-right
            for(int x = sq.col-nOne, y = sq.row+nOne; x >= 0 && y >= 0 && x <= 7 && y <= 7; x-=nOne, y+=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, y])) {
                        killing_moves.Add(resources.mf.grid[x, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, y])) moving_moves.Add(resources.mf.grid[x, y]);
            }
            
            // Down-left
            for(int x = sq.col+nOne, y = sq.row-nOne; x >= 0 && y >= 0 && x <= 7 && y <= 7; x+=nOne, y-=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, y])) {
                        killing_moves.Add(resources.mf.grid[x, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, y])) moving_moves.Add(resources.mf.grid[x, y]);
            }
            
            // Down-right
            for(int x = sq.col-nOne, y = sq.row-nOne; x >= 0 && y >= 0 && x <= 7 && y <= 7; x-=nOne, y-=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, y])) {
                        killing_moves.Add(resources.mf.grid[x, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, y])) moving_moves.Add(resources.mf.grid[x, y]);
            }
        }
        
        void GetRookMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(16);
            killing_moves = new List<Square>(16);
            
            int nOne = Suit == SuitType.black ? 1 : -1;
            Piece piece;

            // Left
            for(int x = sq.col+nOne; x >= 0 && x <= 7; x+=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, sq.row]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, sq.row])) {
                        killing_moves.Add(resources.mf.grid[x, sq.row]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, sq.row])) moving_moves.Add(resources.mf.grid[x, sq.row]);
            }
            
            // Right
            for(int x = sq.col-nOne; x >= 0 && x <= 7; x-=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, sq.row]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, sq.row])){
                        killing_moves.Add(resources.mf.grid[x, sq.row]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[x, sq.row])) moving_moves.Add(resources.mf.grid[x, sq.row]);
            }
            
            // Top
            for(int y = sq.row+nOne; y >= 0 && y <= 7; y+=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col, y])){
                        killing_moves.Add(resources.mf.grid[sq.col, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[sq.col, y])) moving_moves.Add(resources.mf.grid[sq.col, y]);
            }
            
            // Down
            for(int y = sq.row-nOne; y >= 0 && y <= 7; y-=nOne){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col, y]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col, y])){
                        killing_moves.Add(resources.mf.grid[sq.col, y]) ;
                    }
                    break;
                }
                if(SafeForKing(resources.mf.grid[sq.col, y])) moving_moves.Add(resources.mf.grid[sq.col, y]);
            }
        }
        
        void GetKingMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(16);
            killing_moves = new List<Square>(16);
            Piece piece;
            
            // Black: Top 3 squares
            // White: Bottom 3 squares
            for(int x = sq.col-1; x != sq.col+2; x++){
                if(x < 0 || sq.row+1 < 0 || x > 7 || sq.row+1 > 7)
                    continue;
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, sq.row+1]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, sq.row+1]) ){
                        killing_moves.Add(resources.mf.grid[x, sq.row+1]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[x, sq.row+1])){
                    moving_moves.Add(resources.mf.grid[x, sq.row+1]);
                }
            }
            
            // Black: Bottom 3 squares
            // White: Top 3 squares
            for(int x = sq.col-1; x != sq.col+2; x++){
                if(x < 0 || sq.row-1 < 0 || x > 7 || sq.row-1 > 7)
                    continue;
                piece = resources.mf.GetPieceInSq(resources.mf.grid[x, sq.row-1]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[x, sq.row-1])){
                        killing_moves.Add(resources.mf.grid[x, sq.row-1]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[x, sq.row-1])) {
                    moving_moves.Add(resources.mf.grid[x, sq.row-1]);
                }
            }
            
            // Black: left
            // White: right
            if(sq.col+1 <= 7){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col+1, sq.row]);
                if(piece != null && SafeForKing(resources.mf.grid[sq.col+1, sq.row])){
                    if(piece.suit != Suit){
                        killing_moves.Add(resources.mf.grid[sq.col+1, sq.row]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col+1, sq.row])) {
                    moving_moves.Add(resources.mf.grid[sq.col+1, sq.row]);
                }
            }
            
            // Black: right
            // White: left
            if(sq.col-1 >= 0){
                piece = resources.mf.GetPieceInSq(resources.mf.grid[sq.col-1, sq.row]);
                if(piece != null){
                    if(piece.suit != Suit && SafeForKing(resources.mf.grid[sq.col-1, sq.row])){
                        killing_moves.Add(resources.mf.grid[sq.col-1, sq.row]);
                    }
                }
                else if(SafeForKing(resources.mf.grid[sq.col-1, sq.row])) {
                    moving_moves.Add(resources.mf.grid[sq.col-1, sq.row]);
                }
            }
        }
        
        void GetQueenMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(0);
            killing_moves = new List<Square>(0);
            List<Square> b_mm, b_km, r_mm, r_km;
            
            GetBishopMoves(out b_mm, out b_km);
            GetRookMoves(out r_mm, out r_km);
            
            moving_moves.InsertRange(0, b_mm);
            moving_moves.InsertRange(moving_moves.Count, r_mm);
            killing_moves.InsertRange(0, r_km);
            killing_moves.InsertRange(killing_moves.Count, b_km);
        }
        
        bool IsKingInDanger(){
            List<Square> killing_moves = null;
            List<Square> moving_moves = null;
            Pieces pieces;
            Piece king;
            
            if(Suit == SuitType.white){
                if(resources.mf.CurrentTurn != SuitType.white) return false;
                pieces = resources.mf.Black;
                king = resources.mf.White[4];
            }
            else{
                if(resources.mf.CurrentTurn != SuitType.black) return false;
                pieces = resources.mf.White;
                king = resources.mf.Black[4];
            }
            
            for(int i = 0; i != 8; i++){
                if(pieces[i].IsDead) continue;
                pieces[i].GetMoves(out moving_moves, out killing_moves);
                
                foreach(Square cSqr in killing_moves){
                    if(king.sq == cSqr) return true;
                }
                foreach(Square cSqr in moving_moves){
                    if(king.sq == cSqr) return true;
                }
            }
            return false;
        }
        
        bool SafeForKing(Square to){
            Square old_sqr = sq;
            sq = to;
            bool was_alive = false;
            bool safe = false;
            Piece piece = resources.mf.GetPieceInSq(to);
            
            if(piece != null){
                piece.dead = true;
                was_alive = true;
            }
            if(!IsKingInDanger()){
                safe = true;
            }
            if(was_alive){
                piece.dead = false;
            }
            
            sq = old_sqr;
            return safe;
        }
        
        void GetMoves(out List<Square> moving_moves, out List<Square> killing_moves){
            moving_moves = new List<Square>(0);
            killing_moves = new List<Square>(0);

            switch(type){
                case RockType.pawn:
                    GetPawnMoves(out moving_moves, out killing_moves);
                    return;
                case RockType.knight:
                    GetKnightMoves(out moving_moves, out killing_moves);
                    return;
                case RockType.bishop:
                    GetBishopMoves(out moving_moves, out killing_moves);
                    return;
                case RockType.rook:
                    GetRookMoves(out moving_moves, out killing_moves);
                    return;
                case RockType.queen:
                    GetQueenMoves(out moving_moves, out killing_moves);
                    return;
                case RockType.king:
                    GetKingMoves(out moving_moves, out killing_moves);
                    return;
            }
        }
        
        public void HighlightMoves(){
            List<Square> moving_moves ;
            List<Square> killing_moves ;
            
            GetMoves(out moving_moves, out killing_moves);
                    
            foreach(Square Sqr in moving_moves){
                Sqr.move_highlight = true;
            }
            foreach(Square Sqr in killing_moves){
                Sqr.kill_highlight = true;
            }
        }
        
        public bool IsValidMove(Square SqIn){
            List<Square> ls;
            GetMoves(out ls, out ls);
            foreach(Square Sqr in ls){
                if(Sqr == SqIn) return true;
            }
            return false;
        }
        
        private string NetEncode(Square from, Square to){
            string str = from.col.ToString() + "," + from.row.ToString();
            str += ":" + to.col.ToString() + "," + to.row.ToString();
            return str;
        }

        public string MoveTo(Square ToSq){
           List<Square> moves;
           GetMoves(out moves, out moves);
           foreach(Square Sqr in moves){
                if(Sqr == ToSq){
                    string net = NetEncode(sq, ToSq);
                    MovePieceTo(ToSq);
                    return net;
                }
           }
           return "";
        }
    }
}