﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.creatives
{
    public interface ICreativePreviewer
    {
        void Preview(ICreative creative);
    }
}
