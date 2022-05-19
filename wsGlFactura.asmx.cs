/* Constantes switch, activa o desactiva funciones --
    REVISAR ANTES DE PUBLICAR EN PRODUCCION 
*/




//  SI Marca factura con fecha contabilización y la sube al FTP --
#define MARCARFACTURA
//#undef MARCARFACTURA

// crea Lineas de contabilidad en la BD ORACLE
#define CREARCONTABLE
//#undef CREARCONTABLE

// genera fichero de excel de referencia con las mismas lineas contables de la BD, NO necesario, se usa para debug --
//#define CREAREXCELCSV

//#define ORCL_DESARROLLO


// Base de datos a usar ---------------------------

//#define DESARROLLO
#define PROD
//#define LOCAL


// -- antiguas funciones, mantener ya desactivadas 
//#define INSERTARIMAGENENBD



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web.Script.Serialization;

using Oracle.ManagedDataAccess.Client; // nueva Instado paquete ODTx86 con nuget --

using System.Text;
using System.Globalization; 
using System.IO;

using iTextSharp.text;
using iTextSharp.text.pdf;

using System.Data.Linq;
using System.Data.Linq.Mapping;

using System.Diagnostics;
using Newtonsoft.Json.Linq;

// para el ftp Renci.SshNet.dll 
using Renci.SshNet;
// para ftp normal --
using System.Net;
using System.Web.Hosting;

using System.Net.Mail;



namespace GlFactura {

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]


    public class wsGlFactura : System.Web.Services.WebService {
        
       
        #if (DESARROLLO)
            static string Conex = WebConfigurationManager.ConnectionStrings["GAG_DES"].ConnectionString;        
        #endif

        #if (PROD)
            static string Conex = WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString;
        #endif
        
        #if (LOCAL)
             static string Conex = WebConfigurationManager.ConnectionStrings["GAG_LOCAL"].ConnectionString;        
        #endif
                
        #if (ORCL_DESARROLLO)
            static string ConexORCL = WebConfigurationManager.ConnectionStrings["CONEX_CONTABLE_DES"].ConnectionString;
        #else
            static string ConexORCL = WebConfigurationManager.ConnectionStrings["CONEX_CONTABLE"].ConnectionString;
        #endif

        static string ConexGlDistri = WebConfigurationManager.ConnectionStrings["GLDISTRI"].ConnectionString;

/* ref: array creado literalmente, 
   requiere clase tiposOrden  --
    public class tiposOrden { public int id { get; set; }    public string Tx { get; set; }  }    

      No puede usarse, no indexa por campo id
 */
tiposOrden[] OrdenListaCliente = new tiposOrden[] {
      new tiposOrden { id = 1, Tx = "Cod. Descendente" }
    , new tiposOrden { id = 10,Tx = "Cod. Ascendente" }
    , new tiposOrden { id = 2, Tx = "Cadena Ascendente" }
    , new tiposOrden { id = 20,Tx = "Cadena Descendente" }
    , new tiposOrden { id = 3, Tx = "Codogo SAP" }
    , new tiposOrden { id = 4, Tx = "NIF" }
    , new tiposOrden { id = 5, Tx = "Razon Social" }
    , new tiposOrden { id = 6, Tx = "Poblacion Ascendente" }
    , new tiposOrden { id = 60,Tx = "Poblacion Descendente" }
    , new tiposOrden { id = 7, Tx = "Provincia Ascendente" }
    , new tiposOrden { id = 70,Tx = "Provincia Descendente" }
    , new tiposOrden { id = 8, Tx = "Nombre Comercial" }
    , new tiposOrden { id = 9, Tx = "Anulado" }
    , new tiposOrden { id = 11,Tx = "Del Grupo" }
};
        
           
[WebMethod]
public void enviarCorreo() {
    // prueba de envío de correo --
    
    // -- USANDO servidor de Gmail (ok) --
    string Host = "smtp.gmail.com";
    string From = "lopezygomez@gmail.com";
    int Port = 587;
    string Destino = "plopez@globalia-sistemas.com";

    /*-- USANDO servidor de Globalia (NO FUNCIONA) --
    string Host = "smtp.office365.com";
    string From = "plopez@globalia-sistemas.com";
    int Port = 587;
    string Destino = "lopezygomez@gmail.com";
    */
    
    string msgAsunto = "Asunto: Prueba envio correo c# 5";
    string msgCuerpo = "Cuerpo mensaje: Prueba envio correo c# 5";
    string msgCuerpoHtml = "<h1> Cuerpo mensaje con HTML  </h1> "+
                "<br/> <a href='https://www.xataka.com/'> enlace</a>"+
                "<h2> FIN DE MENSAJE HTML 2 </h2>";

    SmtpClient client = new SmtpClient();    
    client.EnableSsl = true;
    client.Timeout = 10000;
    client.DeliveryMethod = SmtpDeliveryMethod.Network;
    client.UseDefaultCredentials = false;
    
    client.Port = Port;
    client.Host = Host;    
    client.Credentials = new System.Net.NetworkCredential("lopezygomez@gmail.com","bi901256");    
    //client.Credentials = new System.Net.NetworkCredential("plopez@globalia-sistemas.com", "901256");
    
    MailMessage msg = new MailMessage();    
    msg.From = new MailAddress(From);
    msg.To.Add(new MailAddress(Destino));
    msg.Subject = msgAsunto;
    // ref: usando html para el body 
    msg.IsBodyHtml = true;   
    msg.Body = msgCuerpoHtml;            

    msg.BodyEncoding = UTF8Encoding.UTF8;    
    msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
    client.Send(msg);
    
}// enviarCorreo


[WebMethod]
public string recDatFacturaJSON(int idFactura, int Ejercicio) {
    // recupera la factura completa, Cabecera, Detalles y costes en objeto tipo datFactura
    datFactura stFactura = new datFactura();

    // Crea la clase con datos de  factura --
    clFactura fact = new clFactura(idFactura, Ejercicio);              
    stFactura.FacCabecera = fact.recCabecera();        
    stFactura.DetFactura = fact.recDetalles();
    stFactura.DetCostes = fact.recDetallesCoste();

    return new JavaScriptSerializer().Serialize(stFactura);
}// recDatFacturaJSON --

[WebMethod]
public string recDetallesFacturaJSON(int idFactura, int Ejercicio) {
    List<DetalleFactura> DetFacturas = new List<DetalleFactura>();    
    // Crea la clase con datos de  factura --
    clFactura fact = new clFactura(idFactura, Ejercicio);      
    DetFacturas = fact.recDetalles();
    // cambia el formato de la lista a string JSON y lo retorna --
    return new JavaScriptSerializer().Serialize(DetFacturas);
} // recDetallesFacturaJSON --

public JObject genJsonDefinicion(string docJson) {
    /* recupera fichero de definion json en texto y retorna objeto Json con su contenido 
     ref: crea objeto json de net (JObject) a partir de fichero de texto --
        añadir referencia Newtonsoft.Json, y using Newtonsoft.Json.Linq;            

        convierte CSV a JSON http://www.convertcsv.com/csv-to-json.htm        
        El formato para contabilizar SAP en Facturas ("FacturasSAP.json"), se ha extraido de HGAF_Origen_JSON.csv                                    
    */
    string docServer= Server.MapPath(docJson);
    
    // lee  fichero json completo y genera objeto json Net (JObject)
    string json;    
    using (StreamReader r = new StreamReader(docServer)) {  json = r.ReadToEnd();  }    
    JObject jsonObj = JObject.Parse(json);

    /* Ref: otras formas de acceso  a datos fichero json de texto 
     
        Resul += " Valor Por nombre:"+ jsonObj["CabeceraFacSAP"]["DESCRIPCIONFACTURA"]; // ok acceso por nombre
        Resul += "\n Detalles 1: " + jsonObj["DetallesFacSAP"][0]; // primer detalle de un array, ok 
          
        //Recorre todos los valores y genera lista con ellos , ok   
        // Todos los valores de CabeceraFacSAP (ok)    
        IList<string> valores = jsonObj["CabeceraFacSAP"].Select(t => (string)t).ToList();
        // Todas las claves del objetos (ok)
        IList<string> keysG = jsonObj.Properties().Select(p => p.Name).ToList();    
        // imprime la lista      
        foreach (string L in keysG) { Resul += " Val:"+L;  } 


        // usando objeto dynamic --
        dynamic dinObj = JObject.Parse(json);
           
        string Resul+= dinObj["campo1"].ToString();
                
        Resul += ", poblacion (formato1) "+ dinObj["CabeceraFacSAP"]["POBLACION"].ToString();
        Resul += ", poblacion " + dinObj.CabeceraFacSAP.POBLACION;
        Resul+= ", IDFISCAL" + dinObj.CabeceraFacSAP.IDFISCAL;      
    */    
    return jsonObj;
}// genJsonDefinicion --

public string stringJson (JToken nodoJs, string separador) {
    // Genera linea de texto con VALORES de un nodo json, separador por un caracter dado 
    string Resul="";
    foreach (JToken child in nodoJs.Children()) {
        if ((JProperty)child != null)
            Resul += ((JProperty)child).Value + separador;// usa el VALOR  JSON
        }      
    return Resul;
}// stringJson --

public string stringJsonProp (JToken nodoJs, string separador) {
    // Genera linea de texto con NOMBRES de un nodo json, separador por un caracter dado 
    string Resul="";
    foreach (JToken child in nodoJs.Children()) {
        if ((JProperty)child != null)       
            Resul += ((JProperty)child).Name + separador; // usa el NOMBRE de la propiedad JSON            
        }      
    return Resul;
}// stringJsonProp --

public void vaciaJson (JToken nodoJs) {
    //  vacia informacion de valores elementos de un nodo json,            
    foreach (JToken child in nodoJs.Children()) {
        if ((JProperty)child != null) {
            var property = child as JProperty;
            property.Value = "";        
        }        
    }          
}// vaciaJson --


 #region Cliente  --------


[WebMethod]
 public string recClientes(selListaClientes Sel, int nPagina, int pageSize) {
    // retorna datos transformados en JSON --
    List<ListaCliente> Lista = recClientesLista(Sel, nPagina, pageSize);    
    return new JavaScriptSerializer().Serialize(Lista); 
}//recClientes


public List<ListaCliente> recClientesLista(selListaClientes Sel, int nPagina, int pageSize) {
    string query = @"WITH 			
		Cade (Codigo, NombreCadena, CadenaActivaCarrito, OfCentalCadena )
		   AS (SELECT  Codigo, isnull(Texto,'') NombreCadena, Importe CadenaActivaCarrito, isnull(Texto3,'') OfCentalCadena FROM Tablas WHERE TipoTabla = 'CADE' ),
		Fpg (Codigo, FormaPago, CtaFormaPago) 
			AS (SELECT Codigo, isnull(Texto,'') FormaPago, isnull(Texto2,'') CtaFormaPago FROM Tablas WHERE TipoTabla = 'FPAG'),
		Tiva (Codigo, TipoIVA, SiglaTipoIVA, ImporteIVA) 
			AS  (SELECT Codigo, isnull(Texto,'') TipoIVA, isnull(Texto2,'') SiglaTipoIVA, Importe ImporteIVA FROM  Tablas WHERE TipoTabla = 'IVA'),
		Pais (Codigo, NombrePais) 
			AS (SELECT Codigo, isnull(Texto,'') NombrePais FROM Tablas WHERE TipoTabla = 'PAIS'),
		Tcl  (Codigo, TipoClienteHotel, CadenaTipoCliente)  
			AS (SELECT Codigo, isnull(Texto,'') TipoClienteHotel, isnull(Importe,0) CadenaTipoCliente FROM Tablas WHERE TipoTabla = 'HTIP'),
		Tprecio (Codigo, TipoPrecio, CadenaTipoPrecio) 
			AS (SELECT Codigo, isnull(Texto,'') TipoPrecio, Importe CadenaTipoPrecio FROM Tablas WHERE TipoTabla = 'TPRE'),
		Marca (Codigo, Marca, CadenaMarca)
			 AS (SELECT Codigo, isnull(Texto,'') Marca, Importe CadenaMarca FROM Tablas WHERE TipoTabla = 'MARC'),        
		TipoDoc (Codigo, NTipoDoc) AS (SELECT 0  ,'Sin asignar'  UNION ALL SELECT 1, 'CIF/NIF' UNION ALL SELECT 2, 'NIE' UNION ALL SELECT 3, 'OTROS')							
	    SELECT  * FROM (SELECT  ROW_NUMBER() OVER ( 					
		    ORDER BY 
			    CASE WHEN @Orden=1 THEN IdCliente END ASC	,			
			    CASE WHEN @Orden=10 THEN IdCliente END DESC	,			
			    CASE WHEN @Orden=2 THEN NombreCadena END ASC,	
			    CASE WHEN @Orden=20 THEN NombreCadena END DESC,
			    CASE WHEN @Orden=3 THEN idSap END ASC,
			    CASE WHEN @Orden=4 THEN NIF END ASC,
			    CASE WHEN @Orden=5 THEN TITULO END ASC,			
			    CASE WHEN @Orden=6 THEN Poblacion END ASC,
			    CASE WHEN @Orden=60 THEN Poblacion END DESC,
			    CASE WHEN @Orden=7 THEN Provincia END ASC,
			    CASE WHEN @Orden=70 THEN Provincia END DESC,
                CASE WHEN @Orden=8 THEN TITULOL END ASC,
	            CASE WHEN @Orden=9 THEN Borrado END ASC,
                CASE WHEN @Orden=11 THEN DelGrupo END ASC,
                CASE WHEN @Orden=120 THEN CodExterno END ASC,			
			    CASE WHEN @Orden=121 THEN CodExterno END DESC			
		    ) AS Fila, 																
		IdCliente,idEmpresa, isnull(idSap,'') idSap, isnull(COD,'') CodGraf , isnull(TITULO,'') RazonSocial, isnull(TITULOL,'') NombreComercial
        ,isnull(idTipoDocumento, 0) idTipoDocumento, NTipoDoc, NIF
		,isnull(DOMICILIO,'') DOMICILIO, Direccion_Envio DomicilioEnvio,isnull(POBLACION,'') Poblacion, PoblacionEnvio ,isnull(PROVINCIA,'') Provincia 
		,ProvinciaEnvio, Codpostal,CodpostalEnvio, isnull(TELEF01,'') Telef01, isnull(FAX01,'') Fax01
		,isnull(EMAIL,'') EMail, isnull(LIMITE,0) LimiteCredito, isnull(DIAS,0) DiasCredito, isnull(FORMA_PAGO,'') FormaPagoTex, isnull(idFormaPago,0) idFormaPago, FormaPago, isnull(Texto_Pago,'') Observaciones
		,ISNULL(CtaFormaPago,'') CtaFormaPago, isnull(CUENTA_BANCARIA,'') CuentaBancaria ,isnull(CodExterno,'') CodExterno, isnull(IdCadena,0) idCadena, NombreCadena, CadenaActivaCarrito	
		,OfCentalCadena,isnull(PedMinimoConCompromiso,'') PedMinimoConCompromiso ,isnull(PedMinimoSinCompromiso,0) PedMinimoSinCompromiso,isnull(Contrasena,'') Contrasena
		,isnull(FAlta,'01/01/1900') FAlta, FBaja, isnull(IdTipoCliente,'') IdTipoCliente, isnull(TipoClienteHotel,'') TipoClienteHotel, isnull(CadenaTipoCliente,'') CadenaTipoCliente
		,isnull(JefeEconomato,'') JefeEconomato	,isnull(idMarca,'') idMarca, isnull(Marca,'') Marca, idTipoPrecio, TipoPrecio, ISNULL(idTipo, 0) idTipo, idValidadora,isnull(Contacto,'') Contacto, isnull(idPais,0) idPais, NombrePais
		,isnull(idTipoIva,0) idTipoIva, TipoIVA, SiglaTipoIVA, ImporteIVA	,isnull(Borrado, 'false') Anulado, isnull(delGrupo, 0) DelGrupo
		,isnull(CodContable,'') CodContable, isnull(ReqAutoriza, 'false') ReqAutoriza		
		FROM Clientes Cl													
			LEFT JOIN Cade ON Cade.Codigo = Cl.idCadena
			LEFT JOIN Fpg  ON idFormaPago = Fpg.Codigo			
			LEFT JOIN Tiva ON idTipoIva = Tiva.Codigo 							
			LEFT JOIN Pais ON idPais = Pais.Codigo					
			LEFT JOIN Tcl  ON idTipocliente = Tcl.Codigo		
			LEFT JOIN Tprecio ON idTipoPrecio = Tprecio.Codigo		
			LEFT JOIN Marca   ON idMarca = Marca.Codigo	
            LEFT JOIN TipoDoc  ON isnull(idTipoDocumento,0) = TipoDoc.Codigo	
			WHERE (@Anulado > 1  OR isnull(Borrado, 'false') = @Anulado)
                AND (@delGrupo > 1  OR isnull(delGrupo, 0) = @delGrupo)			
				AND ((@idCliente = 0 OR idCliente = @idCliente))					
				AND ((@idCadena = 0 OR idCadena = @idCadena))	
				AND ((@idSap = '' OR idSap LIKE  '%'+@idSap+'%')) 
                AND ((@CodExterno = '' OR CodExterno LIKE  '%'+@CodExterno+'%'))
				AND ((@NIF = '' OR NIF LIKE  '%'+@NIF+'%')) 
				AND ((@RazonSocial = '' OR TITULO LIKE  '%'+@RazonSocial+'%')) 	
				AND ((@NombreComercial = '' OR TITULOL LIKE  '%'+@NombreComercial+'%')) 
				AND ((@Domicilio = '' OR Domicilio LIKE  '%'+@Domicilio+'%'))  
                AND ((@Poblacion = '' OR Poblacion LIKE  '%'+@Poblacion+'%'))                         																								
		) AS a WHERE Fila BETWEEN @filaDesde AND @filaHasta;   	";
            
    int filaDesde = (nPagina - 1) * pageSize + 1;
    int filaHasta = (((nPagina - 1) * pageSize + 1) + pageSize) - 1;           
    //Debug.WriteLine(" filaDesde:" + filaDesde+ ", filaHasta:" + filaHasta + ", nPagina:" + nPagina); 

    List <ListaCliente> Lista = new List<ListaCliente>();
    ListaCliente Linea = new ListaCliente(); // crea la primera linea --
    using (SqlConnection conn = new SqlConnection(Conex)){			    
	    using (SqlCommand cmd = new SqlCommand(query, conn))       {
            conn.Open();                     
            cmd.Parameters.Clear();                                   						
            cmd.Parameters.Add("@filaDesde", SqlDbType.Int).Value = filaDesde;  
            cmd.Parameters.Add("@filaHasta", SqlDbType.Int).Value = filaHasta;  
            cmd.Parameters.Add("@Orden", SqlDbType.Int).Value = Sel.Orden;  
            cmd.Parameters.Add("@Anulado", SqlDbType.Int).Value = Sel.Anulado;
            cmd.Parameters.Add("@delGrupo", SqlDbType.Int).Value = Sel.delGrupo;
            cmd.Parameters.Add("@idCliente", SqlDbType.Int).Value = Sel.idCliente; 
            cmd.Parameters.Add("@idCadena", SqlDbType.Int).Value = Sel.idCadena;
            cmd.Parameters.Add("@idSap", SqlDbType.VarChar).Value = Sel.idSap;
            cmd.Parameters.Add("@CodExterno", SqlDbType.VarChar).Value = Sel.CodExterno;
            cmd.Parameters.Add("@NIF", SqlDbType.VarChar).Value = Sel.NIF;
            cmd.Parameters.Add("@RazonSocial", SqlDbType.VarChar).Value = Sel.RazonSocial;
            cmd.Parameters.Add("@NombreComercial", SqlDbType.VarChar).Value = Sel.NombreComercial;
            cmd.Parameters.Add("@Domicilio", SqlDbType.VarChar).Value = Sel.Domicilio;
            cmd.Parameters.Add("@Poblacion", SqlDbType.VarChar).Value = Sel.Poblacion;            
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
		    while (Lector.Read())            {
                Lista.Add(Linea);               
                Linea.Fila = int.Parse(Lector["Fila"].ToString());                
                Linea.idCliente = int.Parse(Lector["idCliente"].ToString());                 
                Linea.idSap = Lector["idSap"].ToString();
                Linea.RazonSocial = Lector["RazonSocial"].ToString();
                Linea.NombreComercial = Lector["NombreComercial"].ToString();
                Linea.idTipoDocumento = int.Parse(Lector["idTipoDocumento"].ToString());
                Linea.NTipoDoc = Lector["NTipoDoc"].ToString();
                Linea.NIF = Lector["NIF"].ToString();
                Linea.Domicilio = Lector["Domicilio"].ToString();
                Linea.Poblacion = Lector["Poblacion"].ToString();
                Linea.Provincia = Lector["Provincia"].ToString();
                Linea.Codpostal = Lector["Codpostal"].ToString();
                Linea.DomicilioEnvio = Lector["DomicilioEnvio"].ToString();
                Linea.PoblacionEnvio = Lector["PoblacionEnvio"].ToString();
                Linea.ProvinciaEnvio = Lector["ProvinciaEnvio"].ToString();
                Linea.CodpostalEnvio = Lector["CodpostalEnvio"].ToString();
                Linea.Observaciones = Lector["Observaciones"].ToString();                
                Linea.idCadena = int.Parse(Lector["idCadena"].ToString());
                Linea.NombreCadena = Lector["NombreCadena"].ToString();                
                Linea.Telef01 = Lector["Telef01"].ToString();
                Linea.Email = Lector["Email"].ToString();
                Linea.Contacto = Lector["Contacto"].ToString();
                Linea.CodExterno = Lector["CodExterno"].ToString();
                Linea.CodGraf = Lector["CodGraf"].ToString();                
                Linea.CodContable = Lector["CodContable"].ToString();
                Linea.FAlta = Convert.ToDateTime(Lector["FAlta"]).ToShortDateString();
                Linea.JefeEconomato = Lector["JefeEconomato"].ToString();
                Linea.Contrasena = Lector["Contrasena"].ToString();
                Linea.idPais = int.Parse(Lector["idPais"].ToString());
                Linea.idTipoCliente = int.Parse(Lector["idTipoCliente"].ToString());
                Linea.idTipoIva = int.Parse(Lector["idTipoIva"].ToString());
                Linea.TipoIVA = Lector["TipoIva"].ToString();                
                Linea.ReqAutoriza = bool.Parse(Lector["ReqAutoriza"].ToString());   
                Linea.idFormaPago = int.Parse(Lector["idFormaPago"].ToString());                
                Linea.DiasCredito = Lector["DiasCredito"].ToString();
                Linea.LimiteCredito = Lector["LimiteCredito"].ToString();
                Linea.CuentaBancaria = Lector["CuentaBancaria"].ToString();
                Linea.Anulado = bool.Parse(Lector["Anulado"].ToString());
                Linea.delGrupo = int.Parse(Lector["delGrupo"].ToString());
                Linea.PedMinimoConCompromiso = int.Parse(Lector["PedMinimoConCompromiso"].ToString());
                Linea.PedMinimoSinCompromiso = int.Parse(Lector["PedMinimoSinCompromiso"].ToString());                
                Linea.idTipoPrecio  = Lector["idTipoPrecio"] == DBNull.Value ? 0 : int.Parse(Lector["idTipoPrecio"].ToString()); // evita null --
                Linea.idMarca  = Lector["idMarca"] == DBNull.Value ? 0 : int.Parse(Lector["idMarca"].ToString()); // evita null --                
                Linea = new ListaCliente(); // crea nueva linea  --                               
		    }
	    }        
    }
    return Lista;
}// recClientesLista --

[WebMethod]
public int cuentaClientes(selListaClientes Sel) {
        string query = @"  
		SELECT count(*) Nreg 
		FROM Clientes Cl																
			WHERE (@Anulado > 1  OR isnull(Borrado, 'false') = @Anulado)	
                AND (@delGrupo > 1  OR isnull(delGrupo, 0) = @delGrupo)		
				AND ((@idCliente = 0 OR idCliente = @idCliente))					
				AND ((@idCadena = 0 OR idCadena = @idCadena))	
				AND ((@idSap = '' OR idSap LIKE  '%'+@idSap+'%')) 
                AND ((@CodExterno = '' OR CodExterno LIKE  '%'+@CodExterno+'%'))
				AND ((@NIF = '' OR NIF LIKE  '%'+@NIF+'%')) 
				AND ((@RazonSocial = '' OR TITULO LIKE  '%'+@RazonSocial+'%')) 	
				AND ((@NombreComercial = '' OR TITULOL LIKE  '%'+@NombreComercial+'%')) 
				AND ((@Domicilio = '' OR Domicilio LIKE  '%'+@Domicilio+'%')) 
                AND ((@Poblacion = '' OR Poblacion LIKE  '%'+@Poblacion+'%')) ";	
        																											
    List <ListaCliente> Lista = new List<ListaCliente>();
    ListaCliente Linea = new ListaCliente(); // crea la primera linea --
    using (SqlConnection conn = new SqlConnection(Conex)){			    
	    using (SqlCommand cmd = new SqlCommand(query, conn))       {
            conn.Open();                     
            cmd.Parameters.Clear();                                   						                        
            cmd.Parameters.Add("@Anulado", SqlDbType.Int).Value = Sel.Anulado; 
            cmd.Parameters.Add("@delGrupo", SqlDbType.Int).Value = Sel.delGrupo; 
            cmd.Parameters.Add("@idCliente", SqlDbType.Int).Value = Sel.idCliente; 
            cmd.Parameters.Add("@idCadena", SqlDbType.Int).Value = Sel.idCadena;
            cmd.Parameters.Add("@idSap", SqlDbType.VarChar).Value = Sel.idSap;
            cmd.Parameters.Add("@CodExterno", SqlDbType.VarChar).Value = Sel.CodExterno;
            cmd.Parameters.Add("@NIF", SqlDbType.VarChar).Value = Sel.NIF;
            cmd.Parameters.Add("@RazonSocial", SqlDbType.VarChar).Value = Sel.RazonSocial;
            cmd.Parameters.Add("@NombreComercial", SqlDbType.VarChar).Value = Sel.NombreComercial;
            cmd.Parameters.Add("@Domicilio", SqlDbType.VarChar).Value = Sel.Domicilio;  
            cmd.Parameters.Add("@Poblacion", SqlDbType.VarChar).Value = Sel.Poblacion;  
		  
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (Lector.Read()) {
                return int.Parse(Lector["Nreg"].ToString());
            }
            // si no hay ningun registro --
            return 0;
        }        
    }           
}// cuentaClientes -- 
        

[WebMethod]
public string recClientesAutoC() {   
    // Aqui, metodo general, podría aislarse en asmx, independiente y compartida --
	StringBuilder st1 = new StringBuilder();    	
    using (SqlConnection conn = new SqlConnection(Conex))        {
        string query = @" SELECT idCliente Codigo, TituloL Texto FROM Clientes 
				WHERE Borrado <>1	AND idEmpresa=1  ";
		using (SqlCommand cmd = new SqlCommand(query, conn))        {
			conn.Open();            
			SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			while (Lector.Read())            {
					  st1.Append(string.Format("{0:}-{1}:", Lector["Codigo"], Lector["Texto"]));
			}
		}
	}
	// eliminamos el último ":"
	return st1.ToString().Substring(0, st1.Length - 2);

} // recClientesAutoC --


[WebMethod] 
 public int actCliente(int operacion, Cliente reg) {
    /* recibe lista de parametro reg desde JSON,
        operacion
        - 3 DELETE,      
        - otros UPSERT (Update o Insert)
            solo actualiza campos con valor , no null
    */
    //Debug.WriteLine(" actCliente, operacion:" + operacion.ToString()+", reg:"+ new JavaScriptSerializer().Serialize(reg));
    string ipMod = Context.Request.UserHostAddress.ToString();
    int resul ; // Retorna nº, modificado o añadido
    using (SqlConnection conex1 = new SqlConnection(Conex))    {            
        using (SqlCommand cmd = new SqlCommand("sp_actCliente", conex1)) {
            cmd.CommandType = CommandType.StoredProcedure;
            conex1.Open();            
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@operacion", SqlDbType.Int).Value = operacion;        
            cmd.Parameters.Add("@idCliente", SqlDbType.Int).Value = reg.idCliente;
            cmd.Parameters.Add("@idSap", SqlDbType.VarChar).Value = reg.idSap;
            cmd.Parameters.Add("@CodExterno", SqlDbType.VarChar).Value = reg.CodExterno;
            cmd.Parameters.Add("@Anulado", SqlDbType.Int).Value = reg.Anulado;
            cmd.Parameters.Add("@delGrupo", SqlDbType.Int).Value = reg.delGrupo;
            cmd.Parameters.Add("@RazonSocial", SqlDbType.VarChar).Value = reg.RazonSocial;
            cmd.Parameters.Add("@NombreComercial", SqlDbType.VarChar).Value = reg.NombreComercial;
            cmd.Parameters.Add("@Telef01", SqlDbType.VarChar).Value = reg.Telef01;
            cmd.Parameters.Add("@Fax01", SqlDbType.VarChar).Value = reg.Fax01;
            cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = reg.Email;
            cmd.Parameters.Add("@Domicilio", SqlDbType.VarChar).Value = reg.Domicilio;
            cmd.Parameters.Add("@Contacto", SqlDbType.VarChar).Value = reg.Contacto;
            cmd.Parameters.Add("@Poblacion", SqlDbType.VarChar).Value = reg.Poblacion;
            cmd.Parameters.Add("@Provincia", SqlDbType.VarChar).Value = reg.Provincia;
            cmd.Parameters.Add("@Codpostal", SqlDbType.VarChar).Value = reg.Codpostal;
            cmd.Parameters.Add("@DireccionEnvio", SqlDbType.VarChar).Value = reg.DireccionEnvio;
            cmd.Parameters.Add("@PoblacionEnvio", SqlDbType.VarChar).Value = reg.PoblacionEnvio;
            cmd.Parameters.Add("@ProvinciaEnvio", SqlDbType.VarChar).Value = reg.ProvinciaEnvio;
            cmd.Parameters.Add("@CodpostalEnvio", SqlDbType.VarChar).Value = reg.CodpostalEnvio;            
            cmd.Parameters.Add("@CodContable", SqlDbType.VarChar).Value = reg.CodContable;
            cmd.Parameters.Add("@CodGraf", SqlDbType.VarChar).Value = reg.CodGraf;                        
            cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar).Value = reg.Contrasena;
            cmd.Parameters.Add("@JefeEconomato", SqlDbType.VarChar).Value = reg.JefeEconomato;
            cmd.Parameters.Add("@ReqAutoriza", SqlDbType.Int).Value = reg.ReqAutoriza;
            cmd.Parameters.Add("@LimiteCredito", SqlDbType.Int).Value = reg.LimiteCredito;
            cmd.Parameters.Add("@DiasCredito", SqlDbType.Int).Value = reg.DiasCredito;
            cmd.Parameters.Add("@CuentaBancaria", SqlDbType.VarChar).Value = reg.CuentaBancaria;
            cmd.Parameters.Add("@idFormaPago", SqlDbType.Int).Value = reg.idFormaPago;
        
            // solo actualiza estos campos con valor, no null
            cmd.Parameters.Add("@FAlta", SqlDbType.DateTime).Value = DBNull.Value;
            cmd.Parameters.Add("@FBaja", SqlDbType.DateTime).Value = DBNull.Value;                                
            cmd.Parameters.Add("@idTipoDocumento", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@idPais", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@idCadena", SqlDbType.Int).Value = DBNull.Value;                                
            cmd.Parameters.Add("@NIF", SqlDbType.VarChar).Value = DBNull.Value;                 
            cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar).Value = DBNull.Value;            
            cmd.Parameters.Add("@idTipoIva", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@idTipoCliente", SqlDbType.Int).Value = DBNull.Value;            
            cmd.Parameters.Add("@PedMinimoConCompromiso", SqlDbType.Int).Value = DBNull.Value; 
            cmd.Parameters.Add("@PedMinimoSinCompromiso", SqlDbType.Int).Value = DBNull.Value;

            cmd.Parameters.Add("@idTipoPrecio", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@idMarca", SqlDbType.Int).Value = DBNull.Value;

            if (!(reg.PedMinimoConCompromiso == 0))
                cmd.Parameters["@PedMinimoConCompromiso"].Value = reg.PedMinimoConCompromiso;        
            if (!(reg.PedMinimoSinCompromiso == 0))
                cmd.Parameters["@PedMinimoSinCompromiso"].Value = reg.PedMinimoSinCompromiso;                       
            if (! (reg.idCadena == 0))
                cmd.Parameters["@idCadena"].Value = reg.idCadena;
            if (! string.IsNullOrEmpty(reg.NIF))
                cmd.Parameters["@NIF"].Value = reg.NIF;                        
            if (! string.IsNullOrEmpty(reg.Observaciones))
                  cmd.Parameters["@Observaciones"].Value = reg.Observaciones;
            if (!string.IsNullOrEmpty(reg.FAlta))
                cmd.Parameters["@FAlta"].Value = reg.FAlta;    
            if (!string.IsNullOrEmpty(reg.FBaja))
                cmd.Parameters["@FBaja"].Value = reg.FBaja;
            if (! (reg.idPais == 0))
                cmd.Parameters["@idPais"].Value = reg.idPais;
            if (! (reg.idTipoDocumento == 0))
                cmd.Parameters["@idTipoDocumento"].Value = reg.idTipoDocumento;

            if (! (reg.idTipoPrecio == 0))
                cmd.Parameters["@idTipoPrecio"].Value = reg.idTipoPrecio;
            if (! (reg.idMarca == 0))
                cmd.Parameters["@idMarca"].Value = reg.idMarca;

            if (!(reg.idTipoIva == 0))
                cmd.Parameters["@idTipoIva"].Value = reg.idTipoIva;
            if (!(reg.idTipoCliente == 0))
                cmd.Parameters["@idTipoCliente"].Value = reg.idTipoCliente;

            //Debug.WriteLine(" actCliente, reg:" + new JavaScriptSerializer().Serialize(reg));

            /*
            if (!(reg.TipoLocalizador == 0))
                cmd.Parameters["@TipoLocalizador"].Value = reg.TipoLocalizador;
            */
            // ref:, retorna nuevo nº insertado usando select, NO OUTPUT INSERTED, USAR lector (SqlDataReader          
            //resul = (int)cmd.ExecuteScalar(); // Usando ExecuteScalar no retorna tipo int;                             
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            resul = 0;
            if (Lector.Read())
                if (Lector["resul"] == DBNull.Value)
                    resul = 0;
                else
                    resul = int.Parse(Lector["resul"].ToString());

            //Debug.WriteLine(" actCliente, resul:" + Lector["resul"].ToString());
        };
        conex1.Close();              
    };
    return resul;    
                     
} // actCliente--

 
[WebMethod]
public void listaClientesExcel(selListaClientes Sel)  {    

    string sFile = Server.MapPath("Informes/listaClientes.xls");
    
    FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite);
    StreamWriter w = new StreamWriter(fs);
   
    string lHtm = "<!DOCTYPE HTML PUBLIC \"-/W3C//DTD HTML 4.0 Transitional/EN\"> <html><head>   <title> Listado Clientes Artes Graficas </title > " +
            "<meta http-equiv =\"Content-Type\" content=\"text/html; charset=UTF-8\" /> <style type=\"text/css\" media=\"all\">";
    w.Write(lHtm.ToString());

    // estilos CSS --
    string lHtmStyle = ".Cabecera { border-bottom:1px solid black; font-size:20px; background-color:antiquewhite; "+
            "text-align:center; vertical-align:middle; height:35px; font-weight:bold;  }";
    w.Write(lHtmStyle.ToString());
    lHtmStyle = ".Selec  { border-bottom:2px solid black; font-size:16px; height:30px;background-color:azure; text-align:left;  vertical-align:middle;}";
    w.Write(lHtmStyle.ToString());
    lHtmStyle = ".Titulos { border-bottom:1px solid black; font-size:14px; height:40px; text-align:center; vertical-align:middle;}";
    w.Write(lHtmStyle.ToString());
    lHtmStyle = ".Detalle { font-size:12px; height:20px; text-align:left; vertical-align:middle;}";
    w.Write(lHtmStyle.ToString());

    // Cabecera general 
    lHtm = "</style></head><body> <table> <tr> <td colspan='10' class='Cabecera'> Listado Clientes Artes Graficas </td>   </tr>";
    w.Write(lHtm.ToString());

    // linea de informacion de seleccion 

    // Orden usado ---
    string orden= ordenTexto(Sel.Orden);    
    lHtm = "<tr> <td class='Selec' colspan='2'> Seleccion </td > <td colspan='8' class='Selec'>  Orden: <b>" + orden + "</b>";
        
    // Filtros aplicados --- 

    // Recupera texto del id de la cadena 
    if (Sel.idCadena !=0) { 
        List<Cadena> lisCadena = recCadenasLista(3, Sel.idCadena);
        lHtm += " ,Cadena: <b>(" + Sel.idCadena +") "+ lisCadena[0].NombreCadena+ "</b>";
     }
    lHtm += (Sel.idCliente != 0) ? ", Codigo Cliente : <b> "+ Sel.idCliente+ "</b>" : "";
    lHtm += (Sel.idSap != "") ? ", Codigo SAP : <b> "+ Sel.idSap + "</b>" : "";
    lHtm += (Sel.CodExterno != "") ? ", Codigo Externo : <b> "+ Sel.CodExterno + "</b>" : "";
    lHtm += (Sel.NIF != "") ? ", NIF: <b> "+ Sel.NIF + "</b>" : "";
    lHtm += (Sel.RazonSocial != "") ? ", Razon Social: <b> " + Sel.RazonSocial + "</b>" : "";
    lHtm += (Sel.NombreComercial != "") ? ", Nombre Comercial: <b> " + Sel.NombreComercial + "</b>" : "";
    lHtm += (Sel.Poblacion != "") ? ", Poblacion: <b> " + Sel.Poblacion + "</b>" : "";
    lHtm += (Sel.Anulado == 1)   ? " , <b>Anulados </b> " : " ";
    lHtm += (Sel.Anulado == 0)   ? " , <b>NO Anulados </b>" : " ";
    lHtm += (Sel.delGrupo == 1)  ? ", <b>Del Grupo </b> " : " ";
    lHtm += (Sel.delGrupo == 0)  ? ", <b>NO Del Grupo </b>" : " ";        
    lHtm += "</td> </tr> ";
    w.Write(lHtm.ToString());
        
    // cabecera con textos de detalles --   
     lHtm = "<tr class='Titulos'>" +
        "<td style ='width:50px;'>Cod </td>" +            
        "<td style ='width:80px;'>C.Sap </td>" +
        "<td style ='width:70px;'>C. Exter </td>" +

        "<td style ='width:50px;'>Id</td>" +
        "<td style ='width:80px;'>Nº. Docum.</td>" +
        "<td style ='width:260px;'>Razon Social</td>" +
        "<td style ='width:260px;'>Nombre Comercial </td>" +
        "<td style ='width:120px; text-align:left;'>Cadena</td>" +
        "<td style ='width:50px;'>Anul.</td>" +
        "<td style ='width:50px;'>Del Grupo</td>" +
        "<td style ='width:260px;'>Domicilio</td>" +
        "<td style ='width:200px;'>Poblacion</td>" +        
        "<td style ='width:150px;'>Provincia</td>" +
        "<td style ='width:90px;'>CP</td>" +
        "<td style ='width:260px;'>DomicilioEnvio</td>" +
        "<td style ='width:200px;'>Poblacion Envio</td>" +
        "<td style ='width:90px;'>Telefono</td>" +
        "<td style ='width:200px;'>Correo</td>" +        
        "<td style ='width:90px;'>C.Contable</td>" +
        "<td style ='width:90px;'>C.Graphis.</td>" +
        "<td style ='width:180px;'>Contacto</td>" +
        "<td style ='width:90px;'>Tipo IVA</td>" +
        "<td style ='width:80px;'>F.Alta</td>" +
        "<td style ='width:80px;'>F.Baja</td>" +
        "<td style ='width:250px;'>Observaciones </td>" +
        "</tr>";    
    w.Write(lHtm);        
    List<ListaCliente> Lista = recClientesLista(Sel, 1, 9999999);
    // Recorre lista de Clientes recuperados            
    foreach (ListaCliente Lis in Lista) {
        lHtm = "<tr class='Detalle'>" +
            "<td> " + Lis.idCliente + " </td>" +               
            "<td>" + Lis.idSap + "</td>" +
            "<td>" + Lis.CodExterno + "</td>" +
            "<td>" + Lis.NTipoDoc + "</td>" +
            "<td>" + Lis.NIF + "</td>" +
            "<td>" + Lis.RazonSocial + "</td>" +
            "<td>" + Lis.NombreComercial + "</td>" +
            "<td>" + Lis.NombreCadena + "</td>" +
            "<td style ='text-align:center;'>" + ((Lis.Anulado) ? "Si" : "") + "</td>" +
            "<td style ='text-align:center;'>" + ((Lis.delGrupo != 0 ) ? "Si" : "") + "</td>" +
            "<td>" + Lis.Domicilio + "</td>" +
            "<td>" + Lis.Poblacion + "</td>" +            
            "<td>" + Lis.Provincia + "</td>" +
            "<td>" + Lis.Codpostal + "</td>" +
            "<td>" + Lis.DomicilioEnvio + "</td>" +
            "<td>" + Lis.PoblacionEnvio + "</td>" +
            "<td>" + Lis.Telef01 + "</td>" +
            "<td>" + Lis.Email + "</td>" +            
            "<td>" + Lis.CodContable + "</td>" +
            "<td>" + Lis.CodGraf + "</td>" +
            "<td>" + Lis.Contacto + "</td>" +
            "<td>" + Lis.TipoIVA + "</td>" +
            "<td>" + Convert.ToDateTime(Lis.FAlta).ToShortDateString() + "</td>" +
            "<td>" + Convert.ToDateTime(Lis.FBaja).ToShortDateString() + "</td>" +
            "<td>" + Lis.Observaciones + "</td>" +
            "</tr>";
        w.Write(lHtm);
    }
    w.Write(" </table> </p> </body> </html>");

    w.Close();        
 }//listaClientesExcel--


 public string ordenTexto (int orden) {
    // retorna texto del orden según indice de este 
    // aqui, intentar sustituir por numerador indexado
    string txOrden = "";
    switch (orden) {
        case 1: { txOrden = "Cod. Ascendente"; break; };
        case 10:{ txOrden = "Cod. Descende"; break; };
        case 2: { txOrden = "Cod. Ascendente"; break; };
        case 20:{ txOrden = "Cadena Descende"; break; };
        case 3: { txOrden = "Codigo SAP"; break; };
        case 4: { txOrden = "NIF"; break; };
        case 5: { txOrden = "Razon Social"; break; };
        case 6: { txOrden = "Poblacion Ascendente"; break;        };
        case 60:{ txOrden = "Poblacion Descende"; break;        };
        case 7: { txOrden = "Provincia Ascendente"; break; };
        case 70:{ txOrden = "Provincia Descendente"; break;        };
        case 8: { txOrden = "Nombre Comercial"; break;        };
        case 9: { txOrden = "Anulado"; break;};        
        case 11:{ txOrden = "Del Grupo"; break; };
        case 120: { txOrden = "Cod. Externo Ascendente"; break;        };
        case 121:{ txOrden = "Cod. Externo Descende"; break;        };
    };
    return txOrden;
}// ordenTexto

  [WebMethod] 
 // ref: otra manera de recibir parámetros JSON --
