using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TischRechner
{
    public partial class MainForm : Form
    {
        //Aktuell bearbeitende Zahl
        private string number = "";
        private string modifier = null;

        //Zwischenablage
        private string savedNumber = "";
        private string savedModifier = null;


        List<Calc> Calcs = new List<Calc>();
        public MainForm()
        {
            InitializeComponent();
            Calcs.Add(new Calc());
        }
        private void UpdateCalc(string op)
        {
            Calcs.Last().AddNumber(this.number, this.modifier);
            Calcs.Last().AddOperator(op);
            ResetNumber();
        }
        private void ResetNumber()
        {
            this.number = "";
            this.modifier = null;

            btn_decimal.Enabled = true;
            btn_00.Enabled = false;
        }
        private void btn_Remover_Click(object sender, EventArgs e)
        {
            Control btn = (Control)sender;

            switch(btn.Text)
            {
                case "C":
                    CalcWindow.ResetText();

                    Calcs.Last().operators.Clear();
                    Calcs.Last().numbers.Clear();

                    ResetNumber();
                    EnableButtons(false, false);
                    break;
                case "CE":
                    if (this.number.Length > 0)
                    {
                        CalcWindow.Text = CalcWindow.Text.Remove(CalcWindow.Text.Length - this.number.Length);

                        ResetNumber();
                        EnableButtons(false, false);
                    }
                    else if(Calcs.Last().numbers.Count == 0)
                        CalcWindow.ResetText();

                    break;
                case "◁":
                    if (this.number.Length > 0)
                    {
                        CalcWindow.Text = CalcWindow.Text.Remove(CalcWindow.Text.Length - 1);
                        this.number = this.number.Remove(this.number.Length - 1);
                        if(this.number.Length == 0)
                        {
                            EnableButtons(false, false);
                            btn_00.Enabled = false;
                        }  
                    }
                    else if(Calcs.Last().operators.Count > 0)
                    {
                        CalcWindow.Text = CalcWindow.Text.Remove(CalcWindow.Text.Length - 1);

                        Calcs.Last().operators.RemoveAt(Calcs.Last().operators.Count - 1);

                        string fullNumber = Calcs.Last().numbers.Last().ToString();

                        this.modifier = fullNumber.Contains("-") ? "-" : "+";
                        this.number = fullNumber.Replace("-", "");

                        Calcs.Last().numbers.RemoveAt(Calcs.Last().numbers.Count - 1);

                        EnableButtons(true, true);
                        if (this.number.Contains(","))
                            btn_decimal.Enabled = false;
                    }
                    break;
            }
        }
        private void EnableButtons(bool enable, bool signed)
        {
            btn_multiplication.Enabled = enable;
            btn_division.Enabled = enable;
            btn_equals.Enabled = enable;
            btn_power.Enabled = enable;
            btn_root.Enabled = enable;
            if(signed)
            {
                btn_plus.Enabled = enable;
                btn_minus.Enabled = enable;
            }
        }
        private void btn_Number_Click(object sender, EventArgs e)
        {
            Control btn = (Control)sender;

            if (Calcs.Last().numbers.Count == 0 && this.number.Length == 0 && this.modifier == null)
                CalcWindow.ResetText();

            if (this.modifier == null)
                this.modifier = "+";

            if (this.number == "0" && btn.Text != ",")
            {
                CalcWindow.Text = CalcWindow.Text.Remove(CalcWindow.Text.Length - 1);
                this.number = this.number.Remove(this.number.Length - 1);
            }

            if(btn.Text == ",")
                btn_decimal.Enabled = false;

            EnableButtons(true, true);
            
            this.number += btn.Text;
            CalcWindow.Text += btn.Text;

            if (this.number != "0")
                btn_00.Enabled = true;
        }
        private void btn_Operator_Click(object sender, EventArgs e)
        {
            Control btn = (Control)sender;

            if (Calcs.Last().numbers.Count == 0 && this.number.Length == 0 && this.modifier == null)
                CalcWindow.ResetText();

            if ((btn.Text == "+" || btn.Text == "-") && this.modifier == null)
            {
                this.modifier = btn.Text;
                EnableButtons(false, true);
            }
            else
            {
                UpdateCalc(btn.Text);
                if(this.modifier == null)
                    EnableButtons(false,false);                    
            }
            
            CalcWindow.Text += btn.Text;
        }
        private void btn_Equals_Click(object sender, EventArgs e)
        {
            UpdateCalc("=");
            EnableButtons(false, false);

            Calcs.Last().Calculate();
            CalcWindow.Text = Calcs.Last().solution.ToString();
            Calcs.Add(new Calc());

            btn_save.Enabled = true;
            btn_copy.Enabled = true;
            btn_round.Enabled = true;
            btn_cutoff.Enabled = true;
            btn_roundCommercial.Enabled = true;
        }
        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("Saved"))
                Directory.CreateDirectory("Saved");

            string date = DateTime.Now.ToString("yyyy.MM.dd");
            string file = $@"Saved/Calculation_{date}.csv";
            StreamWriter sw = new StreamWriter(file, true);

            for (int i = 0; i < Calcs[Calcs.Count - 2].numbers.Count; i++)
            {
                sw.Write($"{Calcs[Calcs.Count - 2].numbers[i]}{Calcs[Calcs.Count - 2].operators[i]}");
            }
            sw.WriteLine(Calcs[Calcs.Count - 2].solution + ";");
            sw.Close();
        }
        private void btn_Paste_Click(object sender, EventArgs e)
        {
            if (this.number.Length > 0)
                CalcWindow.Text = CalcWindow.Text.Remove(CalcWindow.Text.Length - this.number.Length);
            else if(Calcs.Last().numbers.Count == 0)
                CalcWindow.ResetText();


            CalcWindow.Text += this.savedNumber;

            this.number = this.savedNumber;
            this.modifier = this.savedModifier;

            EnableButtons(true,true);
        }

        private void btn_Copy_Click(object sender, EventArgs e)
        {
            string fullNumber = Calcs[Calcs.Count - 2].solution.ToString();

            this.savedModifier = fullNumber.Contains("-") ? "-" : "+";
            this.savedNumber = fullNumber.Replace("-", "");

            CalcSaved.Text = (this.savedModifier + this.savedNumber);
            btn_paste.Enabled = true;
        }

        private void CalcWindow_Click(object sender, EventArgs e)
        {

        }

        enum RoundingType
        {
            NormalRounding,
            CutOff,
            CommercialRounding
        }
        private void Rounding(RoundingType i)
        {
            double number = Calcs[Calcs.Count - 2].solution;
            double updatedNumber = 0;
            switch (i)
            {
                case RoundingType.NormalRounding:
                    updatedNumber = Math.Round(number, 2, MidpointRounding.AwayFromZero); //1,045 -> 1,05
                    break;
                case RoundingType.CutOff:
                    updatedNumber = Math.Truncate(100 * number) / 100; //1,045 -> 104,5 -> 104 -> 1,04
                    break;
                case RoundingType.CommercialRounding:
                    updatedNumber = Math.Round(number, 2); // 1,045 -> 1,04
                    break;
            }
            Calcs[Calcs.Count - 2].solution = updatedNumber;
            CalcWindow.Text = updatedNumber.ToString();
        }

        private void btn_round_Click(object sender, EventArgs e)
        {
            Rounding(RoundingType.NormalRounding);
        }

        private void btn_cutoff_Click(object sender, EventArgs e)
        {
            Rounding(RoundingType.CutOff);
        }

        private void btn_roundCommercial_Click(object sender, EventArgs e)
        {
            Rounding(RoundingType.CommercialRounding);
        }
    }
}