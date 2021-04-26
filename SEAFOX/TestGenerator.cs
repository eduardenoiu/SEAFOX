using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEAFOX
{
    public struct Interval
    {
        public string interval_a;
        public string interval_b;
    }
    public struct Variable
    {
        public string name;
        public string datatype;
        public Interval[] intervals;
    }

    public struct VariableBC
    {
        public Variable variable;
        public string baseChoice;
    }
    public struct ParameterSet
    {
        public string name;
        public string datatype;
        public List <string> values;
    }
    public struct TestSuite
    {
        public List<List<string>> t_case;
    }

    class TestGenerator
    {
        private Random random = new Random();
        private int numTestCases;

        public string[,] GetBaseChoiceTests(VariableBC[] varList)
        {
            int numInput = varList.Length; // number of inputs
            string[] baseValues = new string[numInput];
            string[,] testCases;
            int k = 0, m = 0, q = 0;
            List<string>[] intervals = new List<string>[numInput]; // an array of lists, where each list contains all the values within the interval(s) for an input
            List<string> allValues;

            baseValues = getBaseValues(varList); // get all base values

            numTestCases = 0;
            for (int i = 0; i < numInput; i++)
            {
                intervals[i] = getAllFromIntervals(varList[i]); // all the values within the intervals, e.g. 2-5;7-9;17-19 -> (2,3,4,5,7,8,9,17,18,19)
                numTestCases = numTestCases + intervals[i].Count;
                //printListStr(intervals[i]); // test function just to print all the values within the intervals of a Variable
            }
            numTestCases = numTestCases - numInput + 1; // calculate the number of test cases

            testCases = new string[numTestCases, numInput]; // size of array which will contain the test cases, where each row is a test

            for (int i = 0; i < numInput; i++) // add the combination of all base values as the last test case
            {
                testCases[numTestCases - 1, i] = baseValues[i];
            }

            // generate the base choice test cases 
            for (int i = 0; i < numInput; i++)
            {
                allValues = getUniqueList(intervals[i]);  // list of all the values within the interval(s) for an input (doesn't contain any duplicates)
                //printListStr(allValues);
                //System.Windows.Forms.MessageBox.Show("All values count: " + allValues.Count);
                for (int a = 0; a < allValues.Count; a++)
                {
                    if (baseValues[i] != allValues.ElementAt(a))
                    {
                        testCases[k, i] = allValues.ElementAt(a);
                        k++;
                    }
                }
                for (int j = 0; j < numInput; j++)
                {
                    if (i != j)
                    {
                        for (m = q; m < k; m++)
                        {
                            testCases[m, j] = baseValues[j];
                        }
                    }
                }
                q = m;
            }
            return testCases;
        }

        private List<string> getUniqueList(List<string> list)
        {
            HashSet<string> hs = new HashSet<string>();
            for (int i = 0; i < list.Count; i++)
            {
                hs.Add(list.ElementAt(i));
            }
            return hs.ToList();
        }

        public int GetNumTestCases()
        {
            return numTestCases;
        }

        private List<string> getAllFromIntervals(VariableBC v) // get all the values within the intervals from a variable to an array
        {
            switch (v.variable.datatype)
            {
                case "TIME":
                case "UINT":
                case "USINT":
                    return getAllValues_UInt(v.variable.intervals);
                case "LONG":
                    return getAllValues_LInt(v.variable.intervals);
                case "ULONG":
                    return getAllValues_ULInt(v.variable.intervals);
                case "INT":
                    return getAllValues_Int(v.variable.intervals);
                case "BOOL":
                    return getAllValues_Int(v.variable.intervals);
                case "REAL":
                    return getAllValues_Float(v.variable.intervals);
                case "WORD":
                    return getAllValues_Word(v.variable.intervals);
                default:
                    List<string> s = new List<string>();
                    s.Add("Error");
                    return s;
            }
        }
        private List<string> getAllFromIntervals(Variable v) // get all the values within the intervals from a variable to an array
        {
            switch (v.datatype)
            {
                case "TIME":
                case "UINT":
                case "USINT":
                    return getAllValues_UInt(v.intervals);
                case "LONG":
                    return getAllValues_LInt(v.intervals);
                case "ULONG":
                    return getAllValues_ULInt(v.intervals);
                case "INT":
                    return getAllValues_Int(v.intervals);
                case "BOOL":
                    return getAllValues_Int(v.intervals);
                case "REAL":
                    return getAllValues_Float(v.intervals);
                case "WORD":
                    return getAllValues_Word(v.intervals);
                default:
                    List<string> s = new List<string>();
                    s.Add("Error");
                    return s;
            }
        }
        private List<string> getAllValues_LInt(Interval[] intervals) // returns a list of all the values within the intervals
        {
            long a = 0, b = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = Convert.ToInt64(intervals[i].interval_a);
                b = Convert.ToInt64(intervals[i].interval_b);

                while (a <= b)
                {
                    list.Add(a.ToString());
                    if (a == 9223372036854775807)
                    {
                        break;
                    }
                    a = a + 1;
                }
            }
            return list;
        }
        private List<string> getAllValues_ULInt(Interval[] intervals) // returns a list of all the values within the intervals
        {
            ulong a = 0, b = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = Convert.ToUInt64(intervals[i].interval_a);
                b = Convert.ToUInt64(intervals[i].interval_b);

                while (a <= b)
                {
                    list.Add(a.ToString());
                    if (a == 18446744073709551615)
                    {
                        break;
                    }
                    a = a + 1;
                }
            }
            return list;
        }
        private List<string> getAllValues_UInt(Interval[] intervals) // returns a list of all the values within the intervals
        {
            ulong a = 0, b = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = Convert.ToUInt32(intervals[i].interval_a);
                b = Convert.ToUInt32(intervals[i].interval_b);

                while (a <= b)
                {
                    list.Add(a.ToString());
                    if (a == 4294967295)
                    {
                        break;
                    }
                    a = a + 1;
                }
            }
            return list;
        }
        private List<string> getAllValues_Int(Interval[] intervals) // returns a list of all the values within the intervals
        {
            int a = 0, b = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = Int32.Parse(intervals[i].interval_a);
                b = Int32.Parse(intervals[i].interval_b);
                while (a <= b)
                {
                    list.Add(a.ToString());
                    a = a + 1;
                    if (a == 2147483647)
                    {
                        break;
                    }
                }
            }
            return list;
        }

        private List<string> getAllValues_Float(Interval[] intervals) // returns a list of all the values within the intervals
        {
            float a = 0.0f, b = 0.0f;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = float.Parse(intervals[i].interval_a);
                b = float.Parse(intervals[i].interval_b);
                while (a <= b)
                {
                    a = float.Parse(Math.Round((Decimal)a, 1) + "");
                    list.Add(a.ToString("0.0"));
                    a = a + 0.1f;
                }
            }
            return list;
        }
        private List<string> getAllValues_Word(Interval[] intervals) // returns a list of all the values within the intervals
        {
            ulong a = 0, b = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < intervals.Length; i++)
            {
                a = Convert.ToUInt16(intervals[i].interval_a);
                b = Convert.ToUInt16(intervals[i].interval_b);

                while (a <= b)
                {
                    list.Add(a.ToString());
                    if (a == 65535)
                    {
                        break;
                    }
                    a = a + 1;
                }
            }
            return list;
        }

        private void printListStr(List<string> list)
        {
            string elements = "";
            for (int i = 0; i < list.Count; i++)
            {
                elements = elements + list.ElementAt(i) + " ";
            }
            System.Windows.Forms.MessageBox.Show("All elements within ranges: " + elements);
        }

        private string[] getBaseValues(VariableBC[] varList)
        {
            int numInputs = varList.Length;
            string[] baseValues = new string[numInputs];
            for (int i = 0; i < numInputs; i++)
            {
                baseValues[i] = varList[i].baseChoice;
            }
            return baseValues;
        }

        public string[,] GetRandomTests(int numTests, Variable[] varList)
        {
            string[,] table = new string[numTests, varList.Length];
            for (int i = 0; i < numTests; i++) // loop for each test
            {
                for (int j = 0; j < varList.Length; j++)
                {
                    table[i, j] = GetRandom(varList[j]); // generate a random value for all the inputs in the list
                }
            }
            return table;
        }

        private string GetRandom(Variable variable)
        {
            int iIndex = getRandomIntervalIndex(variable.intervals.Length);
            Interval interval = variable.intervals[iIndex];
            switch (variable.datatype)
            {
                case "INT":
                case "BOOL":
                    return getRandom_int(interval);
                    //return getRandom_bool(interval);
                //case "LREAL":
                //    return getRandom_double(interval);
                case "REAL":
                    return getRandom_float(interval);
                case "TIME":
                case "UINT":
                case "USINT":
                    return getRandom_uint(interval);
                case "WORD":
                    return getRandom_word(interval);
                default:
                    return "-";
            }
        }

        private int getRandomIntervalIndex(int numIntervals)
        {
            return random.Next(0, numIntervals);
        }
        private string getRandom_int(Interval interval)
        {
            int min = Int32.Parse(interval.interval_a);
            int max = Int32.Parse(interval.interval_b);
            int r = random.Next(min, (max + 1));
            return r.ToString();
        }
        private string getRandom_bool(Interval interval)
        {
            int min = Int32.Parse(interval.interval_a);
            int max = Int32.Parse(interval.interval_b);
            int r = random.Next(min, (max + 1));
            return r.ToString();
        }
        private string getRandom_double(Interval interval)
        {
            double min = Double.Parse(interval.interval_a);
            double max = Double.Parse(interval.interval_b);
            double r = min + random.NextDouble() * (max - min);
            return r.ToString("0.0"); ;
        }
        private string getRandom_float(Interval interval)
        {
            float min = float.Parse(interval.interval_a);
            float max = float.Parse(interval.interval_b);
            float r = min + ((float)(random.NextDouble())) * (max - min);
            return r.ToString("0.0");
        }
        private string getRandom_word(Interval interval)
        {
            int min = UInt16.Parse(interval.interval_a);
            int max = UInt16.Parse(interval.interval_b);
            int r = random.Next(min, (max + 1));
            return r.ToString();
        }
        private string getRandom_uint(Interval interval)    //NEWER
        {
            uint a = UInt32.Parse(interval.interval_a);
            uint a_base = a;    // base value (original value) for interval_a. Needed for keeping the lower limit of range for random value.
            uint b = UInt32.Parse(interval.interval_b);

            int a_loops = 0;
            int b_loops = 0;
            int min = 0;
            int max = 0;
            int rand_min = 0;
            uint r = 0;

            // divide the large uint to a value that can be handled by random.Next method.
            // Keep track of loops for each interval value to set the random value within the range.
            while (a > UInt16.MaxValue)
            {
                a = a - UInt16.MaxValue;
                a_loops++;
            }
            while (b > UInt16.MaxValue)
            {
                b = b - UInt16.MaxValue;
                b_loops++;
            }
            if (a > b)
            {
                int new_a = (int)a - UInt16.MaxValue;
                min = new_a;
                max = (int)b;
                rand_min = 1;
            }
            else
            {
                min = (int)a;
                max = (int)b;
            }
            int loop_diff = b_loops - a_loops;
            int m_val = random.Next(rand_min, loop_diff + 1);

            int rand = random.Next(min, (max + 1));

            if (a_loops > 0)
            {
                //Need to add a base value and the UINT16.maxs' that was reduced from lower limit 
                r = (uint)(rand + a_base + UInt16.MaxValue * a_loops + UInt16.MaxValue * m_val);
            }
            else
            {
                r = (uint)(rand + UInt16.MaxValue * m_val);
            }

            return r.ToString();
        }

        private ParameterSet[] sortPS (ParameterSet[]ps, ref List<int> p_order)
        {
            List<int[]> order = new List<int[]>();
            List<int> T = new List<int>();

            for (int i = 0; i < ps.Length; i++)
            {
                order.Add(new int[] { ps[i].values.Count, i });
            }
            order = order.OrderByDescending(o => o.First()).ToList();
            p_order = order.Select(o => o.Last()).ToList();
            ps = ps.OrderByDescending(o => o.values.Count).ToArray();

            /*int swapIndex;
            ParameterSet t;
            for (int i = 0; i < ps.Length; i++)
            {
                p_order.Add(i);
            }
            for (int i = 0; i < ps.Length; i++)
            {
                swapIndex = i;
                
                for (int j = i+1; j < ps.Length; j++)
                {
                    if (ps[swapIndex].values.Count < ps[j].values.Count)
                    {
                        swapIndex = j;
                    }
                }
                if (swapIndex != i)
                {
                    t = ps[i];
                    ps[i] = ps[swapIndex];
                    ps[swapIndex] = t;
                    p_order[i] = swapIndex;
                    p_order[swapIndex] = i;
                }
               
            }*/

            return ps;
        }

        private ParameterSet[] sortVariables(Variable[] varList, ref List <int> p_order)
        {
            int iL = varList.Length;
            ParameterSet[] ps = new ParameterSet[iL];
            for (int i = 0; i < iL; i++)
            {
                ps[i].name = varList[i].name;
                ps[i].datatype = varList[i].datatype;
                ps[i].values = getAllFromIntervals(varList[i]); // all the values within the intervals, e.g. 2-5;7-9;17-19 -> (2,3,4,5,7,8,9,17,18,19)
            }

            /*
            SORT ps
            */
            ps = sortPS(ps,ref  p_order);

            return ps;
        }
        private TestSuite fillSuite(TestSuite ts, ParameterSet[] ps, int t, List<string> L, int k = 0)
        {
            if (k == t)
            {
                return ts;
            }
            for (int i = 0; i < ps[k].values.Count; i++)
            {
                L.Add(ps[k].values[i]);
                fillSuite(ts, ps, t, L, k + 1);
                if (k == t-1)
                {
                    ts.t_case.Add(new List<string>(L));
                }
                L.RemoveAt(k);
            }

            return ts;
        }

        private TestSuite InstantiateTestSuite(int t, ParameterSet[] ps)
        {
            TestSuite ts = new TestSuite();
            ts.t_case = new List<List<string>>();
            List<string> L = new List<string>();
            ts = fillSuite(ts, ps, t, L);
            return ts;
        }

        private TestSuite BAD_FillPie (ParameterSet A, ParameterSet B, TestSuite ts, int pos,int goal)
        {
            List<string> L = new List<string>();

            for (int i = 0; i < pos; i++)
            {
                L.Add("*");
            }
            for (int i = 0; i < A.values.Count; i++)
            {
                L.Add(A.values[i]);
                for (int j = pos+1; j < goal; j++)
                {
                    L.Add("*");
                }
                for (int j = 0; j < B.values.Count; j++)
                {
                    L.Add(B.values[j]);
                    ts.t_case.Add(new List<string>(L));
                    L.RemoveAt(goal);
                }
                for (int j = goal; j > pos; j--)
                {
                    L.RemoveAt(j);
                }
            }
            return ts;
        }

        private TestSuite FillPie(ParameterSet A, ParameterSet B, TestSuite ts, int pos, int goal)
        {
            List<string> L = new List<string>();
            for (int i = 0; i < goal+1; i++)
            {
                L.Add("*");
            }
            for (int i = 0; i < A.values.Count; i++)
            {
                L[pos] = A.values[i];
                for (int j = 0; j < B.values.Count; j++)
                {
                    L[goal] = B.values[j];
                    ts.t_case.Add(new List<string>(L));
                }
               
            }
            return ts;
        }
        private TestSuite MakePie(ParameterSet[] ps, int k)
        {
            TestSuite ts = new TestSuite();
            ts.t_case = new List<List<string>>();
            for (int i = 0; i < k; i++)
            {
                ts = FillPie(ps[i], ps[k], ts,i,k);
            }

            return ts;
        }
        private bool contains_p(List<string> t_case, List<string> t_prim,int t_way)
        {
            int strength = 0;
            if (t_case[t_case.Count - 1] == t_prim[t_prim.Count - 1])
            {
                strength++;
                for (int i = 0; i < t_case.Count-1; i++)
                {
                    if (t_prim[i] == t_case[i])
                    {
                        strength++;
                    }
                    if (strength == t_way)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private List<int> ContainsInPI(TestSuite Pi,List<string> T_prim,int t_way)
        {
            List<int> T_indexes = new List<int>();

            for (int i = 0; i < Pi.t_case.Count; i++)
            {
                if (contains_p(Pi.t_case[i],T_prim,t_way))
                {
                    T_indexes.Add(i);
                }
            }
            return T_indexes;
        }

        private List <int> CheckForDontCare(List<string> t_case)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < t_case.Count; i++)
            {
                if (t_case[i] == "*")
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }

        private int GetParameterValue(List<List<int>> T_prim)
        {
            int index = 0;
            for (int i = 1; i < T_prim.Count; i++)
            {
                if (T_prim[i].Count > T_prim[index].Count)   //NEEDS FIXING FOR TIE'S (NOT THOSE YOU WEAR)
                {
                    index = i;
                }
            }
            return index;
        }
        private TestSuite PiPrim(TestSuite PI, List<int> T)
        {
            for (int i = T.Count-1; i >=0; i--)
            {
                PI.t_case.RemoveAt(T[i]);
            }
            return PI;
        }
        private bool checkIfChangeable(List<string>A,List<string>B)
        {
            for (int i = 0; i < A.Count; i++)
            {
                if (A[i] != B[i] && A[i] != "*" && B[i] != "*")
                {
                    return false;
                }
            }
            return true;
            //if (A[A.Count-1] == B[B.Count - 1])
            //{
            //    return true;
            //}
            //return false;
        }
        private List<int> getChangeableIndex(TestSuite ts)
        {
            List<int> X = new List<int>();
            for (int i = 0; i < ts.t_case.Count; i++)
            {
                if (CheckForDontCare(ts.t_case[i]).Count > 0)
                {
                    X.Add(i);
                }
            }
            return X;
        }
        private List<string> MutateCase(List<string> A, List<string>B )
        {
            for (int i = 0; i < A.Count; i++)
            {
                if (A[i] == "*")
                {
                    A[i] = B[i];
                }
            }
            return A;
        }
        private int checkWhichChange(TestSuite ts, List<int> canChange, List<string> Pie)
        {
            for (int k = 0; k < canChange.Count; k++)
            {
                if (checkIfChangeable(ts.t_case[canChange[k]], Pie))
                {
                    return k;
                }
            }
            return -1;
        }
        private List<string> CheckAndChange(TestSuite ts, List<int> canChange, List<string> Pie)
        {
            for (int k = 0; k < canChange.Count; k++)
            {
                if (checkIfChangeable(ts.t_case[canChange[k]], Pie))
                {
                    return MutateCase(ts.t_case[canChange[k]], Pie);
                }
            }
            return null;
        }
        private int GetLeastAmntTests(ParameterSet[]ps, int t_way)
        {
            int tt = ps[0].values.Count;
            for (int i = 1; i < t_way; i++)
            {
                tt *= ps[i].values.Count;
            }
            return tt;
        }
        public string[,] GetPairwiseTests(Variable[] varList,bool randomiseDCs, int t_way, object sender)
        {
            int nInpt = varList.Length; // number of inputs
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress((1));
            string[,] testCases;
            List<int> p_order = new List<int>();
            ParameterSet[] ps = sortVariables(varList,ref p_order);
            TestSuite ts = InstantiateTestSuite(t_way, ps);
            //int LeastAmntTests = GetLeastAmntTests(ps,t_way);
            int prc = 100 / nInpt;

            worker.ReportProgress(((t_way-1) * prc));

            //for (int i = 0; i < numInput; i++) //Assigns the correct column name
            //{
            //    varList[i].datatype = ps[i].datatype;
            //    //varList[i].intervals = ps[i].values;
            //    varList[i].name = ps[i].name;
            //}

            int offset = 0;
            for (int i = t_way; i < nInpt; i++)
            {
                TestSuite PI = MakePie(ps, i); //needs fixing for t-way 
                offset++;
                for (int j = 0; j < ts.t_case.Count; j++) // HORIZONTAL EXTENSION SUPAAAH DUPAAAH!
                {
                    List<List<int>> T_prim = new List<List<int>>();
                    List<string> T = ts.t_case[j];

                    for (int k = 0; k < ps[i].values.Count; k++)
                    {
                        T.Add(ps[i].values[k]);
                        T_prim.Add(ContainsInPI(PI, T, t_way));
                        T.RemoveAt(T.Count - 1);
                    }
                    int par_value = GetParameterValue(T_prim);
                    ts.t_case[j].Add(ps[i].values[par_value]);
                    PI = PiPrim(PI, T_prim[par_value]);
                }
                
                for (int j = PI.t_case.Count-1; j >=0 ; j--)  // VERTICAL B-E-A-UTIFUL EXTENSION!
                {
                    List<int> canChange = new List <int>(getChangeableIndex(ts));
                    int R = checkWhichChange(ts, canChange, PI.t_case[j]);
                    if (R > -1)
                    {
                        ts.t_case[canChange[R]] = new List<string>(MutateCase(ts.t_case[canChange[R]], PI.t_case[j]));
                    }
                    else
                    {
                        ts.t_case.Add(new List<string>(PI.t_case[j]));
                        PI.t_case.RemoveAt(j);
                    }
                }
                worker.ReportProgress((i * prc));
            }
            testCases = new string[ts.t_case.Count+1, nInpt+1]; // size of array which will contain the test cases
            
            if (testCases.GetUpperBound(1) != nInpt)
            {
                MessageBox.Show("Parameter Amount is wrong " + testCases.GetUpperBound(1));
                throw new System.ArgumentException("array bound not the same as parameter amount", "numInput");
            }
            if (randomiseDCs)
            {
                Random rand = new Random();
                for (int i = 0; i < ts.t_case.Count; i++)
                {
                    for (int j = 0; j < ts.t_case[i].Count; j++)
                    {
                        if (ts.t_case[i][j] == "*")
                        {
                            ts.t_case[i][j] = ps[j].values[rand.Next(0, ps[j].values.Count)];
                        }
                    }
                }
            }

            for (int i = 0; i < ts.t_case.Count; i++)
            {
                for (int j = 0; j < ts.t_case[i].Count; j++)
                {
                    testCases[i, p_order[j]] = ts.t_case[i][j];
                }
            }
            worker.ReportProgress((100));
            return testCases;
        }
    }
}
