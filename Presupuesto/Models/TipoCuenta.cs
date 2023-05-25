using Microsoft.AspNetCore.Mvc;
using Presupuesto.Validaciones;
using System.ComponentModel.DataAnnotations; //namespace para la validación

namespace Presupuesto.Models
{
    public class TipoCuenta//: IValidatableObject (PARA VALIDACIONES A NIVEL DEL MODELO)
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote(action:"Verificar", controller:"TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Nombre != null & Nombre!.Length > 0)
        //    {
        //        var primeraletra = Nombre[0].ToString();
        //        if (primeraletra != primeraletra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayuscula", new[] { nameof(Nombre) });
        //        }
        //    }
        //}
    }
}
