namespace Presupuesto.Servicios
{
    public interface IRepositorioServiciosUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class RepositorioServiciosUsuarios: IRepositorioServiciosUsuarios
    {
        public int ObtenerUsuarioId()
        {
            return 1;
        }

    }
}