public bool pruebaJSONParam(string orderJSON) {
    var serializer = new JavaScriptSerializer();
    var reg = serializer.Deserialize<Cliente> (orderJSON);        

    //Debug.WriteLine("pruebaJSONParam:" + orderJSON);                 
    Debug.WriteLine("pruebaJSONParam, idCliente:" + reg.idCliente);                 
    return true;
}


                
#endregion Cliente 


#region GlDistri  --------


[WebMethod]
public string contaGldistri(int empresa, int año, int mes) {       
    /*  Genera fichero contable Gldistri, Antiguo SApAsien --
     		Empresa: 1-> Halcon 11->Ecuador
            Poner año con 2 Dígitos 19
    */
    
    string sociedad;
    int antFamilia;
    double totalFamilia = 0;
    switch (empresa) {
        case 1:
            sociedad = "VHA";
            break;
        case 11:
            sociedad = "VEC";
            break;
        default:            
            return "Empresa "+empresa+" no Valida, Usar solo 1(Halcon), 11 Ecuador";
            break;
    };
    
    // usa clase anonima --
    var linRes= new { Empresa = 0, Codigo = "", Importe = 0.0, Familia = 0, NombreFamilia="", Ceco = "",Texto="" };    
    var Lista = new[] { linRes }.ToList();// crea lista con una linea        
    Lista.Clear(); // Borra toda la lista 

    // recupera resumen de movimiento de gldistri --
    using (SqlConnection conn = new SqlConnection(ConexGlDistri)){
        string query = @"SELECT a.*,  DESCRIPCION NombreFamilia, SUCPC.Dbo.CodigoSap(Codigo) CECO, DESCRIPCION+ ' - '+Codigo Texto
			FROM (SELECT  Suc.Empresa, Suc.Codigo, SUM(PRECIO * CANTIDAD) Importe, FAMILIA, Suc.SUCURSAL NOMBRE_SUCURSAL
				FROM MOVIMIENTOS Mov					
				 LEFT JOIN ARTICULOS Art ON Mov.CODIGO_ARTICULO = Art.Cod 
				 LEFT JOIN SUCURSALES Suc ON Mov.SUCURSAL=Suc.Cod 			
				WHERE TIPO_MOVIMIENTO = 2
					AND Suc.Empresa =  @empresa					
					AND year(Fecha) = @año+2000 AND month(Fecha) = @mes		
					GROUP BY Suc.Empresa, Suc.Codigo, Art.FAMILIA, Suc.SUCURSAL 			
				 ) a			 
				LEFT JOIN FAMILIAS Fam ON a.Familia = Fam.Cod 		 					
					ORDER BY  Descripcion, Codigo";
		using (SqlCommand cmd = new SqlCommand(query, conn))       {
			conn.Open();                             
            cmd.Parameters.Clear();        
            cmd.Parameters.Add("@empresa", SqlDbType.Int).Value = empresa;
            cmd.Parameters.Add("@año", SqlDbType.Int).Value = año;         
            cmd.Parameters.Add("@mes", SqlDbType.Int).Value = mes;        
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);                       
			while (Lector.Read())            {                
                Lista.Add(new {
                    Empresa = int.Parse(Lector["Empresa"].ToString()),
                    Codigo = Lector["Codigo"].ToString(),                    
                    Importe = double.Parse(Lector["Importe"].ToString()),
                    Familia = int.Parse(Lector["Familia"].ToString()),                    
                    NombreFamilia = Lector["NombreFamilia"].ToString(),
                    Ceco = Lector["Ceco"].ToString(),
                    Texto = Lector["Texto"].ToString()                    
                });
                
            }              
        }                 
	}                     
    string sFile = Server.MapPath("Informes/"+ sociedad + "_Distribuidora_"+mes+"_"+año+".CSV");    

    FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite);    
    StreamWriter w = new StreamWriter(fs);
             
    // Cabecera SAP Excel----------------------------------------
    JObject jsonObj = genJsonDefinicion("GlDistri.json");
    JToken jsTk = jsonObj["CabeceraGldistriSAP"];
                            
    // Genera linea de texto con nombre de propiedades del nodo, TITULOS de la CABECERA                       
    string linTex = stringJsonProp(jsTk, ";");
    w.Write(linTex + " \n "  );
    string XRef1 = "10077" + mes.ToString("00") + (año+2000).ToString("00");    

    // crea estructura de tipo anonimo paramCab, se envia como parametro --
    object paramCab = new { sociedad = "", año = 0, mes= 0, NombreFamilia = ""};                

    antFamilia = 9999;         
    vaciaJson(jsTk);
        
    // recorre lista de detalles recuperada
    foreach ( var l in Lista) {       
        // si ha cambiado la familia
        if (l.Familia != antFamilia) {
            // si no es la primera linea, genera total 
            if (antFamilia != 9999 )                
                totGldistri(w, jsTk, totalFamilia, paramCab);

            // genera cabecera, ref: envía parametro de clase anonima paramCab
            paramCab = new { sociedad = sociedad, año = año, mes= mes, NombreFamilia = l.NombreFamilia };
            
            cabGldistri(w, jsTk, paramCab);
            vaciaJson(jsTk);
            totalFamilia = 0;            
        }
        antFamilia = l.Familia;
        jsTk["tipreg"] = "2";
        jsTk["clave contab."] = "D";
        jsTk["CECO"] = l.Ceco;
        jsTk["IMPORTE"] = l.Importe;
        jsTk["TEXTO POS."] = l.Texto;
        jsTk["XREF1"] = XRef1;
        linTex = stringJson(jsTk, ";");
        w.Write(linTex + " \n ");
        totalFamilia += l.Importe;
    };    
    // genera ultimo total     
     totGldistri(w, jsTk, totalFamilia, paramCab);
    

    w.Close();    
    return "Generado fichero:"+ sFile;
}// contaGldistri --

