using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLModel
{
    public class LocationInfo
    {
        //基站ID
        public string AID = "";
        //定位距离
        public int PositionDistance = 0;

        public LocationInfo()
        {

        }

        public LocationInfo(string id, int range)
        {
            AID = id;
            PositionDistance = range;
        }
    }
}
