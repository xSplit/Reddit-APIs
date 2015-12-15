using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace rdact
{
    class Reddit
    {
        public enum VoteType
        {
            UP = 1, DOWN = -1, RESET = 0
        }

        public enum Submit
        {
            Text,
            Link
        }

        public readonly static string[] ReportReasons = { "spam", "vote manipulation", "personal information", "sexualizing minors", "breaking reddit", "other" };

        public static bool Valid(string page)
        {
            return true;
        }

        public static bool Create(string user, string pass, string proxy = null)
        {
            try
            {
                Post("https://www.reddit.com/api/check_password.json", "passwd=" + pass, proxy);
                Post("https://www.reddit.com/api/check_username.json", "user=" + user, proxy);
                Post("https://www.reddit.com/api/register/" + user, "op=reg&user=" + user + "&passwd=" + pass + "&passwd2=" + pass + "&email=&api_type=json", proxy);
                return true;
            }
            catch (Exception) { return false; }
        }

        public static CookieContainer Login(string user, string pass)
        {
            try
            {
                var session = Post("https://www.reddit.com/api/login/" + user, "op=login&user=" + user + "&passwd=" + pass + "&api_type=json", null, new CookieContainer());
                foreach (Cookie cookie in session.GetCookies(new Uri("https://reddit.com")))
                    if (cookie.Name == "reddit_session")
                        return session;
            }
            catch (Exception) { }
            return null;
        }

        public static bool VotePost(string page, VoteType vote, CookieContainer session, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var vhash = parse.Split(new[] { "\"vote_hash\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
                Post("https://www.reddit.com/api/vote", "id=t3_" + data[6] + "&dir=" + (int)vote + "&vh=" + vhash + "&isTrusted=true&r=" + data[4] +
                    "&uh=" + uh(parse) + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool VoteComment(string page, VoteType vote, CookieContainer session, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var vhash = parse.Split(new[] { "\"vote_hash\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
                Post("https://www.reddit.com/api/vote", "id=t1_" + data.Last() + "&dir=" + (int)vote + "&vh=" + vhash + "&isTrusted=true&r=" + data[4] +
                    "&uh=" + uh(parse) + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ReportPost(string page, string reason, CookieContainer session, string other_reason = null, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var other = other_reason != null ? "&other_reason=" + HttpUtility.UrlEncode(other_reason) : "";
                Post("https://www.reddit.com/api/report", "thing_id=t3_" + data[6] + "&reason=" + HttpUtility.UrlEncode(reason) + other + "&id=%23report-action-form&r="
                    + data[4] + "&uh=" + uh(parse) + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ReportComment(string page, string reason, CookieContainer session, string other_reason = null, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var other = other_reason != null ? "&other_reason=" + HttpUtility.UrlEncode(other_reason) : "";
                Post("https://www.reddit.com/api/report", "thing_id=t1_" + data.Last() + "&reason=" + HttpUtility.UrlEncode(reason) + other + "&id=%23report-action-form&r="
                    + data[4] + "&uh=" + uh(parse) + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Comment(string page, string comment, CookieContainer session, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var h = uh(parse);
                var id = parse.Split(new[] { "id=\"form-t3_" }, StringSplitOptions.None)[1].Split('"')[0];
                //10 MINUTES DELAY
                Post("https://www.reddit.com/api/comment", "thing_id=t3_" + data[6] + "&text=" + HttpUtility.UrlEncode(comment) +
                   "&id=%23form-t3_" + id + "&r=" + data[4] + "&uh=" + h + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Reply(string page, string reply, CookieContainer session, string proxy = null)
        {
            try
            {
                var data = page.Split('/');
                var parse = Get(page, session);
                var h = uh(parse);
                //10 MINUTES DELAY
                Post("https://www.reddit.com/api/comment", "thing_id=t1_" + data.Last() + "&text=" + HttpUtility.UrlEncode(reply) +
                    "&id=%23commentreply_t1_" + data.Last() + "&r=" + data[4] + "&uh=" + h + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string[] PM_Captcha(string user, CookieContainer session, string proxy = null)
        {
            try
            {
                var parse = Get("https://www.reddit.com/message/compose/?to=" + user, session);
                return new string[] { captcha(parse), iden(parse), uh(parse), user };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool PM_Send(string subject, string text, string captcha, string[] data, CookieContainer session, string proxy = null)
        {
            try
            {
                //NO DELAY
                Post("https://www.reddit.com/api/compose", "uh=" + data[2] + "&to=" + data[3] + "&subject=" + HttpUtility.UrlEncode(subject) + "&thing_id=&text=" + HttpUtility.UrlEncode(text) +
                    "&iden=" + data[1] + (captcha != "" ? "&captcha=" + captcha : "") + "&source=compose&id=%23compose-message&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string[] Submit_Captcha(string subreddit, CookieContainer session, string proxy = null)
        {
            try
            {
                var parse = Get("https://www.reddit.com/r/" + subreddit + "/submit", session);
                return new string[] { captcha(parse), iden(parse), uh(parse), subreddit };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Submit_Send(string title, string text, Submit type, string captcha, string[] data, CookieContainer session, string proxy = null)
        {
            try
            {
                var kind = type == Submit.Link ? "link" : "self";
                var key = type == Submit.Link ? "url" : "text";
                //10 MINUTES DELAY
                Post("https://www.reddit.com/api/submit", "uh=" + data[2] + "&title=" + HttpUtility.UrlEncode(title) + "&kind=" + kind + "&thing_id=&" + key + "="
                    + HttpUtility.UrlEncode(text) + "&sr=" + data[3] + "&sendreplies=false&iden=" + data[1] + (captcha != "" ? "&captcha=" + captcha : "") + "&resubmit=&id=%23newlink&r=" + data[3] + "&renderstyle=html", proxy, session);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static CookieContainer Post(string page, string postData, string proxy = null, CookieContainer cookies = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(page);
            var data = Encoding.ASCII.GetBytes(postData);
            if (cookies != null && cookies.Count < 1)
                cookies.Add(new Cookie("_test", "_ok", "/", "reddit.com"));

            request.Method = "POST";
            request.Proxy = proxy != null ? new WebProxy(proxy.Split(':')[0], int.Parse(proxy.Split(':')[1])) : null;
            request.CookieContainer = cookies;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.80 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            request.GetResponse();
            return request.CookieContainer;
        }

        private static string Get(string page, CookieContainer cookies)
        {
            var request = (HttpWebRequest)WebRequest.Create(page);

            request.Method = "GET";
            request.CookieContainer = cookies;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.80 Safari/537.36";

            var resp = (HttpWebResponse)request.GetResponse();
            return new StreamReader(resp.GetResponseStream()).ReadToEnd();
        }

        private static string uh(string data)
        {
            return data.Split(new[] { "<input type=\"hidden\" name=\"uh\" value=\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private static string captcha(string data)
        {
            var captcha = "";
            if (data.Contains("alt=\"visual CAPTCHA\" src=\""))
                captcha = "https://www.reddit.com" + data.Split(new[] { "alt=\"visual CAPTCHA\" src=\"" }, StringSplitOptions.None)[1].Split('"')[0];
            return captcha;
        }

        private static string iden(string data)
        {
            return data.Split(new[] { "<input name=\"iden\" value=\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }
    }
}
