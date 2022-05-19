using System;
using System.Web.UI;

using System.Web.Configuration;

using System.IO;
using System.Text;
using iTextSharp.text.pdf;

namespace GlFactura
{
    public partial class GlFactura : System.Web.UI.Page    {
static int ContPostback = 0;  
protected void Page_Load(object sender, EventArgs e) {        
    //ClientScript.RegisterStartupScript(GetType(), "Alert", "<script>alert('ASP: Page_Load')</script>");     

    bool noValidar = true; // OJO la validación NO funciona en desarrollo, no hay acceso a Oracle --
            
    if (Page.IsPostBack)    {   
        ContPostback++;
    }
    else    {
        // operaciones de inicialización ---
        lbIp.InnerHtml = "Ip:" + Request.ServerVariables["REMOTE_ADDR"];
        infOculta.InnerHtml = "Rev.47 (19/5/2022) Versión 1.4  "+
            //", PhysicalApplicationPath:" + Request.PhysicalApplicationPath +
            // Recupera nombre del servidor  
            //", SrvIIS: " + WebConfigurationManager.AppSettings["SrvIIS"]+
            ", Srv:" + WebConfigurationManager.ConnectionStrings["GAG"].ConnectionString.Substring(12, 25);            
            //   ", FTp Us:" + WebConfigurationManager.AppSettings["FTP_US"];
};
}// Page_Load --

#region SIN_USO

 // SIN USO, se asume en wsGlFactura.asmx  generarFacturaPdf --
protected void btGeneraFacturaSrv_Click(object sender, EventArgs e) {
    // SIN USO, se asume en wsGlFactura.asmx  generarFacturaPdf --, OK funciona, 
    //System.Diagnostics.Debug.WriteLine("btGeneraFacturaSrv_Click");
    
    string PlantillaFacura = Server.MapPath("Plantilla_Factura.pdf");
            
    Response.Clear();
    Response.ContentType = "application/pdf";
    Response.AddHeader("content-disposition", "attachment;filename=Formulario.pdf");

    PdfReader reader = new PdfReader(PlantillaFacura);
    PdfStamper stamp = new PdfStamper(reader, Response.OutputStream);
         
    stamp.FormFlattening = true; // impide edicion del pdf --
    stamp.Close();

}// btGeneraFacturaSrv_Click--


 // SIN USO, se asume en wsGlFactura.asmx excelListaFacturas--
 protected void excelListaFacturas(object sender, EventArgs e){
    // SIN USO, se asume en wsGlFactura.asmx excelListaFacturas--
    //System.Diagnostics.Debug.WriteLine("excelListaFacturas:"); // Ref: Abrir ventana "resultados", output ---
     
    string sFile = Server.MapPath(Request.ApplicationPath) + "/Informes/listaFacturas.xls";

    StreamWriter w;    
    //FileStream fs = new FileStream("nuevo_file.xls", FileMode.Create, FileAccess.ReadWrite); 
    FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite); 

    w = new StreamWriter(fs);     
    StringBuilder html = new StringBuilder();  

    html.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">"); 
    html.Append("<html>"); 
    html.Append("  <head>"); 
    html.Append("<title>HTML-EXCEL </title>"); 
    html.Append("<meta http-equiv=\"Content-Type\"content=\"text/html; charset=UTF-8\" />"); 
    html.Append("</head>");
    html.Append("<body>"); 
    html.Append("<p>"); 
    html.Append("<table>"); 
    html.Append("<tr style=\"font-weight:bold;font-size: 12px;color: white;\">"); 
    html.Append("<td></td><td bgcolor=\"Blue\">Titulo de la tabla:</td>"); 
    html.Append("<td bgcolor=\"Blue\">Iteración:</td>"); 
    html.Append("</tr>");
    w.Write(html.ToString());

    for (int i = 0; i < 20; i++)     {
        // EscribeLinea(i); 
        string bgColor = "", fontColor = "";
        if (i % 2 == 0)  {
            bgColor = " bgcolor=\"LightBlue\" ";
            fontColor = " style=\"font-size: 10px;color: white;\" ";
        }
        w.Write(@"<tr ><td ></td><td {2} {3}>Titulo de la celda LF:{0} </td><td {2} {3}>Valor de la celdaB: {1}</td></tr>"
            , i.ToString(), i.ToString(), bgColor, fontColor);
    }           
    html.Append("  </table>"); 
    html.Append("</p>"); 
    html.Append(" </body>"); 
    html.Append("</html>"); 
    w.Write(html.ToString());
    w.Close();

    //string strScript = "<script language=JavaScript>window.open('FExcel.xls', '_blank');</script>"; 
    //ClientScript.RegisterStartupScript(GetType(), "clientScript", strScript);
        
}// excelListaFacturas--

#endregion SIN_USO

} // partial class --
} // GlFactura --