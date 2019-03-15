using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLBLL
{
    public class ATY_DataItem_V200
    {
        /// <summary>
        /// 0x2010:测距,0x2012:信号质量,0x2014:在线基站
        /// </summary>
        public int Type = -1;
        public int SourceID = -1;
        public int No = -1;
        public int NetCode = -1;
        public int TagID = -1;
        public int AnchorID = -1;
        public int Distance = -1;
        public int RXL = -1;
        public int FPL = -1;
        public List<int> OnlineAnchors = new List<int>();

        public bool IsValid = false;

        public ATY_DataItem_V200(byte[] data)
        {
            if (data[0] == 0xf5 && data[1] == 0x5a && data.Length >= 16)
            {
                int length = data[3] * 256 + data[2];

                // 2019-03-06
                // IsValid = CheckCRC(data);
                IsValid = true;

                if (IsValid && length == data.Length - 4)
                {
                    Type = data[5] * 256 + data[4];
                    int len = data[7] * 256 + data[6];
                    SourceID = data[9];// *256 + data[8];
                    No = data[11] * 256 + data[10];
                    NetCode = data[13] * 256 + data[12];
                    switch (Type)
                    {
                        case 0x2010:
                            TagID = (data[15] * 256 + data[14]) % 32768;
                            AnchorID = data[16];
                            Distance = data[19] * 256 + data[18];
                            break;
                        case 0x2012:
                            TagID = (data[15] * 256 + data[14]) % 32768;
                            AnchorID = data[16];
                            RXL = data[18];
                            FPL = data[19];
                            break;
                        case 0x2014:
                            if (len > 6)
                            {
                                for (int i = 14; i + 1 < data.Length - 2; i = i + 2)
                                {
                                    OnlineAnchors.Add(data[i]);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // 张衡注释 2019-03-06
        // private bool CheckCRC(byte[] data)
        // {
        //     bool res = false;
        //     byte[] crc = Common.CalcCrc16.CRC(data, data.Length - 2);
        //     if (crc.Length >= 2 && crc[0] == data[data.Length - 2] && crc[1] == data[data.Length - 1])
        //         res = true;
        //     return res;
        // }
    }
}
