using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.Bilibili.Model
{
    public class Desc_v2Item
    {
        public string raw_text { get; set; }

        public int type { get; set; }

        public int biz_id { get; set; }
    }

    public class Rights
    {
        public int bp { get; set; }

        public int elec { get; set; }

        public int download { get; set; }

        public int movie { get; set; }

        public int pay { get; set; }

        public int hd5 { get; set; }

        public int no_reprint { get; set; }

        public int autoplay { get; set; }

        public int ugc_pay { get; set; }

        public int is_cooperation { get; set; }

        public int ugc_pay_preview { get; set; }

        public int no_background { get; set; }

        public int clean_mode { get; set; }

        public int is_stein_gate { get; set; }

        public int is_360 { get; set; }

        public int no_share { get; set; }
    }

    public class Owner
    {
        public int mid { get; set; }

        public string name { get; set; }

        public string face { get; set; }
    }

    public class Stat
    {
        public int aid { get; set; }

        public int view { get; set; }

        public int danmaku { get; set; }

        public int reply { get; set; }

        public int favorite { get; set; }

        public int coin { get; set; }

        public int share { get; set; }

        public int now_rank { get; set; }

        public int his_rank { get; set; }

        public int like { get; set; }

        public int dislike { get; set; }

        public string evaluation { get; set; }

        public string argue_msg { get; set; }
    }

    public class Dimension
    {
        public int width { get; set; }

        public int height { get; set; }

        public int rotate { get; set; }
    }

    public class PagesItem
    {
        public int cid { get; set; }

        public int page { get; set; }

        public string from { get; set; }

        public string part { get; set; }

        public int duration { get; set; }

        public string vid { get; set; }

        public string weblink { get; set; }

        public Dimension dimension { get; set; }

        public string first_frame { get; set; }
    }

    public class Subtitle
    {
        public string allow_submit { get; set; }

        public List<string> list { get; set; }
    }

    public class User_garb
    {
        public string url_image_ani_cut { get; set; }
    }

    public class HonorItem
    {
        public int aid { get; set; }

        public int type { get; set; }
        /// <summary>
        /// 热门
        /// </summary>
        public string desc { get; set; }

        public int weekly_recommend_num { get; set; }
    }

    public class Honor_reply
    {
        public List<HonorItem> honor { get; set; }
    }

    public class Data
    {
        public string bvid { get; set; }

        public int aid { get; set; }

        public int videos { get; set; }

        public int tid { get; set; }
        /// <summary>
        /// 日常
        /// </summary>
        public string tname { get; set; }

        public int copyright { get; set; }

        public string pic { get; set; }

        public string title { get; set; }

        public int pubdate { get; set; }

        public int ctime { get; set; }

        public string desc { get; set; }

        public List<Desc_v2Item> desc_v2 { get; set; }

        public int state { get; set; }

        public int duration { get; set; }

        public Rights rights { get; set; }

        public Owner owner { get; set; }

        public Stat stat { get; set; }

        public string dynamic { get; set; }

        public int cid { get; set; }

        public Dimension dimension { get; set; }

        public string no_cache { get; set; }

        public List<PagesItem> pages { get; set; }

        public Subtitle subtitle { get; set; }

        public User_garb user_garb { get; set; }

        public Honor_reply honor_reply { get; set; }
    }

    public class VideoInfo
    {
        public int code { get; set; }

        public string message { get; set; }

        public int ttl { get; set; }

        public Data data { get; set; }
    }
}
