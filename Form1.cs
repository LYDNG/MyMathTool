using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyMathTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitializeOpchTblStack();
            daultScale_X = int.Parse(scale_X.Text);
            daultScale_Y = int.Parse(scale_Y.Text);

            this.menuItem1.Visible = false;
            this.menuItem2.Visible = false;
            this.menuItem3.Visible = false;
            this.menuItem4.Visible = false;
            this.menuItem5.Visible = false;
            this.menuItem10.Visible = false;
        }

        #region 全局变量
        public Graphics graphicsObject;
        //定义存放运算符(包括:'+','-',...,'sin',...,'arcsin',...,'(',...等)及其特性的数据结构
        public struct opTable   //定义存放运算符及其优先级和单双目的结构
        {
            public string op;   //用于存放运算符 op为oprater的简写 
            public int code;    //用存放运算符的优先级
            public char grade;  //用于判断存放的运算符是单目还是双目
        }
        /// <summary>
        /// 用于存放制定好的运算符及其特性(优先级和单双目)的运算符表,其初始化在方法Initialize()中
        /// </summary>
        public opTable[] opchTbl = new opTable[19];
        /// <summary>
        /// 用于存放从键盘扫描的运算符的栈
        /// </summary>
        public opTable[] operateStack = new opTable[30];

        //定义优先级列表 1,2,3,4,5,6,7,8,9,
        public int[] osp = new int[19] { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 5, 3, 3, 2, 2, 7, 0, 1 };  //数组中元素依次为: "sin","cos","tan","cot","arcsin","arccos","arctan","sec","csc","ln","^","*","/","+","-","(",")",""   的栈外(因为有的运算符是从右向左计算,有的是从左往右计算,用内外优先级可以限制其执行顺序)优先级
        public int[] isp = new int[18] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3, 3, 2, 2, 1, 1 };      //数组中元素依次为: "sin","cos","tan","cot","arcsin","arccos","arctan","sec","csc","ln","^","*","/","+","-","(" ,"end" 的栈内(因为有的运算符是从右向左计算,有的是从左往右计算,用内外优先级可以限制其执行顺序)优先级

        //定义存放从键盘扫描的数据的栈
        public double[] dataStack = new double[30] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        //定义表态指针
        public int opTop = -1;  //指向存放(从键盘扫描的)运算符栈的指针
        public int dataTop = -1;//指向存放(从键盘扫描的)数据栈指针

        //定义存放从键盘输入的起始字符串
        public string startString; //存放优化后的表达式字符串
        public int startTop = 0;    // 
        public double start = 0;    //图象显示区间的开始变量        
                                    //定义颜色数组
        int color = 1;
        //定义中间字符串 (如果两次输入一样的表达式,则只显示第一个表达式图像)
        string[] tempString = new string[50];  //存放正在显示图像表达式
        public int tempTop = -1;

        private int sign = 0;  //由于DrawFrontPicture()过程在两个不同的地方调用，用于控制里面的某些语句是否执行
        private bool drawPoint = true;    //如果输入表达式定义域有一个点有意义，则值为真，否则为假

        private int daultScale_X;     //保存初始的比例值，以便恢复时用
        private int daultScale_Y;

        private int view_X = 1, view_Y = 1;   //置视野默认值为1

        public static Form formKeyBoard;   //<鼠标帮助输入窗口> 类的变量声明
        public static bool keyboardWindowCreated = false;  //标志是否显示此窗口		

        public static Form formPictureHelp;  //<帮助窗口> 类的变量声明
        public static bool pictureHelpCreated = false;  //标志是否显示此窗口

        public static Form computerForm;          //<多功能计算器> 类的变量声明
        public static bool computerFormCreated = false; //标志此窗口是否显示

        public int startTopMoveCount = 0;     //在扫描输入的表达式时，用于记录 运算符/三角函数/常数/数据字数据的长度

        #endregion
        /// <summary>
        /// 制定运算符及其特性(优先级和单双目)的运算符表
        /// </summary>
        public void InitializeOpchTblStack()
        {
            //public int[]osp={6,6,6,6,6,6,6,6,6,6,6,5,3,3,2,2,7,0,1};  //数组中元素依次为: "sin","cos","tan","cot","arcsin","arccos","arctan","sec","csc","ln","^","*","/","+","-","(",")",""  的栈外(因为有的运算符是从右向左计算,有的是从左往右计算,用内外优先级可以限制其执行顺序)优先级
            //public int[]isp={5,5,5,5,5,5,5,5,5,5,5,4,3,3,2,2,1,1};      //数组中元素依次为: "sin","cos","tan","cot","arcsin","arccos","arctan","sec","csc","ln","^","*","/","+","-","(","end" 的栈内(因为有的运算符是从右向左计算,有的是从左往右计算,用内外优先级可以限制其执行顺序)优先级
            opchTbl[0].op = "sin"; opchTbl[0].code = 1; opchTbl[0].grade = 's';
            opchTbl[1].op = "cos"; opchTbl[1].code = 2; opchTbl[1].grade = 's';
            opchTbl[2].op = "tan"; opchTbl[2].code = 3; opchTbl[2].grade = 's';
            opchTbl[3].op = "cot"; opchTbl[3].code = 4; opchTbl[3].grade = 's';
            opchTbl[4].op = "arcsin"; opchTbl[4].code = 5; opchTbl[4].grade = 's';
            opchTbl[5].op = "arccos"; opchTbl[5].code = 6; opchTbl[5].grade = 's';
            opchTbl[6].op = "arctan"; opchTbl[6].code = 7; opchTbl[6].grade = 's';
            opchTbl[7].op = "arccot"; opchTbl[7].code = 8; opchTbl[7].grade = 's';
            opchTbl[8].op = "sec"; opchTbl[8].code = 9; opchTbl[8].grade = 's';
            opchTbl[9].op = "csc"; opchTbl[9].code = 10; opchTbl[9].grade = 's';
            opchTbl[10].op = "ln"; opchTbl[10].code = 11; opchTbl[10].grade = 's';
            opchTbl[11].op = "^"; opchTbl[11].code = 12; opchTbl[11].grade = 'd';
            opchTbl[12].op = "*"; opchTbl[12].code = 13; opchTbl[12].grade = 'd';
            opchTbl[13].op = "/"; opchTbl[13].code = 14; opchTbl[13].grade = 'd';
            opchTbl[14].op = "+"; opchTbl[14].code = 15; opchTbl[14].grade = 'd';
            opchTbl[15].op = "-"; opchTbl[15].code = 16; opchTbl[15].grade = 'd';
            opchTbl[16].op = "("; opchTbl[16].code = 17; opchTbl[16].grade = 'd';
            opchTbl[17].op = ")"; opchTbl[17].code = 18; opchTbl[17].grade = 'd';
            opchTbl[18].op = " "; opchTbl[18].code = 19; opchTbl[18].grade = 'd';
        }

        /// <summary>
        /// 检查括号是否匹配
        /// </summary>
        /// <returns></returns>
        public bool CheckParentthese() 
        {
            int number = 0;
            for (int i = 0; i < startString.Length - 1; i++)
            {
                if (i == '(') number++;
                if (i == ')') number--;
                if (number < 0) return false;//右括号不能在前面
            }
            if (number != 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 此函数对输入表达式进行各种规范化，如消去空格/单目转换为双目/统一转化为小写字母
        /// </summary>
        public void CreterionFaction()
        {
            startString = expressBox.Text;
            //以下为消去待扫描字符串中的所有空格字符
            for (int i = 0; i < startString.Length; i++)
                if (startString[i].Equals(' '))
                {
                    startString = startString.Remove(i, 1);
                    i--;
                }
            //以下代码使待扫描字符串的单目('+'和'-')变为双目
            if (startString.Length != 0)
                if (startString[0] == '+' || startString[0] == '-')
                {
                    startString = startString.Insert(0, "0");
                }
            for (int i = 0; i < startString.Length - 1; i++)
            {
                if ((startString[i] == '(') && (startString[i + 1] == '-'))
                    startString = startString.Insert(i + 1, "0");
            }
            startString = startString.Insert(startString.Length, ")");
            //将待扫描字符串字号字母转化为小写字母
            startString = startString.ToLower();
        }

        /// <summary>
        /// 恢复所有图像，即重画所有图像
        /// </summary>
        private void RestoreAllPictures()
        {					
            if (tempTop > -1)
            {
                graphicsObject.Clear(Color.Black);
                DrawBackPicture();
                color = 1;
                sign = 1;           //控制drawFrontPicture()方法里的某些语句是否执行		
                for (int i = 0; i <= tempTop; i++)
                {
                    if (tempString[i] == "")
                        continue;
                    startString = tempString[i]; // 依次从tempString[]数组中读取已画的图像字符串
                    DrawFrontPicture();         //画出刚读取的字符串		
                }
                sign = 0;
            }
        }

        /// <summary>
        /// 给运算表达式分块(三角函数...算术运算符等),再根据其返回值来检验其属于哪类错误
        /// </summary>
        /// <returns></returns>
        public int CheckFollowCorrect()
        {
            string str, oldString = "", newString = "";
            int dataCount = 0, characterCount = 0;
            if (startString.Equals(")"))
                return 0;         //输入字符串为空返回值
            if ((startString[0] == '*') || (startString[0] == '/') || (startString[0] == '^') || (startString[0] == ')'))
                return 11;        //首字符输入错误返回值
            for (int i = 0; i < startString.Length; i++)
            {
                if ((oldString.Equals("三角函数")) && (newString.Equals("右括号")))
                    return 2;     //三角函数直接接右括号错误返回值
                if ((oldString.Equals("左括号")) && (newString.Equals("算术运算符")))
                    return 3;     //左括号直接接算术运算符错误返回值
                if ((oldString.Equals("数字序列")) && (newString.Equals("三角函数")))
                    return 4;     //数字序列后直接接三角函数错误返回值
                if ((oldString.Equals("数字序列")) && (newString.Equals("左括号")))
                    return 5;     //数字序列后直接接左括号错误返回值
                if ((oldString.Equals("算术运算符")) && (newString.Equals("右括号")))
                    return 6;     //算术运算符后直接接右括号错误返回值
                if ((oldString.Equals("右括号")) && (newString.Equals("左括号")))
                    return 7;     //右括号直接接左括号错误返回值
                if ((oldString.Equals("右括号")) && (newString.Equals("三角函数")))
                    return 8;     //右括号直接接三角函数错误返回值
                if ((oldString.Equals("数字序列")) && (newString.Equals("数字序列")))
                    return 9;     //数字序列后直接接'pi'/'e'或'pi'/'e'直接接数字序列错误返回值
                if ((oldString.Equals("算术运算符")) && (newString.Equals("算术运算符")))
                    return 10;     //算术运算符后直接接算术运算符错误返回值
                oldString = newString;
                if (i < startString.Length - 5 && startString.Length >= 6)
                {
                    str = startString.Substring(i, 6);
                    if ((str.CompareTo("arcsin") == 0) || (str.CompareTo("arccos") == 0) || (str.CompareTo("arctan") == 0) || (str.CompareTo("arccot") == 0))
                    {
                        newString = "三角函数";
                        i += 5; characterCount++;
                        continue;
                    }
                }
                if (i < startString.Length - 2 && startString.Length >= 3)
                {
                    str = startString.Substring(i, 3);
                    if ((str.CompareTo("sin") == 0) || (str.CompareTo("cos") == 0) || (str.CompareTo("tan") == 0) || (str.CompareTo("cot") == 0) || (str.CompareTo("sec") == 0) || (str.CompareTo("csc") == 0))
                    {
                        newString = "三角函数";
                        i += 2; characterCount++;
                        continue;
                    }
                }
                if (i < (startString.Length - 1) && (startString.Length) >= 2)
                {
                    str = startString.Substring(i, 2);
                    if (str.CompareTo("ln") == 0)
                    {
                        newString = "三角函数";
                        i += 1; characterCount++;
                        continue;
                    }
                    if (str.CompareTo("pi") == 0)
                    {
                        newString = "数字序列";
                        i += 1; dataCount++;
                        continue;
                    }
                }
                str = startString.Substring(i, 1);
                if (str.Equals("^") || str.Equals("*") || str.Equals("/") || str.Equals("+") || str.Equals("-"))
                {
                    newString = "算术运算符";
                    characterCount++;
                    continue;
                }
                if (str.Equals("e"))
                {
                    newString = "数字序列";
                    dataCount++;
                    continue;
                }
                if (str.Equals("x"))
                {
                    newString = "数字序列";
                    dataCount++;
                    continue;
                }
                if (str.Equals("("))
                {
                    newString = "左括号";
                    characterCount++;
                    continue;
                }
                if (str.Equals(")"))
                {
                    newString = "右括号";
                    characterCount++;
                    continue;
                }
                if (Char.IsDigit(startString[i]))
                {
                    while (Char.IsDigit(startString[i]))
                    {
                        i++;
                    }
                    if (startString[i] == '.' && (!Char.IsDigit(startString[i + 1])) && (i + 1) != startString.Length)
                        return 13;
                    if (startString[i] == '.')
                    {
                        i++;
                    }
                    while (Char.IsDigit(startString[i]))
                    {
                        i++;
                    }
                    newString = "数字序列";
                    i--; dataCount++;
                    continue;
                }
                return 1;         //非法字符
            }
            if ((dataCount == 0 && characterCount != 0) || (startString[0] == '0' && dataCount == 1 && characterCount > 1 && startString.Length != 2))
                return 12;
            return 100;
        }
        public int IsCharacterOrData(ref double num)
        {
            string str = "";
            startTop += startTopMoveCount; startTopMoveCount = 0;
            int i = startTop;
            if (i < startString.Length - 5 && startString.Length >= 6)
            {
                str = startString.Substring(i, 6);
                for (int j = 4; j <= 7; j++)
                    if (str.Equals(opchTbl[j].op))
                    {
                        startTopMoveCount = 6;
                        return opchTbl[j].code;
                    }
            }
            if (i < startString.Length - 2 && startString.Length >= 3)
            {
                str = startString.Substring(i, 3);
                for (int j = 0; j < 10; j++)
                    if (str.CompareTo(opchTbl[j].op) == 0)
                    {
                        startTopMoveCount = 3;
                        return opchTbl[j].code;
                    }
            }
            if (i < (startString.Length - 1) && (startString.Length) >= 2)
            {
                str = startString.Substring(i, 2);
                if (str.CompareTo("ln") == 0)
                {
                    startTopMoveCount = 2;
                    return 11;
                }
                if (str.CompareTo("pi") == 0)
                {
                    startTopMoveCount = 2;
                    num = Math.PI;
                    return 100;
                }
            }
            //以下开始确认一个字符是属于什么值类型
            if (i < startString.Length)
            {
                str = startString.Substring(i, 1);
                for (int j = 11; j < 19; j++)
                {
                    if (str.Equals(opchTbl[j].op))
                    { startTopMoveCount = 1; return opchTbl[j].code; }
                }
                if (str.CompareTo("e") == 0)
                {
                    startTopMoveCount = 1; num = Math.E;
                    return 100;
                }
                if (str.CompareTo("x") == 0)
                {
                    startTopMoveCount = 1; num = start;
                    return 100;
                }
                if (Char.IsDigit(startString[i]))
                {
                    double temp = 0, M = 10; int j = i;
                    while (Char.IsDigit(startString[j]))
                    {
                        temp = M * temp + Char.GetNumericValue(startString[j]);
                        startTop++;
                        j++;
                    }
                    if (startString[j] == '.')
                    {
                        j++; startTop++;
                    }
                    while (Char.IsDigit(startString[j]))
                    {
                        temp += 1.0 / M * Char.GetNumericValue(startString[j]);
                        M /= 10; j++;
                        startTop++;
                    }
                    startTopMoveCount = 0;
                    num = temp;
                    return 100;
                }
            }
            return -1;
        }
        public double DoubleCount(string opString, double data1, double data2)
        {   //双目运算 
            if (opString.CompareTo("+") == 0) return (data1 + data2);
            if (opString.CompareTo("-") == 0) return (data1 - data2);
            if (opString.CompareTo("*") == 0) return (data1 * data2);
            if (opString.CompareTo("/") == 0)
            {
                if (data2 == 0)
                {
                    drawPoint = false;
                    return 0;
                }
                return (data1 / data2);
            }
            if (opString.CompareTo("^") == 0)
            {
                double end = data1;
                for (int i = 0; i < data2 - 1; i++)
                    end *= data1;
                return (end);
            }
            return Double.MaxValue;
        }
        public double DoubleCount(string opString, double data1)
        {   //单目运算			
            if (opString.CompareTo("sin") == 0) return Math.Sin(data1);
            if (opString.CompareTo("cos") == 0) return Math.Cos(data1);
            if (opString.CompareTo("tan") == 0) return Math.Tan(data1);

            if (opString.CompareTo("cot") == 0)
            {
                //if(Math.Tan(data1)==0)
                if (Math.Tan(data1) > -0.001 && Math.Tan(data1) < 0.001)
                {
                    drawPoint = false;
                    return 0;
                }
                return (1 / (Math.Tan(data1)));
            }

            if (opString.CompareTo("arcsin") == 0)
            {
                if (data1 < -1 || data1 > 1)
                {
                    drawPoint = false;
                    return 0;
                }
                return Math.Asin(data1);
            }
            if (opString.CompareTo("arccos") == 0)
            {
                if (data1 < -1 || data1 > 1)
                {
                    drawPoint = false;
                    return 0;
                }
                return Math.Acos(data1);
            }
            if (opString.CompareTo("arctan") == 0)
                /*if(-Math.PI/2<=data1&&data1<=Math.PI/2)*/
                return Math.Atan(data1);
            if (opString.CompareTo("arccot") == 0)
                /*if(-Math.PI/2<=data1&&data1<=Math.PI/2)*/
                return (-Math.Atan(data1));
            if (opString.CompareTo("sec") == 0)
            {
                double temp = Math.Cos(data1);
                ///if(temp==0) 
                if (temp > -0.0001 && temp < 0.0001)
                {
                    drawPoint = false;  //置为无效点，即不把这个点对应坐标画出
                    return 0;    //可以为任意的实数，因为这个值所对应的点已置为无效，即不再画值为零的点
                }
                if (temp != 0)
                    return (1 / (Math.Cos(data1)));
            }
            if (opString.CompareTo("csc") == 0)
            {
                double temp = Math.Sin(data1);
                //if(temp==0) 
                if (temp > -0.0001 && temp < 0.0001)
                {
                    drawPoint = false;//置为无效点，即不把这个点对应坐标画出
                    return 0;    //可以为任意的实数，因为这个值所对应的点已置为无效，即不再画值为零的点
                }
                if (temp != 0)
                    return (1 / (Math.Sin(data1)));
            }

            //if(data1>0) 
            if (opString.CompareTo("ln") == 0)
            {
                if (data1 <= 0)
                {
                    drawPoint = false;
                    return 0;
                }
                return Math.Log(data1);
            }
            return Double.MaxValue;
        }
        /// <summary>
        /// 方法功能为求得一个自变量X的一个解
        /// </summary>
        /// <param name="tempY"></param>
        /// <returns></returns>
        public int CountValueY(ref double tempY)
        {
            int type = -1;                  //存放正在扫描的字符串是为数字类型还是(单双目运算符)
            double num = 0;                   //如果是数据,则返回数据的值
                                              //进栈底结束符"end"
            opTop++;
            operateStack[opTop].op = "end"; operateStack[opTop].code = 18; operateStack[opTop].grade = ' ';
            while (startTop <= startString.Length - 1)
            {
            start:
                type = IsCharacterOrData(ref num);  //调用判断返回值类型函数 
                if (type == -1) { return 1; }
                if (type == 100)
                {
                    dataTop = dataTop + 1;
                    //Console.WriteLine(dataTop);
                    dataStack[dataTop] = num;
                }
                else
                {
                    if (osp[type - 1] > isp[operateStack[opTop].code - 1])   //操作符进栈
                    {
                        opTop++;
                        operateStack[opTop].op = opchTbl[type - 1].op; operateStack[opTop].code = opchTbl[type - 1].code; operateStack[opTop].grade = opchTbl[type - 1].grade;
                    }
                    else
                    {
                        while (osp[type - 1] <= isp[operateStack[opTop].code - 1])  //弹出操作符跟数据计算,并存入数据
                        {
                            if (operateStack[opTop].op.CompareTo("end") == 0)//当遇到"end"结束符表示已经获得结果
                            {
                                if (dataTop == 0)
                                {
                                    tempY = dataStack[dataTop]; startTop = 0; startTopMoveCount = 0; opTop = -1; dataTop = -1;
                                    return 0;
                                }
                                else return 1;       //顺序错乱造成的错误
                            }
                            if (operateStack[opTop].op.CompareTo("(") == 0)  //如果要弹出操作数为'( ',则消去左括号
                            {
                                opTop--; goto start;
                            }
                            //弹出操作码和一个或两个数据计算,并将计算结果存入数据栈
                            double data1, data2; opTable operate;
                            if (dataTop >= 0) data2 = dataStack[dataTop];
                            else return 1;
                            operate.op = operateStack[opTop].op; operate.code = operateStack[opTop].code; operate.grade = operateStack[opTop].grade;
                            opTop--;                        //处理一次,指针必须仅且只能下移一个单位
                            if (operate.grade == 'd')
                            {
                                if (dataTop - 1 >= 0) data1 = dataStack[dataTop - 1];
                                else return 1;
                                //if(operate.op=="/"&&data2==0)													
                                //return 3;             //如果继续，将会发生出零错误								
                                double tempValue = DoubleCount(operate.op, data1, data2);
                                if (tempValue != Double.MaxValue) dataStack[--dataTop] = tempValue;
                                else return 1;
                            }
                            if (operate.grade == 's')
                            {
                                double tempValue = DoubleCount(operate.op, data2);
                                if (tempValue != Double.MaxValue)
                                {
                                    dataStack[dataTop] = tempValue;
                                }
                                else return 2;
                            }
                        }
                        //如果当前栈外操作符比栈顶的操作符优先级别高,则栈外操作符进栈
                        opTop++;
                        operateStack[opTop].op = opchTbl[type - 1].op; operateStack[opTop].code = opchTbl[type - 1].code; operateStack[opTop].grade = opchTbl[type - 1].grade;
                    }
                }
            }
            return 1;
        }
        /// <summary>
        /// 这个方法的功能是调用各个检错函数
        /// </summary>
        /// <returns></returns>
        public bool StartExcute()
        {
            startString = expressBox.Text;
            //InitializeOpchTblStack();
            CreterionFaction();
            if (CheckParentthese() == false)
            {
                MessageBox.Show("括号不匹配,请重新输入!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //this.pictureBox.Refresh();
                RestoreAllPictures();   //重画所有图像,相当于刷新屏幕
                                        //DrawBackPicture();
                return false;
            }
            switch (CheckFollowCorrect())
            {
                case 0: MessageBox.Show("表达式为空,请先输入表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 1: MessageBox.Show("表达式中有非法字符!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 2: MessageBox.Show("三角函数运算符与 ) 之间应输入数据或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 3: MessageBox.Show("' (  ' 与算术运算符之间应输入数据或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 4: MessageBox.Show("数字数列与三角函数之间应输入算术运算符或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 5: MessageBox.Show("数字数列或变量与  ' (  '  之间应输入算术运算符或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 6: MessageBox.Show("算术运算符与右括号之间应输入数据或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 7: MessageBox.Show("'  )  ' 与 '  (  ' 之间应输入算术运算符或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 8: MessageBox.Show("'   )   ' 与三角函数之间应输入算术运算符或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 9: MessageBox.Show("常量 '  PI  '  或  '  E  '  或  '  X  '  与数字数据之间应输入算术运算符或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 10: MessageBox.Show("算术运算符与算术运算符之间应输入数据或其它表达式!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 11: MessageBox.Show("表达式头部不能为'  +   ', '  -  ' , '  *  ' , '  /  ' , '  ^  ' ,'  )  '!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 12: MessageBox.Show("仅有运算符号没有数字数据或数据缺少而无法计算,请输入数字数据!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
                case 13: MessageBox.Show("小数点后面缺少小数部分,请输入小数部分!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); RestoreAllPictures(); return false;
            }
            double tempY = 0;
            switch (CountValueY(ref tempY))  //通过调用一次CountValueY()函数，进行检错
            {
                case 1: MessageBox.Show("输入的表达式不正确!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); startTop = 0; startTopMoveCount = 0; opTop = -1; dataTop = -1; RestoreAllPictures(); return false;
                case 2: MessageBox.Show("三角函数定义域越界或运算过程中发生除零错误，请重新输入正确区间!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); startTop = 0; startTopMoveCount = 0; opTop = -1; dataTop = -1; RestoreAllPictures(); return false;
                    // case 3:MessageBox.Show("运算过程中发生除零错误，请检查表达式!!!","错误",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);startTop=0; startTopMoveCount=0; opTop=-1; dataTop=-1; RestoreAllPictures();return false;
            }
            return true;
        }
        public void DrawBackPicture()
        {
            //画坐标
            graphicsObject = pictureBox.CreateGraphics();
            Pen pen1 = new Pen(Color.White, 2);
            pen1.EndCap = LineCap.ArrowAnchor;
            graphicsObject.DrawLine(pen1, 0 + 10, pictureBox.Size.Height / 2, pictureBox.Size.Width - 10, pictureBox.Size.Height / 2);
            pen1.Dispose();
            Pen pen2 = new Pen(Color.White, 2);
            pen2.StartCap = LineCap.ArrowAnchor;
            graphicsObject.DrawLine(pen2, pictureBox.Size.Width / 2, 0 + 5, pictureBox.Size.Width / 2, pictureBox.Size.Height - 5);
            pen2.Dispose();
            //画尺度
            float footLength_X = (float)Double.Parse(scale_X.Text) / view_X;
            float footLength_Y = (float)Double.Parse(scale_Y.Text) / view_Y;
            float rulerLength = 3;
            //X轴左边尺度标记
            Pen pen3 = new Pen(Color.White, 2);
            pen3.StartCap = LineCap.NoAnchor;
            float x1 = pictureBox.Width / 2, y1 = pictureBox.Height / 2, x2 = pictureBox.Width / 2, y2 = pictureBox.Height / 2 - rulerLength;
            //X轴右边尺度标记
            Pen pen4 = new Pen(Color.White, 2);
            pen4.StartCap = LineCap.NoAnchor;
            float x3 = pictureBox.Width / 2, y3 = pictureBox.Height / 2, x4 = pictureBox.Width / 2, y4 = pictureBox.Height / 2 - rulerLength;
            //Y轴上方尺度标记
            Pen pen5 = new Pen(Color.White, 2);
            pen5.StartCap = LineCap.NoAnchor;
            float x5 = pictureBox.Width / 2, y5 = pictureBox.Height / 2, x6 = pictureBox.Width / 2 + rulerLength, y6 = pictureBox.Height / 2;
            //X轴下方尺度标记
            Pen pen6 = new Pen(Color.White, 2);
            pen6.StartCap = LineCap.NoAnchor;
            float x7 = pictureBox.Width / 2, y7 = pictureBox.Height / 2, x8 = pictureBox.Width / 2 + rulerLength, y8 = pictureBox.Height / 2;
            for (double i = 0; i < pictureBox.Width / 2 || i < pictureBox.Height / 2;)
            {
                graphicsObject.DrawLine(pen3, x1, y1, x2, y2); x1 = x1 - footLength_X; x2 = x2 - footLength_Y; //画一个X轴左标记
                graphicsObject.DrawLine(pen4, x3, y3, x4, y4); x3 = x3 + footLength_X; x4 = x4 + footLength_Y; //画一个X轴右标记
                graphicsObject.DrawLine(pen5, x5, y5, x6, y6); y5 = y5 - footLength_X; y6 = y6 - footLength_Y; //画一个Y轴上方标记
                graphicsObject.DrawLine(pen6, x7, y7, x8, y8); y7 = y7 + footLength_X; y8 = y8 + footLength_Y; //画一个Y轴下方标记
                double x = double.Parse(scale_X.Text) / view_X - double.Parse(scale_Y.Text) / view_Y;
                if (double.Parse(scale_X.Text) / view_X < 3 && double.Parse(scale_Y.Text) / view_Y < 3) //当条件刚好成立时，标尺的间隔等于计算机的相临物理坐标的间隔，即为 1 对 1 的关系，如果视野继续扩大，则计算机的一个物理坐标就对应几百个甚至几千万个标尺的宽度之和，如果继续画，就会严重降低计算机的运行速度；
                {                                                                            //如果继续画也无意义了，因为用我们的肉眼已经看不清了
                    i += 3;                 //标尺的间隔为3能够看清，如果再小的话就很难看清了
                    footLength_X = 3;
                    footLength_Y = 3;
                    continue;
                }
                if (x > 0)
                    i += (double.Parse(scale_X.Text) / view_X);
                else
                    i += (double.Parse(scale_Y.Text) / view_Y);
            }
            pen3.Dispose(); pen4.Dispose(); pen5.Dispose(); pen6.Dispose();
        }
        public void DrawFrontPicture()
        {
            Graphics g;
            g = pictureBox.CreateGraphics();
            //g=e.Graphics;
            int x, y, precsion;
            double end, xscale, yscale;
            x = pictureBox.Size.Width;
            y = pictureBox.Size.Height;                           //The centrer Pointer.	
            start = Double.Parse(area_X.Text);
            end = Double.Parse(area_Y.Text);                       //The line start and end			
            xscale = Double.Parse(scale_X.Text);
            yscale = Double.Parse(scale_Y.Text);                   //The line scale of x,y;
            precsion = Int32.Parse(difinitionExpress.Text);
            //z=(precsion-4)%3;
            //if(z!=0) precsion-=z;     //Precess the line Precsion;					 
            Pen pen = new Pen(Color.Black, 2);   //
            switch (color)
            {
                case 0: pen.Color = Color.Black; pen.Width = 2; break;
                case 1: pen.Color = Color.Green; pen.Width = 2; break;
                case 2: pen.Color = Color.Blue; pen.Width = 2; break;
                case 3: pen.Color = Color.Yellow; pen.Width = 2; break;
                case 4: pen.Color = Color.Red; pen.Width = 2; break;
                case 5: pen.Color = Color.Pink; pen.Width = 2; break;
            }
            PointF[] a;
            a = new PointF[precsion];
            double k;
            k = (end - start) / precsion;
            double tempY = 0;
            bool havePoints = false;

            //int myOk=0;
            a.Initialize();

            for (int j = 0; j < precsion; start += k)
            {
                drawPoint = true;  //恢复默认值,只有当判断表达式定义域不在定义域内时才改为假
                CountValueY(ref tempY);
                if (drawPoint == true)    //只有当drawPoint为真值时才将表达式的值对应的点画出
                {
                    a[j].X = (float)(start * xscale / view_X) + x / 2;
                    a[j].Y = -(float)(tempY * yscale / view_Y) + y / 2;
                    j++;
                    havePoints = true;
                }
                else precsion--;
            }
            for (int i = 0; i < precsion; i++)
            {
                if (a[i].X > pictureBox.Left && a[i].X < pictureBox.Right && a[i].Y > pictureBox.Top && a[i].Y < pictureBox.Bottom)
                    g.DrawLine(pen, a[i].X, a[i].Y, a[i].X + 3, a[i].Y);
            }
            g.Dispose();
            if (havePoints == true) color++;

            int n = 0;
            n = expressBox.Items.IndexOf(expressBox.Text);
            if (n == -1 && expressBox.Text != "" && havePoints == true)
                this.expressBox.Items.Add(expressBox.Text);

            if (color > 5)
                this.btnDisplay.Enabled = false;
            if (sign == 0 && havePoints == true)
                tempString[++tempTop] = startString;
        }

        /// <summary>
        /// 显示一条图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDisplay_Click(object sender, EventArgs e)
        {
            if (double.Parse(area_X.Text) - double.Parse(area_Y.Text) > 0)
            {
                MessageBox.Show("定义域无效，请仔细检验!!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (!StartExcute()) return;     //对输入表达式检验并使其规范化		
            DrawBackPicture();              //画x和y轴  
            for (int i = 0; i <= tempTop; i++)
                if (String.Compare(tempString[i], startString) == 0)
                {
                    MessageBox.Show("图像已经画出,如果看不到图像，请连续按 <放大视野> 按钮.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    RestoreAllPictures(); //重画所有图像,相当于刷新屏幕
                    return;
                }
            DrawFrontPicture();         //画当前表达式的图像
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //显示坐标
            double x1 = pictureBox.Width / 2, y1 = pictureBox.Height / 2;
            double x2 = ((e.X - x1) / double.Parse(scale_X.Text) * view_X);
            double y2 = -((e.Y - y1) / double.Parse(scale_Y.Text) * view_Y);
            string str_X2 = x2.ToString();
            string str_Y2 = y2.ToString();
            if (str_X2.Length > 5)
                str_X2 = str_X2.Remove(5, str_X2.Length - 5);
            if (str_Y2.Length > 5)
                str_Y2 = str_Y2.Remove(5, str_Y2.Length - 5);
            this.label3.Text = str_X2 + "\n" + str_Y2;
            //判断是否显示菜单栏
            if (e.X > pictureBox.Left && e.X < pictureBox.Left + 120 && e.Y > pictureBox.Top && e.Y < pictureBox.Top + 10)
            {
                this.menuItem1.Visible = true;
                this.menuItem2.Visible = true;
                this.menuItem3.Visible = true;
                this.menuItem4.Visible = true;
                this.menuItem5.Visible = true;
                this.menuItem10.Visible = true;
            }
            else if (this.menuItem1.Visible == true)
            {
                this.menuItem1.Visible = false;
                this.menuItem2.Visible = false;
                this.menuItem3.Visible = false;
                this.menuItem4.Visible = false;
                this.menuItem5.Visible = false;
                this.menuItem10.Visible = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RestoreAllPictures();
        }

        private void btnRewrite_Click(object sender, EventArgs e)
        {
            expressBox.Text = "";
        }

        private void functionsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            expressBox.Text = functionsBox.SelectedItem.ToString();
        }

        private void expressBox_TextChanged(object sender, EventArgs e)
        {
            functionsBox.Text = "请点击选择...";
        }

        /// <summary>
        /// 扩大视野
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewBig_Click(object sender, EventArgs e)
        {
            if (color == 1) return;
            if (int.Parse(scale_X.Text) >= 0 && int.Parse(scale_Y.Text) >= 0)
            {
                view_X += 30;
                view_Y += 30;
                RestoreAllPictures(); //重画所有图像,相当于刷新屏幕	
            }
        }
        /// <summary>
        /// 缩小视野
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewSmall_Click(object sender, EventArgs e)
        {
            if (color == 1) return;
            if (int.Parse(scale_X.Text) >= 0 && int.Parse(scale_Y.Text) >= 0)
            {
                if (view_X - 30 >= 1 && view_Y - 30 >= 1)
                {
                    view_X -= 30;
                    view_Y -= 30;
                    RestoreAllPictures();//重画所有图像,相当于刷新屏幕	
                }
            }
        }
        /// <summary>
        /// 恢复视野
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewRestore_Click(object sender, EventArgs e)
        {
            if (color == 1) return;
            view_X = 1;
            view_Y = 1;
            RestoreAllPictures();
        }

        private void btnPictureSmall_Click(object sender, EventArgs e)
        {
            //功能：缩小图像比例，即缩小图像
            if (color == 1) return;
            int x, y;
            x = int.Parse(scale_X.Text);
            y = int.Parse(scale_Y.Text);
            if (x <= 3 || y <= 3) return;
            if (x - 15 >= 1)
                x -= 15;
            else x -= 2;
            if (y - 15 >= 1)
                y -= 15;
            else y -= 2;
            scale_X.Text = x.ToString();
            scale_Y.Text = x.ToString();
            RestoreAllPictures();
        }

        private void btnClearone_Click(object sender, EventArgs e)
        {
            //功能：清除当前图像			
            if (color <= 1)      //判断是否已画了当前图像，如果没画，则返回
            {
                MessageBox.Show("屏幕上没有图像!!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                RestoreAllPictures();   //重画所有图像,相当于刷新屏幕
                return;   //屏幕没有图像，不能删除
            }
            if (!StartExcute()) return;
            int i;
            for (i = 0; i <= tempTop; i++)
            {
                if (String.Compare(tempString[i], startString) == 0)
                    break;
            }
            if (i > tempTop)
            {
                MessageBox.Show("您还没有画此表达式的图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                RestoreAllPictures();   //重画所有图像,相当于刷新屏幕
                return;
            }
            tempString[i] = "";//tempString[]中存有当前已经显示的表达式的字符串，删除要删除的表达式

            ClearFrontPicture();
            RestoreAllPictures();  //重画所有图像,相当于刷新屏幕
            this.btnDisplay.Enabled = true;
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            //功能：清除所有图像			
            //判断是否已画了当前图像，如果没画，则返回
            if (color <= 1)
            {
                MessageBox.Show("屏幕上没有图像!!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                RestoreAllPictures();  //重画所有图像,相当于刷新屏幕
                return;   //屏幕没有图像，不能删除
            }
            if (tempTop == -1) return;
            graphicsObject.Clear(Color.Black);
            DrawBackPicture();
            startTop = 0; startTopMoveCount = 0; opTop = -1; dataTop = -1;  //恢复初值
            color = 1;
            for (int i = 0; i <= tempTop; i++)
                tempString[i] = "";
            tempTop = -1;
            this.btnDisplay.Enabled = true;
        }

        private void btnPictureRestore_Click(object sender, EventArgs e)
        {
            scale_X.Text = daultScale_X.ToString();
            scale_Y.Text = daultScale_Y.ToString();
            RestoreAllPictures();   //重画所有图像,相当于刷新屏幕
        }

        private void btnPictureBig_Click(object sender, EventArgs e)
        {
            if (color == 1) return;  //还没有画图像，执行放大图像这项操作无意义
            int x, y;
            x = int.Parse(scale_X.Text);
            y = int.Parse(scale_Y.Text);
            if (x >= 800 && y >= 800) return;
            if (x < 15)
                x += 2;
            else x += 15;
            if (y < 15)
                y += 2;
            else y += 15;
            scale_X.Text = x.ToString();
            scale_Y.Text = x.ToString();
            RestoreAllPictures();   //重画所有图像,相当于刷新屏幕
        }

        private void btnMouseScanf_Click(object sender, EventArgs e)
        {
            //if (keyboardWindowCreated == false)
            //{
            //    formKeyBoard = new FormKeyBoard();
            //    formKeyBoard.Show();
            //    keyboardWindowCreated = true;
            //}
            //formKeyBoard.Visible = true;
            //formKeyBoard.WindowState = FormWindowState.Normal;
            //expressBox.Text = "";
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            //if (pictureHelpCreated == false)
            //{
            //    formPictureHelp = new pictureHelp();
            //    formPictureHelp.Show();
            //    pictureHelpCreated = true;
            //}
            //formPictureHelp.Show();
            //formPictureHelp.Activate();
            //formPictureHelp.Visible = true;
            //formPictureHelp.WindowState = FormWindowState.Maximized;
        }

        public void ClearFrontPicture()
        {
            Graphics g;
            g = pictureBox.CreateGraphics();
            int x, y, precsion;
            double end, xscale, yscale;
            x = pictureBox.Size.Width;
            y = pictureBox.Size.Height;                           //The centrer Pointer.	
            start = Double.Parse(area_X.Text);
            end = Double.Parse(area_Y.Text);                       //The line start and end			
            xscale = Double.Parse(scale_X.Text);
            yscale = Double.Parse(scale_Y.Text);                   //The line scale of x,y;
            precsion = Int32.Parse(difinitionExpress.Text);
            //z=(precsion-4)%3;
            //if(z!=0) precsion-=z;     //Precess the line Precsion;					 
            Pen pen = new Pen(Color.Black, 2);
            pen.Color = Color.Black; pen.Width = 2;
            Point[] a;
            a = new Point[precsion];
            double k;
            k = (end - start) / precsion;
            double tempY = 0;
            for (int j = 0; j < precsion; start += k)
            {

                CountValueY(ref tempY);
                a[j].X = (int)(start * (xscale)) + x / 2;
                a[j].Y = -(int)(tempY * (yscale)) + y / 2;
                j++;
            }
            for (int i = 0; i < precsion; i++)
            {
                if (a[i].X > pictureBox.Left && a[i].X < pictureBox.Right && a[i].Y > pictureBox.Top && a[i].Y < pictureBox.Bottom)
                    g.DrawLine(pen, a[i].X, a[i].Y, a[i].X + 5, a[i].Y);
            }
            g.Dispose();
            color--;
            DrawBackPicture();  //当图像的轨迹与坐标轴重合时，将会把坐标轴也消除掉
        }
    }
}
