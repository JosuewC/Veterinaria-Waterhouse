using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{
    public class RegistroController : Controller
    {
        private readonly HttpClient _httpClient;

        public RegistroController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Acción para mostrar la vista RegistroC
        public IActionResult RegistroC()
        {
            return View(); // Esto muestra la vista RegistroC.cshtml
        }

        // Acción para registrar el cliente y consumir la API de Node.js
        [HttpPost]
        public async Task<IActionResult> RegistrarCliente(Cliente cliente, Validacion validacion)
        {
            if (cliente == null || validacion == null ||
                string.IsNullOrEmpty(cliente.Correo) ||
                string.IsNullOrEmpty(cliente.Contrasena) ||
                string.IsNullOrEmpty(cliente.Identificacion))
            {
                return Json(new { success = false, message = "Datos inválidos. Complete todos los campos." });
            }

            try
            {
                // 1. Buscar en registro_civil
                var apiUrlBuscar = $"http://localhost:3002/buscar/{cliente.Identificacion}";
                var buscarResponse = await _httpClient.GetAsync(apiUrlBuscar);

                string nombreUsuario = "";

                if (buscarResponse.IsSuccessStatusCode)
                {
                    var buscarUsuario = await buscarResponse.Content.ReadAsStringAsync();
                    dynamic usuario = JsonConvert.DeserializeObject(buscarUsuario);
                    nombreUsuario = usuario.nombre;
                }
                else
                {
                    // Validar que haya un nombre proporcionado
                    if (string.IsNullOrWhiteSpace(cliente.Nombre))
                    {
                        return Json(new { success = false, message = "Debe ingresar el nombre si no está registrado en el sistema." });
                    }

                    // 2. Registrar en registro_civil
                    var registroCivilData = new
                    {
                        identificacion = cliente.Identificacion,
                        nombre = cliente.Nombre
                    };

                    var jsonRegistro = JsonConvert.SerializeObject(registroCivilData);
                    var contenido = new StringContent(jsonRegistro, Encoding.UTF8, "application/json");

                    var registroResponse = await _httpClient.PostAsync("http://localhost:3002/registrar", contenido);

                    if (!registroResponse.IsSuccessStatusCode &&
                        registroResponse.StatusCode != System.Net.HttpStatusCode.Conflict)
                    {
                        var errorMsg = await registroResponse.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = "Error al registrar en registro civil: " + errorMsg });
                    }

                    nombreUsuario = cliente.Nombre;
                }

                // 3. Asignar nombre confirmado
                cliente.Nombre = nombreUsuario;

                // 4. Registrar en el sistema principal
                var requestData = new
                {
                    nombre = cliente.Nombre,
                    identificacion = cliente.Identificacion,
                    celular = cliente.Celular,
                    correo = cliente.Correo,
                    usuario = cliente.Usuario,
                    contrasena = cliente.Contrasena,
                    mascota = validacion.Pregunta1,
                    cantante = validacion.Pregunta2,
                    materia = validacion.Pregunta3
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:3000/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorApi = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Error al registrar cliente: " + errorApi });
                }

                return Json(new { success = true, message = "Verificación enviada al correo", redirectUrl = "/Registro/RegistroC" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno: " + ex.Message });
            }
        }



    }
}


