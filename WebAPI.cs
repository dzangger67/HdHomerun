/*
    The MIT License (MIT)
    Copyright © 2022 David Zangger

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
    and associated documentation files (the “Software”), to deal in the Software without 
    restriction, including without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom 
    the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or 
    substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
    OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
    DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HdHomerun
{
    /// <summary>
    /// This class is used to help perform the web requests to get information
    /// from the HDHomerun device.
    /// </summary>
    internal static class WebAPI
    {
        /// <summary>
        /// Perform an http request using the HEAD method
        /// </summary>
        /// <param name="uri">The uri to make the request</param>
        /// <returns>The length of the recording (in bytes)</returns>
        public static long GetRecoringFileSize(string uri)
        {
            // Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create(uri);

            // Set the Method property of the request to HEAD.
            request.Method = "HEAD";

            // Get the response
            try
            {
                WebResponse resp = request.GetResponse();
                HttpWebResponse response = (HttpWebResponse)resp;
                long contentLength = response.ContentLength;
                resp.Close();

                // contentLength is the size in bytes of the requested recording
                return contentLength;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        /// <summary>
        /// This will make a web call with the provided uri to delete a recording
        /// </summary>
        /// <param name="uri">This is the uri of the recording along with the command to delete it</param>
        /// <returns>True if the recording was successfully deleted</returns>
        public static bool DeleteRecording(string uri)
        {
            // Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create(uri);

            // Set the Method property of the request to POST.
            request.Method = "POST";

            // Get the response
            try
            {
                WebResponse resp = request.GetResponse();
                HttpWebResponse response = (HttpWebResponse)resp;

                string statusCode = response.StatusCode.ToString();
                response.Close();

                // Was it deleted?
                return statusCode.Equals("OK", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<string> GetContentsAsync(string uri)
        {
            var client = new WebClient();
            string data = await client.DownloadStringTaskAsync(uri);

            return data;
        }

        /// <summary>c
        /// Reads the contents of a uri and returns the results.
        /// </summary>
        /// <param name="uri">This is the uri of the source we are trying to read</param>
        /// <returns>A string with the results of the request</returns>
        public static string GetContents(string uri)
        {
            try
            {
                // Create a new WebClient instance.
                WebClient myWebClient = new WebClient();

                // Open a stream to point to the data stream coming from the Web resource.
                Stream myStream = myWebClient.OpenRead(uri);

                // Read the data
                StreamReader sr = new StreamReader(myStream);
                string sContents = sr.ReadToEnd();

                // Close the stream. 
                myStream.Close();

                return sContents;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