public void cabGldistri(StreamWriter w, JToken jsTk, dynamic p) {    
    /* genera cabecera de fichero de texto CSV     
     ref: recibe parametro de clase anonima, p 
    */
    string fechaDoc = "30"+ p.mes.ToString("00") + (p.año+2000).ToString("00");    
    vaciaJson(jsTk);        
    jsTk["tipreg"] = "1";
    jsTk["sociedad"] = p.sociedad;
    jsTk["tipo docum"] = "CG";
    jsTk["fecha doc"] = fechaDoc;
    jsTk["fecha cont."] = fechaDoc;
    jsTk["moneda"] = "EUR";
    jsTk["referencia"] = "10077"+p.año.ToString("00") + p.mes.ToString("00") ;
    jsTk["texto cabecera"] = p.NombreFamilia+" " + p.mes.ToString("00") + "/"+ p.año.ToString("00") ;    
    jsTk["libre1"] = "DISTRI";             
    w.Write(stringJson(jsTk, ";") + " \n ");                    
} //cabGldistri--

public void totGldistri(StreamWriter w, JToken jsTk, double totalFamilia, dynamic p) {
    // genera TOTAL en fichero de texto CSV     
    vaciaJson(jsTk);
    jsTk["tipreg"] = "2";
    jsTk["clave contab."] = "H";
    jsTk["IMPORTE"] = totalFamilia;
    jsTk["TEXTO POS."] = "Total "+ p.NombreFamilia+"- mes " + p.mes.ToString("00");
    jsTk["XREF1"] = "10077"+p.año.ToString("00") + p.mes.ToString("00") ;

    w.Write(stringJson(jsTk, ";") + " \n ");                    
} //totGldistri--

#endregion GlDistri 

    

#region metodosEstaticos -----



public static int crearLineaContable(int idFactura, int Ejercicio, int tipo, int orden, string FechaCierre, string Contenido)        {
    //25/3/22 anulado, usa param de web.config -- const string pathUrl = "http://intranet.halconviajes.com/Facturas_SAP/";
    string pathUrl  = WebConfigurationManager.AppSettings["UrlFacturasSAP"];

    
    Contenido = Contenido.Replace("'", " "); // Elimina el caracter "'" si existe --
    int Resul = 0; // Retorna nº de , modificado o añadido --
    string Query = @" INSERT INTO GAGSAP (idEmpresa, idFactura, Ejercicio, Tipo,  FechaCierre, Contenido, UrlDocumento) OUTPUT INSERTED.IdGagSap VALUES ("+
            "1"+                                            // Empresa --
        ","+idFactura.ToString()+                       // Factura --
        "," + Ejercicio.ToString() +                    // Ejercicio 
        "," + tipo.ToString()+                          // Tipo  (  1-> Factura,   2-> Coste )--                       
        ",'" + FechaCierre + "'" +                      // FechaCierre
        ",'" + Contenido + "'"+                         // Contenido 
        ",'" + pathUrl +"Fact_"+ idFactura.ToString() +"_"+ Ejercicio.ToString() + ".pdf'" +    // UrlDocumento -
        ");";                                                                                      

    using (SqlConnection Conexion = new SqlConnection(ConexORCL))         {
        Conexion.Open();
        using (SqlCommand Command = new SqlCommand(Query, Conexion))                {
            //Command.Parameters.Clear();
            //Command.Parameters.Add("@idDelegacion", SqlDbType.Int).Value = Reg.IdDelegacion;
            //Command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = Reg.IdEmpresa;
            Command.CommandText = Query;
            Resul = (int)Command.ExecuteScalar(); // retorna nuevo nº insertado, usando OUTPUT INSERTED
        }
    }
    return Resul;
 }


#endregion metodosEstaticos -----



#region ListaFacturas -----

 [WebMethod]
public string recListaFacturasJSON(int nPagina, int pageSize, int Orden,
    string fDesde, string fHasta, int Ejercicio, int Cadena,
    string Factura, string Cliente, string Pedido, string Albaran, string idSap, string HRuta,  string Facturados, string CargoAbono) {
    // test de parámetros pasados -- Debug.WriteLine( "PARAMETROS: FDesde:" + fDesde+ ", FHasta:" + fHasta+ ", Ejercicio:" + Ejercicio+ ", Factura:"+Factura);
    int limiteLineas = 4000;

    // Crea la clase con los parametros fijo de selección ya definidos --
    clListaFacturas lfac = new clListaFacturas(fDesde, fHasta, Ejercicio, Cadena, Factura, Cliente, Pedido, Albaran, idSap, HRuta, Facturados, CargoAbono, Orden);   
    // si se estan contando facturas, nº de pagina 0, solo retorna el nº de registros y totales  --
    if (nPagina == 0) {        
        string resulCuenta = lfac.cuentaLista();
        return resulCuenta;
    }
    else {
        int filaDesde = (nPagina - 1) * pageSize + 1;
        int filaHasta = (((nPagina - 1) * pageSize + 1) + pageSize) - 1;
        // recupera lista en objeto list ---       
        List<lisFactura> listaF = lfac.recLista(filaDesde, filaHasta);
        // si sobrepasa límite de registros, retorna en fila 0  ejercicio 9999 y nº de regs y limite de estos  --
        if (listaF.Count > limiteLineas) {
            //Debug.WriteLine("Sobrepasado limite de .lineas");
            return "limite: " + limiteLineas.ToString();            
        }
        else
            return new JavaScriptSerializer().Serialize(listaF); // retorna la lista trasformada en JSON --
    }
}// recListaFacturasJSON --

    
class clListaFacturas      {
private string fDesde, fHasta, nFactura, Cliente, Pedido, Albaran, idSap, HRuta, Facturados, CargoAbono;
private int Ejercicio, Cadena, Orden;
    
// constructor --
public clListaFacturas(string fDesde, string fHasta,   int Ejercicio, 
    int Cadena, string nFactura, string Cliente, string Pedido, string Albaran, string idSap, 
    string HRuta, string Facturados,   string CargoAbono, int Orden  )     {            
        this.fDesde = fDesde; 
        this.fHasta = fHasta; 
        this.Ejercicio = Ejercicio;                                
        this.Cadena = Cadena;   
        this.nFactura = nFactura;   
        this.Cliente = Cliente;      
        this.Pedido = Pedido;      
        this.Albaran = Albaran;
        this.idSap = idSap;
        this.HRuta = HRuta;            
        this.Facturados = Facturados;     
        this.CargoAbono = CargoAbono;     
        this.Orden = Orden;     
} // constructor ---
        
public List<lisFactura> recLista(int filaDesde, int filaHasta) {
    //Debug.WriteLine("recLista PARAM: FDesde:" + fDesde+ ", FHasta:" + fHasta+ ", Ejercicio:" + Ejercicio+ ", Factura:"+nFactura);                       
    /*
    Muy lento con orden DESC el pagin altas, ejecutar como store proc y probar, el sql puro es muy rapido  
    Pasar a store proc 
    Reasigna los parametros a variable locales para evitar parameter sniffing, permite asi orden DESC  -- */

     string query = @"   DECLARE @fDesdeN datetime , @fHastaN datetime, @EjercicioN int;
        SET @EjercicioN = @Ejercicio;
        SET @fDesdeN = @fDesde;
	    SET @fHAstaN = @fHasta;

        SELECT   * FROM  ( SELECT   ROW_NUMBER() OVER ( 
			ORDER BY 
				CASE WHEN @Orden=3 THEN fac.idFactura END DESC,
				CASE WHEN @Orden=5 THEN fac.Fecha END DESC,
				CASE WHEN @Orden=6 THEN fac.idCliente END DESC,
				CASE WHEN @Orden=17 THEN NombreCadena END	DESC ) AS Fila, 
            Fac.IdFactura, Fac.IdEmpresa, Fac.Ejercicio, Fac.IdCliente, PRESUPUESTO, FECHA,  SUBSTRING(NombreCadena  , 1, 15) NombreCadena  
            ,SUBSTRING(REFERENCIA, 1, 12) REFERENCIA, Estado, LOGOTIPO_SALAMANCA, COMENTARIO
            ,SUBSTRING(TituloL, 1, 25) NombreCliente, isnull(Tot.NReg,0) Nreg,  Pedido, SUBSTRING(HRuta, 1, 12) HRuta
            ,isnull(TImporte_Iva + TPrecio, 0) Total, Tot.TImporte_Iva, Tot.TIVA, Tot.TPrecio, isnull(Coste,0) Coste,  Tot.TCantidad
            ,Fac.Fecha_Cierre FContabilizada, Cl.IdSap, Fac.FechaPago, isnull(idFacAbonada , 0) idFacAbonada, Albaran
        FROM Facturas Fac    
        LEFT  JOIN ( SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(1)  NReg, SUM(CANTIDAD) TCantidad, 
	        SUM(Precio)  TPrecio, SUM(IVA) TIVA,SUM(ROUND(Precio * ISNULL(IVA, 0) / 100, 4))  TImporte_Iva			
	        ,MIN(Presupuesto) AS HRuta 			       
		        FROM Facturas_Detalles	GROUP BY Ejercicio, IdFactura, IdEmpresa  )	Tot 
		        ON Fac.IdEmpresa = Tot.IdEmpresa	AND Fac.Ejercicio = Tot.Ejercicio	AND Fac.IdFactura = Tot.IdFactura                   
        LEFT JOIN Clientes Cl ON Fac.IdCliente = Cl.IdCliente
        LEFT JOIN (SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(ISNULL(Importe, 0)) Coste	FROM Costes   
        GROUP BY Ejercicio, IdFactura, IdEmpresa) TotCos 
		        ON Fac.IdEmpresa = TotCos.IdEmpresa	AND Fac.Ejercicio = TotCos.Ejercicio AND Fac.IdFactura = TotCos.IdFactura
        LEFT JOIN(SELECT  Codigo , texto NombreCadena FROM Tablas WHERE TipoTabla = 'CADE') Cad  ON Cad.Codigo = Cl.idCadena
        LEFT JOIN (SELECT  Factura, EjercicioFactura, IdEmpresa, SUM(1)  NRegAlba, MIN(CodSpatam) AS PrimOrden, MIN(NPedido) Pedido, MIN(idAlbaran) Albaran
		        FROM Albaranes GROUP BY IdEmpresa, EjercicioFactura, Factura  
	        ) Alb	 ON Fac.IdEmpresa = Alb.IdEmpresa	 AND Fac.Ejercicio = Alb.EjercicioFactura 	 AND Fac.IdFactura = Alb.Factura";        

    // Genera texto sql con lineas de selección ----  
    string querySel =creaQrySel();

    // une cuerpo de qry con seleccion  --
    query = query + querySel;

    // Información añadida despues de la seleccion, Orden, etc  --
    query = query + ") AS ResultadoFilas	WHERE Fila BETWEEN @filaDesde AND @filaHasta       ";
                
    List<lisFactura> Facturas = new List<lisFactura>();
    lisFactura LinFactura = new lisFactura(); // crea la primera linea --
    using (SqlConnection conn = new SqlConnection(Conex)) {        
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            //Debug.WriteLine("recLista,  filaDesde:" + filaDesde+ "filaHasta:" + filaHasta + "\n Query: " + query);                     
            cmd.Parameters.Clear();   
            cmd.Parameters.Add("@filaDesde", SqlDbType.Int).Value = filaDesde;                
            cmd.Parameters.Add("@filaHasta", SqlDbType.Int).Value = filaHasta;                
            cmd.Parameters.Add("@Ejercicio", SqlDbType.Int).Value = Ejercicio;                          
            cmd.Parameters.Add("@fDesde", SqlDbType.DateTime).Value = fDesde + " 00:00"; ;
            cmd.Parameters.Add("@fHasta", SqlDbType.DateTime).Value = fHasta + " 23:59"; 
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;
            cmd.Parameters.Add("@Orden", SqlDbType.Int).Value = Orden;
            conn.Open();

            //SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            SqlDataReader Lector = cmd.ExecuteReader();
            while (Lector.Read()) {
                Facturas.Add(LinFactura);

                LinFactura.Fila = int.Parse(Lector["Fila"].ToString());
                LinFactura.Ejercicio = int.Parse(Lector["Ejercicio"].ToString());
                LinFactura.idFactura = int.Parse(Lector["idFactura"].ToString());
                LinFactura.Fecha = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();
                LinFactura.idCliente = int.Parse(Lector["idCliente"].ToString());
                LinFactura.idSap = Lector["idSap"].ToString();
                LinFactura.NombreCliente = Lector["NombreCliente"].ToString();
                LinFactura.Referencia = Lector["Referencia"].ToString();
                LinFactura.Total = float.Parse(Lector["Total"].ToString());
                LinFactura.NReg = int.Parse(Lector["NReg"].ToString());
                LinFactura.FContabilizada = (Lector["FContabilizada"] != DBNull.Value ? Convert.ToDateTime(Lector["FContabilizada"]).ToShortDateString() : "");
                LinFactura.NombreCadena = Lector["NombreCadena"].ToString();
                LinFactura.Coste = float.Parse(Lector["Coste"].ToString());
                LinFactura.Pedido = Lector["Pedido"].ToString();
                LinFactura.Albaran = Lector["Albaran"].ToString();
                LinFactura.HRuta = Lector["HRuta"].ToString();
                        
                LinFactura = new lisFactura(); // crea nueva linea  --                               
            }
        }
    }
    return Facturas;
}// recLista--     
   
public string cuentaLista() {
        // 2 /3/ 18 Eliminado enlace con coste, ralentiza mucho la ejecucion--Sum(Coste) TotCostes
        // USAR sp_cuentaFacturas, SI permite sumar costes --
        string query = @" 
            DECLARE @fDesdeN datetime , @fHastaN datetime, @EjercicioN int;
        SET @EjercicioN = @Ejercicio;
        SET @fDesdeN = @fDesde;
	    SET @fHAstaN = @fHasta;
        SELECT  count(*) Nreg, sum(NReg) NregDet, SUM(TCantidad) TotCantidad, sum(isnull(TImporte_Iva + TPrecio, 0)) Total, Sum(Coste) TotCostes
	        FROM Facturas Fac		    
        LEFT  JOIN ( SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(1)  NReg, SUM(CANTIDAD) TCantidad 
	        , SUM(Precio)  TPrecio, SUM(IVA) TIVA,SUM(ROUND(Precio * ISNULL(IVA, 0) / 100, 4))  TImporte_Iva			
	        ,MIN(Presupuesto) AS HRuta 			       
		        FROM Facturas_Detalles GROUP BY Ejercicio, IdFactura, IdEmpresa  
        )	Tot ON Fac.IdEmpresa = Tot.IdEmpresa	AND Fac.Ejercicio = Tot.Ejercicio	AND Fac.IdFactura = Tot.IdFactura                   		
        LEFT JOIN (SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(ISNULL(Importe, 0)) Coste	FROM Costes 
	        GROUP BY Ejercicio, IdFactura, IdEmpresa
            ) TotCos 	ON Fac.IdEmpresa = TotCos.IdEmpresa	AND Fac.Ejercicio = TotCos.Ejercicio AND Fac.IdFactura = TotCos.IdFactura 
        LEFT JOIN (SELECT  Factura, EjercicioFactura, IdEmpresa, SUM(1)  NRegAlba, MIN(CodSpatam) AS PrimOrden, MIN(NPedido) Pedido, 
                MIN(idAlbaran) Albaran   FROM Albaranes GROUP BY IdEmpresa, EjercicioFactura, Factura  
	        ) Alb	 ON Fac.IdEmpresa = Alb.IdEmpresa	 AND Fac.Ejercicio = Alb.EjercicioFactura 	 AND Fac.IdFactura = Alb.Factura
        LEFT JOIN Clientes Cl ON Fac.IdCliente = Cl.IdCliente ";
            
    // Genera texto sql con lineas de selección ----  
    string querySel =creaQrySel();    
    query = query + querySel;
    //Debug.WriteLine("query: " + query);                
    using (SqlConnection conn = new SqlConnection(Conex)) {
        using (SqlCommand cmd = new SqlCommand(query, conn)) {                
            conn.Open();        
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@Ejercicio", SqlDbType.Int).Value = Ejercicio;                
            cmd.Parameters.Add("@fDesde", SqlDbType.DateTime).Value = fDesde;
            cmd.Parameters.Add("@fHasta", SqlDbType.DateTime).Value = fHasta;
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;                                                 
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.SingleRow);
            if (Lector.Read()) {
                return "{\"NReg\":" + Lector["Nreg"].ToString() +
                    ",\"Total\":" + Utilidades.CvNumero(Lector["Total"].ToString()) +
                    ",\"TotCostes\":" + Utilidades.CvNumero(Lector["TotCostes"].ToString()) +
                    " }";
            }
            // si no hay ningun registro --
            return "{\"NReg\":0 }";
        }
    }
}// cuentaLista --    
            
string creaQrySel() {
    // crea texto de sql con la selección --
    // Selección siempre fija (empresa y Ejercicio)--
    string querySelFijo = " WHERE Fac.IdEmpresa = 1  AND Fac.Ejercicio = @EjercicioN  ";

    string querySel =" AND Fecha BETWEEN CAST ( @fDesdeN AS DATETIME) AND  CAST (@fHastaN AS DATETIME)";
            
    if (Facturados == "2")  // Facturados  --
        querySel += " AND Fac.Fecha_Cierre is not null";
    else
        if (Facturados == "1")
        querySel += " AND Fac.Fecha_Cierre is null";

    if (CargoAbono == "1")  // Cargos  --
        querySel += " AND idFacabonada is null ";
    else
        if (CargoAbono == "2")
            querySel += " AND idFacabonada is not null ";

    if (Cadena != 0)
        querySel += " AND idCadena = @Cadena ";


    // Las siguientes selecciones  NO se añaden a la seleción anterior, anula los demás filtros, SALVO: empresa y ejercico --        		
    if (nFactura != "" && nFactura != null) {
        querySel = " AND Fac.IdFactura  =" + nFactura;
    }
    if (Cliente != "" && Cliente != null) {
        querySel = " AND Fac.idCliente =" + Cliente;
    }    

    if (Pedido != "" && Pedido != null) {
        querySel = " AND Pedido ='" + Pedido+"'";
    }    

    if (Albaran != "" && Albaran != null) {
        querySel = " AND Albaran ='" + Albaran + "'";
    }    

    if (idSap != "" && idSap != null) {
        querySel = " AND idSap ='" + idSap + "'";
    }    

    if (HRuta != "" && HRuta != null) {
        querySel = " AND HRuta ='" + HRuta + "'";
    }    
    return querySelFijo+querySel;
}// creaQrySel ---
    
public int genExcel(string ruta)    {               
    FileStream fs = new FileStream(ruta, FileMode.Create, FileAccess.ReadWrite);
    StreamWriter w = new StreamWriter(fs);   
                              
    cabeceraExcel(ref w);        
    detallesExcel(ref w);
            
    w.Close();
    return 0;
}// genExcel --

void cabeceraExcel(ref StreamWriter w)  {
    StringBuilder htm = new StringBuilder(); 

    htm.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
    htm.Append("<html>  <head> <title>Lista Facturas EXCEL</title>");
    htm.Append("<meta http-equiv=\"Content-Type\"content=\"text/html; charset=UTF-8\" />");
    htm.Append("</head> <body>");
    htm.Append("<table><tr style='font-weight:bold;font-size:18px;background-color:antiquewhite; text-align:center; vertical-align:middle;height:40px;'>" +
        "<td colspan='10' style=''>Informe Facturas Artes Gráficas </td> </tr>" +
        " <tr style='font-size:14px;background-color:gainsboro;'>  <td>"+
        " Seleccion..: </td>  <td> Ejercicio: "+ Ejercicio.ToString()+ "</td> <td> Desde:</td><td>"+ fDesde + " </td> <td> Hasta: " + fHasta + " </td></tr> </table>");
        
    htm.Append("<p> <table style='font-size:14px'>");  
    htm.Append("<tr style='font-weight:bold;font-size:16px;background-color:gainsboro;text-align:center; height:30px;'>");
    htm.Append("<td >Factura</td>");
    htm.Append("<td >Fecha</td>");
    htm.Append("<td >Cliente</td>");
    htm.Append("<td >C.SAP</td>");        
    htm.Append("<td >Nombre Cliente</td>");
    htm.Append("<td >Total</td>");
    htm.Append("<td >Lin</td>");
    htm.Append("<td >Referencia</td>"); 

    htm.Append("<td >Coste</td>"); 
    htm.Append("<td >Nº Pedido</td>"); 
    htm.Append("<td >Albaran</td>"); 
    htm.Append("<td >H.Ruta</td>"); 

    htm.Append("<td >F.Conta</td>");
    htm.Append("<td >Cadena</td>");
    htm.Append("</tr>");        
    w.Write(htm.ToString());      
}// cabeceraExcel --

void detallesExcel(ref StreamWriter w)  {                                                            
    List<lisFactura> listaF = recLista(1, 9999999);                                      
    // Recorre lista de facturas recuperadas 
    foreach (lisFactura Lis in listaF)    {
        w.Write(@"<tr>"+
            "<td> "+ Lis.idFactura + " </td>"+
            "<td>" + Convert.ToDateTime(Lis.Fecha).ToShortDateString() + "</td>" +
            "<td>" + Lis.idCliente + "</td>" +
            "<td>" + Lis.idSap+ "</td>" +
            "<td>" + Lis.NombreCliente + "</td>"+
            "<td>" + Lis.Total + "</td>" +
            "<td>" + Lis.NReg + "</td>" +
            "<td>" + Lis.Referencia + "</td>" +          
            
            "<td>" + Lis.Coste + "</td>" +          
            "<td>" + Lis.Pedido + "</td>" +          
            "<td>" + Lis.Albaran + "</td>" +          
            "<td>" + Lis.HRuta + "</td>" +          
                  
            "<td>" + (Lis.FContabilizada != "" ? Convert.ToDateTime(Lis.FContabilizada).ToShortDateString() : "") + "</td>" +                                   
            "<td>" + Lis.NombreCadena + "</td>" +
            "</tr>" );                                  
    }        
    w.Write(" </table> </p> </body> </html>");          
}// detallesExcel --

} // class clListaFacturas -- 


