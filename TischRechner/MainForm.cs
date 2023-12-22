using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
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

            List<string> currencyCodes = new List<string>
            {
            "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN",
            "BAM", "BBD", "BDT", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BRL",
            "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHF", "CLP", "CNY",
            "COP", "CRC", "CUP", "CVE", "CZK", "DJF", "DKK", "DOP", "DZD", "EGP",
            "ERN", "ETB", "EUR", "FJD", "FKP", "FOK", "GBP", "GEL", "GGP", "GHS",
            "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL", "HRK", "HTG", "HUF",
            "IDR", "ILS", "IMP", "INR", "IQD", "IRR", "ISK", "JEP", "JMD", "JOD",
            "JPY", "KES", "KGS", "KHR", "KID", "KMF", "KRW", "KWD", "KYD", "KZT",
            "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL", "MGA", "MKD",
            "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN", "MYR", "MZN",
            "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR", "PAB", "PEN", "PGK",
            "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD", "RUB", "RWF", "SAR",
            "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLE", "SOS", "SRD", "SSP",
            "STN", "SYP", "SZL", "THB", "TJS", "TMT", "TND", "TOP", "TRY", "TTD",
            "TVD", "TWD", "TZS", "UAH", "UGX", "USD", "UYU", "UZS", "VES", "VND",
            "VUV", "WST", "XAF", "XCD", "XDR", "XOF", "XPF", "YER", "ZAR", "ZMW",
            "ZWL"
            };

            foreach (string code in currencyCodes)
            {
                ListBox_CC_From.Items.Add(code);
                ListBox_CC_To.Items.Add(code);
            }

            ListBox_CC_From.SelectedItem = "EUR";
            ListBox_CC_To.SelectedItem = "GBP";
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
            btn_CC_Convert.Enabled = true;
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

        private void UpdateRound(double updatedNumber)
        {
            Calcs[Calcs.Count - 2].solution = updatedNumber;
            CalcWindow.Text = updatedNumber.ToString();
        }

        private void btn_CC_Convert_Click(object sender, EventArgs e)
        {
            string apiKey = "80f9b45c29ba5394ea3e6603";
            string fromCurrency = ListBox_CC_From.SelectedItem.ToString();
            string toCurrency = ListBox_CC_To.SelectedItem.ToString();

            HttpClient request = new HttpClient();
            HttpResponseMessage response = request.GetAsync($"https://v6.exchangerate-api.com/v6/{apiKey}/pair/{fromCurrency}/{toCurrency}").Result;
            request.Dispose();

            string result = response.Content.ReadAsStringAsync().Result;

            result = result.Replace("\"", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty);
            string[] results = result.Split(',');

            double converionRate = Convert.ToDouble(results[results.Length - 1].Split(':')[1]) / 10000;
            double number = Calcs[Calcs.Count - 2].solution;
            double numberConversion = number * converionRate;
            UpdateRound(numberConversion);
        }
    }
}