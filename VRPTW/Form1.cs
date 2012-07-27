using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VRP.VRPTW.Data;
using VRP.VRPTW.Heuristics.Construction;
using VRP.VRPTW.Heuristics.LocalSearch;
using VRP.VRPTW.Gain;

namespace VRP
{
    public partial class Form1 : Form
    {
        private Solution _p123 = null;
        private Solution _p1 = null;
        private Solution _p2 = null;
        private Solution _p3 = null;
        private Solution _p12 = null;
        private Solution _p23 = null;
        private Solution _p31 = null;


        private Problem _problem = null;



        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        private List<Solution> Sols()
        {
            if (comboBox1.SelectedIndex == 0) return new List<Solution> { _p123 };
            return new List<Solution> { _p1, _p2, _p3 };
        }

        private void Button1Click(object sender, EventArgs e)
        {
            var reader = new CsvProblemReader();
            _problem = reader.Read(@"TestData/c1_2_1_v50_c200.csv");
            var sol87Solver = new Solomon87();

            var t = ShapleyDivider.Divide(_problem);

            _p1 = sol87Solver.Solve(t[0]);
            _p2 = sol87Solver.Solve(t[1]);
            _p3 = sol87Solver.Solve(t[2]);

            _p12 = sol87Solver.Solve(ShapleyDivider.Merge(t[0], t[1]));
            _p23 = sol87Solver.Solve(ShapleyDivider.Merge(t[1], t[2]));
            _p31 = sol87Solver.Solve(ShapleyDivider.Merge(t[2], t[0]));

            _p123 = sol87Solver.Solve(_problem);


            textBox1.Text = _p123.PrintToString();
            Draw();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*var wl09Solver = new WochLebkowski09();
            for (int i = 0; i < 1; ++i )
                _p123 = wl09Solver.Solve(_problem, _p123);*/

            var x = ShapleyDivider.ComputeGains(ref _p1, ref _p2, ref  _p3, ref _p12, ref _p23, ref _p31, ref _p123, 300, Draw);


            Draw();
        }


        private void Draw(int totalIters=0, int currentIter=0)
        {
            var sols = Sols();

            var scale = (double) numericUpDown1.Value;

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            var g = Graphics.FromImage(pictureBox1.Image);

            g.Clear(Color.White);

            var iCol = 0;
            var rColors = new List<Brush>{Brushes.Red, Brushes.Blue, Brushes.Green};
            var vColors = new List<Brush> { Brushes.DarkRed, Brushes.DarkBlue, Brushes.DarkGreen };

            textBox1.Text = "";

            foreach(var s in sols){


                if (s != null)
                {
                    textBox1.Text += s.PrintToString() + "\r\n\r\n\r\n";


                    var rPen = new Pen(rColors[iCol], 1);
                    

                    var p = s.Problem;
                    if (p == null)
                        continue;
                    var x0 = 230 - (int)Math.Round(p.Depot.Info.X * scale);
                    var y0 = 230 - (int)Math.Round(p.Depot.Info.Y * scale);

                    foreach (var route in s.Routes)
                    {
                        for (var i = 0; i < route.RouteList.Count - 1; ++i)
                        {
                            var p0 = new Point((int)Math.Round(route.RouteList[i].Info.X * scale + x0),
                                            (int)Math.Round(route.RouteList[i].Info.Y * scale + y0));
                            var p1 = new Point((int)Math.Round(route.RouteList[i + 1].Info.X * scale + x0),
                                            (int)Math.Round(route.RouteList[i + 1].Info.Y * scale + y0));
                            g.DrawLine(rPen, p0, p1);
                        }
                    }

                    Point dp = new Point((int)Math.Round(p.Depot.Info.X * scale + x0),
                                        (int)Math.Round(p.Depot.Info.Y * scale + y0));

                    g.FillEllipse(Brushes.IndianRed, dp.X - 4, dp.Y - 4, 8, 8);

                    foreach (var customer in p.Customers)
                    {
                        Point pnt = new Point((int)Math.Round(customer.Info.X * scale + x0),
                                            (int)Math.Round(customer.Info.Y * scale + y0));
                        g.FillEllipse(vColors[iCol], pnt.X - 3, pnt.Y - 3, 6, 6);
                    }

                }
                iCol++;

            }

            if (totalIters > 0)
                label3.Text = "performing: " + currentIter.ToString() + " / " + totalIters.ToString();
            else
                label3.Text = "";

            Update();
            Refresh();
            Application.DoEvents();


        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Draw();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Draw();
        }
    }
}