[WebMethod]
public void excelListaFacturas(string fDesde, string fHasta, int Ejercicio,  int Cadena,
    string Factura, string Cliente, string Pedido, string Albaran, string idSap, string HRuta, string Facturados,   string CargoAbono )  {    
    string sFile = Server.MapPath("Informes/listaFacturas.xls");
    clListaFacturas lExcel = new clListaFacturas(fDesde, fHasta, Ejercicio, Cadena, Factura, Cliente, Pedido, Albaran, idSap, HRuta,Facturados, CargoAbono, 2);          
    lExcel.genExcel(sFile);

}//excelListaFacturas()


#endregion ListaFacturas -----
 


#region Contabiliza --------------------


[WebMethod]
public string generarContaFacturas(List<lisFacSel> lisFacturas) {       
    // COntbiliza una lista completa de facturas (LisFacturas )--
    string resul = "";

    JObject jsonObj = genJsonDefinicion("FacturasSAP.json");
    
    // recorre datos   JSON recuperados --    
    foreach (lisFacSel lfac in lisFacturas) {
        //Debug.WriteLine("Factura de lista Json:"+ lfac.idFactura.ToString());                         
        resul += generarConta(lfac.idFactura, lfac.Ejercicio, jsonObj);
    };
    
    return resul;
}// generarContaFacturas --

public string generarConta(int idFactura, int Ejercicio, JObject jsonObj) {               
    // Crea la clase con datos de factura -    
    clFactura fact = new clFactura(idFactura, Ejercicio, jsonObj);
        
    // recupera información de la cabecera de factura --
    fact.RegCabecera = fact.recCabecera();
        
    // si la factura ya esta contabilizada, SALE --
    if (fact.RegCabecera.FContabilizada != "")
        return "Factura: " + idFactura.ToString() + " YA contabilizada \r\n";        
    
        
    // genera reg. de cabecera factura y devuelve el nº insertado, RETORNA -1 SI YA ESTA CONTABILIZADA  --   
    int idGagSap = fact.generarSAPFacturaCabecera();

    /* anulado 18/7/18, ahora se recupera y valida previamente 
          if (idGagSap == -1)                return "Factura: " + idFactura.ToString() + " YA contabilizada \r\n"; */

    fact.generarSAPFacturaDetalles();
    fact.generarSAPCostes();

     // actualiza el reg. de cabecera de factura con la imagen de esta en pdf -
    #if (INSERTARIMAGENENBD)
        fact.insertarImagenPdf(idGagSap);
    #endif
    // actualiza la cabecera de factura con la fecha actual y la sube al ftp --        
    #if (MARCARFACTURA)
        fact.marcaFechaFactura();    
        // sube la factura al FTP --
        fact.subirFtp();
    #endif

    return "Factura:" + idFactura.ToString()+", id:"+ idGagSap.ToString()+ "\r\n";
}// generarConta --

