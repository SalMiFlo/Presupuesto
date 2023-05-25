using AutoMapper;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();
        }
    }
}
