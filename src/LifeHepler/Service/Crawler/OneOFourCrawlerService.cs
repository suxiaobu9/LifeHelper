using HeplerLibs.ExtLib;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Model.Crawler;
using Service.Crawler.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Service.Crawler
{
    public class OneOFourCrawlerService : IOneOFourCrawlerService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly OneOFourJobInfoSourceUrlModel _oneOFourJobInfoSourceUrl;
        private string _dirPath => Path.Combine(_hostingEnvironment.ContentRootPath, "OneOFourXml");

        private static readonly object LockObj = new object();

        public OneOFourCrawlerService(IHostingEnvironment hostingEnvironment,
            IOptions<OneOFourJobInfoSourceUrlModel> oneOFourJobInfoSourceUrl)
        {
            _hostingEnvironment = hostingEnvironment;
            _oneOFourJobInfoSourceUrl = oneOFourJobInfoSourceUrl.Value;
        }

        private string GetFilePath(int userType)
        {
            return Path.Combine(_dirPath, $"{GetTime.TwNow:yyyyMMdd}_{userType}.xml");
        }

        public string GetSourceUrl(int userType)
        {
            return (userType == 1 ?
                _oneOFourJobInfoSourceUrl.Bo :
                _oneOFourJobInfoSourceUrl.Chien).Replace("page=1", "page={0}");
        }

        /// <summary>
        /// 取得當天已存在的xml檔案資料
        /// </summary>
        /// <returns></returns>
        public List<OneOFourHtmlModel> GetOneOFourLocalXmlInfo(int userType, bool onlyShowMatch)
        {
            var filePath = GetFilePath(userType);
            if (!Directory.Exists(_dirPath) || !File.Exists(filePath))
                return new List<OneOFourHtmlModel>();

            var oldXmlDoc = XDocument.Load(filePath);

            var result = oldXmlDoc.Element("Data").Elements("Block").Select(x => new OneOFourHtmlModel
            {
                SynchronizeDate = Convert.ToDateTime(x.Element("SynchronizeDate").Value),
                OneOFourHtmlJobInfos = x.Elements("Job").Select(y => new OneOFourHtmlModel.OneOFourHtmlJobInfo
                {
                    No = y.Element("No").Value,
                    Name = y.Element("Name").Value,
                    CompanyNo = y.Element("CompanyNo").Value,
                    CompanyName = y.Element("CompanyName").Value,
                    DetailLink = y.Element("DetailLink").Value,
                    Pay = y.Element("Pay").Value,
                    WorkPlace = y.Element("WorkPlace").Value,
                    WorkTime = y.Element("WorkTime").Value,
                    IsShow = y.Element("IsShow").Value.Ext_IsTrue(),
                    IsReaded = y.Element("IsReaded").Value.Ext_IsTrue()
                }).Where(x =>
                {
                    if (onlyShowMatch)
                        return x.IsShow;
                    return true;
                }).ToList()
            }).Where(x => !x.OneOFourHtmlJobInfos.Ext_IsNullOrEmpty()).ToList();

            return result;
        }

        /// <summary>
        /// 同步104職缺資料
        /// </summary>
        public void SynchronizeOneOFourXml(int userType)
        {
            lock (LockObj)
            {

                var localJobData = GetOneOFourLocalXmlInfo(userType, false);
                var newJobModel = new OneOFourHtmlModel
                {
                    SynchronizeDate = GetTime.TwNow,
                    OneOFourHtmlJobInfos = new List<OneOFourHtmlModel.OneOFourHtmlJobInfo>()
                };

                if (!IsNeedToSynJobData(userType))
                    return;

                HttpWebRequest request;
                var page = 1;
                var requestUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
                var requestAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                var htmlJobInfo = new List<OneOFourHtmlModel.OneOFourHtmlJobInfo>();

                while (true)
                {
                    var url = string.Format(GetSourceUrl(userType), page++);
                    //爬104資料清單
                    request = (HttpWebRequest)WebRequest.Create(url);

                    request.UserAgent = requestUserAgent;
                    request.Accept = requestAccept;

                    using var response = (HttpWebResponse)request.GetResponse();
                    HttpStatusCode code = response.StatusCode;
                    if (code != HttpStatusCode.OK)
                        break;
                    HtmlDocument htmlDoc = new HtmlDocument
                    {
                        OptionFixNestedTags = true
                    };
                    using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        htmlDoc.Load(sr);
                    }
                    var articleList = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"js-job-content\"]/article");
                    if (articleList == null)
                        break;
                    //html To model
                    var tmpSimpleJobInfo = new OneOFourHtmlModel(articleList);
                    htmlJobInfo.AddRange(tmpSimpleJobInfo.OneOFourHtmlJobInfos);
                }

                //以No去除重複資料
                htmlJobInfo = htmlJobInfo.Ext_DistinctByKey(x => x.No).ToList();

                //取得詳細資料的BaseUrl
                var baseUrl = @"https://www.104.com.tw/job/ajax/content/{0}";

                //判斷詳細工作資訊的condition
                var filterJobCondition = new Func<OneOFourHtmlModel.OneOFourHtmlJobInfo, OneOFourHtmlModel.OneOFourHtmlJobInfo>((x) =>
                {
                    //取得該工作的網址編號
                    var jobUrlKey = new Uri(x.DetailLink).AbsolutePath.Trim('/').Split('/').LastOrDefault();

                    if (jobUrlKey.Ext_IsNullOrEmpty())
                        return null;

                    var ajaxUrl = string.Format(baseUrl, jobUrlKey);

                    request = (HttpWebRequest)WebRequest.Create(ajaxUrl);
                    request.UserAgent = requestUserAgent;
                    request.Accept = requestAccept;
                    //驗證用的
                    request.Headers.Add("Referer", x.DetailLink);
                    using (var response = (HttpWebResponse)(request.GetResponse()))
                    {
                        HttpStatusCode code = response.StatusCode;
                        if (code == HttpStatusCode.OK)
                        {
                            x.IsShow = true;
                            using var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                            var jobDetailJson = sr.ReadToEnd();
                            var jobData = jobDetailJson.Ext_ToType<JobDetailInfo>();

                            x.Pay = jobData.data.jobDetail.salary;
                            x.WorkPlace = jobData.data.jobDetail.addressRegion + jobData.data.jobDetail.addressDetail;
                            x.WorkTime = jobData.data.jobDetail.workPeriod;

                            if (userType != 1)
                                return x;

                            ////不找內湖
                            //if (jobData.data.jobDetail.addressDetail.Contains("內湖") || jobData.data.jobDetail.addressRegion.Contains("內湖"))
                            //    return false;
                            //工作標題有沒有net
                            if (!jobData.data.header.jobName.ToLower().Contains("net") &&
                                //工作內容有沒有net
                                !jobData.data.jobDetail.jobDescription.ToLower().Contains("net") &&
                                //需要的技能有沒有net
                                (!jobData.data.condition.specialty.Any() ||
                                !jobData.data.condition.specialty.Any(x => x.description.ToLower().Contains("net"))))
                            {
                                x.IsShow = false;
                                return x;
                            }

                            return x;
                        }
                    }
                    return x;
                });

                var existJobNoList = localJobData.Ext_IsNullOrEmpty() ?
                new List<string>() :
                localJobData.SelectMany(x => x.OneOFourHtmlJobInfos.Select(y => y.No)).ToList();

                newJobModel.OneOFourHtmlJobInfos = htmlJobInfo
                           .Where(x => !existJobNoList.Contains(x.No))
                           .Select(x => filterJobCondition(x))
                           .Where(x => x != null)
                           .ToList();

                if (!newJobModel.OneOFourHtmlJobInfos.Ext_IsNullOrEmpty())
                {
                    localJobData.Add(newJobModel);

                    SaveJobDataToLocal(localJobData.OrderBy(x => x.SynchronizeDate), userType);
                }
            }
        }

        /// <summary>
        /// 將資料存到Local
        /// </summary>
        /// <param name="jobData">The job data.</param>
        private void SaveJobDataToLocal(IEnumerable<OneOFourHtmlModel> jobData, int userType)
        {

            if (!Directory.Exists(_dirPath))
                Directory.CreateDirectory(_dirPath);

            var newXmlDoc = new XDocument(new XElement("Data",
                        jobData.Select(x => new XElement("Block",
                                                new XElement("SynchronizeDate", x.SynchronizeDate.Value.ToString("yyyy/MM/dd HH:mm:ss")),
                                                x.OneOFourHtmlJobInfos.Select(y =>
                                                new XElement("Job",
                                                    new XElement("No", ReplaceHexadecimalSymbols(y.No)),
                                                    new XElement("Name", ReplaceHexadecimalSymbols(y.Name)),
                                                    new XElement("CompanyNo", ReplaceHexadecimalSymbols(y.CompanyNo)),
                                                    new XElement("CompanyName", ReplaceHexadecimalSymbols(y.CompanyName)),
                                                    new XElement("DetailLink", ReplaceHexadecimalSymbols(y.DetailLink)),
                                                    new XElement("Pay", ReplaceHexadecimalSymbols(y.Pay)),
                                                    new XElement("WorkPlace", ReplaceHexadecimalSymbols(y.WorkPlace)),
                                                    new XElement("WorkTime", ReplaceHexadecimalSymbols(y.WorkTime)),
                                                    new XElement("IsShow", ReplaceHexadecimalSymbols(y.IsShow.ToString())),
                                                    new XElement("IsReaded", ReplaceHexadecimalSymbols(y.IsReaded.ToString()))
                                                    ))))));

            newXmlDoc.Save(GetFilePath(userType));
        }

        private string ReplaceHexadecimalSymbols(string txt)
        {
            if (txt != "")
            {
                string r = "[\x00-\x08\x0B\x0C\x0E-\x1F]";
                return Regex.Replace(txt, r, "", RegexOptions.Compiled);
            }
            else
            {
                return "";
            }

        }

        public bool IsNeedToSynJobData(int userType)
        {
            var localJobData = GetOneOFourLocalXmlInfo(userType, false).OrderByDescending(x => x.SynchronizeDate);

            return localJobData.Ext_IsNullOrEmpty() || localJobData.Max(x => x.SynchronizeDate.Value).AddMinutes(10) < GetTime.TwNow;

        }

        /// <summary>
        /// 更新成已讀
        /// </summary>
        /// <param name="model">The model.</param>
        public void UpdateJobInfoToReaded(OneOFourForm model)
        {
            lock (LockObj)
            {

                if (model.JobNo.Ext_IsNullOrEmpty() || !new int[] { 1, 2 }.Contains(model.UserType))
                    return;

                var localJobData = GetOneOFourLocalXmlInfo(model.UserType, false);
                var done = false;

                foreach (var item in localJobData)
                {
                    foreach (var subItem in item.OneOFourHtmlJobInfos)
                    {
                        if (subItem.No == model.JobNo)
                        {
                            done = true;
                            subItem.IsReaded = true;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
                if (done)
                    SaveJobDataToLocal(localJobData, model.UserType);
            }
        }

    }
}
