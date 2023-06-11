using System.Security.Claims;

namespace Presupuesto.Servicios
{
    public interface IRepositorioServiciosUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class RepositorioServiciosUsuarios: IRepositorioServiciosUsuarios
    {
        private readonly HttpContext httpContext;
        public RepositorioServiciosUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            //return 1;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClain = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClain.Value);
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no esta autenticado");
            }
        }

    }
}
