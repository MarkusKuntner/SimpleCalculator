/* <Mini Calculator>
    Copyright(C) <2019>  <Markus Kuntner>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<https://www.gnu.org/licenses/>. 
*/
/*
 * Created by SharpDevelop.
 * User: Markus
 * Date: 28.03.2016
 * Time: 14:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

namespace Rechner
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private ContextMenuStrip myMenu;
		private bool Form_on_Top = true;
		private bool Main_mouse = false;
		private Point _start_point=new Point(0,0);
		private bool Radiant = true;
		private bool Output = true;
		private double Result = 0;

		/// <summary>
        /// Pointer to the recent input[] string
        /// </summary>
        private int input_counter = 0;
		/// <summary>
        /// Contains up to 10 input strings
        /// </summary>
        private string[] input = new string[10];
		
		private bool marked = false;
		private int caret_pos;

        private MathParser calc = new MathParser(true, true, true, true);


        public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			myMenu = new ContextMenuStrip();
			SetMenu(Form_on_Top);
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		private void SetMenu(bool ontop)
		{
			ToolStripMenuItem copy = new ToolStripMenuItem("Copy");
			copy.Click += new System.EventHandler(this.CopyClick);
			
			ToolStripMenuItem paste = new ToolStripMenuItem("Paste");
			paste.Click += new System.EventHandler(this.PasteClick);
			
			ToolStripMenuItem alwaysontop = new ToolStripMenuItem("Always on top");
			alwaysontop.Checked = ontop;
			alwaysontop.Click += new System.EventHandler(this.AlwaysOnTopClick);

            ToolStripMenuItem clearstack = new ToolStripMenuItem("Clear stack");
            clearstack.Click += new System.EventHandler(this.ClearStack);

            ToolStripMenuItem info = new ToolStripMenuItem("Info");
			info.Click += new System.EventHandler(this.InfoClick);
			
			ToolStripMenuItem exit = new ToolStripMenuItem("Exit the calculator");
			exit.Click += new System.EventHandler(this.ExitClick);
			
		    myMenu.Items.AddRange(new ToolStripItem[]{
			                      	copy, 
			                      	paste, 
                                    clearstack,
			                      	new ToolStripSeparator(),
			                      	alwaysontop, 
			                      	info, 
			                      	new ToolStripSeparator(),
			                      	exit});
			
			textBox1.ContextMenuStrip = myMenu;
			textBox2.ContextMenuStrip = myMenu;
		}
		
		void MainFormMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) // just if right mouse button
			{
				_start_point = new Point(e.X, e.Y);
				myMenu.Show(this, _start_point);
			}
		}
		
		private void CopyClick(object sender, EventArgs e)
		{
			textBox2.Copy();
		}
		private void PasteClick(object sender, EventArgs e)
		{
			textBox1.Paste();
		}
        private void ClearStack(object sender, EventArgs e)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = String.Empty;
            }
            textBox1.Text = string.Empty;
        }

		private void AlwaysOnTopClick(object sender, EventArgs e)
		{
			myMenu.Items.Clear();
			Form_on_Top =! Form_on_Top;
			this.TopMost = Form_on_Top;
			SetMenu(Form_on_Top);
		}
		
		private void InfoClick(object sender, EventArgs e)
		{
			Form1 infoform = new Form1();
			infoform.ShowDialog();
		}
		
		
		private void ExitClick(object sender, EventArgs e)
		{
			DialogResult res = MessageBox.Show("Do you want to quit this application?", "Mini Calculator", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (res == DialogResult.Yes)
			{
				this.Close();
			}
         }
			
		void MainFormMouseDown(object sender, MouseEventArgs e)
		{
			Main_mouse = true;
			_start_point = new Point(e.X, e.Y);

		}

		void MainFormMouseUp(object sender, MouseEventArgs e)
		{
			Main_mouse = false;
		}

		void MainFormLoad(object sender, EventArgs e)
		{
            try
			{
                textBox1.SelectionChanged += new System.EventHandler(TextBox1SelectionChanged);
                string calcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCalc", "Calculator.ini");
                using (StreamReader reader = new StreamReader(calcPath))
				{
					Point pt = new Point();
					Screen.GetWorkingArea(pt);
					string a = reader.ReadLine();
                    int i = 0;
                    if (int.TryParse(a, out i))
                    {
                        this.Left = i;
                    }
					a = reader.ReadLine();
                    if (int.TryParse(a, out i))
                    {
                        this.Top = i;
                    }
                    for (i = 0; i < input.Length; i++)
                    {
                        if (!reader.EndOfStream)
                        {
                            input[i] = reader.ReadLine();
                        }
                    }
				};
			}
			catch
			{}
		}

		void MainFormMouseMove(object sender, MouseEventArgs e)
		{
			if (Main_mouse == true)
			{
				Point p = PointToScreen(e.Location);
				Location = new Point(p.X - this._start_point.X,p.Y - this._start_point.Y);
			}
		}
	
		/// <summary>
        /// Analyze the user input and parse a value or rotate through the input[] string array
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox1KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				string parseString = textBox1.Text.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
                string NewVariable = string.Empty;

                if (parseString.Contains("=") && parseString.IndexOf("=") > 0)
                {
                    NewVariable = RemoveWhiteSpace(parseString);
                    NewVariable = parseString.Substring(0, NewVariable.IndexOf("=")).ToLower();
                    parseString = parseString.Substring(parseString.IndexOf("=") + 1);
                }
                
                try
				{
					input[input_counter] = textBox1.Text;
                    label1.Text = input_counter.ToString();

                    input_counter++;
					if (input_counter >= input.Length) input_counter = 0;
                    
					Result = calc.Parse(parseString);
					WriteResult();
					textBox2.BackColor = Color.WhiteSmoke;

                    if (NewVariable.Length > 0)
                    {
                        if (calc.LocalVariables.ContainsKey(NewVariable))
                            calc.LocalVariables[NewVariable] = Result;
                        else
                            calc.LocalVariables.Add(NewVariable, Result);
                    }
                    
                }
				catch(Exception ex)
				{
                    textBox2.Text = "Syntax Error" + ex.Message;
					textBox2.BackColor = Color.Yellow;
					Result = Double.NaN;
				}
                
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			
			if (e.KeyCode == Keys.PageUp)
			{
				textBox1.Text = textBox2.Text;
				textBox1.Select(textBox1.Text.Length, 0);
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			
			if (e.KeyCode == Keys.Down)
			{
				input_counter--;
				if (input_counter < 0) input_counter = input.Length-1;
				textBox1.Text = input[input_counter];
                label1.Text = input_counter.ToString();
            }

            if (e.KeyCode == Keys.Up)
            {
                input_counter++;
                if (input_counter >= input.Length) input_counter = 0;
                textBox1.Text = input[input_counter];
                label1.Text = input_counter.ToString();
            }
		}
		
		/// <summary>
        /// Write a result to the 2nd line
        /// </summary>
        void WriteResult()
		{
			string result;
			if (Output)
				result = Convert.ToString(Result);
			else
			{
				if (Result != 0)
				{
					try
					{
						int mantisse = Convert.ToInt32(Math.Floor(Math.Log10(Math.Abs(Result))));
						double a = Result / Math.Pow(10, mantisse);
						if (mantisse >0)
							result = Convert.ToString(a) +"E+"+Convert.ToString(mantisse);
						else
							result = Convert.ToString(a) +"E"+Convert.ToString(mantisse);
						//result = Result.ToString("E", CultureInfo.InvariantCulture);
					}
					catch
					{
						result = Convert.ToString(Result); // if infinity
					}
				}
				else
					result = Convert.ToString(Result); //0
			}
			textBox2.Text = result;
		}
		
		
		
		/// <summary>
        /// Toggle Radiant or Degree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button1Click(object sender, EventArgs e)
		{
			Radiant =! Radiant;

            // deep copy of local vars
            Dictionary<string, double> _localDic = new Dictionary<string, double>();
            foreach (var locV in calc.LocalVariables)
            {
                if (!_localDic.ContainsKey(locV.Key))
                {
                    _localDic.Add(locV.Key, locV.Value);
                }
            }

            calc = new MathParser(true, true, true, Radiant);
            
            //restore values
            foreach (var locV in _localDic)
            {
                if (!calc.LocalVariables.ContainsKey(locV.Key))
                {
                    calc.LocalVariables.Add(locV.Key, locV.Value);
                }
            }

            if (Radiant)
			{
				button1.BackgroundImage = Rechner.Resource1.RAD;
				button1.BackgroundImageLayout = ImageLayout.Stretch;
            }
			else
			{
				button1.BackgroundImage = Rechner.Resource1.Deg;
				button1.BackgroundImageLayout = ImageLayout.Stretch;             
            }
		}
		/// <summary>
        /// Toggle Floating or Fixed comma 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button2Click(object sender, EventArgs e)
		{
			Output =! Output;
			if (Output)
			{
				button2.BackgroundImage = Rechner.Resource1.Dec;
				button2.BackgroundImageLayout = ImageLayout.Stretch;
				
			}
			else
			{
				button2.BackgroundImage = Rechner.Resource1.Fix;
				button2.BackgroundImageLayout = ImageLayout.Stretch;
			}
			WriteResult();
		}
		
		/// <summary>
        /// Close and write ini file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
                string calcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCalc");
                if (!Directory.Exists(calcPath))
                {
                    Directory.CreateDirectory(calcPath);
                }

                calcPath = Path.Combine(calcPath, "Calculator.ini");

                using (StreamWriter writer = new StreamWriter(calcPath))
				{
					writer.WriteLine(Convert.ToString(this.Left));
					writer.WriteLine(Convert.ToString(this.Top));

                    for (int i = 0; i < input.Length; i++)
                    {
                        writer.WriteLine(input[i]);
                    }
                };		
	 		}
			catch
			{}
		}

        /// <summary>
        /// Find corresponding brackets 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void TextBox1SelectionChanged(Object sender, EventArgs e)
		{
			int length = textBox1.SelectionLength;
			
			if (length == 0 && marked) // reset color if user clicks again
			{
				marked = false; // make sure, that no endless loop occures
				textBox1.SelectionChanged -= new System.EventHandler(TextBox1SelectionChanged);
				
				// remove selection
				textBox1.SelectAll();
				textBox1.SelectionBackColor = textBox1.BackColor;
				textBox1.SelectionColor = textBox1.ForeColor;
				textBox1.DeselectAll();
				
				//textBox1.Text = textBox1.Text.ToString(); // a new selection change is forced!
				
				if (caret_pos >= 0 && caret_pos < textBox1.Text.Length)
				{
					textBox1.SelectionStart = caret_pos;
					textBox1.ScrollToCaret();
				}
				textBox1.SelectionChanged += new System.EventHandler(TextBox1SelectionChanged);
				return;
			}
			
			if (length > 0 && !marked) // if marked, don't select twice
			{
				int start = textBox1.SelectionStart;
				caret_pos = start;
				string a = textBox1.Text;
				
				if (a[start].Equals('('))
				{
					marked = true;

					// search the corresponding ")"
					int i = start+1;
					int count = 1; // input counter for "("
					while (i < a.Length && count > 0)
					{
						if (a[i].Equals('(')) count++;
						if (a[i].Equals(')')) count--;
						i++;
					}
					if (count == 0) // found corresponding ")"
					{
						textBox1.SelectionChanged -= new System.EventHandler(TextBox1SelectionChanged);
						textBox1.SelectionColor = Color.Yellow;
						textBox1.SelectionBackColor = Color.Red;
						textBox1.SelectionStart = i-1;
						textBox1.SelectionLength = 1;
						textBox1.SelectionBackColor = textBox1.BackColor;
						textBox1.SelectionColor = textBox1.ForeColor;
						textBox1.SelectionChanged += new System.EventHandler(TextBox1SelectionChanged);
					}
				}

				if (a[start].Equals(')') && start > 2)
				{
					marked = true;

					// search the corresponding ")"
					int i = start-1;
					int count = 1; // input counter for ")"
					while (i >= 0 && count > 0)
					{
						if (a[i].Equals(')')) count++;
						if (a[i].Equals('(')) count--;
						i--;
					}
					i++;
					if (count == 0 && i < a.Length-2 && i >= 0) // found corresponding "("
					{
						textBox1.SelectionChanged -= new System.EventHandler(TextBox1SelectionChanged);
						textBox1.SelectionColor = Color.Yellow;
						textBox1.SelectionBackColor = Color.Red;
						textBox1.SelectionStart = i;
						textBox1.SelectionLength = 1;
						textBox1.SelectionBackColor = textBox1.BackColor;
						textBox1.SelectionColor = textBox1.ForeColor;
						textBox1.SelectionChanged += new System.EventHandler(TextBox1SelectionChanged);

					}
				}
			}
        }

        /// <summary>
        /// Remove white space characters
        /// </summary>
        /// <param name="inpString"></param>
        /// <returns></returns>
        private string RemoveWhiteSpace(string inpString)
        {
            char[] result = new char[inpString.Length];
            int j = 0;
            for (int i = 0; i < inpString.Length; ++i)
            {
                char tmp = inpString[i];

                if (!char.IsWhiteSpace(tmp))
                {
                    result[j] = tmp;
                    j++;
                }
            }
            return new String(result, 0, j);
        }

    }
}
