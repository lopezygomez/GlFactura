using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlFactura {

public class UsuarioCliente {
    public int Empleado { get; set; }
    public int IdCliente { get; set; }    
    public string NombreUsuario { get; set; }
    public string EMail { get; set; }
    public bool Activo { get; set; }        
}


public class selListaClientes {    
    public int Orden { get; set; }
    public int Anulado { get; set; }
    public int delGrupo { get; set; }
    public int idCliente { get; set; }
    public int idCadena { get; set; }
    public string idSap { get; set; }
    public string CodExterno { get; set; }        
    public string NIF { get; set; }
    public string RazonSocial { get; set; }
    public string NombreComercial { get; set; }
    public string Domicilio { get; set; }
    public string Poblacion { get; set; }
    }

public class ListaCliente {
    public int Fila { get; set; }
    public int idCliente { get; set; }    
    public string idSap { get; set; }
    public int delGrupo { get; set; }    
    public string RazonSocial { get; set; }
    public string NombreComercial { get; set; }
    public int idTipoDocumento { get; set; }        
    public string NTipoDoc { get; set; }
    public string NIF { get; set; }

    public string Domicilio { get; set; }
    public string DomicilioEnvio { get; set; }
    public string Poblacion { get; set; }
    public string PoblacionEnvio { get; set; }
    public string Provincia { get; set; }
    public string ProvinciaEnvio { get; set; }
    public string Codpostal { get; set; }
    public string CodpostalEnvio { get; set; }
    public string Telef01{ get; set; }
    public string Fax01 { get; set; }
    public string Email { get; set; }
    public string LimiteCredito { get; set; }
    public string DiasCredito { get; set; }    
    public int idFormaPago { get; set; }
    public string FormaPago { get; set; }
    public string Observaciones { get; set; }
    public string CtaFormaPago { get; set; }
    public string CuentaBancaria { get; set; }
    public string CodExterno { get; set; }
    public int idCadena { get; set; }
    public string NombreCadena { get; set; }
    public float CadenaActivaCarrito { get; set; }
    public string OfCentalCadena { get; set; }
    public int PedMinimoConCompromiso { get; set; }
    public int PedMinimoSinCompromiso { get; set; }
    public string Contrasena { get; set; }
    public string FAlta { get; set; }
    public string FBaja { get; set; }    
    public bool Anulado { get; set; }
    public bool ReqAutoriza { get; set; }
    public int idTipoCliente { get; set; }
    public string TipoClienteHotel { get; set; }
    public float CadenaTipoCliente { get; set; }
    public string JefeEconomato { get; set; }
    public int idMarca { get; set; }
    public string Marca { get; set; }
    public int idTipoPrecio { get; set; }
    public string TipoPrecio { get; set; }
    public int idTipo { get; set; }
    public int idValidadora { get; set; }
    public string Contacto { get; set; }
    public int idPais { get; set; }
    public string NombrePais { get; set; }
    public int idTipoIva { get; set; }
    public string TipoIVA { get; set; }
    public string SiglaTipoIVA { get; set; }
    public float ImporteIVA { get; set; }
    public string CodContable { get; set; }
    public string CodGraf { get; set; }
}


public class Cliente {    
    public int idCliente { get; set; }            
    public string idSap { get; set; }
    public int delGrupo { get; set; }    
    public string RazonSocial { get; set; }
    public string NombreComercial { get; set; }
    public int idTipoDocumento { get; set; }        
    public string NIF { get; set; }
    public string Domicilio { get; set; }    
    public string Poblacion { get; set; }    
    public string Provincia { get; set; }
    public string Codpostal { get; set; }
    public string DireccionEnvio { get; set; }
    public string PoblacionEnvio { get; set; }
    public string ProvinciaEnvio { get; set; }    
    public string CodpostalEnvio { get; set; }
    public string Telef01{ get; set; }
    public string Fax01{ get; set; }
    public string Email{ get; set; }            
    public string LimiteCredito { get; set; }
    public string DiasCredito { get; set; }
    public string CuentaBancaria { get; set; }
    public int idFormaPago { get; set; }
    public string FormaPago { get; set; }    
    public string Observaciones { get; set; }    
    public string CodExterno{ get; set; }
    public int idCadena{ get; set; }
    public int PedMinimoConCompromiso{ get; set; }
    public int PedMinimoSinCompromiso{ get; set; }
    public string Contrasena{ get; set; }
    public string FAlta{ get; set; }
    public string FBaja{ get; set; }
    public bool Anulado { get; set; }
    public bool ReqAutoriza{ get; set; }
    public int idTipoCliente{ get; set; }
    public string JefeEconomato{ get; set; }
    public int idMarca{ get; set; }
    public int idTipoPrecio{ get; set; }
    public int idTipo{ get; set; }
    public int idValidadora{ get; set; }
    public string Contacto{ get; set; }
    public int idPais{ get; set; }
    public int idTipoIva{ get; set; }
    public int idTratoEspecial{ get; set; }
    public string CodContable { get; set; }        
    public string CodGraf { get; set; } // COD
   }

public class lisFacSel {
    public int idFactura { get; set; }
    public int Ejercicio { get; set; }
}


public class tiposIVA {
    public string LitIVA { get; set; }
    public float porIVA { get; set; }
    public float importeIVA { get; set; }
}


 public class lisFactura {
    public int Fila { get; set; }
    public int Ejercicio { get; set; }
    public int idFactura { get; set; }
    public int idCliente { get; set; }
    public string idSap { get; set; }
    public string NombreCliente { get; set; }
    public string Fecha { get; set; }
    public string Presupuesto { get; set; }
    public string Referencia { get; set; }
    public int Estado { get; set; }
    public float Total { get; set; }
    public int NReg { get; set; }
    public string FContabilizada { get; set; }
    public string NombreCadena { get; set; }
    public float Coste { get; set; }
    public string Pedido { get; set; }
    public string Albaran { get; set; }
    public string HRuta { get; set; }
}

public class datFactura {
    public Factura FacCabecera { get; set; }
    public List<DetalleFactura> DetFactura { get; set; }
    public List<DetalleCoste> DetCostes { get; set; }
}

public class Factura {
    public int idFactura { get; set; }
    public int idCliente { get; set; }
    public string FechaFactura { get; set; }
    public int Ejercicio { get; set; }
    public string NombreCliente { get; set; }
    public string RazonSocial { get; set; }
    public int Estado { get; set; }
    public string Referencia { get; set; }
    public string FContabilizada { get; set; }
    public string Domicilio { get; set; }
    public string Poblacion { get; set; }
    public string CodPostal { get; set; }
    public string Provincia { get; set; }
    public string DomicilioEnvio { get; set; }
    public string PoblacionEnvio { get; set; }
    public string CodPostalEnvio { get; set; }
    public string ProvinciaEnvio { get; set; }
    public string idSap { get; set; }
    public string Nif { get; set; }
    public string Presupuesto { get; set; }
    public float Total { get; set; }
    public float TIva { get; set; }
    public float Base { get; set; }
    public float Cuota { get; set; }
    public float TCostes { get; set; }
    public float TImporteExistencias { get; set; }
    public float TPrecio { get; set; }
    public int idFacAbonada { get; set; }
    public string PrimFechaDetalle { get; set; }
    public string FormaPago { get; set; }
    public string CtaFormaPago { get; set; }
    public string Pedido { get; set; }
    public string Orden { get; set; }
    public float Coste { get; set; }
    public string CodExterno { get; set; }    
    public int idCadena { get; set; }
};

public class DetalleFactura {
    public int idFacturaDetalles { get; set; }
    public string Fecha { get; set; }
    public string Nombre { get; set; }
    public float Cantidad { get; set; }
    public double Precio { get; set; }
    public int idTipoIva { get; set; }
    public float IVA { get; set; }
    public float PrecioUnitario { get; set; }
    public float ImporteIva { get; set; }
    public float Total { get; set; }
    public string Albaranes { get; set; }
    public string Presupuesto { get; set; }
    public string Comentario { get; set; }
    public string HRuta { get; set; }
}

public class DetalleCoste {
    public int idCoste { get; set; }
    public int idFactura { get; set; }
    public int idProveedor { get; set; }
    public string NombreProveedor { get; set; }
    public float Importe { get; set; }
    public string Presupuesto { get; set; }
    public string idSapProveedor { get; set; }
}

public class lisAlbaran {
    public int idAlbaran { get; set; }
    public int idCliente { get; set; }
    public string NombreCliente { get; set; }
    public string Fecha { get; set; }
    public bool Anulado { get; set; }
    public string Observaciones { get; set; }
    public int Factura { get; set; }
    public int Pedido { get; set; }
    public string NReg { get; set; }
    public string TotImporte { get; set; }
    public string NTrabajo { get; set; }
    public string NombreCadena { get; set; }
    public bool DelGrupo { get; set; }
};

public class Cadena {
    public int Codigo { get; set; }
    public string NombreCadena { get; set; }
    public int Activa { get; set; }
    public string ofCentral { get; set; }
}


public class tiposOrden {
    public int id { get; set; }
    public string Tx { get; set; }
}

public class Estructuras {
}


}