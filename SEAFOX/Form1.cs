using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Timers;

namespace SEAFOX
{

    public partial class Form1 : Form
    {

        XMLParser Parser = new XMLParser();
        private string cellOldValue, path, shortPath, filename, name;
        private List<string> xmlFiles = new List<string>();
        private String sdira = @"..\\..\\..\\XMLFiles";

        private List<XMLParser.varData> dataList = new List<XMLParser.varData>();

        private List<string> comboBox2Items = new List<string>();
        private Variable[] varList;
        private string[,] tableTests;

        private string sRootTreeFolder;
        private string sFullFilePath;
        private BackgroundWorker bw = new BackgroundWorker();
        //        System.Timers.Timer t = new System.Timers.Timer();
        private static System.Timers.Timer aTimer;
        private static double milisecons;
        public Form1()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            aTimer = new System.Timers.Timer(10);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            //aTimer.Enabled = true;
            //t.Interval = 50;  //in milliseconds
            //t.Elapsed += async (sender, e) => await HandleTimer();
            //t.Tick += new System.EventHandler(t_Tick);
            //timer1.Interval = 50;
            //timer1.Tick += new System.EventHandler(t_Tick);

            //start timer when form loads
            //t.Start();  //this will use t_Tick() method
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            milisecons += aTimer.Interval;
            ////get current time
            //int hh = DateTime.Now.Hour;
            //int mm = DateTime.Now.Minute;
            //int ss = DateTime.Now.Second;

            ////time
            //string time = "";

            ////padding leading zero
            //if (hh < 10)
            //{
            //    time += "0" + hh;
            //}
            //else
            //{
            //    time += hh;
            //}
            //time += ":";

            //if (mm < 10)
            //{
            //    time += "0" + mm;
            //}
            //else
            //{
            //    time += mm;
            //}
            //time += ":";

