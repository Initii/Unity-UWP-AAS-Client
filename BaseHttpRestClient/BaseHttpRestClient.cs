using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
#if UNITY_WSA && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Foundation;
#else
using System.Net;
#endif

namespace BaseHttpRestClient
{
    public class BaseHttpRestClient
    {
        // ================================================================================
        // CONSTANTS
        // ================================================================================
        private const string URL_SCHEME = "http://";
        private const string REGEX_URL_HOST_SEARCH_PATTERN = "^http://([a-z0-9\\-._~%])+";
        private const string REGEX_IPV4_SEARCH_PATTERN = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$";
        private const long DNS_TIMEOUT_MS = 3000;
        private const long TASK_TIMEOUT_MS = 3000;
        // ================================================================================

        // ================================================================================
        // ENUMS
        // ================================================================================
        public enum CompletedStatus
        {
            SUCCESS,
            FAILED,
            TIME_OUT
        }

        public enum RestMethod
        {
            GET,
            POST,
            PUT,
            DELETE,
            PATCH
        }
        // END OF ENUMS

        // ================================================================================
        // CLASS VARIABLES
        // ================================================================================
        public delegate void _OnRestCallCompleted(CompletedStatus status, RestMethod method, string id, string result);
        public delegate void _OnInitializeCompleted(CompletedStatus status);
        public _OnInitializeCompleted OnInitializeCompleted;

        private HttpClient _httpClient;
        private Stopwatch _stopwatch;
        private bool _httpClientInitialized = false;
        private List<TaskData> _taskDataList = new List<TaskData>();
        private bool _runHandleTaskChecks = true;
        private object _taskDataListLock = new object();
        private string _contentType;
        // END OF CLASS VARIABLES


        // ================================================================================
        // PUBLIC METHODS
        // ================================================================================

        /// <summary>
        /// Creates a BaseHttpRestClient.
        /// </summary>
        /// <param name="url">Base url.</param>
        /// <param name="contentType">Content type. Default: application/json.</param>
        public BaseHttpRestClient(string baseUrl, string contentType = "application/json")
        {
            _httpClient = new HttpClient();
            _stopwatch = Stopwatch.StartNew();
            _contentType = contentType;

            Thread initializeHttpClientThread = new Thread(() =>
            {
                InitializeHttpClient(baseUrl, contentType);
            });
            initializeHttpClientThread.IsBackground = true;
            initializeHttpClientThread.Start();
        }

        /// <summary>
        /// Returns the base address of the http client. Host name is preplaced by the ip.
        /// </summary>
        /// <returns></returns>
        public string GetBaseAddress()
        {
            return _httpClient.BaseAddress.ToString();
        }

        /// <summary>
        /// Stops the thread which handles the tasks. Rest calls can still be done but no callbacks will be called.
        /// </summary>
        public void Stop()
        {
            _runHandleTaskChecks = false;
        }

        /// <summary>
        /// Call the Rest API Get method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="id">An Id to identify the callback.</param>
        /// <param name="callback">Will be called when corresponding task has completed.</param>
        public void Get(string path, string id, _OnRestCallCompleted callback)
        {
            if (!_httpClientInitialized) return;

            TaskData taskData = new TaskData(_stopwatch.ElapsedMilliseconds, _httpClient.GetAsync(path), RestMethod.GET, id, callback);
            lock (_taskDataListLock)
            {
                _taskDataList.Add(taskData);
            }
        }

        /// <summary>
        /// Call the Rest API Post method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="id">An Id to identify the callback.</param>
        /// <param name="callback">Will be called when corresponding task has completed.</param>
        /// <param name="payload">The payload.</param>
        public void Post(string path, string id, _OnRestCallCompleted callback, string payload, string _headerContentType = "application/json")
        {
            if (!_httpClientInitialized) return;

            StringContent content = new StringContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue(_headerContentType);
            TaskData taskData = new TaskData(_stopwatch.ElapsedMilliseconds, _httpClient.PostAsync(path, content), RestMethod.POST, id, callback);
            lock (_taskDataListLock)
            {
                _taskDataList.Add(taskData);
            }
        }

