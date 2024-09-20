using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestreamFRBot.Configuration
{
    public class RestreamModuleConfig
    {
        public int ModuleId { get; set; }
        public string SheetUri { get; set; } = "";
        public DateTime MinDate { get; set; } = new DateTime(2024, 09, 19);
        public ulong BotRestreamChannel { get; set; }
    }
}