#endregion Contabiliza ---

        
 class clFactura  {
    private int Ejercicio, idFactura;
    private JObject jsonObj; // para recuperar definición de datos contables SAP de factura en objeto json 

    //4/5/18 probando a crear instancia de la clase principal (iwsGlFactura), evita usar metodo  estaticos  --      
    wsGlFactura iwsGlFactura = new wsGlFactura();

    // día actual --  //ref: un mes menos-- DateTime.Today.AddDays(-30).ToString("dd/MM/yyyy");     
    string fechaCierre = DateTime.Today.ToString("dd/MM/yyyy");     
    public Factura RegCabecera; // Datos de la cabecera de factura --
    List<DetalleFactura> DetFactura; // Datos de lineas de detalles de la factura --
    List<DetalleCoste> DetCostes; // Datos de lineas de costes de la factura --
    List<tiposIVA> listaIvas ;
    
    // constructores -- --       
    public clFactura( int idFactura, int Ejercicio)     {            
           this.Ejercicio = Ejercicio; 
           this.idFactura = idFactura;            
    }
    // sobrecarga de constructor, añade parametro de definicion Json --
    public clFactura( int idFactura, int Ejercicio, JObject jsonObj)     {            
        this.Ejercicio = Ejercicio; 
        this.idFactura = idFactura;
        this.jsonObj = jsonObj;
    }
    // constructores ---
            
    public Factura recCabecera() {
        Factura RegFactura = new Factura();
        string query = @"     	               
		  SELECT	 Fac.idFactura, Fac.Ejercicio, Fac.IdCliente, Titulo RazonSocial,TituloL, idCadena, Fecha, Estado, Referencia,   Comentario
	        ,Fecha_cierre FContabilizada ,Domicilio, Poblacion,CodPostal,Provincia, idFormaPago, FormaPago, CtaFormaPago 
	        ,Direccion_Envio DomicilioEnvio, PoblacionEnvio,CodPostalEnvio,PROVINCIAENVIO
	        ,idSap, Nif, PrimFechaDetalle, PrimPresupuesto Presupuesto, isnull(PrimPedido,0) Pedido, PrimOrden Orden,TIVA, cl.idTipoIva
			, round(Base,2) Base,  round(Cuota,2) Cuota 	, round(round(Cuota,2)  + round(Base,2),2) Total
			,TPrecio, ISNULL(TotCos.TImporteCoste, 0) - ISNULL(TotEx.TImporteCoste, 0) TCostes
	        ,ISNULL(TotEx.TImporteCoste, 0) TImporteExistencias, isnull(idFacAbonada , 0) idFacAbonada	  ,ISNULL(TotCos.TImporteCoste, 0) Coste, CodExterno
	        FROM Facturas Fac			
	            LEFT  JOIN ( SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(1)  NReg, SUM(CANTIDAD) TCantidad,  SUM(Precio)  TPrecio
                    --, SUM(ROUND(Precio * ISNULL(IVA, 0) / 100, 2))  Cuota
                    , SUM(Precio * ISNULL(IVA, 0) / 100)  Cuota
			        ,MIN(Presupuesto) AS PrimPresupuesto 	,isnull(MIN(IVA),0)  TIVA	,MIN(idTipoIva) AS idTipoIva
			        ,SUM(ROUND(Precio, 2)) AS Base, MAX(Fecha) PrimFechaDetalle					
				        FROM Facturas_Detalles		
				        GROUP BY Ejercicio, IdFactura, IdEmpresa  )	Tot 
					        ON Fac.IdEmpresa = Tot.IdEmpresa AND Fac.Ejercicio = Tot.Ejercicio AND Fac.IdFactura = Tot.IdFactura       						
	        LEFT JOIN (SELECT  Ejercicio, IdFactura, IdEmpresa, SUM(Importe) TImporteCoste 
		        FROM Costes WHERE IdProveedor = 1002 GROUP BY Ejercicio, IdFactura, IdEmpresa) TotEx	
		        ON Fac.IdEmpresa = TotEx.IdEmpresa	AND Fac.Ejercicio = TotEx.Ejercicio AND Fac.IdFactura = TotEx.IdFactura
	        LEFT JOIN (SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(ISNULL(Importe, 0)) TImporteCoste	
		        FROM Costes WHERE IdProveedor <> 1143 GROUP BY Ejercicio, IdFactura, IdEmpresa) TotCos
			        ON Fac.IdEmpresa = TotCos.IdEmpresa	AND Fac.Ejercicio = TotCos.Ejercicio AND Fac.IdFactura = TotCos.IdFactura   						
	        LEFT JOIN (
		        SELECT  Factura, EjercicioFactura, IdEmpresa, SUM(1)  NRegAlba, MIN(CodSpatam) AS PrimOrden, MIN(NPedido) PrimPedido
		        FROM Albaranes		        
			        GROUP BY IdEmpresa, EjercicioFactura, Factura  
	        ) Alb
			        ON Fac.IdEmpresa = Alb.IdEmpresa	
			        AND Fac.Ejercicio = Alb.EjercicioFactura 
			        AND Fac.IdFactura = Alb.Factura   				 							
	        LEFT JOIN Clientes Cl ON Fac.IdCliente = Cl.IdCliente
	        LEFT JOIN (SELECT Codigo, Texto FormaPago, Texto2 CtaFormaPago FROM Tablas WHERE TipoTabla = 'FPAG') Fpg
		        ON idFormaPago = Fpg.Codigo
			        WHERE Fac.idFactura = @idFactura  AND Fac.Ejercicio = @Ejercicio
            ";
        
        using (SqlConnection conn = new SqlConnection(Conex)) {
            using (SqlCommand cmd = new SqlCommand(query, conn)) {
                conn.Open();
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
                cmd.Parameters["@idFactura"].Value = idFactura;
                cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));            
                cmd.Parameters["@Ejercicio"].Value = Ejercicio;
                SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (Lector.Read()) {
                    RegFactura.idFactura = int.Parse(Lector["idFactura"].ToString());
                    RegFactura.FechaFactura = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();
                    RegFactura.Ejercicio = int.Parse(Lector["Ejercicio"].ToString());
                    RegFactura.idCliente = int.Parse(Lector["idCliente"].ToString());
                    RegFactura.NombreCliente = Lector["TituloL"].ToString();
                    RegFactura.RazonSocial = Lector["RazonSocial"].ToString();
                    RegFactura.Estado = int.Parse(Lector["Estado"].ToString());
                    RegFactura.Referencia = Lector["Referencia"].ToString();
                    RegFactura.FContabilizada = (Lector["FContabilizada"] != DBNull.Value ? Convert.ToDateTime(Lector["FContabilizada"]).ToShortDateString() : "");
                    RegFactura.Domicilio = Lector["Domicilio"].ToString();
                    RegFactura.Poblacion = Lector["Poblacion"].ToString();
                    RegFactura.CodPostal = Lector["CodPostal"].ToString();
                    RegFactura.Provincia = Lector["Provincia"].ToString();
                    RegFactura.DomicilioEnvio = Lector["DomicilioEnvio"].ToString();
                    RegFactura.PoblacionEnvio = Lector["PoblacionEnvio"].ToString();
                    RegFactura.CodPostalEnvio = Lector["CodPostalEnvio"].ToString();
                    RegFactura.ProvinciaEnvio = Lector["ProvinciaEnvio"].ToString();
                    RegFactura.idSap = Lector["idSap"].ToString();
                    RegFactura.Nif = Lector["Nif"].ToString();
                    RegFactura.Presupuesto = Lector["Presupuesto"].ToString();
                    RegFactura.Total = float.Parse(Lector["Total"].ToString());
                    RegFactura.TIva = float.Parse(Lector["TIVA"].ToString());
                    RegFactura.Base = float.Parse(Lector["Base"].ToString());
                    RegFactura.Cuota = float.Parse(Lector["Cuota"].ToString());
                    RegFactura.TCostes = float.Parse(Lector["TCostes"].ToString());
                    RegFactura.TImporteExistencias = float.Parse(Lector["TImporteExistencias"].ToString());
                    RegFactura.TPrecio = float.Parse(Lector["TPrecio"].ToString());
                    RegFactura.idFacAbonada = int.Parse(Lector["idFacAbonada"].ToString());
                    RegFactura.PrimFechaDetalle = Convert.ToDateTime(Lector["PrimFechaDetalle"]).ToShortDateString();
                    RegFactura.FormaPago = Lector["FormaPago"].ToString();
                    RegFactura.CtaFormaPago = Lector["CtaFormaPago"].ToString();
                    RegFactura.Pedido = Lector["Pedido"].ToString();
                    RegFactura.Orden = Lector["Orden"].ToString();
                    RegFactura.Coste = float.Parse(Lector["Coste"].ToString());
                    RegFactura.CodExterno = Lector["CodExterno"].ToString();
                    RegFactura.idCadena = int.Parse(Lector["idCadena"].ToString());                
                }   
            }
        }
        return RegFactura;
    }// recCabecera--  
   
    public List<DetalleFactura> recDetalles() {
        // Recupera lista de detalles de factura en objeto tipo List --
        
        List<DetalleFactura> DetFacturas = new List<DetalleFactura>();
        DetalleFactura LinDetFactura = new DetalleFactura(); // crea la primera linea --            
        using (SqlConnection conn = new SqlConnection(Conex))        {
            string query = @"
            SELECT idFacturaDetalles, Det.Fecha, Nombre, Cantidad , ROUND(precio, 2) Precio
                , idTipoIva, isnull(IVA,0) IVA, Albaranes, Presupuesto, Presupuesto2, Comentario 
		        ,NPedido, CodSpatam Orden      , isnull(ROUND(Precio / NULLIF(CANTIDAD, 0), 5),0)  AS PrecioUnitario
		        , ROUND(Precio * ISNULL(IVA, 0) / 100, 4) AS ImporteIva   , ROUND(Precio + ROUND(Precio * ISNULL(IVA, 0) / 100, 2), 2) AS Total
	        FROM Facturas_Detalles Det
		        LEFT JOIN Albaranes Alb 		ON Det.IdAlbaran = Alb.IdAlbaran			
	            WHERE idFactura = @idFactura  AND Ejercicio = @Ejercicio
	        ORDER BY idFacturaDetalles                ";

            using (SqlCommand cmd = new SqlCommand(query, conn)) {
                conn.Open();
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
                cmd.Parameters["@idFactura"].Value = idFactura;
                cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));                
                cmd.Parameters["@Ejercicio"].Value = Ejercicio;

                SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (Lector.Read()) {
                    DetFacturas.Add(LinDetFactura);
                    LinDetFactura.idFacturaDetalles = int.Parse(Lector["idFacturaDetalles"].ToString());
                    LinDetFactura.Fecha = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();
                    LinDetFactura.Nombre = Lector["Nombre"].ToString();                    
                    LinDetFactura.Cantidad  = Lector["Cantidad"] == DBNull.Value ? 0 : int.Parse(Lector["Cantidad"].ToString()); // evita null --
                    LinDetFactura.Precio = float.Parse(Lector["Precio"].ToString());
                    LinDetFactura.idTipoIva = int.Parse(Lector["idTipoIva"].ToString());
                    LinDetFactura.IVA = float.Parse(Lector["IVA"].ToString());
                    LinDetFactura.PrecioUnitario = float.Parse(Lector["PrecioUnitario"].ToString());
                    LinDetFactura.ImporteIva = float.Parse(Lector["ImporteIva"].ToString());
                    LinDetFactura.Total = float.Parse(Lector["Total"].ToString());
                    LinDetFactura.Albaranes = Lector["Albaranes"].ToString();
                    LinDetFactura.HRuta = Lector["Presupuesto"].ToString();
                    LinDetFactura.Comentario = Lector["Comentario"].ToString();
                    LinDetFactura.Presupuesto = Lector["Presupuesto2"].ToString();

                    LinDetFactura = new DetalleFactura(); // crea nueva linea --
                }
            }
        }
        return DetFacturas;

    } // recDetalles --

    public List<DetalleCoste> recDetallesCoste() {
        // Recupera lista de costes de factura en objeto tipo List --
        List<DetalleCoste> DetCostes = new List<DetalleCoste>();
        DetalleCoste LinDetCoste = new DetalleCoste(); // crea la primera linea --        
        using (SqlConnection conn = new SqlConnection(Conex))       {
            string query = @"SELECT idCoste, idFactura, Cos.idProveedor, Titulo NombreProveedor,  isnull(idSap,0) idSapProveedor, Importe, Presupuesto
            FROM Costes Cos
	        LEFT JOIN Proveedores Pro on Cos.idProveedor = Pro.idProveedor
                WHERE Cos.idFactura = @idFactura  AND Cos.Ejercicio = @Ejercicio
                    AND Cos.IdProveedor <> 1143;  ";    //  28/7/17 - se excluye  la cta. 1143 (portes) de los costes 
            using (SqlCommand cmd = new SqlCommand(query, conn)) {
                conn.Open();

                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));
                cmd.Parameters["@idFactura"].Value = idFactura;
                cmd.Parameters["@Ejercicio"].Value = Ejercicio;

                SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (Lector.Read()) {
                    DetCostes.Add(LinDetCoste);
                    LinDetCoste.idCoste = int.Parse(Lector["idCoste"].ToString());
                    LinDetCoste.idFactura = int.Parse(Lector["idFactura"].ToString());
                    LinDetCoste.idProveedor = int.Parse(Lector["idProveedor"].ToString());
                    LinDetCoste.Importe = float.Parse(Lector["Importe"].ToString());
                    LinDetCoste.Presupuesto = Lector["Presupuesto"].ToString();
                    LinDetCoste.NombreProveedor = Lector["NombreProveedor"].ToString();
                    LinDetCoste.idSapProveedor = Lector["idSapProveedor"].ToString();

                    LinDetCoste = new DetalleCoste(); // crea nueva linea --
                }
            }
        }
        return DetCostes;
    }// recDetallesCoste--  

    public List<tiposIVA> recIvas() {
        // Recupera información de tipos de IVA de los detalles de la factura, 2 maximos --	
        List<tiposIVA> listaIvas = new List<tiposIVA>();

        string query = @" SELECT Ejercicio,idFactura,  idTipoIva, LitIVA, PorIva , sum(Precio) Total
        FROM Facturas_Detalles  det
	        LEFT JOIN (SELECT Codigo, Texto2 LitIVA, Importe PorIVA FROM Tablas WHERE TipoTabla = 'IVA'
        ) tivas ON det.idTipoIva = tivas.Codigo
        WHERE det.idFactura = @idFactura  AND det.Ejercicio = @Ejercicio
        GROUP BY Ejercicio,idFactura, idTipoIva, LitIVA, PorIva";        
        using (SqlConnection conn = new SqlConnection(Conex))       {
            using (SqlCommand cmd = new SqlCommand(query, conn)) {
                conn.Open();
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
                cmd.Parameters["@idFactura"].Value = idFactura;
                cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));                
                cmd.Parameters["@Ejercicio"].Value = Ejercicio;

                SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                tiposIVA linTipoIVA = new tiposIVA(); // crea la primera linea de lista de clase  --
                // Solo lee un maximo de 2 tipos de IVA, crea simepre 2 lineas de LisIvas --
                if (Lector.Read()) { 
                    listaIvas.Add(linTipoIVA);         
                    linTipoIVA.porIVA = float.Parse(Lector["PorIva"].ToString()); 
                    linTipoIVA.importeIVA = float.Parse(Lector["Total"].ToString()); 
                    linTipoIVA.LitIVA = '#'+Lector["LitIVA"].ToString();
                }
                linTipoIVA = new tiposIVA(); // crea 2ª linea de lista de clase y lo inicializa a ceros --
                listaIvas.Add(linTipoIVA);                    
                linTipoIVA.LitIVA = ""; linTipoIVA.importeIVA = 0; linTipoIVA.importeIVA = 0;
                // lee el segundo reg. de IVA, si existe, 
                if (Lector.Read()) {                                    
                    linTipoIVA.porIVA = float.Parse(Lector["PorIva"].ToString()); 
                    linTipoIVA.importeIVA = float.Parse(Lector["Total"].ToString()); 
                    linTipoIVA.LitIVA = '#' + Lector["LitIVA"].ToString();
                }                
            }
        };
        return listaIvas;
    }// recIvas-- 
            
    public int generarSAPFacturaCabecera() {                                 
        // recupera información de la cabecera de factura, anulado 18/7/18, ahora se recupera previamente 
            //RegCabecera = recCabecera();        

        // recupera definición de datos contables SAP de factura en objeto json 
        //jsonObj = iwsGlFactura.genJsonDefinicion("FacturasSAP.json");
        // si la factura ya esta contabilizada, SALE CON CODIGO -1, (no necesario, se valida antes)
        if (RegCabecera.FContabilizada != "")
            return -1;
            //return "Factura: " + idFactura.ToString() + " YA contabilizada el :" + RegCabecera.FContabilizada + "\r\n";
                
        // Recupera información de tipos de IVA los detalles de la factura, normalmente solo 2 lineas  --             
        listaIvas = recIvas();
        
        // Cabecera SAP ----------------------------------------
        JToken jsCab = jsonObj["CabeceraFacSAP"];
        // vacia linea JSON , recorre todos los elementos de un nodo json --
        foreach (JToken child in jsCab.Children()) {
            var property = child as JProperty;
            // ref:para referenciar tambien la clave (property.Name)
            //  Resul += property.Name + ":" + property.Value+";";         
            property.Value= "";        
        };
    
        //jsonObj["CabeceraFacSAP"]["VERSION"] = "v->HOLA"; // ref: asigna valor a dato de un subnodo, ok --  
        //foreach (JToken child in jsonObj.Children()) -- // ref: Todos los nodos      
        jsCab["VERSION"] = "1.0 beta"; 
        jsCab["TIPOFUENTE"] = "ARTESGRAFICAS";
        jsCab["SOCIEDADSAP"] = "HGA";
        jsCab["CANAL"]  += "INT";
        jsCab["RAZONSOCIAL"] = RegCabecera.RazonSocial;
        jsCab["CALLENUM"] = RegCabecera.Domicilio;
        jsCab["CODIGOPOSTAL"] = RegCabecera.CodPostal;
        jsCab["POBLACION"] = RegCabecera.Poblacion;
        jsCab["PROVINCIA"] = RegCabecera.Provincia;
        jsCab["CODIGOSAP"] = RegCabecera.idSap;
        jsCab["CODIGOEXTERNO"] = RegCabecera.idCliente;
        jsCab["PAIS"] = "ES";
        jsCab["IDFISCAL"] = RegCabecera.Nif;
        jsCab["NUMEROFACTURA"] = "HGA" + RegCabecera.Ejercicio + '/' + RegCabecera.idFactura.ToString("000000");
        jsCab["TIPOFACTURA"] = (RegCabecera.idFacAbonada == 0) ? "FAC" : "REC"; 
        jsCab["FECHAFACTURA"] = Convert.ToDateTime(RegCabecera.FechaFactura).ToString("yyyyMMdd");
        jsCab["MONEDA"] = "EUR";    
        jsCab["CENTRO"] = RegCabecera.CodExterno;
        jsCab["NUMEROFACTURAREF"] = (RegCabecera.idFacAbonada == 0) ? "" : "HGA" + RegCabecera.Ejercicio + "/"+RegCabecera.idFacAbonada.ToString("000000"); // si abono, indica nº que se abona 
        jsCab["REFERENCIA1"] = (RegCabecera.Pedido == "0" ) ? RegCabecera.Presupuesto + "" : RegCabecera.Pedido; // Pedido o Presupuesto si pedido es 0                   
        jsCab["REFERENCIA2"] = RegCabecera.Presupuesto;     
        jsCab["DESCRIPCIONFACTURA"] = "TOTAL FACTURA Nº: " + RegCabecera.idFactura;        
        //Usa "." como separador decimal           
        jsCab["TOTALFACTURA"] =string.Format(CultureInfo.InvariantCulture,"{0,6:#####0.00}", RegCabecera.Total);
        //  18/7/18, misma informacion que la referencia en factura impresa --        
        jsCab["ANALITICA"] = RegCabecera.Referencia;
        //  6/8/18,  si se trata de la cadena 'BE LIVE' (2), duplica en columna AA - ANALITICA el dato  de REFERENCIA1
        if (RegCabecera.idCadena == 2) 
            jsCab["ANALITICA"] = jsCab["REFERENCIA1"];
        
        jsCab["CLASEIMPUESTO4"]= listaIvas[0].LitIVA;
        jsCab["PORCENTAJEIMPUESTO4"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", listaIvas[0].porIVA);
        jsCab["BASEIMPONIBLE4"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", listaIvas[0].importeIVA);
        jsCab["CUOTA4"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegCabecera.Cuota);
        
        // Genera linea de texto con todos los valores del nodo                      
        string linTex = iwsGlFactura.stringJson(jsCab, ";");
        
        #if (CREARCONTABLE)
            //  crea Linea de cabecera en la BD      --
            int IdGagSap = crearLineaContable( idFactura, Ejercicio, 1, 1, fechaCierre, linTex);
            // Genera linea en fichero de texto con la misma informacion de GAGSAP , para referencia , NO  necesaria, anular en el futuro  --                               
            #if (CREAREXCELCSV)
                iwsGlFactura.insertaExcelCsvFactura(linTex);
            #endif
        #else
            int IdGagSap = 0;
        #endif

        
        return IdGagSap;
    }// generarSAPFacturaCabecera 

    public void generarSAPFacturaDetalles() {             
        // recupera información del los detalles de la factura --        
        DetFactura = recDetalles();
         
        // recupera descripcion de registro de costes de fichero JSON de texto en objeto Jtoken --       
        JToken jsDet = jsonObj["DetallesFacSAP"][0];   
        // vacia datos de objetos JSON 
        foreach (JToken child in jsDet.Children()) { ((JProperty)child).Value = "";    };
        
        // recorre detalles recuperados --
        string linTex = "";
        foreach (DetalleFactura lin in DetFactura)    {            
            linTex = "";
            jsDet["VERSION"] = "1.0 beta"; 
            jsDet["TIPOFUENTE"] = "ARTESGRAFICAS";
            jsDet["SOCIEDADSAP"] = "HGA";
            jsDet["CANAL"]  = "INT";
            jsDet["NUMEROFACTURA"] = "DESGLOSE";
            // añadido 15/10/18
            jsDet["CENTRO"] = RegCabecera.CodExterno;
            // añadido el 18/7/18, mismo valor que en cabecera (REFERENCIA1) 
            jsDet["REFERENCIA3"] = (RegCabecera.Pedido == "0") ? RegCabecera.Presupuesto + "" : RegCabecera.Pedido; // Pedido o Presupuesto si pedido es 0           

            jsDet["DESCRIPCIONFACTURA"] = lin.Nombre;
            jsDet["REFERENCIACONCEPTO"] = (lin.Presupuesto != "" && lin.Presupuesto != "0") ? lin.Presupuesto : lin.HRuta;            
            jsDet["CODIGOCONCEPTO"] = "VTOTAL";
            jsDet["REFERENCIA2"] = Convert.ToDateTime(lin.Fecha).ToString("yyyyMMdd");
            jsDet["IMPORTE"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", lin.Precio);
            jsDet["PORCENTAJE IMPUESTO1"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", lin.IVA);
            jsDet["DESCRIPCIONCONCEPTO1"] = RegCabecera.idSap; // dato de la cabecera 

            // Genera linea de texto con todos los valores del nodo                   
            linTex += iwsGlFactura.stringJson(jsDet,";");
            #if (CREARCONTABLE)
                // crea Linea de detalle en la BD con ese texto   -
                int IdGagSap = crearLineaContable( idFactura, Ejercicio, 1, 1, fechaCierre, linTex);
                // Genera linea en fichero de texto con la misma informacion de GAGSAP , para referencia , NO  necesaria, anular en el futuro  --                               
                #if (CREAREXCELCSV)
                    iwsGlFactura.insertaExcelCsvFactura(linTex);
                #endif
            #endif            
        };                        
        //return "Detalle Id:datos: " + LinDet;
    }// generarSAPFacturaDetalles 

    public void generarSAPCostes() {                     
        // recupera información del los costes de la factura --            
         DetCostes = recDetallesCoste();
          
        // recupera descripcion de registro de costes de fichero JSON de texto en objeto Jtoken --       
        JToken jsCoste = jsonObj["CostesSAP"];   
        // vacia datos de objetos JSON 
        foreach (JToken child in jsCoste.Children()) { ((JProperty)child).Value = "";    };
       
        // recorre detalles de costes recuperados --
        int orden = 1;  // secuencia incremental para cada coste, comienza en 1 --
        string linTex = "";        
        foreach (DetalleCoste lin in DetCostes) {
            //LinDet+= "det nº:"+lin.idFacturaDetalles.ToString();
            linTex = "";
            jsCoste["VERSION"] = "1.0 beta";
            jsCoste["TIPOFUENTE"] = "ARTESGRAFICAS";
            jsCoste["SOCIEDADSAP"] = "HGA";
            jsCoste["CANAL"] = "INT";            
            if (lin.idProveedor == 1002) {
                jsCoste["TIPODOC"] = "PCINT";                
            }
            else {
                jsCoste["TIPODOC"] = "PCEXT";
                jsCoste["RAZONSOCIAL"] = lin.NombreProveedor;
                jsCoste["CODIGOEXTERNO"] = lin.idProveedor;
                jsCoste["CODIGOSAP"] = lin.idSapProveedor;                        
            }            
            // Fecha Documento, en costes fijos la de la factura
            jsCoste["FECHADOCUMENTO"] = Convert.ToDateTime(RegCabecera.FechaFactura).ToString("yyyyMMdd");
            
            // Referencia1 -1er. presupuesto ,   Si no hay poner el presupuesto el coste mod. 18/101/17 ,  COL K
            if (RegCabecera.Presupuesto != "")
                jsCoste["REFERENCIA1"] = RegCabecera.Presupuesto;            
            else                
                jsCoste["REFERENCIA1"] = lin.Presupuesto;

            jsCoste["REFERENCIA2"] = lin.Presupuesto;
            jsCoste["REFERENCIA3"] = lin.idFactura;            
            if (lin.idProveedor == 1002)
                jsCoste["CODIGOCONCEPTO"] = "CPROPIOS"; // Codigo Concepto (Existencias) col N                                  
            else
                jsCoste["CODIGOCONCEPTO"] = "CSUBCON";// Codigo Concepto (Proveedor)  col N

            jsCoste["DESCRIPCIONCOSTE"] = "Coste Factura N " + lin.idFactura;   // Descripción coste COl O
            jsCoste["IMPORTE"] = string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", lin.Importe);
            jsCoste["MONEDA"] = "EUR";
            jsCoste["IDUNICO"] = lin.idCoste;                              // IDUNICO Col S

            // Genera linea de texto con todos los valores del nodo                   
            linTex += iwsGlFactura.stringJson(jsCoste, ";");

            crearLineaContable(idFactura, Ejercicio, 2, orden, fechaCierre, linTex);
            #if (CREAREXCELCSV)
                iwsGlFactura.insertaExcelCsvCoste(linTex); // Genera linea en fichero de texto/CSV de costes --
            #endif
            orden++;
        };                
       
    }// generarSAPCostes --

    public void insertarImagenPdf(int idgagSap)       {            
        string Doc = iwsGlFactura.Server.MapPath("/Facturas_SAP/Fact_" + idFactura +"_"+ Ejercicio + ".pdf");  
                  
        // asigna el stream y lo añade a campo Oracle --              
        var blobByte = genBinFacturaPdf();
        
        //using (OracleConnection conex1 = new OracleConnection(ConexORCL)) {
        using (SqlConnection conex1 = new SqlConnection(ConexORCL)) {
            conex1.Open();                 
            string query = @"UPDATE GAGSAP SET Documento = :blopfile WHERE idGagSAP = :idGag";      
            using (SqlCommand cmd = new SqlCommand(query, conex1))       {
                // definir los parametros en el mimos order de la linea 
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter(":blopfile", DbType.Binary                 ));
                cmd.Parameters[":blopfile"].Value = blobByte;
                cmd.Parameters.Add(new SqlParameter(":idGag", DbType.Int32));
                cmd.Parameters[":idGag"].Value = idgagSap;
                       
                cmd.ExecuteNonQuery();
            }
            conex1.Close();
        }
             
        // 8/3/18, ahora se genera tambien factura en disco para acceder generarla con URL --            
        Stream ms = new MemoryStream(blobByte);    
        // guarda el memory stream en un fichero fisico --     
        using (FileStream file = new FileStream(Doc, FileMode.Create, System.IO.FileAccess.Write)) {
           byte[] bytes = new byte[ms.Length];
           ms.Read(bytes, 0, (int)ms.Length);
           file.Write(bytes, 0, bytes.Length);
           ms.Close();
        }            
    }// insertarImagenPdf --

    public void ImprimeFac() {  
        // recupera información de la cabecera de factura --
        RegCabecera = recCabecera();
        // recupera información del los detalles de la factura --        
        DetFactura = recDetalles();
        //Debug.WriteLine("DetFactura:" + new JavaScriptSerializer().Serialize(DetFactura));

        
        string nuevoDoc = iwsGlFactura.Server.MapPath("facturas/Fact_"+ idFactura.ToString() + "_"+ Ejercicio.ToString() + ".pdf");                               
        var blobByte = genBinFacturaPdf();
    
        // convierte byte[] en stream --
        Stream ms = new MemoryStream(blobByte);
    
        // guarda el memory stream en un fichero fisico --     
        using (FileStream file = new FileStream(nuevoDoc, FileMode.Create, System.IO.FileAccess.Write)) {
           byte[] bytes = new byte[ms.Length];
           ms.Read(bytes, 0, (int)ms.Length);
           file.Write(bytes, 0, bytes.Length);
           ms.Close();
        }    
    }// ImprimeFac

    public void subirFtp(){
        // transfiere factura generada en bytes[] al FTP --
        

        // aqui, 13/11/19 -- probar a crear el fichero en disco para comprobación --
        string nuevoDoc = "Fact_"+ idFactura.ToString() + "_"+ Ejercicio.ToString() + ".pdf";                               


        string localPath = HostingEnvironment.ApplicationPhysicalPath + @"\facturas\";
        string uriFtp = "ftp://"+ WebConfigurationManager.AppSettings["FtpFacturasSAP"] + "/Facturas_HGA/"+ nuevoDoc;            

        var blobByte = genBinFacturaPdf();                
        Stream ms = new MemoryStream(blobByte);

        /*usando Secure Ftp (ok), no necesario para este ftp, no es secure    --       
        using (SftpClient sftp = new SftpClient("ftp.globalia-corp.com", 22, "Estampador_pro", "85est4mpador58")) {
        sftp.Connect();
            sftp.ChangeDirectory(@"\outbox\tmp\");                
            sftp.UploadFile(ms, nuevoDoc);            
            sftp.Disconnect();                         
        };
        */
        // Usando no secure ftp, no necesita Renci.SshNet ni Renci.SshNet.dll, Ok            
        FtpWebRequest request;
       // request = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://ftp.globalia-corp.com/Facturas_HGA/"+ nuevoDoc));            
        request = (FtpWebRequest)FtpWebRequest.Create(new Uri(uriFtp));            
        request.Method = WebRequestMethods.Ftp.UploadFile;  

        request.Credentials = new NetworkCredential (WebConfigurationManager.AppSettings["FTP_US"], "FacturasS4P");   // FtpFacturasSAP_DES pass:Graficas1       
        request.ContentLength = blobByte.Length;  
        Stream requestStream = request.GetRequestStream();          
        requestStream.Write(blobByte, 0, blobByte.Length);          
        requestStream.Close();
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
        response.Close();        
    }// subirFtp

    public byte[] genBinFacturaPdf() {
        // Requiere recuperar antes datos de factura en RegCabecera y DetFactura
        Document doc = new Document();
        using (MemoryStream output = new MemoryStream())   {
            PdfWriter writer = PdfWriter.GetInstance(doc, output);        
            doc.Open();                
            doc.AddAuthor("Pedro Lopez Castro");
            // constantes 
            const int lineasAlbaran = 15;       // nº de lineas de albaran 
            const int linInicDetalle= 480;      // Principio de la linea de detalle 
            const int linSepara= 15;            // separación entre lineas  
            // alineaciones de texto --
            const byte alDr = PdfContentByte.ALIGN_RIGHT;
            const byte alIz = PdfContentByte.ALIGN_LEFT;  

            int Lineas = 0, Pagina = 1;
            string[] multiLin; // para imprimir una cadena de texto en multiple lineas sin cortar usando  cortarEspacios 

            // Fuentes usados --
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);     
            BaseFont bfN = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);        // para negrita, bold 
   
            PdfContentByte cb = writer.DirectContent;
            
            CabeceraFacturaPdf(cb, doc, Pagina, ref writer);    
            // detalles ----    
            int Px =66, Py= linInicDetalle;
            // 7/5/18 -- ahora usa los detalles recuperados antes en la clase 
            //Debug.WriteLine("DetFactura reg:" + new JavaScriptSerializer().Serialize(DetFactura));
                        
            foreach (DetalleFactura LinDetFactura in DetFactura) {     
                //Debug.WriteLine("Precio:" + LinDetFactura.Precio);                
                if (Lineas > lineasAlbaran)     {
                    Py = 112;
                    cb.ShowTextAligned(alDr, "Suma y Sigue", Px+476, Py, 0); 
                    Pagina++;                    
                    CabeceraFacturaPdf(cb, doc, Pagina, ref writer);
                    Lineas = 0; 
                    Py = linInicDetalle; // se situa en primera lineas --
                    //cb.SetFontAndSize(bf, 8);// Retorna a la fuente actual de los detalles --
                }
                cb.BeginText();
                cb.SetFontAndSize(bf, 10); 
        
                if (LinDetFactura.Albaranes.Length > 7)
                    cb.ShowTextAligned(alDr, LinDetFactura.Albaranes.Substring(0, 7) , Px, Py, 0);   // Albaran            
                else
                    cb.ShowTextAligned(alDr, LinDetFactura.Albaranes, Px, Py, 0);

                // Col. 2, Presupuesto  (Presupuesto u hoja de ruta si este esta vacio)                
                string tmPpto = (LinDetFactura.Presupuesto != "" && LinDetFactura.Presupuesto != "0") ? LinDetFactura.Presupuesto : LinDetFactura.HRuta;
                
                cb.ShowTextAligned(alDr, tmPpto, Px+72, Py, 0);                  
                
                cb.ShowTextAligned(alDr, LinDetFactura.Cantidad.ToString(), Px+113, Py, 0);        // Cantidad            
                cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00000}", LinDetFactura.PrecioUnitario), Px + 176, Py, 0);
                cb.SetFontAndSize(bfN, 8);         
        
                multiLin = Utilidades.cortarEspacios(LinDetFactura.Nombre.Trim(), 50); // retorna array con lineas cortadas --
                foreach (string st2 in multiLin) {
                    cb.ShowTextAligned(alIz, st2, Px +184, Py, 0);
                    Lineas++; Py-= linSepara; 
                }
                Lineas--;  Py+= linSepara; // al recorrer el array se genera un salto de mas, retrocede una linea --                                  

                // si hay comentario, genera nuevas lineas --
                if (LinDetFactura.Comentario.Trim() != "") { 
                    Py-= linSepara; Lineas++; // salto de linea en comentario --
                    cb.SetFontAndSize(bf, 8);
                    multiLin = Utilidades.cortarEspacios(LinDetFactura.Comentario.Trim(), 60);
                    //Debug.WriteLine("*****"+ LinDetFactura.Presupuesto+ ", Cantidad: "+ LinDetFactura.Cantidad.ToString()+" *****");  
                    foreach (string st2 in multiLin)       {
                        cb.ShowTextAligned(alIz, st2, Px + 184, Py, 0);
                        //Debug.WriteLine("-> " + st2);  
                
                        Lineas++; Py -= linSepara;
                    }
                    Lineas--; Py += linSepara; 
                }                
                cb.SetFontAndSize(bf, 10); 
                cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,15:##,##0.00}", LinDetFactura.Precio), Px+474, Py, 0);  
                cb.EndText();
                Py-= linSepara; Lineas++; // salto de linea --
            }
              
            // ref: añade otra pagina vacia --doc.NewPage();  doc.Add(new Paragraph("2ª Pagina 2"));
            
            // Pie de factura -- 
            cb.BeginText();
            Px=26; Py= 138 ; 
            cb.SetFontAndSize(bf, 9);  
            cb.ShowTextAligned(alIz,  RegCabecera.FormaPago, Px, Py, 0); 
            cb.ShowTextAligned(alIz,  RegCabecera.CtaFormaPago, Px, Py-linSepara, 0); 
            cb.SetFontAndSize(bfN, 11);  
            cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", RegCabecera.Base), Px+300, Py, 0); 
            cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", RegCabecera.TIva), Px+380, Py, 0);

            // cambiado 18/5/18, se elimina redondeo y no se usa el campo Total de SQL, se suman
            //cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", Math.Round(RegCabecera.Cuota,2)), Px+440, Py, 0);
            //cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", Math.Round(RegCabecera.Total,2)), Px+510, Py, 0);
            //cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", RegCabecera.Total), Px + 510, Py, 0);

            cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", RegCabecera.Cuota), Px + 440, Py, 0);            
            cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", RegCabecera.Total), Px + 510, Py, 0);
            cb.EndText();           

            doc.Close();                                       
            return output.ToArray();              
        }        
    }// genBinFacturaPdf --

    protected void CabeceraFacturaPdf(PdfContentByte cb, Document doc, int Pagina, ref PdfWriter writerP) {
        string Plantilla = iwsGlFactura.Server.MapPath("Plantilla_Factura.pdf");
    
        //lee el pdf de la plantilla y lo adjunta al documento     --
        PdfReader reader = new PdfReader(Plantilla);
        PdfImportedPage page1 = writerP.GetImportedPage(reader, 1);

        doc.NewPage();
        cb.AddTemplate(page1, 1, 1);
        reader.Close();
    
        // Fuentes usados --
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);     
        BaseFont bfN = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);        // para negrita, bold        
    
        // alineaciones de texto --
        byte alDr = PdfContentByte.ALIGN_RIGHT;
        byte alIz = PdfContentByte.ALIGN_LEFT;
    

        // Cabecera , Razon Social 
        int Px = 290, Py = 748;    
        cb.BeginText();
        cb.SetFontAndSize(bfN, 10);                 
        cb.ShowTextAligned(alIz, RegCabecera.RazonSocial, Px, Py, 0);                
        cb.SetFontAndSize(bf, 10);                 
        Py-=15;    
        cb.ShowTextAligned(alIz, RegCabecera.Domicilio, Px, Py, 0);
        Py-=15;        
        cb.ShowTextAligned(alIz, RegCabecera.CodPostal+' '+ RegCabecera.Poblacion, Px, Py, 0);    
        Py-=15;        
        cb.ShowTextAligned(alIz,  "CIF/DNI:"+' '+ RegCabecera.Nif, Px, Py, 0);    
        cb.EndText();

        // Cabecera , Direccion de envio 
        Px = 290; Py = 680;    
        cb.BeginText();
        cb.SetFontAndSize(bfN, 10);                 
        cb.ShowTextAligned(alIz, RegCabecera.NombreCliente, Px, Py, 0); // nombre comercial --
        Py-=15;
        cb.ShowTextAligned(alIz, RegCabecera.DomicilioEnvio, Px, Py, 0);        
        cb.SetFontAndSize(bf, 10);                 
        Py-=15;
        cb.ShowTextAligned(alIz, RegCabecera.CodPostalEnvio + '-'+ RegCabecera.PoblacionEnvio, Px, Py, 0);    
        Py-=15;
        cb.ShowTextAligned(alIz, RegCabecera.ProvinciaEnvio, Px, Py, 0); 
        cb.EndText();

        const string NifGlobalia = "ES B37056041";
        // Texto con NIF de globalia
        Px =26;   Py= 620;
        cb.SetFontAndSize(bfN, 9); 
        cb.BeginText();
        cb.ShowTextAligned(alIz, NifGlobalia, Px, Py,0);
        cb.EndText();
    
        // Linea inferior de cabecera 
        Px =26;   Py= 542;
        cb.SetFontAndSize(bfN, 8); 
        cb.BeginText();
        cb.ShowTextAligned(alIz, RegCabecera.FechaFactura, Px, Py,0);
        // 9/5/18 se eliminan estos campos de la cabecera, se mantiene el pedido --
        //  cb.ShowTextAligned(alIz, RegCabecera.Presupuesto, Px+77, Py,0);        
        cb.ShowTextAligned(alIz, RegCabecera.Pedido, Px+126, Py,0);            

        //  cb.ShowTextAligned(alIz, RegCabecera.Orden, Px+170, Py,0);   
        cb.ShowTextAligned(alIz, RegCabecera.Referencia, Px+224, Py,0);            
         
        cb.ShowTextAligned(alIz, "HGA"+ RegCabecera.Ejercicio+"/"+ RegCabecera.idFactura.ToString("000000"), Px+450, Py, 0);            
        cb.EndText();

    }// CabeceraFacturaPdf --

    public void marcaFechaFactura() {        
        using (SqlConnection conn = new SqlConnection(Conex)) {        
            conn.Open();
            string query = "UPDATE Facturas SET Fecha_Cierre = GETDATE() "+
                "  WHERE idFactura = "+ idFactura+
                " AND Ejercicio = "+ Ejercicio;
            using (SqlCommand cmd = new SqlCommand(query, conn)) {               
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }// marcaFechaFactura
            
}// class Factura 
         
[WebMethod]
public void ImprimeFactura(int idFactura, int Ejercicio) {            
    // Crea la clase con datos de factura -    
    clFactura fact = new clFactura(idFactura, Ejercicio);        
    fact.ImprimeFac();
     
}//ImprimeFactura--

public void insertaExcelCsvFactura(string Contenido){
    // añade linea a fichero de excel/CSV, de Facturas  con la misma informacion de GAGSAP  de Oracle --     
    //string TextoSAP = Server.MapPath("Informes/HGAF" + DateTime.Today.ToString("yyyyMMdd") + ".CSV");     
        
    string fExcel = Server.MapPath("Informes/HGAF" + DateTime.Today.ToString("yyyyMMdd") + ".CSV"); 
     // si no existe el fichero lo crea nuevo y añade cabecera        
    if (! File.Exists(fExcel))      {           
        string linCab = "VERSION;TIPOFUENTE;SOCIEDADSAP;CANAL;RAZONSOCIAL;CALLENUM;CODIGOPOSTAL;POBLACION;PROVINCIA;CODIGOSAP;CODIGOEXTERNO;PAIS;"+
         "IDFISCAL;NUMEROFACTURA;TIPOFACTURA;FECHAFACTURA;MONEDA;NUMEROFACTURAREF;CENTRO;REFERENCIA1;REFERENCIA2;REFERENCIA3;DESCRIPCIONFACTURA;" +
        "TOTALFACTURA;CODIGOCONCEPTO;REFERENCIACONCEPTO;ANALITICA;IMPORTE;PORCENTAJE IMPUESTO;DESCRIPCIONCONCEPTO;CODIGOCONCEPTO;REFERENCIACONCEPTO;" +
        "ANALITICA;IMPORTE;PORCENTAJEIMPUESTO;DESCRIPCIONCONCEPTO;CODIGOCONCEPTO;REFERENCIACONCEPTO;ANALITICA;IMPORTE;PORCENTAJEMPUESTO;" +
        "DESCRIPCIONCONCEPTO;CLASEIMPUESTO;PORCENTAJEIMPUESTO;BASEIMPONIBLE;BASEEXENTA;CUOTA;CLASEIMPUESTO;PORCENTAJEIMPUESTO;BASEIMPONIBLE;" +
        "BASEEXENTA;CUOTA;CLASEIMPUESTO;PORCENTAJEIMPUESTO;BASEIMPONIBLE;BASEEXENTA;CUOTA;CLASERETENCION;PORCENTAJERETENCION;BASERETENCION;" +
        "CUOTARETENCION;FORMACOBRO1;IMPORTE COBRO1;REFERENCIACOBRO1;FECHAVENCIMIENTO1;FORMACOBRO2;IMPORTE COBRO2;REFERENCIACOBRO2;FECHAVENCIMIENTO2;" +
        "DESCRIPCIONDESCUENTO;PORCENTAJEDESCUENTO;IMPORTEDESCUENTO";
           
        FileStream stream = new FileStream(fExcel, FileMode.Create, FileAccess.Write);
        StreamWriter wTextoSAP = new StreamWriter(stream);
        // Genera la linea de cabecera --
        wTextoSAP.Write(linCab + Environment.NewLine);    
        wTextoSAP.Close();     
    }
    // Añade la linea --
    FileStream stream2 = new FileStream(fExcel, FileMode.Append, FileAccess.Write);                  
    StreamWriter wTextoSAP2 = new StreamWriter(stream2);      
    wTextoSAP2.Write(Contenido +  Environment.NewLine);    
    wTextoSAP2.Close();    

 }// insertaExcelCsvFactura--

public void insertaExcelCsvCoste(string Contenido){
    // añade linea a fichero de excel/CSV de Costes,   con la misma informacion de GAGSAP  de Oracle --     
    //string TextoSAP = Server.MapPath("Informes/HGAF" + DateTime.Today.ToString("yyyyMMdd") + ".CSV");     
        
    string fExcel = Server.MapPath("Informes/HGAC" + DateTime.Today.ToString("yyyyMMdd") + ".CSV"); 
     // si no existe el fichero lo crea nuevo y añade cabecera        
    if (! File.Exists(fExcel))      {           
        string linCab = "VERSION;TIPOFUENTE;SOCIEDADSAP;CANAL;CENTRO;TIPODOC;RAZONSOCIAL;CODIGOEXTERNO;CODIGOSAP;FECHADOCUMENTO;" +
            "REFERENCIA1;REFERENCIA2;REFERENCIA3;CODIGOCONCEPTO;DESCRIPCIONCOSTE;IMPORTE;MONEDA;ANALITICA;IDUNICO;";
           
        FileStream stream = new FileStream(fExcel, FileMode.Create, FileAccess.Write);
        StreamWriter wTextoSAP = new StreamWriter(stream);
        // Genera la linea de cabecera --
        wTextoSAP.Write(linCab + Environment.NewLine);    
        wTextoSAP.Close();     
    }
    // Añade la linea --
    FileStream stream2 = new FileStream(fExcel, FileMode.Append, FileAccess.Write);                  
    StreamWriter wTextoSAP2 = new StreamWriter(stream2);      
    wTextoSAP2.Write(Contenido +  Environment.NewLine);    
    wTextoSAP2.Close();    

 }// insertaExcelCsvCoste--
      

[WebMethod]

 public string recCadenas(int activo, int Cadena) {
    // retorna datos transformados en JSON --
    // activo (1), 'false' (0), >1 Todos 	
    // cadena (0) Todas 
    List<Cadena> Lista = recCadenasLista(activo, Cadena);
    return new JavaScriptSerializer().Serialize(Lista); 
}//recCadenas


public List<Cadena> recCadenasLista( int activo, int Cadena) {
    // activo (1), 'false' (0), >1 Todos 	
    // cadena (0) Todas 
    List <Cadena> Lista = new List<Cadena>();
    Cadena Linea = new Cadena(); // crea la primera linea --
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"     
		SELECT Codigo,  Texto NombreCadena, Importe Activo, Texto3 ofCentral   
		FROM Tablas WHERE TipoTabla = 'CADE' 
            AND ((@Cadena = 0 OR Codigo = @Cadena))
		   AND (@Activo > 1  OR isnull(Importe, 0) = @Activo)
		ORDER By Texto";
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();           						
            cmd.Parameters.Clear();               
            cmd.Parameters.Add("@Activo", SqlDbType.Int).Value = activo;            
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;        
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			while (Lector.Read())            {
                Lista.Add(Linea);                
                Linea.Codigo = int.Parse(Lector["Codigo"].ToString());
                Linea.NombreCadena = Lector["NombreCadena"].ToString();           
                Linea.Activa =  int.Parse(Lector["Activo"].ToString());
                Linea.ofCentral = Lector["ofCentral"].ToString();  
                Linea = new Cadena(); // crea nueva linea  --                               
			};
		};        
	};
    return Lista;            
}// recCadenasLista --


