using System;
using MLBLL;
using System.IO;
using System.Text;
using Read;
using System.Collections.Generic;


namespace C_
{
    class Program
    {
        static void Main(string[] args)
        {
            // 读取文件
            // string path="/Users/apple/Desktop/data/dim_1_80m_data2.txt";
            string path="/Users/apple/Desktop/data/kw1.txt";

            Myread r = new Myread(path);

            DateTime now=DateTime.Now; 
            
            // 如何生成
            Dictionary<int, List<ATY_TagModel_V200>> lastLocateDatas =new Dictionary<int, List<ATY_TagModel_V200>>();

            Protocol_Data_ATY_V200 info=new Protocol_Data_ATY_V200(0,r.byData,1,now,ref lastLocateDatas,0,1,1,1);    
            // test
            // public static double[] toa(int anchor_num, double[,] anchor_position, double[] distance_vector)
            int anchorNum=2;
            double[,] anchorPosition=new double[,]{{0,0},{0,80}};


            // 读数据
            // 保存距离信息，供matlab调用
            string savePath="/Users/apple/Desktop/data/dim_1_80m_diatance_data.txt";
            StreamReader sr = new StreamReader(savePath);
            List<ATY_DataItem_V200> finaDisItems = new List<ATY_DataItem_V200>();
            String line;
            var distanceList=new List<double>();
            while ((line = sr.ReadLine()) != null) 
            {
                distanceList.Add(Convert.ToDouble(line)/100);
                Console.WriteLine(line);            
            }

            for(var i =0;i<distanceList.Count-1;i+=2)
            {
                double[] distance=new double[]{66.44,14.82};
                distance[0]=distanceList[i];
                distance[1]=distanceList[i+1];
                double[] pos=Protocol_Data_ATY_V200.toa(anchorNum,anchorPosition,distance);
                Console.WriteLine("x：{0}\ty：{1}\tline:{2}",pos[0],pos[1],i);
            }
            Console.WriteLine("Hello World!");
        }
    }
}