        /// <summary>
        /// Call the Rest API Put method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="id">An Id to identify the callback.</param>
        /// <param name="callback">Will be called when corresponding task has completed.</param>
        /// <param name="payload">The payload.</param>
        public void Put(string path, string id, _OnRestCallCompleted callback, string payload, string _headerContentType = "application/json")
        {
            if (!_httpClientInitialized) return;

            StringContent content = new StringContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue(_headerContentType);
            TaskData taskData = new TaskData(_stopwatch.ElapsedMilliseconds, _httpClient.PutAsync(path, content), RestMethod.PUT, id, callback);
            lock (_taskDataListLock)
            {
                _taskDataList.Add(taskData);
            }
        }

        /// <summary>
        /// Call the Rest API Delete method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="id">An Id to identify the callback.</param>
        /// <param name="callback">Will be called when corresponding task has completed.</param>
        public void Delete(string path, string id, _OnRestCallCompleted callback)
        {
            if (!_httpClientInitialized) return;

            TaskData taskData = new TaskData(_stopwatch.ElapsedMilliseconds, _httpClient.DeleteAsync(path), RestMethod.DELETE, id, callback);
            lock (_taskDataListLock)
            {
                _taskDataList.Add(taskData);
            }
        }

        /// <summary>
        /// Call the Rest API Patch method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="id">An Id to identify the callback.</param>
        /// <param name="callback">Will be called when corresponding task has completed.</param>
        /// <param name="payload">The payload.</param>
        public void Patch(string path, string id, _OnRestCallCompleted callback, string payload, string _headerContentType = "application/json")
        {
            if (!_httpClientInitialized) return;

            StringContent content = new StringContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue(_headerContentType);
            TaskData taskData = new TaskData(_stopwatch.ElapsedMilliseconds, _httpClient.PatchAsync(path, content), RestMethod.PATCH, id, callback);
            lock (_taskDataListLock)
            {
                _taskDataList.Add(taskData);
            }
        }
        // END OF PUBLIC METHODS


        // ================================================================================
        // PRIVATE METHODS
        // ================================================================================
        