[WebMethod]
public string recPaises( ) {           
    var linPais= new { Codigo = 0, Pais = "", Siglas="" };    
    var Lista = new[] { linPais }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"  SELECT Codigo, Texto Pais, Texto2, Texto3 Siglas
		FROM Tablas WHERE TipoTabla = 'PAIS' ORDER By Texto";    
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();            
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    Pais = Lector["Pais"].ToString(),
                    Siglas = Lector["Siglas"].ToString()
                 });
            }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); // retorna datos transformados en JSON --    
}// recPaises --

[WebMethod]
public string recTiposClientes(int Cadena) {           
    var linPais= new { Codigo = 0, TipoClienteHotel = "", CadenaTipoCliente = 0 };    
    var Lista = new[] { linPais }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @" SELECT Codigo, isnull(Texto,'') TipoClienteHotel, isnull(Importe,0) CadenaTipoCliente 
		    FROM Tablas WHERE TipoTabla = 'HTIP'
            AND (@Cadena = 0 OR Importe = @Cadena)";    

        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();               
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;                                      
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    TipoClienteHotel = Lector["TipoClienteHotel"].ToString(),
                    CadenaTipoCliente = int.Parse(Lector["CadenaTipoCliente"].ToString())
                });
            }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); // retorna datos transformados en JSON --    
}// recTiposClientes --


[WebMethod]
public string recTiposIva( ) {           
    var linIvas = new { Codigo = 0, TipoIva = "", SiglaTipoIVA = "", ImporteIVA = 0.0F };    
    var Lista = new[] { linIvas }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"SELECT Codigo, isnull(Texto,'') TipoIva, isnull(Texto2,'') SiglaTipoIVA, Importe ImporteIVA 
			FROM  Tablas WHERE TipoTabla = 'IVA'";    
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();            
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    TipoIva = Lector["TipoIva"].ToString(),
                    SiglaTipoIVA = Lector["SiglaTipoIVA"].ToString(),                    
                    ImporteIVA= float.Parse(Lector["ImporteIVA"].ToString())
            });
           }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista);
}// recTiposIva --


[WebMethod]
public string recFormasPago( ) {           
    var linFPag = new { Codigo = 0, FormaPago = "", CtaFormaPago = ""};    
    var Lista = new[] { linFPag }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"SELECT Codigo, Texto FormaPago, Texto2 CtaFormaPago 
			FROM  Tablas WHERE TipoTabla = 'FPAG'";    
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();            
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    FormaPago = Lector["FormaPago"].ToString(),
                    CtaFormaPago = Lector["CtaFormaPago"].ToString()                                       
            });
           }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); 
}// recFormasPago --



[WebMethod]
public string recTiposPrecios(int Cadena) {           
    var linFPag = new { Codigo = 0, TipoPrecio = "", Cadena = 0};    
    var Lista = new[] { linFPag }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"SELECT Codigo, isnull(Texto,'') TipoPrecio, isnull(Importe,0) Cadena 
			FROM Tablas WHERE TipoTabla = 'TPRE'
			AND ((@Cadena = 0 OR Importe = @Cadena))
			ORDER BY Importe, Codigo ";   
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();               
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;                 
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    TipoPrecio = Lector["TipoPrecio"].ToString(),
                    Cadena = int.Parse(Lector["Cadena"].ToString()),
            });
           }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); 
}// recTiposPrecios --



[WebMethod]
public string recTiposMarcas(int Cadena) {           
    var linFPag = new { Codigo = 0, NombreMarca = "", Cadena = 0};    
    var Lista = new[] { linFPag }.ToList();// crea lista con una linea            
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"SELECT Codigo, isnull(Texto,'') NombreMarca, isnull(Importe,0) Cadena 
			FROM Tablas WHERE TipoTabla = 'MARC'
			AND ((@Cadena = 0 OR Importe = @Cadena))
			ORDER BY Importe, Codigo;	";   
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();               
            cmd.Parameters.Add("@Cadena", SqlDbType.Int).Value = Cadena;                 
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(new {
                    Codigo = int.Parse(Lector["Codigo"].ToString()),
                    NombreMarca = Lector["NombreMarca"].ToString(),
                    Cadena = int.Parse(Lector["Cadena"].ToString()),
            });
           }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); 
}// recTiposMarcas --


[WebMethod]
public string recUsuariosCliente(int idCliente, int Activo) {                   
    List <UsuarioCliente> Lista = new List<UsuarioCliente>();
    UsuarioCliente Linea = new UsuarioCliente(); 
    Lista.Clear();     
	using (SqlConnection conn = new SqlConnection(Conex)){						
        string query = @"	SELECT Usuario Empleado, IdCliente, NombreUsuario, EMail, isnull(Activo, 'false') Activo	
            FROM Usuarios
            WHERE (@idCliente = 0 OR idCliente = @idCliente)
            AND (@Activo > 1  OR isnull(Activo, 'false') = @Activo)";    
        using (SqlCommand cmd = new SqlCommand(query, conn)) {  
            conn.Open();                          
            cmd.Parameters.Clear();                                   						
            cmd.Parameters.Add("@idCliente", SqlDbType.Int).Value = idCliente;              
            cmd.Parameters.Add("@Activo", SqlDbType.Int).Value = Activo;          
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Lista.Add(Linea);               
                Linea.Empleado = int.Parse(Lector["Empleado"].ToString());                
                Linea.IdCliente = int.Parse(Lector["IdCliente"].ToString());
                Linea.NombreUsuario = Lector["NombreUsuario"].ToString();
                Linea.EMail = Lector["EMail"].ToString();
                Linea.Activo = bool.Parse(Lector["Activo"].ToString());
                Linea = new UsuarioCliente(); // crea nueva linea  --                                       
           }
        }
    }	          
    return new JavaScriptSerializer().Serialize(Lista); 
}// recUsuariosCliente --



[WebMethod]   
public  string infoServidor() {
    /* recupera informacion variada del servidor         
            ref: conectandose a la BD --   using (SqlConnection conn = new SqlConnection(Conex)) { string Servidor = conn.Database; var BaseD = conn.Database; return "Bd:" + BaseD;};
    */

    // Extrayendo info. del la cadena de texto de conexion --
    System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
    builder.ConnectionString = Conex;

    // Si esta conectado en BD Local no contine esa información --
    #if (LOCAL)
        var infoServidor = new {
            Version = ""
            ,Servidor = "LOCAL" as string
            ,BD = "" as string
            ,UserID = "" as string
            ,Pass = "" as string
            };            
        #else
            var infoServidor = new {
                Version = ""
                ,Servidor = builder["Data Source"] as string
                ,BD = builder["Initial Catalog"] as string
                ,UserID = builder["User ID"] as string
                ,Pass = builder["Password"] as string
            };            
    #endif
    
    

    return new JavaScriptSerializer().Serialize(infoServidor); 

}// infoServidor



#region PRUEBAS_LINQ


public class linTab  {    
    public string TipoTabla { get; set; }
    public string Texto { get; set; }            
}

[WebMethod]
public string recCadenas_LINQ()  {
    // Pruebas linq, 

    // Prueba LinQ to SQL, 
    SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString);
    DataContext dc1 = new DataContext(conn);


    // 1 -- Usando ExecuteQuery OK --
    /*
    var linTabS = dc1.ExecuteQuery<linTab> (@"SELECT TipoTabla, Texto FROM Tablas WHERE TipoTabla = 'USUA'");
    foreach (linTab c in linTabS)            
        System.Diagnostics.Debug.WriteLine("Texto BD:" + c.Texto);
    */

    // 2- Leyendo contenido de tabla. 
    // Get a typed table to run queries.



    // usando tabla de prueba OK --
    /*
    Table<Prueba> Pr = dc1.GetTable<Prueba>(); // ok --
    IQueryable <Prueba> Query =
        from a in Pr        
        select a;

    foreach (Prueba a in Query) {
        //System.Diagnostics.Debug.WriteLine("C1:"+ linTab.Int1.ToString()+ ", C2:" + linTab.St1+ ", C3 :" + linTab.St2); 
        System.Diagnostics.Debug.WriteLine(a.St1+ ", " + a.St2); 
    }
    */

    // prueba para hacer join con esta tabla y otra procedente de sql : https://docs.microsoft.com/es-es/dotnet/csharp/language-reference/keywords/join-clause
    List<Cade> TipoTablas = new List<Cade> {
        new Cade {Tipo="ASM"},
        new Cade {Tipo="HALCON"}
       };

    Table<Tablas> Tbl = dc1.GetTable<Tablas>();


    // Usando objeto tipo var (ok) 
    var QueryV =
        from b in Tbl
        where b.TipoTabla == "USUA"
        orderby b.Codigo
        select new { Cod = b.Codigo.ToString(), Tipo = b.TipoTabla, b.Texto, b.Texto2, imp = b.Importe.ToString() };


    /* Prueba uso de join con table en memoria --
            genera      /* ERROR:
                    System.NotSupportedException: No se puede usar una secuencia local en implementaciones LINQ to SQL de operadores de consulta
                    excepto el operador Contains.


        QueryV =
            from b in Tbl      
                join t in TipoTablas on b.TipoTabla equals t.Tipo

                select new { Cod = b.Codigo.ToString(), Tipo = b.TipoTabla, b.Texto, b.Texto2, imp = b.Importe.ToString() };


                //select new { Cod = b.Codigo.ToString(), Tipo = b.TipoTabla, b.Texto, b.Texto2 };
                // Ttabla = t.Tipo };

        */
    foreach (var b in QueryV)    {
        //Debug.WriteLine("Cod;" + b.Cod + ", " + b.Tipo + ", " + b.Texto + ", " + b.Texto2 + ", Importe:" + b.imp);
        // System.Diagnostics.Debug.WriteLine("Cod;" + b.Cod );

    }

    // información de la tabla en memoria--
    /*
        foreach (var b in TipoTablas) {
            System.Diagnostics.Debug.WriteLine("Tipo Tabla;" + b.Tipo);
        }
    */

    //--------------------


    // Usando objeto tipo  IQueryable (ok) --
    /*
    IQueryable<Tablas> QueryT =
        from b in Tbl                                
        select b;    

        foreach (Tablas b in QueryT) {
            System.Diagnostics.Debug.WriteLine("Codigo;"+b.Codigo.ToString()+", "+b.TipoTabla+", "+b.Texto+", " + b.Texto2);
        }
        */



    // 3 -- Prueba consulta en memoria OK --
    /*
        int[] scores = new int[] { 97, 92, 81, 60 };
        // Define the query expression.
        IEnumerable<int> scoreQuery =
            from score in scores
            where score > 80
            select score;

        foreach (int i in scoreQuery)    {
            System.Diagnostics.Debug.WriteLine("Valor Memoria:"+i);   
            Console.WriteLine("Valor Memoria:" + i);
        }
    */
    // ---------------------------


    /*
    // 4. ------------------

    List<Student> students = new List<Student> {
        new Student {First="Svetlana", Last="Omelchenko", ID=111, Scores= new List<int> {97, 92, 81, 60}},
        new Student {First="Claire", Last="O'Donnell", ID=112, Scores= new List<int> {75, 84, 91, 39}},
        new Student {First="Sven", Last="Mortensen", ID=113, Scores= new List<int> {88, 94, 65, 91}},
        new Student {First="Cesar", Last="Garcia", ID=114, Scores= new List<int> {97, 89, 85, 82}},
        new Student {First="Debra", Last="Garcia", ID=115, Scores= new List<int> {35, 72, 91, 70}},
        new Student {First="Fadi", Last="Fakhouri", ID=116, Scores= new List<int> {99, 86, 90, 94}},
        new Student {First="Hanying", Last="Feng", ID=117, Scores= new List<int> {93, 92, 80, 87}},
        new Student {First="Hugo", Last="Garcia", ID=118, Scores= new List<int> {92, 90, 83, 78}},
        new Student {First="Lance", Last="Tucker", ID=119, Scores= new List<int> {68, 79, 88, 92}},
        new Student {First="Terry", Last="Adams", ID=120, Scores= new List<int> {99, 82, 81, 79}},
        new Student {First="Eugene", Last="Zabokritski", ID=121, Scores= new List<int> {96, 85, 91, 60}},
        new Student {First="Michael", Last="Tucker", ID=122, Scores= new List<int> {94, 92, 91, 91} }
    };

    IEnumerable<Student> studentQuery =
        from student in students
        where student.Scores[0] > 90
        select student;

    foreach (Student student in studentQuery){
        System.Diagnostics.Debug.WriteLine("Prueba 3:{0}, {1}", student.Last, student.First);
    }

    */

    //------------------------
    return "OK";

}// recCadenas_LINQ --

[Table(Name = "Tablas")]
public class Tablas   {
    [Column] public string TipoTabla;
    [Column] public int Codigo;
    [Column] public string Texto;
    [Column] public string Texto2;
    [Column]public float Importe;  // los valores float generan error, hay que tranformarlo en el select- 
    //[Column] public string Texto3; // ref: Es posible NO definir algún campo de la tabla --
}

public class Cade    {
    public string Tipo { get; set; }
}


public class Student    {
    public string First { get; set; }
    public string Last { get; set; }
    public int ID { get; set; }
    public List<int> Scores;
}
    

// ** Fin Prueba LINQ --


#endregion  // -- pruebas LINQ --




    
#region SIN_USO_REF




[WebMethod]
public string recCadenasJSON_OBSOLETO() {    
    return new JavaScriptSerializer().Serialize(recCadenas_OBSOLETO());
} // recCadenasJSON_OBSOLETO --

public List<Cadena> recCadenas_OBSOLETO() {
    List<Cadena> Cadenas = new List<Cadena>();
    Cadena linCadena = new Cadena(); // crea la primera linea -- 
    using (SqlConnection conn = new SqlConnection(Conex)) {
        string query = @" SELECT Codigo,  Texto NombreCadena, Importe Activa, Texto3 ofCentral
	            FROM Tablas WHERE TipoTabla = 'CADE' ORDER By Texto";
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                Cadenas.Add(linCadena);
                linCadena.Codigo = int.Parse(Lector["Codigo"].ToString());
                linCadena.NombreCadena = Lector["NombreCadena"].ToString();
                linCadena.Activa = int.Parse(Lector["Activa"].ToString());
                linCadena.ofCentral = Lector["ofCentral"].ToString();

                linCadena = new Cadena(); // crea nueva linea --
            }
        }
    }
    return Cadenas;
} // recCadenas_OBSOLETO --


public void ImprimeFactura_ANT(int idFactura, int Ejercicio) {  
    // activo hasta 9/5/18 
    // recupera la factura completa, Cabecera, Detalles y costes en objeto tipo datFactura
    datFactura stFactura = new datFactura();
            
    // Crea la clase con datos de  factura --
    clFactura fact = new clFactura(idFactura, Ejercicio);      
    stFactura.FacCabecera = fact.recCabecera();        
    stFactura.DetFactura = fact.recDetalles();
    stFactura.DetCostes = fact.recDetallesCoste();     
    string nuevoDoc = Server.MapPath("facturas/Fact_"+ idFactura.ToString() + "_"+ Ejercicio.ToString() + ".pdf");       
    /*
        // recupera la factura completa, Cabecera, Detalles y costes en objeto tipo datFactura
        datFactura stFactura = new datFactura();
        stFactura.FacCabecera = recFactura(idFactura, Ejercicio);     
        stFactura.DetFactura = recDetallesFactura(idFactura, Ejercicio);
        stFactura.DetCostes = recDetallesCoste(idFactura, Ejercicio);    
         // generarFacturaPdf(ref stFactura); 
    */       
    
    // asigna el stream y lo añade a campo Oracle --      
    var blobByte = creaBytesFacturaPdf(ref stFactura);
    
    // convierte byte[] to stream --
    Stream ms = new MemoryStream(blobByte);
    
    // guarda el memory stream en un fichero fisico --     
    using (FileStream file = new FileStream(nuevoDoc, FileMode.Create, System.IO.FileAccess.Write)) {
       byte[] bytes = new byte[ms.Length];
       ms.Read(bytes, 0, (int)ms.Length);
       file.Write(bytes, 0, bytes.Length);
       ms.Close();
    }
    
}// ImprimeFactura_ANT


[WebMethod]
public void insertarImagenPdf_ANT(int idgagSap, ref datFactura stFactura)       {    
    //string txFactura = idFactura.ToString();
    //string txEjercicio = Ejercicio.ToString();    

    string txFactura = stFactura.FacCabecera.idFactura.ToString();
    string txEjercicio = stFactura.FacCabecera.Ejercicio.ToString();
    string Doc = Server.MapPath("/Facturas_SAP/Fact_" + txFactura +"_"+ txEjercicio + ".pdf");  
                
  
    // asigna el stream y lo añade a campo Oracle --      
    var blobByte = creaBytesFacturaPdf(ref stFactura);
        
    //using (OracleConnection conex1 = new OracleConnection(ConexORCL)) {
    using (SqlConnection   conex1 = new SqlConnection(ConexORCL)) {
        conex1.Open();                 
        string query = @"UPDATE GAGSAP SET Documento = :blopfile WHERE idGagSAP = :idGag";      
        using (SqlCommand   cmd = new SqlCommand(query, conex1))       {
        
            // definir los parametros en el mimos order de la linea 
            cmd.Parameters.Clear();
            //cmd.Parameters.Add(new OracleParameter(":blopfile", OracleDbType.Blob));
            cmd.Parameters.Add(new SqlParameter(":blopfile", DbType.Binary                ));
            cmd.Parameters[":blopfile"].Value = blobByte;
            cmd.Parameters.Add(new SqlParameter(":idGag", DbType.Int32));
            cmd.Parameters[":idGag"].Value = idgagSap;
                       
            cmd.ExecuteNonQuery();
        }
        conex1.Close();
    }        


    // aqui, 8/3/18, ahora se genera tambien factura en disco para acceder generarla con URL --    
    // convierte byte[] to stream --
    Stream ms = new MemoryStream(blobByte);    
    // guarda el memory stream en un fichero fisico --     
    using (FileStream file = new FileStream(Doc, FileMode.Create, System.IO.FileAccess.Write)) {
       byte[] bytes = new byte[ms.Length];
       ms.Read(bytes, 0, (int)ms.Length);
       file.Write(bytes, 0, bytes.Length);
       ms.Close();
    }
    
        
}// insertarImagenPdf_ANT --

