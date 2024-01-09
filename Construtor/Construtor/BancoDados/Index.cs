using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Construtor.BancoDados
{
    internal class Index
    {
        internal string Table { get; set; }
        internal string NonUnique { get; set; }
        internal string KeyName { get; set; }
        internal string SeqInIndex{ get; set; }
        internal string CollumnName { get; set; }
        internal string Collation { get; set; }
        internal string Visible { get; set; }
    }
}
