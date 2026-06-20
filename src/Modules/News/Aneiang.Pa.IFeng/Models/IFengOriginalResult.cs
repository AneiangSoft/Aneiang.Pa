using System.Collections.Generic;

namespace Aneiang.Pa.IFeng.Models
{
    public class IFengOriginalResult
    {
        public List<IFengOriginalData> hotNews1 { get; set; } = new List<IFengOriginalData>();
    }

    public class IFengOriginalData
    {
        public string url { get; set; }
        public string title { get; set; }
        public string newsTime { get; set; }
        public string thumbnail { get; set; }
        public object children { get; set; }
    }


}
