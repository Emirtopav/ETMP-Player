using ETMPClient.Enums;
using ETMPData.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPClient.Events
{
    public class PageChangedEventArgs : EventArgs
    {
        public PageType Page { get; set; }

        public PageChangedEventArgs(PageType page)
        {
            Page = page;
        }
    }
}
