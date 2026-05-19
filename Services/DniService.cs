// Services/DniService.cs
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml;
using CURSO_INTERCULTURALIDAD.Models;
namespace CURSO_INTERCULTURALIDAD.Services
{
    public class DniService : IDniService
    {
        private readonly ApiConfig _apiConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DniService> _logger;

        public DniService(IOptions<ApiConfig> apiConfig, IHttpClientFactory httpClientFactory, ILogger<DniService> logger)
        {
            _apiConfig = apiConfig.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(string nombres, string apellidos)> ConsultarDniAsync(string dni)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Configurar timeout
                client.Timeout = TimeSpan.FromSeconds(30);

                // ETAPA 1: OBTENER TOKEN
                var token = await ObtenerTokenAsync(client);
                _logger.LogInformation("Token obtenido exitosamente");

                // ETAPA 2: CONSULTAR DNI
                var datos = await ConsultarDniConTokenAsync(client, dni, token);
                _logger.LogInformation("DNI consultado exitosamente: {Dni}", dni);

                return datos;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión al consultar DNI: {Dni}", dni);
                throw new Exception("Error de conexión con el servicio de consulta DNI");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout al consultar DNI: {Dni}", dni);
                throw new Exception("Tiempo de espera agotado al consultar el DNI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al consultar DNI: {Dni}", dni);
                throw;
            }
        }

        private async Task<string> ObtenerTokenAsync(HttpClient client)
        {
            var xmlBodyToken = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <GeneraToken xmlns=""http://insnsb.gob.pe/"">
      <tipodocusuario>1</tipodocusuario>
      <usuario>{_apiConfig.Usuario}</usuario>
      <clave>{_apiConfig.Clave}</clave>
    </GeneraToken>
  </soap:Body>
</soap:Envelope>";

            var content = new StringContent(xmlBodyToken, Encoding.UTF8, "text/xml");

            // Agregar headers SOAP específicos
            content.Headers.Add("SOAPAction", "http://insnsb.gob.pe/GeneraToken");

            var response = await client.PostAsync(_apiConfig.EndpointToken, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error {(int)response.StatusCode} al obtener token: {response.ReasonPhrase}");
            }

            var xmlText = await response.Content.ReadAsStringAsync();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            var tokenNodes = xmlDoc.GetElementsByTagName("string");
            if (tokenNodes.Count < 3)
                throw new Exception("No se pudo obtener token válido - estructura de respuesta incorrecta");

            var token = tokenNodes[2]?.InnerText?.Trim();

            if (string.IsNullOrEmpty(token))
                throw new Exception("Token obtenido está vacío o es inválido");

            return token;
        }

        private async Task<(string nombres, string apellidos)> ConsultarDniConTokenAsync(HttpClient client, string dni, string token)
        {
            var xmlBodyConsulta = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <ConsultaReniec xmlns=""http://insnsb.gob.pe/"">
      <idtipodocumentou>1</idtipodocumentou>
      <nrodocumentou>{_apiConfig.Usuario}</nrodocumentou>
      <clave>{_apiConfig.Clave}</clave>
      <token>{token}</token>
      <tipodocumento>1</tipodocumento>
      <nrodocumento>{dni}</nrodocumento>
      <idAplicativo>4</idAplicativo>
      <moduloAplicativo>ConsultaReniec</moduloAplicativo>
      <urlAplicativo>PIDE</urlAplicativo>
      <ipConsultante>10</ipConsultante>
      <idUsuarioFinal>1</idUsuarioFinal>
    </ConsultaReniec>
  </soap:Body>
</soap:Envelope>";

            var content = new StringContent(xmlBodyConsulta, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://insnsb.gob.pe/ConsultaReniec");

            var response = await client.PostAsync(_apiConfig.EndpointToken, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error {(int)response.StatusCode} al consultar DNI: {response.ReasonPhrase}");
            }

            var xmlText = await response.Content.ReadAsStringAsync();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            var respuestaNode = xmlDoc.GetElementsByTagName("wsRespuesta")[0];
            if (respuestaNode == null)
                throw new Exception("Respuesta inválida del servicio - no se encontró wsRespuesta");

            var datosStr = respuestaNode.InnerText;
            var datosArray = datosStr.Split('|');

            if (datosArray.Length < 4)
                throw new Exception("Formato de respuesta inválido - datos insuficientes");

            var nombres = datosArray[3]?.Trim() ?? "";
            var apellidoPaterno = datosArray[1]?.Trim() ?? "";
            var apellidoMaterno = datosArray[2]?.Trim() ?? "";

            if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidoPaterno))
                throw new Exception("Datos incompletos en la respuesta del servicio");

            return (nombres, $"{apellidoPaterno} {apellidoMaterno}".Trim());
        }
    }
}