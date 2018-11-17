using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    class HttpTool
    {
        public async Task<String>  GetData() {
            HttpClient httpClient =  new HttpClient();
            String url = "http://api.v1.dc-express.cn/oauth/getToken";
            try
            {
                HttpContent httpContent = new StringContent("1");
                //设置Http的内容标头
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //设置Http的内容标头的字符
                httpContent.Headers.ContentType.CharSet = "utf-8";
                
                HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), httpContent);
                String result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(response);
                return result;
            }
            catch (Exception E) {
                return "";
            }
           
        }
    }

}
