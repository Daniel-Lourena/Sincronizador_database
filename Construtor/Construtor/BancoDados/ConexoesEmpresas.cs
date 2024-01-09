using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Construtor.BancoDados
{
    internal static class ConexoesEmpresas
    {
        internal static string STRCONEMPRESAORIGEM = "Server=hostorigem;Port=xxxx;Database=banco_origem;Uid=user_origem;Pwd=senha_origem;SslMode=none;";
        internal static string STRCONEMPRESADESTINO = "Server=hostdestino;Port=xxxx;Database=banco_destino;Uid=user_destino;Password=senha_destino;SslMode=none;";
    }
}
