using System.Collections.Generic;

namespace Aneiang.Pa.ThePaper.Models
{
    public class ThePaperOriginalResult
    {
        public int resultCode { get; set; }
        public string resultMsg { get; set; }
        public ThePaperOriginalData data { get; set; } = new ThePaperOriginalData();
    }

    public class ThePaperOriginalData
    {
        public List<ThePaperOriginalItem> hotNews { get; set; } = new List<ThePaperOriginalItem>();
    }

    public class ThePaperOriginalItem
    {
        public string contId { get; set; }
        public string isOutForword { get; set; }
        public string isOutForward { get; set; }
        public string forwardType { get; set; }
        public int mobForwardType { get; set; }
        public string interactionNum { get; set; }
        public string praiseTimes { get; set; }
        public string pic { get; set; }
        public int imgCardMode { get; set; }
        public string smallPic { get; set; }
        public string sharePic { get; set; }
        public string pubTime { get; set; }
        public string pubTimeNew { get; set; }
        public string name { get; set; }
        public string closePraise { get; set; }
        public Nodeinfo nodeInfo { get; set; }
        public int nodeId { get; set; }
        public int contType { get; set; }
        public long pubTimeLong { get; set; }
        public int specialNodeId { get; set; }
        public string cardMode { get; set; }
        public int dataObjId { get; set; }
        public bool closeFrontComment { get; set; }
        public bool isSupInteraction { get; set; }
        public Taglist[] tagList { get; set; }
        public bool hideVideoFlag { get; set; }
        public int praiseStyle { get; set; }
        public int isSustainedFly { get; set; }
        public int softLocType { get; set; }
        public bool closeComment { get; set; }
        public Voiceinfo voiceInfo { get; set; }
        public string softAdTypeStr { get; set; }
        public int originalContId { get; set; }
        public bool paywalled { get; set; }
        public bool audiovisualBlogSwitch { get; set; }
        public string audiovisualBlogGuests { get; set; }
        public string seriesTagRecType { get; set; }
    }

    public class Nodeinfo
    {
        public int nodeId { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string pic { get; set; }
        public int nodeType { get; set; }
        public int channelType { get; set; }
        public int forwordType { get; set; }
        public string forwardType { get; set; }
        public string liveType { get; set; }
        public int parentId { get; set; }
        public string bigDataCode { get; set; }
        public string isOrder { get; set; }
        public string dataType { get; set; }
        public string shareName { get; set; }
        public string nickName { get; set; }
        public long publishTime { get; set; }
        public string mobForwardType { get; set; }
        public string summarize { get; set; }
        public string color { get; set; }
        public string videoLivingRoomDes { get; set; }
        public int wwwSpecNodeAlign { get; set; }
        public string govAffairsType { get; set; }
        public bool showSpecialBanner { get; set; }
        public bool showSpecialTopDesc { get; set; }
        public bool topBarTypeCustomColor { get; set; }
        public bool showVideoBottomRightBtn { get; set; }
    }

    public class Voiceinfo
    {
        public string voiceSrc { get; set; }
        public string imgSrc { get; set; }
        public string isHaveVoice { get; set; }
    }

    public class Taglist
    {
        public int tagId { get; set; }
        public string tag { get; set; }
        public string isOrder { get; set; }
        public string isUpdateNotify { get; set; }
        public string isWonderfulComments { get; set; }
    }

}
