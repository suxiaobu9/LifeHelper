using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Crawler
{
    public class OneOFourHtmlModel
    {
        public OneOFourHtmlModel() { }

        public OneOFourHtmlModel(HtmlNodeCollection sourceJobHtml)
        {
            OneOFourHtmlJobInfos = sourceJobHtml.Select(x =>
            {
                var data = new OneOFourHtmlJobInfo
                {
                    No = x.GetAttributeValue("data-job-no", ""),
                    Name = x.GetAttributeValue("data-job-name", ""),
                    CompanyNo = x.GetAttributeValue("data-cust-no", ""),
                    CompanyName = x.GetAttributeValue("data-cust-name", ""),
                };
                if (string.IsNullOrEmpty(data.No))
                    return null;
                data.DetailLink = @"https:" + x.SelectSingleNode("ul/li[3]/a").GetAttributeValue("href", "");
                return data;
            }).Where(x => x != null).ToList();
        }

        public DateTime? SynchronizeDate { get; set; }

        public List<OneOFourHtmlJobInfo> OneOFourHtmlJobInfos { get; set; }

        public class OneOFourHtmlJobInfo
        {
            /// <summary>
            /// 職缺編號
            /// </summary>
            public string No { get; set; }

            /// <summary>
            /// 職缺名稱
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 公司編號
            /// </summary>
            public string CompanyNo { get; set; }

            /// <summary>
            /// 公司名稱
            /// </summary>
            public string CompanyName { get; set; }

            /// <summary>
            /// 職缺詳細內容網址
            /// </summary>
            public string DetailLink { get; set; }

            /// <summary>
            /// 工作待遇
            /// </summary>
            public string Pay { get; set; }

            /// <summary>
            /// 工作地點
            /// </summary>
            public string WorkPlace { get; set; }

            /// <summary>
            /// 上邊時間
            /// </summary>
            public string WorkTime { get; set; }

            public bool IsShow { get; set; }

            public bool IsReaded { get; set; }
        }
    }

}