            //if (ss < 10)
            //{
            //    time += "0" + ss;
            //}
            //else
            //{
            //    time += ss;
            //}
            //update label
            //label6.Text = time;
            label6.Invoke((MethodInvoker)delegate {
                label6.Text = milisecons.ToString();
            });
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //BackgroundWorker worker = sender as BackgroundWorker;
            TestGenerator tg = new TestGenerator();
            List<object> genericlist = e.Argument as List<object>;
            //this.label5.Text = "0%";
            //MessageBox.Show("0%");
            label5.Invoke((MethodInvoker)delegate {
                label5.Text = "0%";
            });
            //this.timer1.Start();
            milisecons = 0.0;
            aTimer.Start();
            BeginInvoke(new Action(() => timer1.Start()), null);
            //MessageBox.Show(((Variable[])genericlist[0]).Length.ToString());
            e.Result = tg.GetPairwiseTests((Variable[]) genericlist[0], (bool) genericlist[1], (int) genericlist[2],sender);
            
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label5.Invoke((MethodInvoker)delegate {
                label5.Text = (e.ProgressPercentage.ToString() + "%");
            });
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            aTimer.Stop();
            dataGridView2.Columns.Clear(); // empty dataGridView
            dataGridView2.Rows.Clear();
            if (e.Cancelled == true)
            {
                MessageBox.Show("Canceled!");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message);
            }
            else
            {
                tableTests = (string[,]) e.Result;
                int numInput = tableTests.GetUpperBound(1);
                int numTests = tableTests.GetUpperBound(0);
                label8.Invoke((MethodInvoker)delegate
                {
                    label8.Text = numTests.ToString();
                });
                for (int i = 0; i < numInput; i++) // add a column for each input
                {
                    dataGridView2.Columns.Add("Column", varList[i].name + "(" + varList[i].datatype + ")");
                }
                for (int row = 0; row < numTests; row++) // set test to each cell in the dataGridView (one test case per row)
                {
                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[row].HeaderCell.Value = row.ToString();
                    for (int col = 0; col < numInput; col++)
                    {
                        dataGridView2.Rows[row].Cells[col].Value = tableTests[row, col];
                    }
                }
            }
            GenerateButton.Enabled = true;
        }
        private void OnLoad(object sender, EventArgs e)
        {
            
            comboBox1.SelectedIndex = 0;
            Type dgvType = dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dataGridView1, true, null);
            dgvType = dataGridView2.GetType();
            pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dataGridView2, true, null);

            //string[] arrays;
            //arrays = Directory.GetFiles(sdira, "*", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();

            /*foreach (string d in Directory.GetDirectories(sdira))
                foreach (string f in Directory.GetFiles(d, "*.*"))
                    xmlFiles.Add(f);*/

            //listBox1.DataSource = xmlFiles;
            //listBox1.Items.AddRange(arrays);

            //base.OnLoad(e);
        }
        private void DataGridView1_Scroll(Object sender, ScrollEventArgs e)
        {
            dataGridView1.Update();
        }
        private void DataGridView2_Scroll(Object sender, ScrollEventArgs e)
        {
            dataGridView2.Update();
        }
        private void DGV1(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].HeaderCell.Value = (e.RowIndex + 1).ToString();
        }
        private void DGV2(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            dataGridView2.Rows[e.RowIndex].HeaderCell.Value = (e.RowIndex + 1).ToString();
        }


        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows.Count != 0 && /*(*/e.ColumnIndex == 2 /*|| e.ColumnIndex == 3)*/)
            {
                int i = 0;
                string cellContent = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();

                string OLD_pattern_INT = @"^([-]?[0-9]+[_][-]?[0-9]+[;]?)+$"; // e.g. 2_3 or 4_7;9_11;
                string OLD_pattern_BOOL = @"^([0-1][_][0-1][;]?)+$"; // e.g. 0_0 or 0_1 or 1_1 (also 1_0)
                string pattern_BOOL = @"^([0-1]{1}([_][0-1]{1}?)?)+$";
                string pattern_REAL = @"^([-]?[0-9]+[,][0-9][_][-]?[0-9]+[,][0-9][;]?)+$"; // e.g. 2,0_3,0 or 4,0_7,1;9,2_11,2;
                string pattern_INT = @"^([-]?[0-9]+[;]?([_][-]?[0-9]+[;]?)?)+$";
                string pattern_UINT = @"^([0-9]+[;]?([_][0-9]+[;]?)?)+$";
                MatchCollection matches = Regex.Matches(cellContent, pattern_BOOL);


                switch (dataGridView1[e.ColumnIndex - 1, e.RowIndex].Value.ToString())
                {
                    case "INT":
                    case "LONG":
                        matches = Regex.Matches(cellContent, pattern_INT);
                        break;
                    case "BOOL":
                        matches = Regex.Matches(cellContent, pattern_BOOL);
                        break;
                    case "REAL":
                        matches = Regex.Matches(cellContent, pattern_REAL);
                        break;
                    case "WORD":
                    case "UINT":
                    case "USINT":
                    case "ULONG":
                        matches = Regex.Matches(cellContent, pattern_UINT);
                    deafult:
                        break;
                }

                if (matches.Count <= 0)
                {
                    MessageBox.Show("Incorrect input", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                /*if (matches.Count > 0)
                    MessageBox.Show("Match");
                else
                    MessageBox.Show("Incorrect range");*/

                /*//dataGridView1[e.ColumnIndex, e.RowIndex].Value = "hej";
                isNumeric = int.TryParse(cellContent, out i);

                if (!isNumeric)
                {
                    //MessageBox.Show("The input need to be numeric", "Wrong Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //dataGridView1[e.ColumnIndex, e.RowIndex].Value = cellOldValue;
                }*/
            }
        }

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            //cellOldValue = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            dataGridView1.Update();
            try
            {
                cellOldValue = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
            catch
            {
                return;
            }
        }

        private bool checkIfSupportSingles(string datatype)
        {
            switch (datatype)
            {   
                case "UINT":

                    return true;                
                default:return false;
            }
        }

        private void GenerateButton_Click(object sender, EventArgs e) // <---
        {

            TestGenerator tg = new TestGenerator();
            int numTests = 0;
            int numInput = dataGridView1.Rows.Count;
            int rows = 0;
            string rangeStr = "";
            string[] range = new string[2];
            char cSplitInterval = '_';
            char cSplitIntervals = ';';



            if (dataGridView1.Rows.Count == 0) // error handling
                return;

            // error handling
            if (XMLChoice.Checked)
                rows = dataGridView1.Rows.Count - 1;
            else
                rows = dataGridView1.Rows.Count;

            numInput = rows; // <---

            varList = new Variable[numInput];
            for (int row = 0; row < rows; row++) // read information from GUI <---
            {
                varList[row].name = dataGridView1.Rows[row].Cells[0].Value.ToString(); // name is in first column
                varList[row].datatype = dataGridView1.Rows[row].Cells[1].Value.ToString(); // datatype in second
                rangeStr = dataGridView1.Rows[row].Cells[2].Value.ToString(); // interval

                string[] i_vals = rangeStr.Split(cSplitIntervals);
                int numIntervals = i_vals.Length;
                //MessageBox.Show("Number of intervals: " + i_vals.Length);
                varList[row].intervals = new Interval[numIntervals];
                for (int i = 0; i < numIntervals; i++) // set all the intervals to the Variable
                {
                    if (Regex.Matches(i_vals[i], @"_").Count <= 0)
                    {
                        if (varList[row].datatype == "BOOL")
                        {
                            range[0] = "0";
                            range[1] = "1";
                        }
                        else
                        {
                            range[0] = range[1] = i_vals[i];
                        }

                    }
                    else
                    {
                        range = i_vals[i].Split(cSplitInterval);
                    }
                    varList[row].intervals[i].interval_a = range[0]; // start of interval 
                    varList[row].intervals[i].interval_b = range[1]; // end of interval 
                    //MessageBox.Show("Interval start: " + varList[row].intervals[i].interval_a + "");
                }
            }

            if (comboBox1.SelectedIndex == 0) // Base choice
            {
                VariableBC[] varListBC;
                varListBC = new VariableBC[numInput];

                for (int row = 0; row < rows; row++) // <---
                {
                    varListBC[row].variable = varList[row]; // add Variable to VariableBC list
                    varListBC[row].baseChoice = dataGridView1.Rows[row].Cells[3].Value.ToString(); // add base value to VariableBC list
                }

                tableTests = tg.GetBaseChoiceTests(varListBC); // generate base choice test cases
                numTests = tg.GetNumTestCases(); // get number of tests, which i calculated in GetBaseChoiceTests


            }
            else if (comboBox1.SelectedIndex == 1) // Random
            {
                numTests = Decimal.ToInt32(numericUpDown1.Value); // get number of tests from GUI
                tableTests = tg.GetRandomTests(numTests, varList); // generate random test cases
            }
            else if (comboBox1.SelectedIndex == 2) // Pairwise
            {
                //tableTests = tg.GetPairwiseTests(varList, checkBox1.Checked); // generate random test cases
                List<object> arguments = new List<object>();
                arguments.Add(varList);
                arguments.Add(checkBox1.Checked);
                arguments.Add(2);

                bw.RunWorkerAsync(arguments);
                GenerateButton.Enabled = false;
                return;
               // MessageBox.Show( numTests.ToString());
            }
            dataGridView2.Columns.Clear(); // empty dataGridView
            dataGridView2.Rows.Clear();

            for (int i = 0; i < numInput; i++) // add a column for each input
            {
                dataGridView2.Columns.Add("Column", varList[i].name + "(" + varList[i].datatype + ")");
            }

            for (int row = 0; row < numTests; row++) // set test to each cell in the dataGridView (one test case per row)
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[row].HeaderCell.Value = row.ToString();
                for (int col = 0; col < numInput; col++)
                {
                    dataGridView2.Rows[row].Cells[col].Value = tableTests[row, col];
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            int j = 0;
            float k = 0.0F;
            ushort l = 0;
            bool fromIsNumeric = true, toIsNumeric = true;

            int numInputs = dataGridView1.Rows.Count;
            if (XMLChoice.Checked)
                numInputs -= 1;

            switch (comboBox2.SelectedItem.ToString())
            {
                case "BOOL":
                    fromIsNumeric = int.TryParse(FromTextBox.Text.ToString(), out j); // check if content is an integer
                    toIsNumeric = int.TryParse(ToTextBox.Text.ToString(), out j); // check if content is an integer
                    break;
                case "INT":
                    fromIsNumeric = int.TryParse(FromTextBox.Text.ToString(), out j); // check if content is an integer
                    toIsNumeric = int.TryParse(ToTextBox.Text.ToString(), out j); // check if content is an integer
                    break;
                case "REAL":
                    fromIsNumeric = float.TryParse(FromTextBox.Text.ToString(), out k);
                    toIsNumeric = float.TryParse(ToTextBox.Text.ToString(), out k);
                    break;
                case "WORD": 
                    fromIsNumeric = UInt16.TryParse(FromTextBox.Text.ToString(), out l); // check if content is an integer
                    toIsNumeric = UInt16.TryParse(ToTextBox.Text.ToString(), out l); // check if content is an integer
                    break;
                default:
                    break;
            }

            if (!fromIsNumeric || !toIsNumeric) // error handling
            {
                MessageBox.Show("Incorrect input", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < numInputs; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString() == comboBox2.SelectedItem.ToString() && FromTextBox.Text != "" && ToTextBox.Text != "")
                {
                    switch (dataGridView1.Rows[i].Cells[2].Value.ToString())
                    {
                        case "0":
                            dataGridView1.Rows[i].Cells[2].Value = FromTextBox.Text + '_' + ToTextBox.Text;
                            break;

                        default:
                            dataGridView1.Rows[i].Cells[2].Value += ';' + FromTextBox.Text + '_' + ToTextBox.Text;
                            break;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            //dataGridView1.Rows.Add("1", "2", "3", "4");
            int j = 0;
            float k = 0.0F;
            ushort l = 0;
            bool fromIsNumeric = true, toIsNumeric = true;

            switch (comboBox2.SelectedItem.ToString())
            {
                case "BOOL":
                    fromIsNumeric = int.TryParse(FromTextBox.Text.ToString(), out j); // check if content is an integer
                    toIsNumeric = int.TryParse(ToTextBox.Text.ToString(), out j); // check if content is an integer
                    break;
                case "INT":
                    fromIsNumeric = int.TryParse(FromTextBox.Text.ToString(), out j); // check if content is an integer
                    toIsNumeric = int.TryParse(ToTextBox.Text.ToString(), out j); // check if content is an integer
                    break;
                case "REAL":
                    fromIsNumeric = float.TryParse(FromTextBox.Text.ToString(), out k);
                    toIsNumeric = float.TryParse(ToTextBox.Text.ToString(), out k);
                    break;
                case "WORD":
                    fromIsNumeric = UInt16.TryParse(FromTextBox.Text.ToString(), out l); // check if content is an integer
                    toIsNumeric = UInt16.TryParse(ToTextBox.Text.ToString(), out l); // check if content is an integer
                    break;
                default:
                    break;
            }

            if (!fromIsNumeric || !toIsNumeric) // error handling
            {
                MessageBox.Show("Incorrect input", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            int numInputs = dataGridView1.Rows.Count;
            if (XMLChoice.Checked)
                numInputs -= 1; // cannot compare to the empty 'add new'-row, therefore don't check the last row
            for (int i = 0; i < numInputs; i++) //save/overwrite range to all inputs of comboBox2 selected item
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString() == comboBox2.SelectedItem.ToString())
                    dataGridView1.Rows[i].Cells[2].Value = FromTextBox.Text + '_' + ToTextBox.Text; //
            }
        }

        private void Selected_pairwise()
        {
            checkBox1.Visible = true;
        }
        private void UnSelected_pairwise()
        {
            checkBox1.Visible = false;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: //base choice
                    if (dataGridView1.Columns.Count == 4)
                        return;
                    dataGridView1.Columns.Add(Column4);
                    numericUpDown1.Visible = false;
                    label7.Visible = false;
                    UnSelected_pairwise();
                    int numInputs = dataGridView1.Rows.Count;
                    if (XMLChoice.Checked)
                        numInputs -= 1;
                    for (int i = 0; i < numInputs; i++)
                        dataGridView1.Rows[i].Cells[3].Value = "0";
                    break;
                case 1: //random choice
                    if (dataGridView1.Columns.Contains(Column4)) 
                        dataGridView1.Columns.Remove(Column4);
                    numericUpDown1.Visible = true;
                    label7.Visible = true;
                    UnSelected_pairwise();
                    break;
                case 2: //pairwise
                    if (dataGridView1.Columns.Contains(Column4))
                        dataGridView1.Columns.Remove(Column4);
                    numericUpDown1.Visible = false;
                    label7.Visible = false;
                    Selected_pairwise();
                    break;
                default:
                    numericUpDown1.Visible = false;
                    label7.Visible = false;
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e) // Save test cases button click
        {
            if (dataGridView2.Rows.Count == 0) // error handling
                return;

            CSV csv = new CSV();
            //string name = listBox1.SelectedItem.ToString();

            if (TB_name.Text.Length > 0 && XMLChoice.Checked == false)
            {
                filename = TB_name.Text;
                csv.SaveTests(filename, getTestCasesFromGUI(), varList);
            }
            else if (TB_name.Text.Length <= 0 && XMLChoice.Checked == false)
            {
                MessageBox.Show("Please Enter filename");
            }
            else if (TB_name.Text.Length <= 0 && treeView1.SelectedNode.Text == null && XMLChoice.Checked == false)
            {
                MessageBox.Show("Please Enter filename");
            }
            else if (XMLChoice.Checked && TB_name.Text.Length > 0)
            {
                filename = TB_name.Text;
                csv.SaveTests(filename, getTestCasesFromGUI(), varList);
            }
            else if (XMLChoice.Checked && TB_name.Text.Length <= 0)
            {
                MessageBox.Show("Please Enter filename");
            }
            else
                MessageBox.Show("Please Enter filename");

            //csv.SaveTests(filename, getTestCasesFromGUI(), varList);
        }
        private List<List<string>> getTestCasesFromGUI() // read from the datagridview containing the test cases
        {
            List<List<string>> TestCases = new List<List<string>>();
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                List<string> temp = new List<string>();
                for (int j = 0; j < dataGridView2.Columns.Count; j++)
                {
                    temp.Add(dataGridView2.Rows[i].Cells[j].Value.ToString());
                }
                TestCases.Add(temp);
            }
            return TestCases;
        }

        private void button4_Click(object sender, System.EventArgs e) // MoveUp Multi
        {
            moveSelectedRowsUp();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            moveSelectedRowsDown();
        }


        private void moveSelectedRowsUp()
        { //uses the index of the selected rows and reorder them to be able to move several rows at once
            int numSelectedRows = dataGridView2.Rows.GetRowCount(DataGridViewElementStates.Selected);
            int numRows = dataGridView2.Rows.Count;
            DataGridViewRowCollection rows = dataGridView2.Rows; // all the rows in dataGridviw
            List<int> selected = new List<int>(); // index of all the selected rows

            if (numRows > 1) // if the gridView contains at least one row (other than the 'add new test'-row)
            {
                numRows -= 1; // don't include the 'add new test'-row

                for (int s = 0; s < numSelectedRows; s++) // get all the index of all the selected rows into a list
                {
                    selected.Add(dataGridView2.SelectedRows[s].Index); // get index in dataGridView for each of the selected rows
                }
                selected.Sort(); // increasing order
                foreach (int rowIndex in selected)
                {
                    if (rowIndex > 0 && rowIndex < numRows)
                    {
                        DataGridViewRow rowAboveSelected = rows[rowIndex - 1];
                        rowAboveSelected.Selected = false;
                        rows.Remove(rowAboveSelected);
                        rows.Insert(rowIndex, rowAboveSelected);
                        rows[rowIndex - 1].Selected = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        private void moveSelectedRowsDown()
        {
            int numSelectedRows = dataGridView2.Rows.GetRowCount(DataGridViewElementStates.Selected);
            int numRows = dataGridView2.Rows.Count;
            DataGridViewRowCollection rows = dataGridView2.Rows;
            List<int> selected = new List<int>();

            if (numRows > 1)
            {
                numRows -= 1; // don't include the 'add new test'-row and don't move beyond the last row

                for (int s = 0; s < numSelectedRows; s++)
                {
                    selected.Add(dataGridView2.SelectedRows[s].Index);
                }
                selected.Sort();
                selected.Reverse(); // Get values in decreasing order
                foreach (int rowIndex in selected)
                {
                    if (rowIndex >= 0 && rowIndex < (numRows - 1))
                    {
                        DataGridViewRow rowBelowSelected = rows[rowIndex + 1];
                        rowBelowSelected.Selected = false;
                        rows.Remove(rowBelowSelected);
                        rows.Insert(rowIndex, rowBelowSelected);
                        rows[rowIndex + 1].Selected = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFormState();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            LoadFormState();
        }

        private void SaveFormState()
        {
            FormState state = new FormState();

            // select range input
            state.RangeStart = FromTextBox.Text;
            state.RangeEnd = ToTextBox.Text;
            state.DataTypeSelectedIndex = comboBox2.SelectedIndex;

            state.FullFilePath = sFullFilePath; // path for file selected in treeview
            if (path != null)
                state.RootFolder = path;//sRootTreeFolder;

            state.CheckBoxXML = XMLChoice.Checked;

            state.Ranges = new List<string>();
            state.BaseValues = new List<string>();

            int numInputs = dataGridView1.Rows.Count; // number of inputs = number of rows in dataGridView1
            if (state.CheckBoxXML) // if XMLChoice is true, then there will be an extra empty 'add new input'-row which shouldn't be saved
                numInputs -= 1;
            for (int i = 0; i < numInputs; i++) // save all ranges (intervals) and base values if base choice is selected
            {
                state.Ranges.Add(dataGridView1.Rows[i].Cells[2].Value.ToString());
                if (comboBox1.SelectedIndex == 0 && dataGridView1.Rows[i].Cells[3].Value != null)
                {
                    state.BaseValues.Add(dataGridView1.Rows[i].Cells[3].Value.ToString());
                }
            }
            // save input from dataGridView1
            state.InputGrid = new List<List<string>>();
            for (int i = 0; i < numInputs; i++)
            {
                List<string> temp = new List<string>();
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    temp.Add(dataGridView1.Rows[i].Cells[j].Value.ToString());
                }
                state.InputGrid.Add(temp);
            }
            // save all generated test case from dataGridView2
            state.TestCases = new List<List<string>>();
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                List<string> temp = new List<string>();
                for (int j = 0; j < dataGridView2.Columns.Count; j++)
                {
                    temp.Add(dataGridView2.Rows[i].Cells[j].Value.ToString());
                }
                state.TestCases.Add(temp); // add each test case as a list to the list of lists
            }

            state.AlgorithmSelectedIndex = comboBox1.SelectedIndex; // index in combobox of selected algorithm
            state.NumTests = numericUpDown1.Value;


            state.VarList = varList; // list of all variables and their intervals, this list in generated when tests are generated

            state.Filename = TB_name.Text;
            //state.Filename = filename; // filename of file to save

            XmlSerializer xs = new XmlSerializer(typeof(FormState));
            using (FileStream fs = new FileStream("DataFormState.xml", FileMode.Create))
            {
                xs.Serialize(fs, state);
            }
        }

        private void LoadFormState()
        {
            FormState state;
            XmlSerializer xs = new XmlSerializer(typeof(FormState));
            using (FileStream fs = new FileStream("DataFormState.xml", FileMode.Open))
            {
                state = xs.Deserialize(fs) as FormState;
            }

            if (state != null)
            {
                XMLChoice.Checked = state.CheckBoxXML;
                comboBox1.SelectedIndex = state.AlgorithmSelectedIndex;

                dataGridView1.Rows.Clear();
                for (int i = 0; i < state.InputGrid.Count; i++) // add the input to the datagridview1
                {
                    dataGridView1.Rows.Add();
                    for (int j = 0; j < state.InputGrid.ElementAt(i).Count; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = state.InputGrid.ElementAt(i).ElementAt(j);
                    }
                }

                int numInputs = dataGridView1.Rows.Count; // number of inputs (variables) are the same as the number of rows in datagridview1
                if (state.CheckBoxXML) // if XMLChoice is true, then there will be an extra empty 'add new input'-row which shouldn't be added, since it is already there
                    numInputs -= 1;
                //for (int i = 0; i < numInputs; i++) // add a column for each input to the testcases
                //{
                //    dataGridView2.Columns.Add("Column", dataGridView1.Rows[i].Cells[0].Value + "(" + dataGridView1.Rows[i].Cells[1].Value + ")");
                //}


                if (!state.CheckBoxXML) // load tree view if NOT CheckBoxXML is false
                {
                    if (!File.Exists(state.FullFilePath)) // => check if state.FullFilePath exist
                    {
                        MessageBox.Show(Path.GetFileName(state.FullFilePath) + " not found");
                        return;
                    }
                    sFullFilePath = state.FullFilePath; // sett full file path

                    path = state.RootFolder;

                    ListDirectory(treeView1, state.RootFolder);
                }
                //Parser.xmlString = File.ReadAllText(state.FullFilePath);
                //dataList = new List<XMLParser.varData>();
                //dataList = Parser.readXMLDoc(dataList);
                dataGridView2.Columns.Clear();
                for (int i = 0; i < numInputs; i++) // add a column for each input to the testcases in datagridview2
                {
                    dataGridView2.Columns.Add("Column", dataGridView1.Rows[i].Cells[0].Value + "(" + dataGridView1.Rows[i].Cells[1].Value + ")");
                }
                dataGridView2.Rows.Clear();
                for (int i = 0; i < state.TestCases.Count; i++) // add the testcases to the datagridview2
                {
                    dataGridView2.Rows.Add();
                    for (int j = 0; j < state.TestCases.ElementAt(i).Count; j++)
                    {
                        dataGridView2.Rows[i].Cells[j].Value = state.TestCases.ElementAt(i).ElementAt(j);
                    }
                }

                if (state.AlgorithmSelectedIndex == 1) // if random, set number of tests
                {
                    numericUpDown1.Value = state.NumTests;
                }

                FromTextBox.Text = state.RangeStart;
                ToTextBox.Text = state.RangeEnd;
                comboBox2.SelectedIndex = state.DataTypeSelectedIndex;

                //filename = state.Filename;
                TB_name.Text = state.Filename;
                varList = state.VarList;
            }
            this.Refresh();
        }

        private void ToTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void XMLChoice_CheckedChanged(object sender, EventArgs e)
        {
            if (XMLChoice.Checked)
            {
                comboBox2Items.Clear();
                dataGridView1.Rows.Clear();
                dataGridView1.AllowUserToAddRows = true;
                Column1.ReadOnly = false;
                //Column2.ReadOnly = false;
                //TB_name.Enabled = true;
                B_FolderLoad.Enabled = false;
                treeView1.Enabled = false;
                //L_EnterFilename.Enabled = true;
                //L_csv.Enabled = true;
                TB_name.Text = "myTestCases";
                dataGridView2.Rows.Clear();
                dataGridView2.Columns.Clear();
            }
            else
            {
                dataGridView1.AllowUserToAddRows = false;
                Column1.ReadOnly = true;
                //Column2.ReadOnly = false;
                //TB_name.Enabled = false;
                B_FolderLoad.Enabled = true;
                treeView1.Enabled = true;
                //L_EnterFilename.Enabled = false;
                //L_csv.Enabled = false;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                comboBox2Items.Add(dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString());

                // remove doublets
                var uniqueItems = new HashSet<string>(comboBox2Items);
                comboBox2Items.Clear();
                foreach (string s in uniqueItems)
                    comboBox2Items.Add(s);

                // update datasource for comboBox2
                comboBox2.DataSource = null;
                comboBox2.DataSource = comboBox2Items;
            }
        }

        private void ToTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                AddButton.PerformClick();
            }
        }

        private void FromTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                AddButton.PerformClick();
            }
        }

        private void Button_Load()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
                ListDirectory(treeView1, path);
            }
            else
                MessageBox.Show("load failed");
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();

            var stack = new Stack<TreeNode>();
            if (path == null)
            {
                MessageBox.Show("please select root for treeview");
                Button_Load();
            }
            var rootDirectory = new DirectoryInfo(path);
            var node = new TreeNode(rootDirectory.Name) { Tag = rootDirectory };
            stack.Push(node);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                var directoryInfo = (DirectoryInfo)currentNode.Tag;
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    var childDirectoryNode = new TreeNode(directory.Name) { Tag = directory };
                    currentNode.Nodes.Add(childDirectoryNode);
                    stack.Push(childDirectoryNode);
                }
                foreach (var file in directoryInfo.GetFiles())
                    currentNode.Nodes.Add(new TreeNode(file.Name));
            }

            treeView.Nodes.Add(node);
        }

        private void B_FolderLoad_Click(object sender, EventArgs e)
        {
            Button_Load();
        }

        void treeView1_NodeMouseDoubleClick(object sender, EventArgs e)
        {
            shortPath = "";
            string[] pathWords = path.Split('\\');


            for (int i = 0; i < path.Split('\\').Count() - 1; i++)
            {
                shortPath += pathWords[i] + "\\";
            }

            string treeNodeName = treeView1.SelectedNode.ToString().Replace("TreeNode: ", String.Empty);

            if (treeView1.SelectedNode.FullPath.Substring(treeView1.SelectedNode.FullPath.Length - 4) == ".xml")
            {
                string filePath = shortPath + treeView1.SelectedNode.FullPath;
                List<XMLParser.varData> dataList = new List<XMLParser.varData>();
                sFullFilePath = filePath; // save full path to selected file
                XmlDocument xmlDoc = new XmlDocument();
                TB_name.Text = treeView1.SelectedNode.Text.Substring(0, treeView1.SelectedNode.Text.Length - 4);//set name for csv file
                //Parser.xmlString = File.ReadAllText(filePath);
                xmlDoc.Load(sFullFilePath);
                dataList = Parser.readXMLDoc(dataList, xmlDoc);

                comboBox2Items.Clear();
                dataGridView1.Rows.Clear();
                foreach (XMLParser.varData item in dataList)
                {
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0: //base choice
                            dataGridView1.Rows.Add(item.name, item.type, "0", "0");
                            break;
                        case 1: //random choice
                            dataGridView1.Rows.Add(item.name, item.type, "0");
                            break;
                        case 2: //pairwise
                            dataGridView1.Rows.Add(item.name, item.type, "0");
                            break;
                        default:
                            dataGridView1.Rows.Add(item.name, item.type, "0");
                            break;
                    }
                    comboBox2Items.Add(item.type);
                }
                dataGridView2.Columns.Clear();
                dataGridView2.Rows.Clear();

                // remove doublets
                var uniqueItems = new HashSet<string>(comboBox2Items);
                comboBox2Items.Clear();
                foreach (string s in uniqueItems)
                    comboBox2Items.Add(s);

                // update datasource for comboBox2
                comboBox2.DataSource = null;
                comboBox2.DataSource = comboBox2Items;
            }
            else
                MessageBox.Show("Can not open \"" + treeView1.SelectedNode.FullPath.Substring(treeView1.SelectedNode.FullPath.Length - 4) + "\" files");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView1.Enabled = false;
            dataGridView2.Enabled = false;
            DialogResult dialogResult = MessageBox.Show("Do you want to save state?", "Save State", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                SaveFormState();
            }

        }
    }
}
