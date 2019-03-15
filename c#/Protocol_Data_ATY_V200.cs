using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLModel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MLBLL
{
    public class Protocol_Data_ATY_V200
    {
        public Dictionary<int, ATY_TagModel_V200> Tags = new Dictionary<int, ATY_TagModel_V200>();

        public List<string> OnlineAnchors = new List<string>();

        /// <summary>
        /// TOA解算算法
        /// </summary>
        /// <param name="anchor_num">基站数量</param>
        /// <param name="anchor_position">基站坐标阵列</param>
        /// <param name="distance_vector">标签到各个基站的距离矢量</param>
        /// <returns>最终点坐标X,Y</returns>
        public static double[] toa(int anchor_num, double[,] anchor_position, double[] distance_vector)
        {
            int row = anchor_position.GetLength(0);
            int col = anchor_position.GetLength(1);
            Matrix<double> model = DenseMatrix.OfArray(anchor_position);
            var A = model;
            Vector<double> distance_model = DenseVector.OfArray(distance_vector);
            double[] oneVector = Enumerable.Repeat<double>(1, anchor_num).ToArray();
            Vector<double> colInser = DenseVector.OfArray(oneVector);
            A = -2 * A;
            A = A.InsertColumn(2, colInser); //插入一列
            var temp5 = (Matrix.op_DotMultiply(model, model) * -1);
            var temp6 = Vector.op_DotMultiply(distance_model, distance_model);
            var temp7 = temp6 + temp5.Column(0) + temp5.Column(1);
            var temp_left = A.Transpose() * (A);
            var temp_right = A.Transpose() * temp7;
            var theta = temp_left.QR().Solve(temp_right);

            if (theta[2]-theta[1]*theta[1]<0)
            {
                Console.WriteLine(" data error!!!");
                theta[0]=0;
                theta[1]=0;
            }
            else
            {
                theta[0]=Math.Abs(Math.Sqrt(theta[2]-theta[1]*theta[1]));
            }
            var estimate_tag_position = new double[] { theta[0], theta[1] };
            return estimate_tag_position;
        }

        /// <summary>
        /// 协议解析
        /// </summary>
        /// <param name="count">未用</param>
        /// <param name="data">原始数据</param>
        /// <param name="checkAccSpeed">加速度判断方式</param>
        /// <param name="time">收到数据的时间</param>
        /// <param name="lastLocateDatas">之前收到的数据，计算加速度用</param>
        /// <param name="lower">加速度阈值下限，计算加速度用</param>
        /// <param name="upper">加速度阈值上限，计算加速度用</param>
        /// <param name="maxDistance">固定时间段内移动的距离最大值，计算加速度用</param>
        /// <param name="difDistance">固定时间段内移动的距离差最大值，计算加速度用</param>
        public Protocol_Data_ATY_V200(int count, byte[] data, int checkAccSpeed, DateTime time, ref Dictionary<int, List<ATY_TagModel_V200>> lastLocateDatas, int lower, int upper, int maxDistance, int difDistance)//, int version, bool useFlag
        {
            //counter = count;
            //DTime = time;
            if (data.Length < 16)//至少有一条数据
            {
                return;
            }
            List<ATY_DataItem_V200> items = new List<ATY_DataItem_V200>();
            List<byte> list = data.ToList();
            int index = list.IndexOf(0xf5);//F5 5a开头
            while (index >= 0 && list.Count > index + 6 && list[index + 1] == 0x5a)
            {
                if (index > 0)
                    list.RemoveRange(0, index);
                int len = list[3] * 265 + list[2] + 4; //长度
                if (list.Count >= len)
                {
                    byte[] bytes = list.GetRange(0, len).ToArray();//解析一条
                    ATY_DataItem_V200 item = new ATY_DataItem_V200(bytes);
                    if (item.IsValid)
                        items.Add(item);
                    list.RemoveRange(0, len);
                }
                else
                    break;
                index = list.IndexOf(0xf5);
            }

            // 数据拆分以10 20开头，14 20结尾作为一个测量结果
            // List<List<double>> distanceList2D = new List<List<double>>();//纪录有效测试距离
            // List<List<double>> tagIdList = new List<List<double>>();//纪录有效测试距离 tag
            // List<List<double>> anchorIdList = new List<List<double>>();//纪录有效测试距离 anchor
            var disTmp = new List<double>();
            var tagTmp = new List<double>();
            var ancTmp = new List<double>();
            var srcTmp = new List<double>();
            var rxlTmp = new List<double>();

            var saveIndex=0;
            for (var i=0;i<items.Count-1;i++)
            {
                if (items[i].Type==0x2010)
                {
                    disTmp.Add(items[i].Distance);
                    tagTmp.Add(items[i].TagID);
                    ancTmp.Add(items[i].AnchorID);
                    srcTmp.Add(items[i].SourceID);
                    rxlTmp.Add(items[i].RXL);
                }
                else
                {
                    if (items[i].Type==0x2014)
                    {
                        string basePath="/Users/apple/Desktop/data/Dim2/";
                        if (disTmp.Count==0)
                        {
                            continue;
                        }

                        var disFileName=basePath+"dis"+saveIndex.ToString()+".txt";
                        var tagFileName=basePath+"tag"+saveIndex.ToString()+".txt";
                        var ancFileName=basePath+"anc"+saveIndex.ToString()+".txt";
                        var srcFileName=basePath+"src"+saveIndex.ToString()+".txt";
                        var rxlFileName=basePath+"rxl"+saveIndex.ToString()+".txt";

                        
                        FileStream fs1 = new FileStream(disFileName, FileMode.Create);
                        StreamWriter sw1 = new StreamWriter(fs1);
                        for (var j=0;j<disTmp.Count;j++)
                        {
                            sw1.Write(disTmp[j]);
                            sw1.Write('\n');  
                        }
                        FileStream fs2 = new FileStream(tagFileName, FileMode.Create);
                        StreamWriter sw2 = new StreamWriter(fs2);
                        for (var j=0;j<disTmp.Count;j++)
                        {
                            sw2.Write(tagTmp[j]);
                            sw2.Write('\n'); 
                        }
                        FileStream fs3 = new FileStream(ancFileName, FileMode.Create);
                        StreamWriter sw3 = new StreamWriter(fs3);
                        for (var j=0;j<disTmp.Count;j++)
                        {
                            sw3.Write(ancTmp[j]);
                            sw3.Write('\n'); 
                        }

                        FileStream fs4 = new FileStream(srcFileName, FileMode.Create);
                        StreamWriter sw4 = new StreamWriter(fs4);
                        for (var j=0;j<disTmp.Count;j++)
                        {
                            sw4.Write(srcTmp[j]);
                            sw4.Write('\n'); 
                        }

                        FileStream fs5 = new FileStream(rxlFileName, FileMode.Create);
                        StreamWriter sw5 = new StreamWriter(fs5);
                        for (var j=0;j<disTmp.Count;j++)
                        {
                            sw5.Write(rxlTmp[j]);
                            sw5.Write('\n'); 
                        }


                        sw1.Flush();
                        sw1.Close();
                        fs1.Close();
                        sw2.Flush();
                        sw2.Close();
                        fs2.Close();
                        sw3.Flush();
                        sw3.Close();
                        fs3.Close();
                        sw4.Flush();
                        sw4.Close();
                        fs4.Close();
                        sw5.Flush();
                        sw5.Close();
                        fs5.Close();
                        saveIndex++;
                        disTmp.Clear();
                        tagTmp.Clear();
                        ancTmp.Clear();
                        srcTmp.Clear();
                        rxlTmp.Clear();
                    }
                }
            }

            
            // 保存距离信息，供matlab调用
            string savePath="/Users/apple/Desktop/data/dim_1_80m_diatance_data.txt";
            FileStream fs = new FileStream(savePath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            List<ATY_DataItem_V200> finaDisItems = new List<ATY_DataItem_V200>();
            foreach(var item in items)
            {
                if(item.Type == 0x2010 && (item.AnchorID==0 || item.AnchorID==8))
                {
                    finaDisItems.Add(item);
                    sw.Write(item.Distance);
                    sw.Write('\n');
                }
            }
            //清空缓冲区
            sw.Flush();
            sw.Close();
            fs.Close();





            //以上解析完一组
            foreach (var item in items)
            {
                if (item.Type == 0x2014)
                {
                    item.OnlineAnchors.ForEach(o => OnlineAnchors.Add(item.SourceID.ToString() + "_" + o.ToString()));
                }
                else if (item.Type == 0x2012)
                {
                    if (!Tags.ContainsKey(item.TagID))
                    {
                        Tags[item.TagID] = new ATY_TagModel_V200();
                        Tags[item.TagID].DTime = DateTime.Now;
                        Tags[item.TagID].TagID = item.TagID;
                        Tags[item.TagID].NetID = item.SourceID.ToString();
                        Tags[item.TagID].IsValid = item.IsValid;
                    }
                    string anchorid = item.SourceID.ToString() + "_" + item.AnchorID.ToString();
                    Tags[item.TagID].SQDic[anchorid] = new SignalQualityV200();
                    Tags[item.TagID].SQDic[anchorid].FPL = item.FPL;
                    Tags[item.TagID].SQDic[anchorid].RXL = item.RXL;
                }
                else if (item.Type == 0x2010)
                {
                    if (item.Distance > 0)
                    {
                        if (!Tags.ContainsKey(item.TagID))
                        {
                            Tags[item.TagID] = new ATY_TagModel_V200();
                            Tags[item.TagID].DTime = DateTime.Now;
                            Tags[item.TagID].TagID = item.TagID;
                            Tags[item.TagID].NetID = item.SourceID.ToString();
                            Tags[item.TagID].IsValid = item.IsValid;
                        }
                        string aid = item.SourceID.ToString() + "_" + item.AnchorID.ToString();
                        
                        if (checkAccSpeed > 0)
                        {
                            if (!lastLocateDatas.ContainsKey(item.TagID))
                                lastLocateDatas[item.TagID] = new List<ATY_TagModel_V200>();
                            int c = lastLocateDatas[item.TagID].Count;
                            if (c >= 2)
                            {
                                LocationInfo li1 = lastLocateDatas[item.TagID][c - 2].OrgLocationDic.ContainsKey(aid) ? lastLocateDatas[item.TagID][c - 2].OrgLocationDic[aid] : null;
                                LocationInfo li2 = lastLocateDatas[item.TagID][c - 1].OrgLocationDic.ContainsKey(aid) ? lastLocateDatas[item.TagID][c - 1].OrgLocationDic[aid] : null;
                                if (li1 != null && li2 != null)
                                {
                                    if (checkAccSpeed == 1)
                                    {
                                        double accspeed = GetAccSpeedFunc(li1.PositionDistance, lastLocateDatas[item.TagID][c - 2].DTime, li2.PositionDistance, lastLocateDatas[item.TagID][c - 1].DTime, item.Distance * 10, time);
                                        //Console.WriteLine(string.Format("【{0}】{1}:{2}【{3}:{4} {5}:{6} {7}:{8}】", TagID, aid, accspeed, li1.PositionDistance, lastLocateDatas[TagID][c - 2].DTime, li2.PositionDistance, lastLocateDatas[TagID][c - 1].DTime, range * 10, time));
                                        if (accspeed - lower >= 0.001 && upper - accspeed >= 0.001)
                                        {
                                            Tags[item.TagID].LocationDic[aid] = new LocationInfo(aid, item.Distance * 10);
                                        }
                                    }
                                    else if (checkAccSpeed == 2)
                                    {
                                        int dis = GetDistance(li2.PositionDistance, item.Distance * 10);
                                        int difD = GetAccSpeedFunc2(li1.PositionDistance, li2.PositionDistance, item.Distance * 10);
                                        if (dis <= maxDistance * 10 && difD <= difDistance * 10)
                                        {
                                            Tags[item.TagID].LocationDic[aid] = new LocationInfo(aid, item.Distance * 10);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Tags[item.TagID].LocationDic[aid] = new LocationInfo(aid, item.Distance * 10);
                        }
                        Tags[item.TagID].OrgLocationDic[aid] = new LocationInfo(aid, item.Distance * 10);
                    }
                }
            }
        }

        private int GetDistance(int dis1, int dis2)
        {
            int res = Math.Abs(dis2 - dis1);
            //Console.WriteLine("speed:" + res);
            return res;
        }

        //private bool CheckCRC(byte[] data)
        //{
        //    bool res = false;
        //    byte[] crc = Common.CalcCrc16.CRC(data, data.Length - 2);
        //    if (crc.Length >= 2 && crc[0] == data[data.Length - 2] && crc[1] == data[data.Length - 1])
        //        res = true;
        //    return res;
        //}

        /// <summary>
        /// 等时间差
        /// </summary>
        /// <returns></returns>
        private int GetAccSpeedFunc2(int dis1, int dis2, int dis3)
        {
            int res = Math.Abs(Math.Abs(dis3 - dis2) - Math.Abs(dis2 - dis1));
            //Console.WriteLine("Acc:" + res);
            return res;
        }

        private double GetAccSpeedFunc(int dis1, DateTime time1, int dis2, DateTime time2, int dis3, DateTime time3)
        {
            double r = -99;
            try
            {
                double s1 = Math.Abs(dis2 - dis1);
                double t1 = time2.Subtract(time1).TotalMilliseconds;
                double v1 = Math.Round(s1 / t1, 2);

                double s2 = Math.Abs(dis3 - dis2);  
                double t2 = time3.Subtract(time2).TotalMilliseconds;
                double v2 = Math.Round(s2 / t2, 2);

                r = Math.Round(Math.Abs(v2 - v1) / ((t1 + t2) / 2000), 2);
            }
            catch
            {

            }
            return r;
        }
    }
}
