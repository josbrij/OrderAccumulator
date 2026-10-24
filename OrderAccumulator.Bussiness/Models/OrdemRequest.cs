using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator.Bussiness.Models
{
    public class OrdemRequest
    {
        public TipoSimbolo Simbolo { get; set; }
        public TipoOrdem Lado { get; set; }
        public int Quantidade { get; set; }
        public decimal Preco { get; set; }
        public enum TipoSimbolo
        {
            [Description("Petrobras")]
            PETR4 = 0,
            [Description("Vale")]
            VALE3 = 1,
            [Description("ViaVarejo")]
            VIIA4 = 2
        }
        public enum TipoOrdem
        {
            [Description("Compra")]
            Compra = 0,
            [Description("Venda")]
            Venda = 1
        }
    }
}
