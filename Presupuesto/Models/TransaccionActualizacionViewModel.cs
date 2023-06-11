namespace Presupuesto.Models
{
    public class TransaccionActualizacionViewModel: TransaccionCreacionViewModel
    {
        public int cuentaAnterioId { get; set; }
        public decimal MontoAnterior { get; set; }
        public string UrlRetorno { get; set; }
    }
}
