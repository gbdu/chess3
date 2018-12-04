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
using System.Threading;

namespace Gess{
    public enum RockType { rook = 0, knight, bishop, queen, king, pawn };
    public enum SuitType { white, black };

    public partial class MainForm : Form{
        private Piece SelectedPiece = null;
        private bool started = false;
        private Netman netman;
        public SuitType PlayerSuit;
        public SuitType View;
        
        public int TurnCount = 0;
        public Grid grid;
        public Pieces White;
        public Pieces Black;
        public SuitType CurrentTurn = SuitType.white;
        
        public MainForm(){
            InitializeComponent();
            resources.SquareDim = (Width / 8) - 8;
            resources.mf = this;
            grid = new Grid();
            White = new Pieces(grid, SuitType.white);
            Black = new Pieces(grid, SuitType.black);
            netman = new Netman("127.0.0.1", 2010);
            resources.InitImages(SuitType.white);
            netman.Start();
        }
        
        public void Start(SuitType PlayerType){
            started = true;
            PlayerSuit = PlayerType;
            
            Invalidate();
        }
       
        void DrawLabels(PaintEventArgs e){
            Font font = new Font("Verdana", 20);
            for(int i = 0; i != 8; i++){
                e.Graphics.DrawString((8-i).ToString(), font, Brushes.Black, 8 * resources.SquareDim, i * resources.SquareDim);
                e.Graphics.DrawString(((char)('A' + i)).ToString(), font, Brushes.Black,
                 i * resources.SquareDim + 5, 8 * resources.SquareDim );
            }
            Font font2 = new Font("Verdana", 9);
            e.Graphics.DrawString("Current turn: " + CurrentTurn.ToString().ToLower(), font2, Brushes.Blue, 10, 9 * resources.SquareDim - 10);
        }
        
        void DrawRect(PaintEventArgs e, int i, int j, bool alt){
            Brush brush = alt ? Brushes.White : Brushes.Black;
            if(PlayerSuit == SuitType.white){
                brush = alt ? Brushes.Black : Brushes.White;
            }
            
            Pen pen = alt ? Pens.Gray : Pens.DarkGray;
            
            if(grid[i, j].highlight){
                brush = Brushes.Gold;
            }
            else if(grid[i, j].move_highlight){
                brush = Brushes.Lavender;
            }
            else if(grid[i, j].kill_highlight){
                brush = Brushes.DarkRed;
            }
            else if(grid[i, j].hover_highlight){
                brush = Brushes.LightBlue;
            }

            e.Graphics.FillRectangle(brush, i * resources.SquareDim,
             j * resources.SquareDim - 5, resources.SquareDim, resources.SquareDim);
             
            e.Graphics.DrawRectangle(pen, i * resources.SquareDim,
             j * resources.SquareDim - 5, resources.SquareDim, resources.SquareDim);
        }
        
        public void DrawPiece(PaintEventArgs e, Piece piece){
            Image image;
            int x , y;
            
            if(piece.suit == SuitType.white){
                image = resources.WhiteImages[(int)piece.type];
            }
            else{
                image = resources.BlackImages[(int)piece.type];
            }
            
            x = piece.sq.col;
            y = piece.sq.row;
            
            e.Graphics.DrawImage(image, new Point(x * resources.SquareDim + 5, y * resources.SquareDim + 2));
        }
        
        void DrawRectangles(PaintEventArgs e){
            bool alt = false;
            for(int j = 0; j != 8; j++, alt = !alt) {
                for(int i = 0; i != 8; i++, alt = !alt){
                    DrawRect(e, i, j, alt);
                }
            }
        }
        
        void DrawPieces(PaintEventArgs e){
            for(int i = 0; i != 16; i++){
                if(!White[i].IsDead) DrawPiece(e, White[i]);
                if(!Black[i].IsDead) DrawPiece(e, Black[i]);
            }
        }
        
        private void MainForm_Paint(object sender, PaintEventArgs e){
            if(!started) return;
            DrawRectangles(e);
            DrawPieces(e);
            DrawLabels(e);
        }
        
        Square GetSquareFromClick(MouseEventArgs e){
            Rectangle rect;
            Square sq = grid[0,0];
            for(int i = 0; i != 8; i++){
                for(int j = 0; j != 8; j++){
                    rect = new Rectangle(grid[i, j].col * resources.SquareDim, grid[i, j].row * resources.SquareDim,
                        resources.SquareDim, resources.SquareDim);
                    if(rect.Contains(new Point(e.X, e.Y))){
                        sq = grid[i, j];
                    }
                    grid[i, j].move_highlight = false;
                    grid[i, j].highlight = false;
                    grid[i, j].selected = false;
                    grid[i, j].kill_highlight = false;
                    grid[i, j].hover_highlight = false;
                }
            }
            return sq;
        }
        
        public Piece GetPieceInSq(Square sq){
            int i = 0;
            for(; i != 16; i++){
                if(White[i].currentSquare == sq && !White[i].IsDead){
                    return White[i];
                }
                if(Black[i].currentSquare == sq && !Black[i].IsDead){
                    return Black[i];
                }
            }
            return null;
        }
        
        private void MainForm_MouseClick(object sender, MouseEventArgs e){
            if(!started) return;
            
            Square sq = GetSquareFromClick(e);
            Piece piece = GetPieceInSq(sq);
            
            if(e.Button == MouseButtons.Right){
                SelectedPiece = null;
                Invalidate();
                return;
            }
            if(SelectedPiece != null){
                // A move to do with an already selected piece
                if(PlayerSuit != CurrentTurn){
                    MessageBox.Show("error");
                    SelectedPiece = null;
                    return;
                }
                else if(SelectedPiece == piece){
                    // User clicked the same piece again
                    return;
                }
                else if(SelectedPiece.IsValidMove(sq)){
                    // An already selected piece is being 
                    netman.MoveToNet(SelectedPiece.MoveTo(sq));
                    CurrentTurn = CurrentTurn == SuitType.white ? SuitType.black : SuitType.white;
                    SelectedPiece = null;
                }
                else if(piece != null && SelectedPiece.suit == CurrentTurn){
                    // Selecting another piece
                    sq.highlight = true;
                    sq.selected = true;
                    SelectedPiece = piece;
                    SelectedPiece.HighlightMoves();
                }
                else{
                    // Player de-selects his piece (to nothing)
                    SelectedPiece = null;
                }
            }
            else if(piece != null){
                // User selects a new piece
                sq.highlight = true;
                sq.selected = true;
                SelectedPiece = piece;
                piece.HighlightMoves();
            }
            Invalidate();
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e){
            if(!started) return;
            if(SelectedPiece != null)
                return;
            Square sq = GetSquareFromClick(e);
            Piece piece = GetPieceInSq(sq);
            if(piece != null){
                piece.HighlightMoves();
                sq.hover_highlight = true;
                Invalidate();
            }
        }
    }
    
    public class Square{
        public int row;
        public int col;
        public bool highlight;
        public bool kill_highlight;
        public bool move_highlight;
        public bool selected;
        public bool hover_highlight;

        public Square(int InCol, int InRow){
            row = InRow;
            col = InCol;
        }
    }
}
