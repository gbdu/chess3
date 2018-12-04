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
    static class resources{
        static public Image[] WhiteImages = new Image[6];
        static public Image[] BlackImages = new Image[6];
        static public int SquareDim;
        static public MainForm mf;
        
        static public void InitImages(SuitType player){
            for(int i = (int)RockType.rook; i != (int)RockType.pawn+1; i++){
                RockType rock = (RockType) i;
                    WhiteImages[i] = Image.FromFile("B:\\code\\C#\\Gess\\rocks\\white_" + rock.ToString() + ".gif");
                    BlackImages[i] = Image.FromFile("B:\\code\\C#\\Gess\\rocks\\black_" + rock.ToString() + ".gif");
            }
        }
   }
}
