using System;
using System.Collections.Generic;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

//using System.Web.Services;
//using System.Text;



namespace GlFactura{
public class Utilidades    {

protected static void generaLog(string textoLog){
//File.AppendAllText(FLog, textoLog + Environment.NewLine);
}

public static string[] cortarEspacios(string texto, int maxLon) {
    // divide una cadena en varias lineas desde el utimo espcio --
    // Evita cortar ultimo espacio --
    //// maxLon --> longitud de tamaño de las subcadenas a generar --
    // aqui, mejorar: solo funciona con 3 lineas, 

    // ref: usando un string de tamaño fijo --
    /*   //string[] resul = new string[] { "Uno", "Dos"};
    string[] resul = new string[2];
    resul[0] = texto;
    resul[1] = "Segunda Linea";*/
    
    // usando lista y transformandola despues a array, no requiere tamaño fijo  --
    List<string> termsList = new List<string>();
    if (texto.Length <= maxLon) 
        termsList.Add(texto);
    // si sobrepasa la longuitud, genera 2 lineas --
    else {                  
        // busca el primer espacio de la primera cadena por la derecha --
        //busca el primer espacio en el texto por la izquierda a partir de la maxima longitud a cortar 
        int espacio1 = texto.LastIndexOf(' ', maxLon);
        termsList.Add(texto.Substring(0, espacio1)); // texto HASTA el espacio, lin 1 --
        //termsList.Add(texto.Substring(espacio1)); // texto DESDE el espacio, lin 2--        
        // repite el proceso con la segunda linea (esto es una chapuza) --
        string texto2 = texto.Substring(espacio1);  // texto DESDE el espacio, lin 2--
        if (texto2.Length <= maxLon)  // si sobrepasa la longuitud, genera 3 lineas --
            termsList.Add(texto2);        
        else { 
            espacio1 = texto2.LastIndexOf(' ', maxLon);
            termsList.Add(texto2.Substring(0, espacio1)); // texto HASTA el espacio, lin 2 --   
            termsList.Add(texto2.Substring(espacio1)); // texto DESDE el espacio, lin 3--     
        }                  
    }
        
    string[] terms = termsList.ToArray(); // pasa lista a array --

    return terms;
} // cortarEspacios --


public static bool IsNumeric(object Expression){
    bool esnumero;
    double returnNumero;

    esnumero = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any,
        System.Globalization.NumberFormatInfo.InvariantInfo, out returnNumero);
    return esnumero;
}

public static bool IsNumeric_reg(string value){
    return Regex.IsMatch(value, "^\\d+$");
}

public static bool esEntero(string text){
    int i;
    if (int.TryParse(text, out i))
        return true;
    else
        return false;
}// esEntero --

public static string CvNumero(string value){
    if (string.IsNullOrEmpty(value))
        return "0";
    return double.Parse(value).ToString(CultureInfo.CreateSpecificCulture("en-GB")); // cambia de "," decimal a ".", sin cambiar antes  de cultura --             
}// CvNumero --
    
public static DataTable GetDataTbl(string query, out int Nreg ){
    //ref:https://code.msdn.microsoft.com/How-to-Create-and-Execute-86922261
    //  http://hermosaprogramacion.blogspot.com.es/2014/07/sql-server-c-sharp-conectar-como.html#
    using (SqlConnection conex1 = new SqlConnection(WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString))       {
        using (SqlCommand cmd = new SqlCommand())        {
            cmd.CommandText = query;
            using (SqlDataAdapter sda = new SqlDataAdapter())            {
                cmd.Connection = conex1;
                sda.SelectCommand = cmd;
                using (DataTable dt = new DataTable())                {
                    sda.Fill(dt);
                    Nreg = dt.Rows.Count; // retorna nº total de reg. --
                    return dt;
                }
            }
        }
    }
}// GetDataTbl --

public static SqlDataReader GetDataLector(String commandText){    
    //SqlConnection conn = new SqlConnection(((SqlConnection)Session["ConexGAG"]).ConnectionString);  
    SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString);
    using (SqlCommand cmd = new SqlCommand(commandText, conn))    {
        //cmd.CommandType = commandType;
        //cmd.Parameters.AddRange(parameters); 
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        return reader;
    }
} // GetDataLector --

public  static int recStockLocal(string articulo, string ntrabajo){
    //Recupera stock de un Articulo y/o Lote (Nº de trabajo )--
    string linQry1 = "SELECT *, Entradas-Salidas Stock FROM ( " +
    " SELECT  sum(1) NLineas,SUM (CASE WHEN Tipo = 1 THEN Cantidad ELSE 0 END ) Entradas,  SUM (CASE WHEN Tipo <> 1 THEN Cantidad ELSE 0 END ) Salidas " +
    " FROM  PedidosDetalles  Det  WHERE IdArticulo = " + articulo;
    if (ntrabajo !="")
        linQry1 += "  AND idNtrabajo = '" + ntrabajo + "'";
    linQry1 +=")  a";
    SqlDataReader reader = Utilidades.GetDataLector(linQry1);    
    if (reader.HasRows)
        if (reader.Read())
            if (reader["Stock"].ToString() != "")
                return (int)reader["Stock"];

    return 0;
} // recStock --

public static string GenerarAlbaran(string idPedido){
    // Genera albaran a partir del envio. Cabecera y detalles -     
    // ref: http://msdn.microsoft.com/es-es/library/ms175010.aspx 
    //using (SqlConnection conex1 = new SqlConnection(((SqlConnection)Session["ConexGAG"]).ConnectionString))    {
    using (SqlConnection conex1 = new SqlConnection(WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString))    {
        using (SqlCommand cmd = new SqlCommand("", conex1))        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.sp_CreaAlbaranPedido";
            cmd.Parameters.AddWithValue("@IdPedido", idPedido); // Nº de pedido         
            cmd.Parameters.Add(new SqlParameter("@idNuevo", SqlDbType.Int));
            cmd.Parameters["@idNuevo"].Direction = ParameterDirection.Output;
            conex1.Open();
            cmd.ExecuteScalar();
            // Recupera valor del insertado O modificado --        
            // lbIdSel.InnerHtml = cmd.Parameters["@idNuevo"].Value.ToString();    
            return cmd.Parameters["@idNuevo"].Value.ToString();
        }// using (SqlCommand  --
    }
}// GenerarAlbaran --



}// public class Utilidades
}// namespace AeaStock --