public byte[] creaBytesFacturaPdf(ref datFactura stFactura) {
    //Debug.WriteLine("btGeneraFacturaSrv_Click");

    /* ref: usando documento inicial como plantilla  ok, pero no puede incluirse contenido salvo con formularios ?? --           
        PdfReader reader = new PdfReader(orgDoc);
        PdfStamper stamp = new PdfStamper(reader, fs);

            // ref: para   completar pdf con campos de formulario y rellenandolos 
            // asigna valor a campo del formulario pdf         
                //stamp.AcroFields.SetField("NombreComercial","Este es el nombre comercial");
        stamp.Close();      
    //----------------------------------  */

    Document doc = new Document();
    using (MemoryStream output = new MemoryStream()) {
    PdfWriter writer = PdfWriter.GetInstance(doc, output);
    doc.Open();
    doc.AddAuthor("Pedro Lopez Castro");
    // constantes 
    const int lineasAlbaran = 15;       // nº de lineas de albaran 
    const int linInicDetalle = 480;      // Principio de la linea de detalle 
    const int linSepara = 15;            // separación entre lineas  
    // alineaciones de texto --
    const byte alDr = PdfContentByte.ALIGN_RIGHT;
    const byte alIz = PdfContentByte.ALIGN_LEFT;

    int Lineas = 0, Pagina = 1;
    string[] multiLin; // para imprimir una cadena de texto en multiple lineas sin cortar usando  cortarEspacios 

    // Fuentes usados --
    BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    BaseFont bfN = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);        // para negrita, bold 

    PdfContentByte cb = writer.DirectContent;

    CabeceraFacturaPdf_ANT(cb, doc, Pagina, ref writer, ref stFactura);

    // detalles ----    
    int Px = 66, Py = linInicDetalle;
    foreach (DetalleFactura LinDetFactura in stFactura.DetFactura) {
    if (Lineas > lineasAlbaran) {
    Py = 112;
    cb.ShowTextAligned(alDr, "Suma y Sigue", Px + 476, Py, 0);
    Pagina++;
    CabeceraFacturaPdf_ANT(cb, doc, Pagina, ref writer, ref stFactura);
    Lineas = 0;
    Py = linInicDetalle; // se situa en primera lineas --
                            //cb.SetFontAndSize(bf, 8);// Retorna a la fuente actual de los detalles --
    }
    cb.BeginText();
    cb.SetFontAndSize(bf, 10);

    if (LinDetFactura.Albaranes.Length > 7)
        cb.ShowTextAligned(alDr, LinDetFactura.Albaranes.Substring(0, 7), Px, Py, 0);   // Albaran            
    else
        cb.ShowTextAligned(alDr, LinDetFactura.Albaranes, Px, Py, 0);


    cb.ShowTextAligned(alDr, LinDetFactura.Presupuesto, Px + 72, Py, 0);                  // Presupuesto                  
    cb.ShowTextAligned(alDr, LinDetFactura.Cantidad.ToString(), Px + 113, Py, 0);        // Cantidad            
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00000}", LinDetFactura.PrecioUnitario), Px + 176, Py, 0);
    cb.SetFontAndSize(bfN, 8);

    multiLin = Utilidades.cortarEspacios(LinDetFactura.Nombre.Trim(), 50); // retorna array con lineas cortadas --
    foreach (string st2 in multiLin) {
    cb.ShowTextAligned(alIz, st2, Px + 184, Py, 0);
    Lineas++; Py -= linSepara;
    }
    Lineas--; Py += linSepara; // al recorrer el array se genera un salto de mas, retrocede una linea --                                  

    // si hay comentario, genera nuevas lineas --
    if (LinDetFactura.Comentario.Trim() != "") {
    Py -= linSepara; Lineas++; // salto de linea en comentario --
    cb.SetFontAndSize(bf, 8);
    multiLin = Utilidades.cortarEspacios(LinDetFactura.Comentario.Trim(), 60);
    //Debug.WriteLine("*****"+ LinDetFactura.Presupuesto+ ", Cantidad: "+ LinDetFactura.Cantidad.ToString()+" *****");  
    foreach (string st2 in multiLin) {
    cb.ShowTextAligned(alIz, st2, Px + 184, Py, 0);
    //Debug.WriteLine("-> " + st2);  

    Lineas++; Py -= linSepara;
    }
    Lineas--; Py += linSepara;
    }
    cb.SetFontAndSize(bf, 10);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", LinDetFactura.Precio), Px + 474, Py, 0);
    cb.EndText();
    Py -= linSepara; Lineas++; // salto de linea --
    }

    /*ref: añade otra pagina vacia --
        doc.NewPage();
        doc.Add(new Paragraph("2ª Pagina 2"));
    */

    // Pie de factura -- 
    cb.BeginText();
    Px = 26; Py = 138;
    cb.SetFontAndSize(bf, 9);
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.FormaPago, Px, Py, 0);
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.CtaFormaPago, Px, Py - linSepara, 0);
    cb.SetFontAndSize(bfN, 11);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.Base), Px + 300, Py, 0);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.TIva), Px + 380, Py, 0);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.Cuota), Px + 440, Py, 0);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", Math.Round(stFactura.FacCabecera.Total, 2)), Px + 510, Py, 0);
    cb.EndText();

    doc.Close();
    return output.ToArray();
    }
}// creaBytesFacturaPdf --

protected void CabeceraFacturaPdf_ANT(PdfContentByte cb, Document doc, int Pagina, ref PdfWriter writerP, ref datFactura stFactura) {
    string Plantilla = Server.MapPath("Plantilla_Factura.pdf");
    
    //lee el pdf de la plantilla y lo adjunta al documento     --
    PdfReader reader = new PdfReader(Plantilla);
    PdfImportedPage page1 = writerP.GetImportedPage(reader, 1);

    doc.NewPage();
    cb.AddTemplate(page1, 1, 1);
    reader.Close();
    
    // Fuentes usados --
    BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);     
    BaseFont bfN = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);        // para negrita, bold 
       
    
    // alineaciones de texto --
    byte alDr = PdfContentByte.ALIGN_RIGHT;
    byte alIz = PdfContentByte.ALIGN_LEFT;
    

    // Cabecera , Razon Social 
    int Px = 290, Py = 748;    
    cb.BeginText();
    cb.SetFontAndSize(bfN, 10);                 
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.RazonSocial, Px, Py, 0);                
    cb.SetFontAndSize(bf, 10);                 
    Py-=15;    
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.Domicilio, Px, Py, 0);
    Py-=15;        
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.CodPostal+' '+ stFactura.FacCabecera.Poblacion, Px, Py, 0);    
    Py-=15;        
    cb.ShowTextAligned(alIz,  "CIF/DNI:"+' '+ stFactura.FacCabecera.Nif, Px, Py, 0);    
    cb.EndText();

    // Cabecera , Direccion de envio 
    Px = 290; Py = 680;    
    cb.BeginText();
    cb.SetFontAndSize(bfN, 10);                 
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.NombreCliente, Px, Py, 0); // nombre comercial --
    Py-=15;
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.DomicilioEnvio, Px, Py, 0);        
    cb.SetFontAndSize(bf, 10);                 
    Py-=15;
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.CodPostalEnvio + '-'+stFactura.FacCabecera.PoblacionEnvio, Px, Py, 0);    
    Py-=15;
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.ProvinciaEnvio, Px, Py, 0); 
    cb.EndText();
    
    // Linea inferior de cabecera 
    Px =26;   Py= 542;
    cb.SetFontAndSize(bfN, 8); 
    cb.BeginText();
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.FechaFactura, Px, Py,0);
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.Presupuesto, Px+77, Py,0);
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.Pedido, Px+140, Py,0);            
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.Orden, Px+170, Py,0);   
    cb.ShowTextAligned(alIz, stFactura.FacCabecera.Referencia, Px+224, Py,0);            
    cb.ShowTextAligned(alIz, "HGA"+ stFactura.FacCabecera.Ejercicio+"/"+stFactura.FacCabecera.idFactura.ToString("000000"), Px+450, Py, 0);            
    cb.EndText();

}// CabeceraFacturaPdf_ANT --

        

// ** proc. de contabilidad a eliminar (activos hasta 8/5/18 -------------------------------------------------

public string generarConta_ANT(int idFactura, int Ejercicio) {
    // genera contabilidad de UNA factura en SAP e incluye imagen pdf en el registro--

    // Crea la clase con datos de  factura -    
    // recupera la factura completa, Cabecera, Detalles y costes en objeto tipo datFactura --
    datFactura stFactura = new datFactura();        
    clFactura fact = new clFactura(idFactura, Ejercicio);    
           
    stFactura.FacCabecera = fact.recCabecera();     

    //Debug.WriteLine("Datos Cabecera Contabilizada: " + stFactura.FacCabecera.FContabilizada);
    if (stFactura.FacCabecera.FContabilizada != "")
        return "Factura: " + idFactura.ToString() + " YA contabilizada el :" + stFactura.FacCabecera.FContabilizada + "\r\n";
    
    stFactura.DetFactura = fact.recDetalles();  
    stFactura.DetCostes = fact.recDetallesCoste();
     
    // genera reg. de cabecera factura y devuelve el nº insertado --   
    int idGagSap = generarSAPFactura(ref stFactura);


    //Debug.WriteLine("IdGAGSap de Cabecera insertada:"+ idCabecera.ToString());                 
    generarSAPCostes_ANT(ref stFactura);

    // actualiza el reg. de cabecera de factura con la imagen de esta en pdf -
    insertarImagenPdf_ANT(idGagSap, ref stFactura);    
    //insertarImagenPdf_PruebaSoloStream(idGagSap, ref stFactura);

    // actualiza la cabecera de factura con la fecha actual --
    //  *** ANULAR para no marcar contabilizadas     
    // aqui activar //marcaFechaFactura(idFactura, Ejercicio); 

    return "Factura:"+ idFactura.ToString()+", id:"+ idGagSap.ToString()+ "\r\n";
}// generarConta_ANT --

[WebMethod]
public int generarSAPFactura(ref datFactura stFactura)        {
    
    Factura RegFactura = stFactura.FacCabecera;

    int  idFactura = stFactura.FacCabecera.idFactura;
    int  Ejercicio = stFactura.FacCabecera.Ejercicio;
               
    // Crea la clase con datos de  factura --
    clFactura fact = new clFactura(idFactura, Ejercicio);              
    List<tiposIVA> listaIvas = new List<tiposIVA>();        
    listaIvas= fact.recIvas(); 
     
    // Recupera información de textos de tipos de IVA los detalles de la factura, solo 2 lineas  --     

    //Debug.WriteLine("Ivas:" + LitIVA1.ToString()+","+ LitIVA2.ToString());          
    // -- para comprobacion --
    //foreach (tiposIVA lin in listaIvas)    Debug.WriteLine("recIvasFactura, Ivas:" + lin.LitIVA);          
    
    string fechaCierre = DateTime.Today.ToString("dd/MM/yyyy");     // día actual --  //ref: un mes menos-- DateTime.Today.AddDays(-30).ToString("dd/MM/yyyy");     
    // Datos de la cabecera --
    string linCab = "1.0 beta;";                                                        // VERSION --
    linCab += "ARTESGRAFICAS;";                                                         // TIPOFUENTE --
    linCab += "HGA;";                                                                   // SOCIEDADSAP  --
    linCab += "INT;";                                                                   // CANAL col D  --
    linCab += RegFactura.RazonSocial + ';';                                             // RAZONSOCIAL  --    
    linCab += RegFactura.Domicilio + ';';                                               // CALLENUM --
    linCab += RegFactura.CodPostal + ';';                                               // CODIGOPOSTAL
    linCab += RegFactura.Poblacion + ';';                                               // POBLACION
    linCab += RegFactura.Provincia + ';';                                               // PROVINCIA
    linCab += RegFactura.idSap + ';';                                                   // CODIGOSAP --
    linCab +=RegFactura.idCliente.ToString() + ';';                                     // CODIGOEXTERNO
    linCab +="ES;";                                                                     // PAIS
    linCab +=RegFactura.Nif + ';';                                                      // IDFISCAL                             
    linCab +="HGA" + Ejercicio.ToString()+ '/'+ idFactura.ToString("000000") +  ';';    // Ejercicio+NUMEROFACTURA  col N --       
    linCab +=(RegFactura.idFacAbonada == 0) ? "FAC;":"REC;";                            // TIPOFACTURA  FAC o REC si es un abono  col O --                                         
    linCab +=Convert.ToDateTime(RegFactura.FechaFactura).ToString("yyyyMMdd") + ';';    // FECHAFACTURA col P --
    linCab +="EUR;";                                                                    // MONEDA  col Q     

    linCab +=(RegFactura.idFacAbonada == 0)? ";":
        ";HGA" + Ejercicio.ToString()+ "/" + RegFactura.idFacAbonada.ToString("000000"); // NUMEROFACTURAREF col R, si es un abono, indica nº que se abona 
    linCab +=";";                                                                       // CENTRO col S 
    linCab +=(RegFactura.Pedido == "0") ? RegFactura.Presupuesto + ";" : RegFactura.Pedido + ";";   // REFERENCIA1 Pedido o Presupuesto si pedido es 0, col T, mod 31/7/17,     
    
    //linCab +=RegFactura.Pedido.ToString();    Debug.WriteLine("Pedido:"+ RegFactura.Pedido);                 
        
    linCab += RegFactura.Presupuesto;                                               // REFERENCIA2  Presupuesto col U
    linCab += ";";                                                                      // COL V, Vacia 
    linCab += ";TOTAL FACTURA Nº: " + idFactura + ";";                                  // DESCRIPCIONFACTURA col W        
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:#####0.00}",RegFactura.Total);   // IMPORTE (TOTALFACTURA) col X Usa "." como separador decimal --
    linCab += ";;;;;;;;;;;;;;;;;;;";                                                    // Columnas Vacias hasta AQ (18 vacías) --
    
    linCab +=listaIvas[0].LitIVA+";";                                              // CLASEIMPUESTO col AQ --           
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}", listaIvas[0].porIVA) + ';';        // PORCENTAJEIMPUESTO col AR         
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}", listaIvas[0].importeIVA) + ';';    // BASEIMPONIBLE 1 col AS.
    linCab += ";";                                                                      // BASEEXENTA   col AT, vacia 
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}",RegFactura.Cuota)+ ';'; // CUOTA 1 col AU--     
       

    linCab += listaIvas[1].LitIVA+";";                                            // CLASEIMPUESTO 2 col AV  12/4/17, se cambia por literal iva                          
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}", listaIvas[1].porIVA) + ';';         // PORCENTAJEIMPUESTO 2 col AW
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}", listaIvas[1].importeIVA) + ';';     // BASEIMPONIBLE 2 col AX
    linCab +=";";                                                                       // BASE EXENTA 2 col AY ,se indica vacia --
        
    linCab +=string.Format(CultureInfo.InvariantCulture,"{0,5:##0.00}", listaIvas[1].porIVA * listaIvas[1].importeIVA / 100) + ';';    // CUOTA 2 col AZ
    
    // Crea la linea de cabecera, conserva y retorna  nº de linea de la cabecera que se ha insertado --
    int IdGagSap = crearLineaContable( idFactura, Ejercicio, 1, 1, fechaCierre, linCab); // crea Linea de cabecera    --
    // Genera linea en fichero de texto con la misma informacion de GAGSAP , para referencia , NO  necesaria --
    #if (CREAREXCELCSV)
        insertaExcelCsvFactura(linCab);
    #endif

    // Primera linea desglose --
    string linDet = "1.0 beta;";        // VERSION --
    linDet += "ARTESGRAFICAS;";         // TIPOFUENTE --
    linDet += "HGA;";                   // SOCIEDADSAP  --
    linDet += "INT;";                   // CANAL col D --
    linDet += ";;;;;;;;;";              // 9 columnas vacias --
    linDet += "DESGLOSE;";              // Texto "DESGLOSE"  col N--    
    linDet += ";;;;;;";                 // 6 columnas vacias --    
     // FECHAFACTURA  fecha factura (1er. detalle ) (columna U)--         
    linDet += Convert.ToDateTime(RegFactura.PrimFechaDetalle).ToString("yyyyMMdd") + ';';
    linDet += ";;;";                // 3 columnas vacias -- 

    if (RegFactura.TCostes != 0) { 
        linDet += "VSUBCON;";           // Codigo Concepto SAP "VSUBCON;" col  Y--
        linDet += ";;";                 // 2 columnas vacias --  
        if (RegFactura.TImporteExistencias != 0) 
            linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TCostes) + ';';  // Importe  col AB (si hay existencias)
         else
            linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TPrecio) + ';'; // Importe  col AB (si no hay existencias)
    }
    else { 
        linDet += "VPROPIAS;";          // Codigo Concepto SAP "VPROPIAS;" col  Y--
        linDet += ";;";                 // 2 columnas vacias --  
        linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TPrecio) + ';'; // Importe  col AB (si no hay existencias)
    }
    
    linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TIva) + ';';          // Porcentaje Impuesto col AC     
    linDet += RegFactura.idSap + ';';           // DESCRIPCION col AD, CODIGOSAP --    
    crearLineaContable( idFactura, Ejercicio, 1, 2, fechaCierre, linDet); // crea Linea de desglose    --    
    #if (CREAREXCELCSV)
        insertaExcelCsvFactura(linDet); // Genera linea en fichero de texto/CSV --
    #endif

    // linea 2 de DESGLOSE si hay propias y subcontratadas a la vez, apunte de propias -----
    if (RegFactura.TImporteExistencias != 0 &&  RegFactura.TCostes != 0)     {
        linDet = "1.0 beta;";                   // VERSION --
        linDet += "ARTESGRAFICAS;";             // TIPOFUENTE --
        linDet += "HGA;";                       // SOCIEDADSAP  --
        linDet += "INT;";                       // CANAL col D --
        linDet += ";;;;;;;;;";                  // 9 columnas vacias --
        linDet += "DESGLOSE;";                  // Texto "DESGLOSE"  col N  --
        linDet += ";;;;;;";                     // 6 columnas vacias --    
        linDet += Convert.ToDateTime(RegFactura.PrimFechaDetalle).ToString("yyyyMMdd") + ';';  // Col U , Fecha factura 
        linDet += ";;;";                        // 3 columnas vacias --    
        linDet += "VPROPIAS;";                  // Codigo Concepto SAP "VPROPIAS;" col  Y--
        linDet += ";;";                         // 2 columnas vacias --  
        linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TPrecio - RegFactura.TCostes) + ';'; // Importe  col AB --
        linDet += string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00}", RegFactura.TIva) + ';';          // Porcentaje Impuesto col AC     
        linDet += RegFactura.idSap + ';';       // DESCRIPCION col AD, CODIGOSAP --    

        crearLineaContable(idFactura, Ejercicio, 1, 3, fechaCierre, linDet); // crea Linea de desglose    --  
        #if (CREAREXCELCSV)
            insertaExcelCsvFactura(linDet); // Genera linea en fichero de texto/CSV --
        #endif
    }    
        
    // retorna el nº de registro generado --
    return IdGagSap;
}// generarSAPFactura--
                
[WebMethod]
public void generarSAPCostes_ANT(ref datFactura stFactura) {     
    //Debug.WriteLine("generarSAPCostes:");                 
    // Usa costes de factura del parametro 
    List<DetalleCoste> DetCostes = new List<DetalleCoste>();
    //DetCostes = recDetallesCoste(idFactura, Ejercicio);
    DetCostes = stFactura.DetCostes;
    int  idFactura = stFactura.FacCabecera.idFactura;
    int  Ejercicio = stFactura.FacCabecera.Ejercicio;

    // Usa tambien cabecera de la factura --
    Factura RegFactura = new Factura();
    //RegFactura = recFactura(idFactura, Ejercicio);     
    RegFactura = stFactura.FacCabecera;

    int orden = 1;  // secuencia incremental para cada coste, comienza en 1 --

    string linDet = "";        
    string fechaCierre = DateTime.Today.ToString("dd/MM/yyyy");

    // recorre detalles de costes recuperados --    
    foreach (DetalleCoste LinDetCoste in DetCostes) {   
        linDet = "1.0 beta;";                           // VERSION col A--  
        linDet+= "ARTESGRAFICAS;";                      // TIPOFUENTE col B --
        linDet += "HGA;";                               // SOCIEDADSAP  col C --
        linDet += "INT;;";                              // CANAL col D --
        if (LinDetCoste.idProveedor == 1002) {
            linDet += "PCINT;";                             // TIPO DOC (existencias) col F
            linDet += ";;;";                                // 3 columnas vacias -- 
        }
        else {
            linDet += "PCEXT;";                             // TIPO DOC (Proveedor ) col F 
            linDet += LinDetCoste.NombreProveedor + ";";    // RAZON SOCIAL col G 
            linDet += LinDetCoste.idProveedor + ";";        // Codigo Externo col H
            linDet += LinDetCoste.idSapProveedor + ";";     // Codigo SAP col I             
        }                
        // Fecha Documento, en costes fijos la de la factura, COL J 
        linDet += Convert.ToDateTime(RegFactura.FechaFactura).ToString("yyyyMMdd") + ';'; 

        // Referencia1 -1er. presupuesto ,   Si no hay poner el presupuesto el coste mod. 18/101/17 ,  COL K
        if (RegFactura.Presupuesto !="")
            linDet += RegFactura.Presupuesto+ ";";                                
        else
            linDet += LinDetCoste.Presupuesto+ ";"; 

        linDet += LinDetCoste.Presupuesto+ ";";           //  (29/5/08) Referencia2  presupuesto del coste   col L

        linDet += LinDetCoste.idFactura+ ";";             // Referencia3 (Nº de factura) col M
        if (LinDetCoste.idProveedor == 1002) 
            linDet += "CPROPIOS;";                        // Codigo Concepto (Existencias) col N                
        else 
            linDet += "CSUBCON;";                                   // Codigo Concepto (Proveedor)  col N

        linDet += "Coste Factura Nº:"+LinDetCoste.idFactura+ ";";   // Descripción coste COl O
        linDet += string.Format(CultureInfo.InvariantCulture,
             "{0,5:##0.00}", LinDetCoste.Importe) + ';';            // Total Coste Col P
        linDet += "EUR"+ ";";                                       // Moneda Col Q
        linDet += ";";                                              // Analitica, vacia  Col R
        linDet += LinDetCoste.idCoste;                              // IDUNICO Col S
                      
        crearLineaContable(idFactura, Ejercicio, 2, orden, fechaCierre, linDet);
        #if (CREAREXCELCSV)
            insertaExcelCsvCoste(linDet); // Genera linea en fichero de texto/CSV de costes --
        #endif
        orden++;        
    }    
}// generarSAPCostes_ANT --

// FIN  proc. de contabiliza a eliminar -----------------------



