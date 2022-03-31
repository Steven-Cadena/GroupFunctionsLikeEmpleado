using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace GroupFunctionsLikeEmpleado
{
    
    public static class Function1
    {
        [FunctionName("functionlikeempleado")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("Aplicación LIKE Empleado");

            string idempleado = req.Query["idempleado"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            idempleado = idempleado ?? data?.idempleado;
            if (idempleado == null) 
            {
                //devolvemos una respuesta de error
                return new BadRequestObjectResult("El ID EMPLEADO es obligatorio");
            }
            //string cadenaconexion = @"Data Source=sqltajamar94.database.windows.net;Initial Catalog=AZURESTEVEN;Persist Security Info=True;User ID=adminsql;Password=Admin123";
            //DEBEMOS RECUPERAR EL FICHERO DE CONFIGURACION POR SU NOMBRE 
            //var config = new ConfigurationBuilder().SetBasePath(context.FunctionAppDirectory)
            //    .AddJsonFile("local.settings.json",optional:true, reloadOnChange: true).AddEnvironmentVariables().Build();
            //string cadenaconexion = config.GetConnectionString("azureSql");
            /*importante*/
            string cadenaconexion = Environment.GetEnvironmentVariable("azureSql");

            string sqlUpdate = "UPDATE EMP SET SALARIO = SALARIO + 1 WHERE EMP_NO=@EMPNO";
            SqlParameter paramId = new SqlParameter("@EMPNO", idempleado);
            SqlCommand com = new SqlCommand
            {
                Connection = new SqlConnection(cadenaconexion),
                CommandType = System.Data.CommandType.Text,
                CommandText = sqlUpdate
            };
            com.Parameters.Add(paramId);
            com.Connection.Open();
            com.ExecuteNonQuery();
            com.Connection.Close();
            com.Parameters.Clear();
            string sqlSelect = "Select * from emp where emp_no=" + idempleado;
            SqlDataAdapter adEmp = new SqlDataAdapter(sqlSelect, cadenaconexion);
            DataTable tabla = new DataTable();
            adEmp.Fill(tabla);
            if (tabla.Rows.Count == 0)
            {
                return new BadRequestObjectResult("El idEmpleado " + idempleado + " no existe.");
            }
            else 
            {
                DataRow fila = tabla.Rows[0];
                string mensaje = "El empleado " + fila["APELLIDO"]
                    + " con oficio " + fila["OFICIO"]
                    + " ha incrementado su salario a "
                    + fila["SALARIO"];
                return new OkObjectResult(mensaje);
            }
        }
    }
}
