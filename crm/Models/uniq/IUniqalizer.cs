﻿using crm.Models.creatives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.uniq
{
    public interface IUniqalizer
    {
        Task Uniqalize(ICreative creative, int n, string outputdir);
        Task Uniqalize(string inputPath, int n, string outpurdir, string scntr = null);
        void Cancel();
        event Action<int> UniqalizeProgessUpdateEvent;
    }

    public class UniqalizerException : Exception
    {
        public UniqalizerException(string message) : base(message) { }
    }
}
