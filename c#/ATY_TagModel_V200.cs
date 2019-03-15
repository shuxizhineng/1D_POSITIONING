using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLModel;

namespace MLBLL
{
    public class ATY_TagModel_V200
    {
        public int TagID = -1;

        public string NetID = "";

        public DateTime DTime = DateTime.MinValue;

        /// <summary>
        /// CRC校验合格
        /// </summary>
        public bool IsValid = false;

        //public int counter = 0;

        public Dictionary<string, SignalQualityV200> SQDic = new Dictionary<string, SignalQualityV200>();

        /// <summary>
        /// 后续计算用测距值列表，已经过筛选
        /// </summary>
        public Dictionary<string, LocationInfo> LocationDic = new Dictionary<string, LocationInfo>();

        /// <summary>
        /// 原始测距值列表，未经过筛选
        /// </summary>
        public Dictionary<string, LocationInfo> OrgLocationDic = new Dictionary<string, LocationInfo>();

        /// <summary>
        /// 符合后续运算要求
        /// </summary>
        public int CalcValidNum
        {
            get
            {
                return LocationDic.Count;
            }
        }
    }
}
