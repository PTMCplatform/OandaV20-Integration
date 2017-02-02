// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using OandaV20ExternalVendor.TradeLibrary.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor
{
    public abstract class StreamSession<T> where T : IHeartbeat
    {
        protected readonly string _accountId;
        private WebResponse _response;
        private WebRequest _request;

        CancellationTokenSource shutdown_CancelToken;

        public delegate void DataHandler(T data);

        public event DataHandler DataReceived;

        public void OnDataReceived(T data)
        {
            DataReceived?.Invoke(data);
        }

        protected StreamSession(string accountId)
        {
            _accountId = accountId;
        }

        protected StreamSession(List<string> accountIds)
        {
            _accountId = accountIds[0];
        }
        
        protected abstract Task<WebRequest> GetSessionRequest();

        public void StartSession()
        {
            try
            {
                _request = GetSessionRequest().Result;
                _response = _request.GetResponse();

                shutdown_CancelToken = new CancellationTokenSource();

                Task.Factory.StartNew(() =>
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    using (var reader = new StreamReader(_response.GetResponseStream()))
                    {
                        while (true)
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                string line = reader.ReadLine();

                                // Task was cancelled
                                shutdown_CancelToken.Token.ThrowIfCancellationRequested();

                                memStream.Write(Encoding.UTF8.GetBytes(line), 0, Encoding.UTF8.GetByteCount(line));
                                memStream.Position = 0;

                                // Task was cancelled
                                shutdown_CancelToken.Token.ThrowIfCancellationRequested();

                                T data;

                                try
                                {
                                    data = (T)serializer.ReadObject(memStream);

                                    // Don't send heartbeats
                                    if (!data.IsHeartbeat())
                                    {
                                        OnDataReceived(data);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var x = 0;
                                }

                                memStream.Close();
                            }

                            // Task was cancelled
                            shutdown_CancelToken.Token.ThrowIfCancellationRequested();
                        }
                    }
                });
            }
            catch (WebException ex)
            {
                _response = ex.Response;

                if (_response == null)
                    throw new Exception(ex.Status.ToString());

                string result = string.Empty;
                using (var stream = new StreamReader(_response.GetResponseStream()))
                {
                    result = stream.ReadToEnd();
                }
                throw new Exception(result);
            }
        }

        public void StopSession()
        {
            if (_request != null)
            {
                _request.Abort();
                _request = null;
            }

            if (_response != null)
            {
                _response.Close();
                _response = null;
            }

            if (shutdown_CancelToken != null)
                shutdown_CancelToken.Cancel();
        }
    }
}
