using System.Text.Json.Serialization;

namespace CURSO_INTERCULTURALIDAD.Models
{
    public sealed class PagoIzipayOptions
    {
        public string CrearPagoUrl { get; set; } = string.Empty;
        public string EstadoPagoUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DireccionPredeterminada { get; set; } = "NO REGISTRADA";
        public string CiudadPredeterminada { get; set; } = "Lima";
        public string DepartamentoPredeterminado { get; set; } = "Lima";
        public string PaisPredeterminado { get; set; } = "PE";
        public string PostalCodePredeterminado { get; set; } = "15001";
        public string DescripcionPredeterminada { get; set; } = "Pago por servicios";
        public bool PermitirCertificadoInseguro { get; set; }
    }

    public sealed class SolicitudCrearPagoIzipay
    {
        public string IdReferencia { get; init; } = string.Empty;
        public string CodigoExterno { get; init; } = string.Empty;
        public string TituloPasarela { get; init; } = string.Empty;
        public string InformacionAdicional { get; init; } = string.Empty;
        public string InformacionExtra { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public string Direccion { get; init; } = string.Empty;
        public string Ciudad { get; init; } = string.Empty;
        public string Departamento { get; init; } = string.Empty;
        public string Pais { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public decimal Monto { get; init; }
        public string TipoDocumento { get; init; } = string.Empty;
        public string NroDocumento { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        [JsonPropertyName("CodigoCPMS")]
        public string CodigoCPMS { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
    }

    public sealed class RespuestaCrearPagoIzipay
    {
        public bool Exitoso { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public string? UrlPago { get; init; }
        public long? IdPagoIziPay { get; init; }
        public string? CodigoError { get; init; }
    }

    public sealed class RespuestaEstadoPagoIzipay
    {
        public bool Exitoso { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public long? IdPagoIziPay { get; init; }
        public int? EstadoGeneral { get; init; }
        public string Estado { get; init; } = string.Empty;
        public bool YaPago { get; init; }
        public string? CodigoAutorizacion { get; init; }
        public string? Cip { get; init; }
        public string? NumeroOrden { get; init; }
        public string? NumeroTransaccion { get; init; }
        public DateTime? FechaCreacion { get; init; }
        public string? NumeroComprobante { get; init; }
        public string? UrlComprobantePdf { get; init; }
        public string? CodigoError { get; init; }
    }
}