[WebMethod]
public string recListaFacturasJSON_ANT(string fDesde, string fHasta, int Ejercicio,    string Factura,  string Cliente,  string Facturados )  {
    // Anulado SIN_USO:  Para datables, mantener como referencia --
    //ref: Para cambiar el Length DE DATOS json  ---  //serializer.MaxJsonLength = 500000000;
    int limiteLineas = 4000;
    // recupera antes lista en objeto list ---
    List<lisFactura> listaF = recListaFac_Ex_SINUSO(fDesde, fHasta, Ejercicio, Factura, Cliente, Facturados);    
    // si sobrepasa límite de registros, retorna en fila 0  ejercicio 9999 y nº de regs y limite de estos  --
    if (listaF.Count > limiteLineas)     
        return "[{\"Ejercicio\":9999, \"NReg\":"+ listaF.Count.ToString()+", \"limite\":"+ limiteLineas + "}]"; 
    else    
        return new JavaScriptSerializer().Serialize(listaF); // retorna la lista trasformada en JSON --
}// recListaFacturasJSON_ANT -

 public static List<lisFactura> recListaFac_Ex_SINUSO(string fDesde, string fHasta,
    int Ejercicio,
    string Factura,   
    string Cliente,
    string Facturados  // 0->Todos, 1-> NO Facturados, 2-> Facturados --
)  {
    // solo se usa en detallesExcel, emiminar y usar 
    // ref: Para datables y lista facturas excel, mantener como referencia --
    string queryS = @"         
        SELECT   Fac.IdFactura, Fac.IdEmpresa, Fac.Ejercicio, Fac.IdCliente, PRESUPUESTO, FECHA,NombreCadena  
	        ,SUBSTRING(REFERENCIA, 1, 30) REFERENCIA, Estado, LOGOTIPO_SALAMANCA, COMENTARIO
	        ,SUBSTRING(TituloL, 1, 30) NombreCliente, isnull(Tot.NReg,0) Nreg, PrimPedido Pedido
	        ,isnull(TImporte_Iva + TPrecio, 0) Total,Tot.TImporte_Iva, Tot.TIVA, Tot.TPrecio, isnull(Coste,0) Coste,  Tot.TCantidad
	        ,Fac.Fecha_Cierre FContabilizada, Cl.idSap, Fac.FechaPago, isnull(idFacAbonada , 0) idFacAbonada 
        FROM Facturas Fac    
	        LEFT  JOIN ( SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(1)  NReg, SUM(CANTIDAD) TCantidad, 
		        SUM(Precio)  TPrecio, SUM(IVA) TIVA,SUM(ROUND(Precio * ISNULL(IVA, 0) / 100, 4))  TImporte_Iva
		        ,SUM(ROUND(Precio + ROUND(Precio * ISNULL(IVA, 0) / 100, 2), 2)) TotalAnt
		        ,MIN(Presupuesto) AS PrimPresupuesto 			       
			        FROM Facturas_Detalles		GROUP BY Ejercicio, IdFactura, IdEmpresa  )	Tot 
			        ON Fac.IdEmpresa = Tot.IdEmpresa	AND Fac.Ejercicio = Tot.Ejercicio	AND Fac.IdFactura = Tot.IdFactura                   
	        LEFT JOIN Clientes Cl ON Fac.IdCliente = Cl.IdCliente
	        LEFT JOIN (SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(ISNULL(Importe, 0)) Coste	FROM Costes 
		        GROUP BY Ejercicio, IdFactura, IdEmpresa) TotCos 
			        ON Fac.IdEmpresa = TotCos.IdEmpresa	AND Fac.Ejercicio = TotCos.Ejercicio AND Fac.IdFactura = TotCos.IdFactura
	        LEFT JOIN(SELECT  Codigo , texto NombreCadena FROM Tablas WHERE TipoTabla = 'CADE') Cad  ON Cad.Codigo = Cl.idCadena
	        LEFT JOIN (	SELECT  Factura, EjercicioFactura, IdEmpresa, SUM(1)  NRegAlba, MIN(CodSpatam) AS PrimOrden, MIN(NPedido) PrimPedido
			        FROM Albaranes	GROUP BY IdEmpresa, EjercicioFactura, Factura	    ) Alb
			        ON Fac.IdEmpresa = Alb.IdEmpresa	
				        AND Fac.Ejercicio = Alb.EjercicioFactura 
				        AND Fac.IdFactura = Alb.Factura   				 						
        WHERE Fac.IdEmpresa = 1   AND Fac.Ejercicio = @Ejercicio 				               	   			
        ";        
    List<lisFactura> Facturas = new List<lisFactura>();
    lisFactura LinFactura = new lisFactura(); // crea la primera linea --
    
    using (SqlConnection conn = new SqlConnection(Conex))        {
        // maxJsonLength, max 15.000 lin, probema en epxlorer, se cuelga despues de una consulta  con muchas lineas     --

        string query = queryS;

        // lineas de selección --      
        string querySel = " AND Fecha BETWEEN CAST ( @fDesde AS DATETIME) AND  CAST (@fHasta AS DATETIME)";

        if (Facturados == "2")  // Facturados  --
            querySel += " AND Fac.Fecha_Cierre is not null";
        else
            if (Facturados == "1")
            querySel += " AND Fac.Fecha_Cierre is null";


        // Las siguientes selecciones  NO se añaden a la seleción anterior, anula los demás filtros, SALVO: empresa y ejercico --        		
        if (Factura != "" && Factura != null)		{
	        querySel = " AND Fac.IdFactura  =" + Factura;
        }
        if (Cliente != "" && Cliente != null)	{
	        querySel = " AND Fac.idCliente =" + Cliente;
        }

        // une cuerpo de qry con seleccion, añade orden al final  --
        query = query + querySel + " ORDER By Fac.Ejercicio DESC, Fac.IdFactura DESC      "; 
        using (SqlCommand cmd = new SqlCommand(query, conn))       {
	        conn.Open();           
	        cmd.Parameters.Clear();
	        cmd.Parameters.Add(new SqlParameter("@fDesde", SqlDbType.VarChar));
            cmd.Parameters["@fDesde"].Value = fDesde + " 00:00";
	        cmd.Parameters.Add(new SqlParameter("@fHasta", SqlDbType.VarChar));
            cmd.Parameters["@fHasta"].Value = fHasta + " 23:59";
	        cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));					 
	        cmd.Parameters["@Ejercicio"].Value = Ejercicio;			
	        SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
	        while (Lector.Read())            {
		        Facturas.Add(LinFactura);

		        LinFactura.idFactura = int.Parse(Lector["idFactura"].ToString());
		        LinFactura.Fecha = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();                
		        LinFactura.idCliente = int.Parse(Lector["idCliente"].ToString());
		        LinFactura.idSap = Lector["idSap"].ToString();                        
		        LinFactura.NombreCliente = Lector["NombreCliente"].ToString();
		        LinFactura.Referencia = Lector["Referencia"].ToString();
		        LinFactura.Total = float.Parse(Lector["Total"].ToString());
		        LinFactura.NReg = int.Parse(Lector["NReg"].ToString());                    
		        LinFactura.FContabilizada = (Lector["FContabilizada"] != DBNull.Value ? Convert.ToDateTime(Lector["FContabilizada"]).ToShortDateString() : "");                
		        LinFactura.NombreCadena = Lector["NombreCadena"].ToString();
                LinFactura.Coste = float.Parse(Lector["Coste"].ToString());
                LinFactura.Pedido = Lector["Pedido"].ToString();
						
		        LinFactura = new lisFactura(); // crea nueva linea  --                               
	        }
        }        
    }   
    return Facturas;
        }//recListaFac_Ex_SINUSO -- 

public Factura recFactura_ANT(int idFactura, int Ejercicio) {
    Factura RegFactura = new Factura();
    string query = @"      		    	    
        SELECT  Fac.idFactura, Fac.Ejercicio, Fac.IdCliente, Titulo RazonSocial,TituloL, Fecha, Estado, Referencia,   Comentario
	    ,Fecha_cierre FContabilizada ,Domicilio, Poblacion,CodPostal,Provincia, idFormaPago, FormaPago, CtaFormaPago 
	    ,Direccion_Envio DomicilioEnvio, PoblacionEnvio,CodPostalEnvio,PROVINCIAENVIO
	    ,IdSap, Nif, PrimFechaDetalle	, PrimPresupuesto Presupuesto, PrimPedido Pedido, PrimOrden Orden
	    ,TIVA, idTipoIva, Cuota, Base,  TPrecio   ,(Cuota + Base) Total
	    ,ISNULL(TotCos.TImporteCoste, 0) - ISNULL(TotEx.TImporteCoste, 0) TCostes
	    ,ISNULL(TotEx.TImporteCoste, 0) TImporteExistencias, isnull(idFacAbonada , 0) idFacAbonada	
	    ,ISNULL(TotCos.TImporteCoste, 0) Coste
	    FROM Facturas Fac			
	        LEFT  JOIN ( SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(1)  NReg, SUM(CANTIDAD) TCantidad, 
			    SUM(Precio)  TPrecio, SUM(ROUND(Precio * ISNULL(IVA, 0) / 100, 4))  Cuota
			    ,MIN(Presupuesto) AS PrimPresupuesto 	,isnull(MIN(IVA),0)  TIVA	,MIN(idTipoIva) AS idTipoIva
			    ,SUM(ROUND(Precio, 2)) AS Base, MAX(Fecha) PrimFechaDetalle					
				    FROM Facturas_Detalles		
				    GROUP BY Ejercicio, IdFactura, IdEmpresa  )	Tot 
					    ON Fac.IdEmpresa = Tot.IdEmpresa AND Fac.Ejercicio = Tot.Ejercicio AND Fac.IdFactura = Tot.IdFactura       
						
	        LEFT JOIN (SELECT  Ejercicio, IdFactura, IdEmpresa, SUM(Importe) TImporteCoste 
		    FROM Costes WHERE IdProveedor = 1002 GROUP BY Ejercicio, IdFactura, IdEmpresa) TotEx	
		    ON Fac.IdEmpresa = TotEx.IdEmpresa	AND Fac.Ejercicio = TotEx.Ejercicio AND Fac.IdFactura = TotEx.IdFactura
	    LEFT JOIN (SELECT  IdFactura, Ejercicio, IdEmpresa, SUM(ISNULL(Importe, 0)) TImporteCoste	
		    FROM Costes GROUP BY Ejercicio, IdFactura, IdEmpresa) TotCos
			    ON Fac.IdEmpresa = TotCos.IdEmpresa	AND Fac.Ejercicio = TotCos.Ejercicio AND Fac.IdFactura = TotCos.IdFactura   						
	    LEFT JOIN (
		    SELECT  Factura, EjercicioFactura, IdEmpresa, SUM(1)  NRegAlba, MIN(CodSpatam) AS PrimOrden, MIN(NPedido) PrimPedido
		    FROM Albaranes
		    WHERE Factura = @idFactura  AND EjercicioFactura = @Ejercicio
			    GROUP BY IdEmpresa, EjercicioFactura, Factura  
	    ) Alb
			    ON Fac.IdEmpresa = Alb.IdEmpresa	
			    AND Fac.Ejercicio = Alb.EjercicioFactura 
			    AND Fac.IdFactura = Alb.Factura   				 							
	    LEFT JOIN Clientes Cl ON Fac.IdCliente = Cl.IdCliente
	    LEFT JOIN (SELECT Codigo, Texto FormaPago, Texto2 CtaFormaPago FROM Tablas WHERE TipoTabla = 'FPAG') Fpg
		    ON idFormaPago = Fpg.Codigo
			    WHERE Fac.idFactura = @idFactura  AND Fac.Ejercicio = @Ejercicio
    ";

    
    using (SqlConnection conn = new SqlConnection(Conex))        {
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
            cmd.Parameters["@idFactura"].Value = idFactura;
            cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));            
            cmd.Parameters["@Ejercicio"].Value = Ejercicio;

            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (Lector.Read()) {
                RegFactura.idFactura = int.Parse(Lector["idFactura"].ToString());
                RegFactura.FechaFactura = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();
                RegFactura.Ejercicio = int.Parse(Lector["Ejercicio"].ToString());
                RegFactura.idCliente = int.Parse(Lector["idCliente"].ToString());
                RegFactura.NombreCliente = Lector["TituloL"].ToString();
                RegFactura.RazonSocial = Lector["RazonSocial"].ToString();
                RegFactura.Estado = int.Parse(Lector["Estado"].ToString());
                RegFactura.Referencia = Lector["Referencia"].ToString();
                RegFactura.FContabilizada = (Lector["FContabilizada"] != DBNull.Value ? Convert.ToDateTime(Lector["FContabilizada"]).ToShortDateString() : "");
                RegFactura.Domicilio = Lector["Domicilio"].ToString();
                RegFactura.Poblacion = Lector["Poblacion"].ToString();
                RegFactura.CodPostal = Lector["CodPostal"].ToString();
                RegFactura.Provincia = Lector["Provincia"].ToString();
                RegFactura.DomicilioEnvio = Lector["DomicilioEnvio"].ToString();
                RegFactura.PoblacionEnvio = Lector["PoblacionEnvio"].ToString();
                RegFactura.CodPostalEnvio = Lector["CodPostalEnvio"].ToString();
                RegFactura.ProvinciaEnvio = Lector["ProvinciaEnvio"].ToString();
                RegFactura.idSap = Lector["idSap"].ToString();
                RegFactura.Nif = Lector["Nif"].ToString();
                RegFactura.Presupuesto = Lector["Presupuesto"].ToString();
                RegFactura.Total = float.Parse(Lector["Total"].ToString());
                RegFactura.TIva = float.Parse(Lector["TIVA"].ToString());
                RegFactura.Base = float.Parse(Lector["Base"].ToString());
                RegFactura.Cuota = float.Parse(Lector["Cuota"].ToString());
                RegFactura.TCostes = float.Parse(Lector["TCostes"].ToString());
                RegFactura.TImporteExistencias = float.Parse(Lector["TImporteExistencias"].ToString());
                RegFactura.TPrecio = float.Parse(Lector["TPrecio"].ToString());
                RegFactura.idFacAbonada = int.Parse(Lector["idFacAbonada"].ToString());
                RegFactura.PrimFechaDetalle = Convert.ToDateTime(Lector["PrimFechaDetalle"]).ToShortDateString();
                RegFactura.FormaPago = Lector["FormaPago"].ToString();
                RegFactura.CtaFormaPago = Lector["CtaFormaPago"].ToString();
                RegFactura.Pedido = Lector["Pedido"].ToString();
                RegFactura.Orden = Lector["Orden"].ToString();
                RegFactura.Coste = float.Parse(Lector["Coste"].ToString());
            }
        }
    }
    return RegFactura;

}// recFactura_ANT--  


public List<DetalleFactura> recDetallesFactura_ANT(int idFactura, int Ejercicio) {
    // Recupera lista de detalles de factura en objeto tipo List --
    List<DetalleFactura> DetFacturas = new List<DetalleFactura>();
    DetalleFactura LinDetFactura = new DetalleFactura(); // crea la primera linea --    

    
    using (SqlConnection conn = new SqlConnection(Conex))        {
        string query = @"
    SELECT idFacturaDetalles, Det.Fecha, Nombre, Cantidad, Precio, idTipoIva, isnull(IVA,0) IVA, Albaranes, Presupuesto, Comentario 
		,NPedido, CodSpatam Orden
	    , ROUND(Precio / NULLIF (CANTIDAD, 0), 5) AS PrecioUnitario
		, ROUND(Precio * ISNULL(IVA, 0) / 100, 4) AS ImporteIva
		, ROUND(Precio + ROUND(Precio * ISNULL(IVA, 0) / 100, 2), 2) AS Total
	FROM Facturas_Detalles Det
		LEFT JOIN Albaranes Alb 		ON Det.IdAlbaran = Alb.IdAlbaran			
	    WHERE idFactura = @idFactura  AND Ejercicio = @Ejercicio
	ORDER BY idFacturaDetalles
        ";

        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();

            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));
            cmd.Parameters["@idFactura"].Value = idFactura;
            cmd.Parameters["@Ejercicio"].Value = Ejercicio;

            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                DetFacturas.Add(LinDetFactura);
                LinDetFactura.idFacturaDetalles = int.Parse(Lector["idFacturaDetalles"].ToString());
                LinDetFactura.Fecha = Convert.ToDateTime(Lector["Fecha"]).ToShortDateString();
                LinDetFactura.Nombre = Lector["Nombre"].ToString();
                LinDetFactura.Cantidad = float.Parse(Lector["Cantidad"].ToString());
                LinDetFactura.Precio = float.Parse(Lector["Precio"].ToString());
                LinDetFactura.idTipoIva = int.Parse(Lector["idTipoIva"].ToString());
                LinDetFactura.IVA = float.Parse(Lector["IVA"].ToString());
                LinDetFactura.PrecioUnitario = float.Parse(Lector["PrecioUnitario"].ToString());
                LinDetFactura.ImporteIva = float.Parse(Lector["ImporteIva"].ToString());
                LinDetFactura.Total = float.Parse(Lector["Total"].ToString());
                LinDetFactura.Albaranes = Lector["Albaranes"].ToString();
                LinDetFactura.Presupuesto = Lector["Presupuesto"].ToString();
                LinDetFactura.Comentario = Lector["Comentario"].ToString();

                LinDetFactura = new DetalleFactura(); // crea nueva linea --
            }
        }
    }
    return DetFacturas;
} // recDetallesFactura_ANT --

public List<DetalleCoste> recDetallesCoste_ANT(int idFactura, int Ejercicio) {
    // Recupera lista de costes de factura en objeto tipo List --
    List<DetalleCoste> DetCostes = new List<DetalleCoste>();
    DetalleCoste LinDetCoste = new DetalleCoste(); // crea la primera linea --    
    
    using (SqlConnection conn = new SqlConnection(Conex)) {                
        string query = @"SELECT idCoste, idFactura, Cos.idProveedor, Titulo NombreProveedor,  isnull(idSap,0) idSapProveedor, Importe, Presupuesto
        FROM Costes Cos
	    LEFT JOIN Proveedores Pro on Cos.idProveedor = Pro.idProveedor
        WHERE Cos.idFactura = @idFactura  AND Cos.Ejercicio = @Ejercicio";
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();

            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));
            cmd.Parameters["@idFactura"].Value = idFactura;
            cmd.Parameters["@Ejercicio"].Value = Ejercicio;

            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (Lector.Read()) {
                DetCostes.Add(LinDetCoste);
                LinDetCoste.idCoste = int.Parse(Lector["idCoste"].ToString());
                LinDetCoste.idFactura = int.Parse(Lector["idFactura"].ToString());
                LinDetCoste.idProveedor = int.Parse(Lector["idProveedor"].ToString());
                LinDetCoste.Importe = float.Parse(Lector["Importe"].ToString());
                LinDetCoste.Presupuesto = Lector["Presupuesto"].ToString();
                LinDetCoste.NombreProveedor = Lector["NombreProveedor"].ToString();
                LinDetCoste.idSapProveedor = Lector["idSapProveedor"].ToString();

                LinDetCoste = new DetalleCoste(); // crea nueva linea --
            }
        }
    }
    return DetCostes;

}// recDetallesCoste_ANT--  


public List<tiposIVA> recIvasFactura_ANT(int idFactura, int Ejercicio, ref string LitIVA1, ref string LitIVA2) {
    // Recupera información de tipos de IVA de los detalles de la factura, 2 maximos --	
    List<tiposIVA> listaIvas = new List<tiposIVA>();

    string query = @" SELECT Ejercicio,idFactura,  idTipoIva, LitIVA, PorIva , sum(Precio) Total
    FROM Facturas_Detalles  det
	    LEFT JOIN (SELECT Codigo, Texto2 LitIVA, Importe PorIVA FROM Tablas WHERE TipoTabla = 'IVA'
    ) tivas ON det.idTipoIva = tivas.Codigo
    WHERE det.idFactura = @idFactura  AND det.Ejercicio = @Ejercicio
    GROUP BY Ejercicio,idFactura, idTipoIva, LitIVA, PorIva";

    
    using (SqlConnection conn = new SqlConnection(Conex)) {
        using (SqlCommand cmd = new SqlCommand(query, conn)) {
            conn.Open();
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@idFactura", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@Ejercicio", SqlDbType.Int));
            cmd.Parameters["@idFactura"].Value = idFactura;
            cmd.Parameters["@Ejercicio"].Value = Ejercicio;

            SqlDataReader Lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            tiposIVA linTipoIVA = new tiposIVA(); // crea la primera linea de lista de clase  --
                                                    // Solo lee un maximo de 2 tipos de IVA --
            if (Lector.Read())
                listaIvas.Add(linTipoIVA);
            LitIVA1 = "#"+Lector["LitIVA"].ToString();
            linTipoIVA.LitIVA = Lector["LitIVA"].ToString();

            // lee el segundo reg. de IVA, si existe, 
            if (Lector.Read()) {
                linTipoIVA = new tiposIVA(); // crea nueva linea de lista de clase  --
                listaIvas.Add(linTipoIVA);
                LitIVA2 = "#" + Lector["LitIVA"].ToString();
                linTipoIVA.LitIVA = Lector["LitIVA"].ToString();
            }
        }
    }
    return listaIvas;
}// recIvasFactura_ANT--  



public void generarFacturaPdf_SINUSO(ref datFactura stFactura) {
    // Ahora se asume con creaBytesFacturaPdf creando un fichero físico despues --
    
    string txFactura = stFactura.FacCabecera.idFactura.ToString();
    string txEjercicio = stFactura.FacCabecera.Ejercicio.ToString();
  
    string nuevoDoc = Server.MapPath("facturas/Fact_"+ txFactura +"_"+ txEjercicio + ".pdf");   
    string orgDoc = Server.MapPath("Plantilla_Factura.pdf");         
    FileStream fs = new FileStream(nuevoDoc, FileMode.Create, FileAccess.Write, FileShare.None);
   
    Document doc = new Document();
    PdfWriter writer = PdfWriter.GetInstance(doc, fs);        
    doc.Open();
    doc.AddAuthor("Pedro Lopez Castro");
    // constantes 
    const int lineasAlbaran = 15;       // nº de lineas de albaran 
    const int linInicDetalle= 480;      // Principio de la linea de detalle 
    const int linSepara= 15;            // separación entre lineas  
    // alineaciones de texto --
    const byte alDr = PdfContentByte.ALIGN_RIGHT;
    const byte alIz = PdfContentByte.ALIGN_LEFT;  

    int Lineas = 0, Pagina = 1;
    string[] multiLin; // para imprimir una cadena de texto en multiple lineas sin cortar usando  cortarEspacios 

    // Fuentes usados --
    BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);     
    BaseFont bfN = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);        // para negrita, bold 

   
    PdfContentByte cb = writer.DirectContent;   
    CabeceraFacturaPdf_ANT(cb, doc, Pagina, ref writer, ref stFactura);
    
    // detalles ----    
    int Px=66, Py= linInicDetalle; 
    foreach (DetalleFactura LinDetFactura in stFactura.DetFactura)    {           
        if (Lineas > lineasAlbaran)     {
            Py = 112;
            cb.ShowTextAligned(alDr, "Suma y Sigue", Px+476, Py, 0); 
            Pagina++;
            CabeceraFacturaPdf_ANT(cb, doc, Pagina, ref writer, ref stFactura);
            Lineas = 0; 
            Py = linInicDetalle; // se situa en primera lineas --
            //cb.SetFontAndSize(bf, 8);// Retorna a la fuente actual de los detalles --
        }
        cb.BeginText();
        cb.SetFontAndSize(bf, 10); 
        
        if (LinDetFactura.Albaranes.Length > 7)
            cb.ShowTextAligned(alDr, LinDetFactura.Albaranes.Substring(0, 7) , Px, Py, 0);   // Albaran            
        else
            cb.ShowTextAligned(alDr, LinDetFactura.Albaranes, Px, Py, 0);               

        
        cb.ShowTextAligned(alDr, LinDetFactura.Presupuesto, Px+72, Py, 0);                  // Presupuesto                  
        cb.ShowTextAligned(alDr,  LinDetFactura.Cantidad.ToString(), Px+113, Py, 0);        // Cantidad            
        cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##0.00000}", LinDetFactura.PrecioUnitario), Px + 176, Py, 0);
        cb.SetFontAndSize(bfN, 8);         
        
        multiLin = Utilidades.cortarEspacios(LinDetFactura.Nombre.Trim(), 50); // retorna array con lineas cortadas --
        foreach (string st2 in multiLin) {
            cb.ShowTextAligned(alIz, st2, Px +184, Py, 0);
            Lineas++; Py-= linSepara; 
        }
        Lineas--;  Py+= linSepara; // al recorrer el array se genera un salto de mas, retrocede una linea --                                  

        // si hay comentario, genera nuevas lineas --
        if (LinDetFactura.Comentario.Trim() != "") { 
            Py-= linSepara; Lineas++; // salto de linea en comentario --
            cb.SetFontAndSize(bf, 8);
            multiLin = Utilidades.cortarEspacios(LinDetFactura.Comentario.Trim(), 60);
            //Debug.WriteLine("*****"+ LinDetFactura.Presupuesto+ ", Cantidad: "+ LinDetFactura.Cantidad.ToString()+" *****");  
            foreach (string st2 in multiLin)       {
                cb.ShowTextAligned(alIz, st2, Px + 184, Py, 0);
                //Debug.WriteLine("-> " + st2);  
                
                Lineas++; Py -= linSepara;
            }
            Lineas--; Py += linSepara; 
        }
        cb.SetFontAndSize(bf, 10); 
        cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", LinDetFactura.Precio), Px+474, Py, 0);  
        cb.EndText();
        Py-= linSepara; Lineas++; // salto de linea --
    }
    
    /* ref: añade otra pagina vacia --
        doc.NewPage();
        doc.Add(new Paragraph("2ª Pagina 2"));
    */
    
    // Pie de factura -- 
    cb.BeginText();
    Px=26; Py= 138 ; 
    cb.SetFontAndSize(bf, 9);  
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.FormaPago, Px, Py, 0); 
    cb.ShowTextAligned(alIz,  stFactura.FacCabecera.CtaFormaPago, Px, Py-linSepara, 0); 
    cb.SetFontAndSize(bfN, 11);  
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.Base), Px+300, Py, 0); 
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.TIva), Px+380, Py, 0); 
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.Cuota ), Px+440, Py, 0);
    cb.ShowTextAligned(alDr, string.Format(CultureInfo.InvariantCulture, "{0,5:##,##0.00}", stFactura.FacCabecera.Total), Px+510, Py, 0);            
    cb.EndText();           

    doc.Close();
}// generarFacturaPdf_SINUSO --

#endregion
        

}// CLASS wsGlFactura --

} // namespaces 