        // Initialize the client, get the ip from host name and start the thread to handle the tasks
        private void InitializeHttpClient(string url, string contentType)
        {
            string hostName = GetHostNameFromUrl(url);
            if (hostName == null)
            {
                return;
            }

            string hostIp = GetIpFromHostName(hostName.Substring(URL_SCHEME.Length));
            if (hostIp == null)
            {
                return;
            }

            string newUrl = url.Replace(hostName, URL_SCHEME + hostIp);
            _httpClient.BaseAddress = new Uri(newUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            _httpClient.DefaultRequestHeaders.Host = hostName.Substring(URL_SCHEME.Length);

            Thread handleTaskChecks = new Thread(HandleTaskChecks);
            handleTaskChecks.IsBackground = true;
            handleTaskChecks.Start();

            _httpClientInitialized = true;
            CallInitializeCompletedCallback(CompletedStatus.SUCCESS);
        }

        // get host name, have to start with http://
        private string GetHostNameFromUrl(string url)
        {
            Match match = Regex.Match(url, REGEX_URL_HOST_SEARCH_PATTERN);
            if (!match.Success)
            {
                CallInitializeCompletedCallback(CompletedStatus.FAILED);
                return null;
            }

            return match.Value;
        }

        // return the ip from a given host name, Ipv6 is returned as [Ipv6]
        private string GetIpFromHostName(string hostName)
        {
            string ip;
            long starTime = _stopwatch.ElapsedMilliseconds;
#if UNITY_WSA && !UNITY_EDITOR
            HostName host = new HostName(hostName);
            IAsyncOperation<IReadOnlyList<EndpointPair>> endpointPairsAsyncOperation = DatagramSocket.GetEndpointPairsAsync(host, "80");
            while (endpointPairsAsyncOperation.Status != AsyncStatus.Completed && endpointPairsAsyncOperation.Status != AsyncStatus.Canceled && endpointPairsAsyncOperation.Status != AsyncStatus.Error)
#else
            Task<IPAddress[]> ipTask = Dns.GetHostAddressesAsync(hostName);
            while (!ipTask.IsCompleted && !ipTask.IsCanceled && !ipTask.IsFaulted && !ipTask.IsCompletedSuccessfully)
#endif
            {
                if (_stopwatch.ElapsedMilliseconds - starTime >= DNS_TIMEOUT_MS) 
                {
#if UNITY_WSA && !UNITY_EDITOR
                    endpointPairsAsyncOperation.Cancel();
#else
#endif
                    CallInitializeCompletedCallback(CompletedStatus.TIME_OUT);
                    return null;
                }
                Thread.Sleep(10);
            }

#if UNITY_WSA && !UNITY_EDITOR
            IReadOnlyList<EndpointPair> result = endpointPairsAsyncOperation.GetResults();
            if (endpointPairsAsyncOperation.Status == AsyncStatus.Completed && result.Count > 0) 
            {
                ip = result[0].RemoteHostName.ToString();
            }
            else {
                CallInitializeCompletedCallback(CompletedStatus.FAILED);
                return null;
            }
#else
            if (ipTask.IsCompletedSuccessfully && ipTask.Result.Length > 0)
            {
                ip = ipTask.Result[0].ToString();
            }
            else
            {
                CallInitializeCompletedCallback(CompletedStatus.FAILED);
                return null;
            }
#endif
            Match match = Regex.Match(ip, REGEX_IPV4_SEARCH_PATTERN);
            return (match.Success) ? ip : "[" + ip + "]";
        }

        // call the callback for initialization
        private void CallInitializeCompletedCallback(CompletedStatus status)
        {
            if (OnInitializeCompleted != null)
            {
                OnInitializeCompleted(status);
            }
        }

        // handles the tasks, run in at thread
        private void HandleTaskChecks()
        {
            while (_runHandleTaskChecks)
            {
                TaskData[] taskDataListCopy;
                List<TaskData> tasksCompleted = new List<TaskData>();
                lock (_taskDataListLock)
                {
                    taskDataListCopy = _taskDataList.ToArray();
                }

                foreach (TaskData taskData in taskDataListCopy)
                {
                    if (taskData.IsTaskFinished(_stopwatch.ElapsedMilliseconds))
                    {
                        tasksCompleted.Add(taskData);
                        taskData.CallOnRestCallCompleted();
                    }
                }

                lock (_taskDataListLock)
                {
                    foreach (TaskData taskData in tasksCompleted)
                    {
                        _taskDataList.Remove(taskData);
                    }
                }

                foreach (TaskData taskData in tasksCompleted)
                {
                    taskData.Dispose();
                }

                Thread.Sleep(10);
            }
        }
        // END OF PRIVATE METHODS


        // ================================================================================
        // TASK DATA CLASS
        // ================================================================================

        // data container to manage a task
        private class TaskData
        {
            private long _createdAt;
            private Task<HttpResponseMessage> _httpResponseMessageTask;
            private Task<string> _stringTask;
            private string _id;
            private RestMethod _restMethod;
            private _OnRestCallCompleted _onRestCallCompleted;
            private CompletedStatus _status;
            private string _result;

            public TaskData(long createdAt, Task<HttpResponseMessage> httpResponseMessageTask, RestMethod restMethod, string id, _OnRestCallCompleted onRestCallCompleted)
            {
                _createdAt = createdAt;
                _httpResponseMessageTask = httpResponseMessageTask;
                _restMethod = restMethod;
                _id = id;
                _onRestCallCompleted = onRestCallCompleted;
            }

            public bool IsTaskFinished(long currentTimeMS)
            {
                if (currentTimeMS - _createdAt <= TASK_TIMEOUT_MS)
                {
                    return CheckTask();
                }
                else
                {
                    _status = CompletedStatus.TIME_OUT;
                    return true;
                }
            }

            public void CallOnRestCallCompleted()
            {
                if (_onRestCallCompleted != null)
                {
                    _onRestCallCompleted(_status, _restMethod, _id, _result);
                }
            }

            private bool CheckTask()
            {
                if (_httpResponseMessageTask.IsCompleted)
                {
                    if (_stringTask == null)
                    {
                        if (_httpResponseMessageTask.IsCompletedSuccessfully)
                        {
                            _stringTask = _httpResponseMessageTask.Result.Content.ReadAsStringAsync();
                            return false;
                        }
                        else
                        {
                            _status = CompletedStatus.FAILED;
                            return true;
                        }
                    }
                    else
                    {
                        if (_stringTask.IsCompleted)
                        {
                            if (_stringTask.IsCompletedSuccessfully)
                            {
                                _result = _stringTask.Result;
                                _status = CompletedStatus.SUCCESS;
                                return true;
                            }
                            else
                            {
                                _status = CompletedStatus.FAILED;
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            public void Dispose()
            {
                _httpResponseMessageTask.Dispose();
                if (_stringTask != null)
                {
                    _stringTask.Dispose();
                }
            }
        }
        // END OF TASK CONTAINER CLASS
    }
}
