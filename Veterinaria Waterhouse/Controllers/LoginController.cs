using Microsoft.AspNetCore.Mvc;
using System.Text;
using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Net;
using System.Net.Mail;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{
    public class LoginController : Controller

    {
        [HttpGet]
        public IActionResult ValidarRespuesta()
        {
            return View();
        }


        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";
        private static string codigoVerificacion = "";
        private static string usuarioCorreo = "";
        private static string usuarioRecuperacion = ""; // ← Línea agregada para evitar errores
        private static string preguntaRecuperacion = "";
        private static string respuestaCorrecta = "";



        // Contador de intentos fallidos por usuario (almacenado temporalmente)
        private static Dictionary<string, int> intentosFallidos = new Dictionary<string, int>();


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string password)

        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View();
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Verificar si el usuario está bloqueado
                    // Verificar si el usuario está bloqueado
                    string checkBlockQuery = "SELECT COUNT(*) FROM usuarios_bloqueados WHERE usuario = @Usuario";
                    using (MySqlCommand checkBlockCmd = new MySqlCommand(checkBlockQuery, conn))
                    {
                        checkBlockCmd.Parameters.AddWithValue("@Usuario", usuario);
                        int isBlocked = Convert.ToInt32(checkBlockCmd.ExecuteScalar());

                        if (isBlocked > 0)
                        {
                            ViewBag.Error = "❌ Tu cuenta ha sido bloqueada. <a href='/Login/RecuperarCuenta'>Recupérala aquí</a>";
                            return View();
                        }
                    }


                    // Validar credenciales
                    string query = "SELECT correo, identificacion FROM clientes WHERE usuario = @Usuario AND contrasena = @Contrasena";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", usuario);
                        cmd.Parameters.AddWithValue("@Contrasena", EncriptarTexto(password));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Usuario válido
                            {
                                usuarioCorreo = reader["correo"].ToString();
                                string identificacionUsuario = reader["identificacion"].ToString();

                                Console.WriteLine($"✅ Correo del usuario: {usuarioCorreo}");

                                TempData["Usuario"] = usuarioCorreo;

                                codigoVerificacion = GenerarCodigo();
                                Console.WriteLine($"🔹 Código generado: {codigoVerificacion}");

                                bool correoEnviado = EnviarCorreo(usuarioCorreo, codigoVerificacion);

                                if (correoEnviado)
                                {
                                    Console.WriteLine("✅ Correo enviado correctamente.");

                                    // Reiniciar contador de intentos fallidos al éxito
                                    if (intentosFallidos.ContainsKey(usuario))
                                    {
                                        intentosFallidos[usuario] = 0;


                                    }

                                    return RedirectToAction("Codigo");
                                }
                                else
                                {
                                    ViewBag.Error = "❌ No se pudo enviar el correo. Verifique la configuración SMTP.";
                                    return View();
                                }
                            }
                        }
                    }

                    // Si llega aquí, significa que las credenciales son incorrectas
                    if (!intentosFallidos.ContainsKey(usuario))
                    {
                        intentosFallidos[usuario] = 0;
                    }

                    intentosFallidos[usuario]++;

                    if (intentosFallidos[usuario] >= 3)
                    {
                        // Bloquear usuario
                        BloquearUsuario(usuario);
                        ViewBag.Error = "❌ Has excedido el límite de intentos. Tu cuenta ha sido bloqueada.";
                    }
                    else
                    {
                        ViewBag.Error = $"❌ Usuario o contraseña incorrectos. Intentos restantes: {3 - intentosFallidos[usuario]}";
                    }

                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "❌ Error al conectar con la base de datos: " + ex.Message;
                Console.WriteLine($"❌ Error BD: {ex.Message}");
                return View();
            }
        }

        private void BloquearUsuario(string usuario)
        {

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Obtener la identificación del usuario antes de bloquearlo
                    string getUserInfoQuery = "SELECT contrasena, identificacion FROM clientes WHERE usuario = @Usuario";
                    string contrasenaUsuario = "";
                    string identificacionUsuario = "";

                    using (MySqlCommand getUserCmd = new MySqlCommand(getUserInfoQuery, conn))
                    {
                        getUserCmd.Parameters.AddWithValue("@Usuario", usuario);
                        using (MySqlDataReader reader = getUserCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                contrasenaUsuario = reader["contrasena"].ToString();
                                identificacionUsuario = reader["identificacion"].ToString();
                            }
                        }
                    }

                    // Insertar en la tabla de usuarios bloqueados
                    string blockUserQuery = "INSERT INTO usuarios_bloqueados (usuario, contrasena, identificacion) VALUES (@Usuario, @Contrasena, @Identificacion)";
                    using (MySqlCommand blockUserCmd = new MySqlCommand(blockUserQuery, conn))
                    {
                        blockUserCmd.Parameters.AddWithValue("@Usuario", usuario);
                        blockUserCmd.Parameters.AddWithValue("@Contrasena", contrasenaUsuario);
                        blockUserCmd.Parameters.AddWithValue("@Identificacion", identificacionUsuario);

                        blockUserCmd.ExecuteNonQuery();
                    }

                    Console.WriteLine($"❌ Usuario {usuario} bloqueado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al bloquear usuario: {ex.Message}");
            }
        }

        public IActionResult Codigo()
        {
            Console.WriteLine("🔹 Redirigiendo a la vista Codigo...");
            return View("~/Views/Login/Codigo.cshtml");
        }

        [HttpPost]
        public IActionResult Codigo(string codigoIngresado)
        {
            string usuarioCorreo = TempData["Usuario"]?.ToString();
            if (codigoIngresado == codigoVerificacion)
            {
                Console.WriteLine("✅ Código correcto. Redirigiendo al menú...");
                HttpContext.Session.SetString("Usuario", usuarioCorreo);
                return RedirectToAction("Menu", "Menu");
            }
            else
            {
                ViewBag.Error = "❌ Código incorrecto. Inténtalo nuevamente.";
                return View("~/Views/Login/Codigo.cshtml");
            }
        }

        private string EncriptarTexto(string texto)
        {
            // Asegurarse de limpiar el texto
            texto = texto.Trim();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                // Convertir el arreglo de bytes a una cadena hexadecimal
                StringBuilder hex = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    hex.AppendFormat("{0:x2}", b);
                }

                return hex.ToString();
            }
        }

        private string GenerarCodigo()
        {
            Random random = new Random();
            int longitud = random.Next(4, 7);
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] codigo = new char[longitud];

            for (int i = 0; i < longitud; i++)
            {
                codigo[i] = caracteres[random.Next(caracteres.Length)];
            }

            return new string(codigo);
        }

        private bool EnviarCorreo(string correoDestino, string codigo)
        {
            try
            {
                string fromEmail = "josuewaterhouse99@gmail.com";
                string fromPassword = "uxjz mqwf rxml alfd";

                var fromAddress = new MailAddress(fromEmail, "Veterinaria Waterhouse");
                var toAddress = new MailAddress(correoDestino);
                const string subject = "Código de Verificación";
                string body = $"Tu código de verificación es: {codigo}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public IActionResult RecuperarCuenta()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult RecuperarCuenta(string usuario)
        {
            Console.WriteLine($"🔹 Usuario ingresado: {usuario}");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 🔹 Buscar la identificación del usuario en la tabla CLIENTES
                    string queryCliente = "SELECT identificacion FROM clientes WHERE correo = @Usuario";
                    using (MySqlCommand cmdCliente = new MySqlCommand(queryCliente, conn))
                    {
                        cmdCliente.Parameters.AddWithValue("@Usuario", usuario);
                        object result = cmdCliente.ExecuteScalar();

                        if (result != null)
                        {
                            usuarioRecuperacion = result.ToString(); // 🔹 GUARDAMOS LA IDENTIFICACIÓN GLOBALMENTE
                            Console.WriteLine($"✅ Identificación guardada: {usuarioRecuperacion}");
                        }
                        else
                        {
                            Console.WriteLine("❌ Usuario no encontrado en clientes.");
                            ViewBag.Error = "❌ Usuario no encontrado.";
                            return View();
                        }
                    }

                    // 🔹 Buscar las preguntas de validación con la IDENTIFICACIÓN en la tabla VALIDACION
                    string queryValidacion = "SELECT mascota, cantante, materia FROM validacion WHERE identificacion = @Identificacion";

                    using (MySqlCommand cmdValidacion = new MySqlCommand(queryValidacion, conn))
                    {
                        cmdValidacion.Parameters.AddWithValue("@Identificacion", usuarioRecuperacion);
                        using (MySqlDataReader reader = cmdValidacion.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 🔹 Elegir una pregunta aleatoria
                                Random rnd = new Random();
                                int preguntaIndex = rnd.Next(0, 3);  // Índices: 0, 1, 2

                                string[] columnasPreguntas = { "mascota", "cantante", "materia" };
                                string[] textosPreguntas = {
                            "¿Cuál es el nombre de tu mascota?",
                            "¿Cuál es tu cantante favorito?",
                            "¿Cuál es tu materia favorita?"
                        };

                                preguntaRecuperacion = textosPreguntas[preguntaIndex];
                                respuestaCorrecta = reader[columnasPreguntas[preguntaIndex]].ToString();

                                Console.WriteLine($"🔹 Pregunta seleccionada: {preguntaRecuperacion}");

                                TempData["Pregunta"] = preguntaRecuperacion;
                                TempData["Identificacion"] = usuarioRecuperacion; // 🔹 GUARDAMOS LA IDENTIFICACIÓN EN TEMPDATA
                                return RedirectToAction("ValidarRespuesta");
                            }
                        }
                    }

                    Console.WriteLine("❌ No se encontraron respuestas de validación para este usuario.");
                    ViewBag.Error = "❌ No se encontraron preguntas de seguridad para este usuario.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en RecuperarCuenta: {ex.Message}");
                ViewBag.Error = "❌ Ocurrió un error. Intenta de nuevo.";
                return View();
            }
        }

        private string DesencriptarTexto(string textoEncriptado)
        {
            try
            {
                byte[] bytesEncriptados = Convert.FromBase64String(textoEncriptado);
                return Encoding.UTF8.GetString(bytesEncriptados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al desencriptar: {ex.Message}");
                return "";
            }
        }



        [HttpPost]
        [HttpPost]
        public IActionResult ValidarRespuesta(string respuesta, string identificacion)
        {
            try
            {
                Console.WriteLine($"🔹 Respuesta ingresada: {respuesta}");
                Console.WriteLine($"🔹 Identificación recibida: {identificacion}");

                // Recuperar la pregunta de TempData
                string pregunta = TempData["Pregunta"]?.ToString();

                // Si la pregunta no existe en TempData, significa que algo falló anteriormente.
                if (string.IsNullOrEmpty(pregunta))
                {
                    ViewBag.Error = "❌ No se encontró la pregunta de seguridad. Intenta nuevamente.";
                    return View("ValidarRespuesta");
                }

                Console.WriteLine($"🔹 Pregunta recuperada: {pregunta}");

                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Consulta para obtener las respuestas encriptadas según la identificación
                    string query = "SELECT mascota, cantante, materia FROM validacion WHERE identificacion = @Identificacion";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Identificacion", identificacion);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Recuperar las respuestas encriptadas de la base de datos
                                string mascotaHash = reader["mascota"]?.ToString();
                                string cantanteHash = reader["cantante"]?.ToString();
                                string materiaHash = reader["materia"]?.ToString();

                                Console.WriteLine($"🔹 Datos leídos de la BD:");
                                Console.WriteLine($"   - Mascota: {mascotaHash}");
                                Console.WriteLine($"   - Cantante: {cantanteHash}");
                                Console.WriteLine($"   - Materia: {materiaHash}");

                                // Encriptar la respuesta del usuario para la comparación
                                string respuestaEncriptada = EncriptarTexto(respuesta.Trim());
                                Console.WriteLine($"🔹 Respuesta encriptada del usuario: {respuestaEncriptada}");

                                // Determinar qué respuesta comparar según la pregunta seleccionada
                                string hashCorrecto = null;

                                if (pregunta == "¿Cuál es el nombre de tu mascota?")
                                    hashCorrecto = mascotaHash;
                                else if (pregunta == "¿Cuál es tu cantante favorito?")
                                    hashCorrecto = cantanteHash;
                                else if (pregunta == "¿Cuál es tu materia favorita?")
                                    hashCorrecto = materiaHash;

                                if (hashCorrecto != null)
                                {
                                    // Imprimir para depurar la comparación
                                    Console.WriteLine($"🔹 Hash correcto de la base de datos: {hashCorrecto}");

                                    // Comparar la respuesta encriptada con la base de datos
                                    if (respuestaEncriptada == hashCorrecto)
                                    {
                                        Console.WriteLine("✅ Respuesta correcta.");
                                        return View("~/Views/Login/CambiarContrasena.cshtml");
                                    }
                                    else
                                    {
                                        Console.WriteLine("❌ La respuesta encriptada no coincide con la de la base de datos.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("❌ No se encontró un hash correcto para la pregunta.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("❌ No se encontraron datos de validación para la identificación.");
                            }
                        }
                    }
                }

                // Si la respuesta no es correcta, volver a mostrar la pregunta y error
                ViewBag.Error = "❌ Respuesta incorrecta.";
                TempData["Pregunta"] = pregunta; // Aseguramos que la pregunta se mantiene para reintentar
                return View("ValidarRespuesta");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ValidarRespuesta: {ex.Message}");
                ViewBag.Error = "❌ Ocurrió un error. Intenta de nuevo.";
                return View("ValidarRespuesta");
            }
        }





        [HttpPost]
        [HttpPost]
        public IActionResult CambiarContrasena(string nuevaContrasena)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioRecuperacion))
                {
                    usuarioRecuperacion = TempData["Identificacion"] as string; // 🔹 Recuperamos el valor si se perdió
                }

                Console.WriteLine($"🔹 Iniciando cambio de contraseña para usuario: {usuarioRecuperacion}");

                if (string.IsNullOrEmpty(usuarioRecuperacion))
                {
                    ViewBag.Error = "❌ Error: No se pudo recuperar el usuario.";
                    return View("CambiarContrasena");
                }

                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 🔹 ACTUALIZAR CONTRASEÑA EN CLIENTES
                    string updateQuery = "UPDATE clientes SET contrasena = @NuevaContrasena WHERE identificacion = @Identificacion";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@NuevaContrasena", EncriptarTexto(nuevaContrasena));
                        cmd.Parameters.AddWithValue("@Identificacion", usuarioRecuperacion);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            ViewBag.Error = "❌ No se pudo actualizar la contraseña.";
                            return View("CambiarContrasena");
                        }
                    }

                    Console.WriteLine("✅ Contraseña actualizada correctamente.");

                    // 🔹 ELIMINAR USUARIO DE `usuarios_bloqueados`
                    string deleteQuery = "DELETE FROM usuarios_bloqueados WHERE identificacion = @Identificacion";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Identificacion", usuarioRecuperacion);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("✅ Usuario eliminado de usuarios_bloqueados.");
                }

                ViewBag.Success = "✅ Contraseña cambiada exitosamente. Puedes iniciar sesión.";
                return View("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CambiarContrasena: {ex.Message}");
                ViewBag.Error = "❌ Ocurrió un error al cambiar la contraseña.";
                return View("CambiarContrasena");
            }
        }


    }
}
