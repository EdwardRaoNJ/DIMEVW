﻿using System;
using System.ServiceModel;
using System.Xml;

namespace Knowte.Core.IO
{
    public class StrongNetNamedPipeBinding : NetNamedPipeBinding
    {
        #region Construction
        public StrongNetNamedPipeBinding() : base(NetNamedPipeSecurityMode.None)
        {
            this.InitializeSettings();
        }
        #endregion

        #region Private
        private void InitializeSettings()
        {
            var bindingReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxDepth = 64,
                MaxStringContentLength = 2147483647,
                MaxArrayLength = 2147483647,
                MaxBytesPerRead = 4096,
                MaxNameTableCharCount = 16384
            };

            this.ReceiveTimeout = TimeSpan.MaxValue;
            this.SendTimeout = new TimeSpan(0, 15, 0);
            this.MaxBufferPoolSize = 2000000000;
            this.MaxConnections = 100000;
            this.MaxReceivedMessageSize = 2000000000;
            this.MaxBufferSize = 2000000000;
            this.ReaderQuotas = bindingReaderQuotas;
        }
        #endregion
    }
}
