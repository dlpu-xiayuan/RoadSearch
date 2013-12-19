using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string[][] road_map;
        Dictionary<string, ArrayList> cross_sta = new Dictionary<string, ArrayList>();//定义交叉站点映射
        ArrayList list = new ArrayList();//存储站点列表
        int[,] path = new int[588, 588];
        int[,] ad = new int[588, 588];
        Dictionary<string, int> road_num_com = new Dictionary<string, int>();//线路名与序号对照
        string[] all_station = new string[588];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader road = new StreamReader(@"source.txt",Encoding.Default);
            int road_num = 117;//应该由程序读出
            int count = 0;
            road_map = new string[road_num][];
            char[] delimit = new char[] {' '};//作为分割数据的分隔符


            while (!road.EndOfStream)
            {
                string temp_road = road.ReadLine();//线路信息
                road_map[count] = temp_road.Split(delimit);
                listBox1.Items.Add(road_map[count][0]);
                count++;
            }
            listBox1.SelectedIndex = 0;
            textBox1.Text = listBox1.SelectedItem.ToString();


            for (int i=0; i < Int32.Parse(road_map[0][1]); i++)//操作起点站
            {
                listBox2.Items.Add(road_map[0][i+2]);
            }
            listBox2.SelectedIndex = 0;
            textBox3.Text = listBox2.SelectedItem.ToString();

            ArrayList list = new ArrayList();//定义一个泛型用来装数组的元素
            
            for (int i=0; i < 117; i++ )
            {
                for (int j = 0; j < Int32.Parse(road_map[i][1]);j++ )//foreach循环出 数组的元素
                {
                    if (list.Contains(road_map[i][j + 2]) == false && !string.IsNullOrEmpty(road_map[i][j]))//list.Contains(str)判断list中是否有相同的元素，list.Contains(str) == false当不同时为true
                    {
                        list.Add(road_map[i][j + 2]);//把数组中的不相同元素添加到list中
                    }

                }
            }
            
            
            //统计所有站点
            for (int i = 0; i < list.Count; i++)
            {
                all_station[i] = list[i].ToString();
            }
            Array.Sort(all_station);
            for (int i = 0; i < all_station.Length; i++)
            {
                 listBox3.Items.Add(all_station[i]);
            }

            
            for(int i = 0;i<all_station.Length;i++)
            {
                road_num_com.Add(all_station[i],i);
            }

            listBox3.SelectedIndex = 0;
            textBox2.Text = listBox3.SelectedItem.ToString();



            for (int i = 0; i < all_station.Length; i++)//站点线路表
            {
                ArrayList cross_list = new ArrayList();
                for (int j = 0; j < 117; j++)
                {
                    if (road_map[j].Contains(all_station[i]))
                    {
                        cross_list.Add(j.ToString());
                    }
                }
                cross_sta.Add(all_station[i].ToString(), cross_list);
            }


            //floyd算可达矩阵
            for (int i = 0; i < 588; i++)
            {
                for (int j = 0; j < 588; j++)
                {
                    ad[i,j] = 1000; 
                }
            }
            string one, other;
            for (int i = 0; i < 117; i++)
                    {
                        for (int j = 2; j < road_map[i].Length-1; j++)
                        {
                            for (int k = j; k < road_map[i].Length-1; k++)
                            {
                                one = road_map[i][j];
                                other = road_map[i][k];
                                if (ad[road_num_com[one],road_num_com[other]] > (k - j))
                                {
                                    ad[road_num_com[one], road_num_com[other]] = (k - j);//System.Collections.Generic.KeyNotFoundException
                                    ad[road_num_com[other],road_num_com[one]] = (k - j);
                                    path[road_num_com[one], road_num_com[other]] = road_num_com[one];
                                    path[road_num_com[other],road_num_com[one]] = road_num_com[other];
                                }
                            }
                        }
                    }
            for(int i =0;i<all_station.Length;i++)
                for(int j=0;j<all_station.Length;j++)
                    for (int k = j; k < all_station.Length; k++)
                    {
                        if (ad[i,k] + ad[k,j] < ad[i,j])
                        {
                            ad[i,j] = ad[i,k] + ad[j,k];
                            ad[j,i] = ad[i,k] + ad[j,k];
                            path[i,j] = path[k,j];
                            path[j,i] = path[k,i];
                        }
                    }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "乘车方案如下：\n";
            string start,final;
            string chang_one, chang_two;
            start = textBox3.Text.ToString();
            final = textBox2.Text.ToString();
            ArrayList res_list = new ArrayList();
            ArrayList res_list1 = new ArrayList();
            ArrayList res_list2 = new ArrayList();
            int c = 1;

            if (!(all_station.Contains(start) && all_station.Contains(final)))
            {

                MessageBox.Show("暂不支持模糊查找，请从站点列表内选择站点！","错误",new MessageBoxButtons());
                textBox3.Text = "";
                textBox2.Text = "";
                return;

            }




            if (ad[road_num_com[start], road_num_com[final]] == 1000)
            {
                richTextBox1.Text = "两站点不可达\n";
                return;
            }


            richTextBox1.Text = "乘车方案如下：\n";
            //判断是否直达，是则结束
            if (path[road_num_com[start], road_num_com[final]] == road_num_com[start])
            {
                richTextBox1.Text = "乘车方案如下：\n";
                res_list = common_road(start, final);
                richTextBox1.Text += "共找到 " + res_list.Count + " 条直达线路：\n";
                for (int i = 0; i < res_list.Count; i++)
                {
                    richTextBox1.Text += "方案"+(i+1)+"：\n坐"+road_map[Int32.Parse(res_list[i].ToString())][0]+"从 "+start+" 站上车，在 "+final+" 站下车\n";
                }
                
            }
            else
            {
                richTextBox1.Text = "乘车方案如下：\n";
                chang_one=all_station[path[road_num_com[start], road_num_com[final]]];
                if (path[road_num_com[start], road_num_com[chang_one]] == road_num_com[start])
                {
                    richTextBox1.Text += "可一次换乘";

                    res_list1 = common_road(chang_one, final);
                    res_list2 = common_road(start, chang_one);

                    for (int i = 0; i < res_list2.Count; i++)
                    {

                        for (int j = 0; j < res_list1.Count; j++)
                        {
                            richTextBox1.Text += "方案" + c + "：\n坐" + road_map[Int32.Parse(res_list2[i].ToString())][0] + "从 " + start + " 站上车，在 " + chang_one + " 站下车\n";
                            richTextBox1.Text += "再坐" + road_map[Int32.Parse(res_list1[j].ToString())][0] + "从 " + chang_one + " 站上车，在 " + final + " 站下车\n";
                            c++;
                        }

                    }

                }

                else
                {
                    richTextBox1.Text = "乘车方案如下：\n";
                    chang_one = all_station[path[road_num_com[start], road_num_com[final]]];
                    chang_two = all_station[path[road_num_com[start], road_num_com[chang_one]]];

                    if (chang_two == start)
                    {

                        res_list1 = common_road(chang_two, final);
                        res_list2 = common_road(chang_one, chang_two);
                        res_list1 = common_road(start, chang_one);

                        for (int i = 0; i < res_list2.Count; i++)
                        {

                            for (int j = 0; j < res_list1.Count; j++)
                            {

                                for (int k = 0; k < res_list.Count; k++)
                                {

                                    richTextBox1.Text += "方案" + c + "：\n坐" + road_map[Int32.Parse(res_list2[i].ToString())][0] + "从 " + start + " 站上车，在 " + chang_one + " 站下车\n";
                                    richTextBox1.Text += "再坐" + road_map[Int32.Parse(res_list1[j].ToString())][0] + "从 " + chang_one + " 站上车，在 " + chang_two + " 站下车\n";
                                    richTextBox1.Text += "再坐" + road_map[Int32.Parse(res_list[k].ToString())][0] + "从 " + chang_two + " 站上车，在 " + final + " 站下车\n";
                                    c++;
                                }
                            }

                        }
                    }

                    else
                    {

                        richTextBox1.Text = "需要换乘的次数大于2次，建议考虑其他出行方式！\n";

                    }

                }

            }

        }

        private ArrayList common_road(string one, string another)
        {
            ArrayList res = new ArrayList();


            for (int i = 0; i < cross_sta[one].Count; i++)
            {
                for (int j = 0; j < cross_sta[another].Count; j++)
                {
                    if (Int32.Parse(cross_sta[one][i].ToString()) == Int32.Parse(cross_sta[another][j].ToString()))
                    {
                        res.Add(cross_sta[one][i]);
                    }
                }
            }


            return res;
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            textBox1.Text = listBox1.SelectedItem.ToString();//初始起点线路
            listBox2.Items.Clear();
            for (int i = 0; i < Int32.Parse(road_map[listBox1.SelectedIndex][1]); i++)//操作起点站
            {
                listBox2.Items.Add(road_map[(int)listBox1.SelectedIndex][i + 2]);
            }

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Text = listBox2.SelectedItem.ToString();//初始起点站
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = listBox3.SelectedItem.ToString();
        }

        private void 关于AToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void 关闭CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 关于AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void 说明EToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
