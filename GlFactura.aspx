    <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GlFactura.aspx.cs" Inherits="GlFactura.GlFactura" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Globalia Artes Gráficas Facturas</title>


<!-- jQuery para  Bootstrap's Mínimo 1.9.1 -->        
<script src="js/libs/jquery-3.3.1.min.js"></script>

<!-- jQuery  UI jquery-ui-1.12.1>     -->       
<script src="js/libs/jquery-ui.min.js"></script>
<link href="css/jquery-ui.min.css" rel="stylesheet" /> 
        
<!-- Bootstrap core CSS-->   
<link href="css/bootstrap.min.css" rel="stylesheet" />         
<script src="js/libs/bootstrap.min.js"></script>
       

<!-- Plugin Paginador bootpag --> 
<script src="js/libs/jquery.bootpag.min.js"></script>   
    
<!-- Nueva version de fontawesome (5) 03/01/18  --> 
<link href="css/fontawesome-all.min.css" rel="stylesheet" />  
    

<!-- para datatables -->
<link href="css/jquery.dataTables.min.css" rel="stylesheet" />  
<script src="js/libs/jquery.dataTables.min.js"></script>
<script src="js/libs/dataTables.select.min.js"></script>
    
<script src="js/libs/jquery.numeric.min.js"></script>

<!--Estilos Sidebar Colapsable sbarc-->
<link href="css/sideMenuColapsable.css" rel="stylesheet" /> 

<!-- Estilos del site -->
<link href="css/App_Class.css" rel="stylesheet" />
<link href="css/App.css" rel="stylesheet" />

<style type="text/css" media="all">   </style> 


<script src="js/Util.js"></script>
<script src="js/App.js"></script>
<script src="js/Clientes.js"></script>
<script src="js/ListaFacturas.js"></script>


<script type="text/javascript"></script> 

</head>
<body>
<form id="form1" runat="server" style="padding: 0px;">

    
<div class=" form-inline navbar-fixed-top" style="background-color: aliceblue;">
    <a href="GlFactura.aspx">
        <img src="img/Logo.png" width="190" height="50" /></a>
    <label style="font-size: 18px"><a href="GlFactura.aspx">Artes Gráficas</a>     </label>


    <div class="navbar-right" style="padding: 10px">
        <button type="button" class="btn btn-default" disabled="disabled">
            <span id="lbIp" runat="server" class="glyphicon glyphicon-flash ">Ip:</span>
        </button>
        <button type="button" class="btn btn-default" disabled="disabled">
            <span id="lbVersion">V.</span>
        </button>

        <a href="#" >
            <span class="glyphicon glyphicon-user"></span><span id="idUserCab">Sin Reg.</span>
        </a>
        <label></label>
    </div>

</div>
 <!-- FIN Barra navegacion SUPERIOR -->

<nav class="navbar navbar-default sbarc" role="navigation" >
    <div class="container-fluid" >		
        <div  class="collapse navbar-collapse" id="bs-sbarc-navbar-collapse-1" >
            <ul  class="nav navbar-nav" >
                <li id="ulFacturas" class="active" > <a href="#pnFacturas" data-toggle="tab">Facturas <span class="pull-right hidden-xs showopacity far fa-money-bill-alt fa-1x"></span></a>	</li> 
                <li id="ulAlbaranes"> <a href="#frmAlbaranes" data-toggle="tab">Albaranes <span class="pull-right hidden-xs showopacity far fa-list-alt fa-1x"></span></a>	</li> 
                <li id="ulTrabajos"> <a href="#frmAlbaranes" data-toggle="tab">Trabajos <span class="pull-right hidden-xs showopacity fas fa-newspaper fa-1x "></span></a>	</li> 
                <li id="ulClientes" >  <a href="#frmClientes" data-toggle="tab" >Clientes <span class="pull-right hidden-xs showopacity fas fa-users fa-1x"></span></a></li>                                                                           
                <li class="dropdown">
                    <a href="#" class="dropdown-toggle" data-toggle="dropdown">
                        Tablas <span class="caret"></span><span style="font-size:16px;" class="pull-right hidden-xs showopacity glyphicon fa fa-table"></span>
                    </a>
                    <ul id="navMaestros" class="dropdown-menu forAnimate" role="menu">
                        <li id="ulTablas1"><a href="#frmConductores" data-toggle="tab"><span class="pull-right hidden-xs showopacity far fa-user fa-1x"></span>Cadenas</a></li>						
                        <li id="ulTablas2"><a href="#frmTerceros" data-toggle="tab"><span class="pull-right hidden-xs showopacity fas fa-handshake fa-1x"></span>Formas de Pago</a></li>                          
                        <li id="ulTablas3"><a href="#frmTerceros" data-toggle="tab"><span class="pull-right hidden-xs showopacity fas fa-handshake fa-1x"></span>Tipos IVA</a></li>                                                  
                        <li class="divider"></li>
                        <li id="ulPapeles1"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity far fa-map fa-1x"></span>Tipo Papel</a></li>						
                        <li id="ulPapeles2"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity far fa-map fa-1x"></span>Tipo Trabajos</a></li>	
                        <li id="ulPapeles3"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity far fa-map fa-1x"></span>Tamaños Papel</a></li>	
                        <li id="ulPapeles4"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity far fa-map fa-1x"></span>Encuadernaciones</a></li>	
                        <li class="divider"></li>
                        <li id="ulTiposCar1"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity fab fa-bandcamp fa-1x"></span>Tipo Cliente</a></li>						
                        <li id="ulTiposCar2"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity fab fa-bandcamp fa-1x"></span>Tipo Precios</a></li>	
                        <li id="ulTiposCar3"><a href="#frmAutocares" data-toggle="tab"><span class="pull-right hidden-xs showopacity fab fa-bandcamp fa-1x"></span>Marcas</a></li>	                            
                    </ul>
                </li>                                                           
            </ul>
        </div>
    </div>
</nav>
<!--FIN Menu Lateral Colapsable--> 



<div id="tabGeneral" class="tab-content container" style="padding:0px;  " >
  
    <div id="pnFacturas" class="tab-pane fade in active" style="width:1250px;" >          
         <div class="form-inline" style="background-color:antiquewhite;height:50px;padding:6px;">                                                             
            <a class="form-control " id="btFiltro" accesskey="i"><i class="glyphicon glyphicon-search fa-1x"></i>F<u>i</u>ltro </a>            
            <a class="btn btn-primary btn-sl" id="btActualiza" >            <i class="fa fa-refresh fa-1x"></i>A<u>c</u>t.       </a>            
            <button id="btListaFacturasExcel" type="button" class="btn btn-default dropdown-toggle " >                
                <i class="fa fa-file-excel-o fa-1x "></i>Lista Excel  
            </button>
             <button id="btContabSel" type="button" class="btn btn-default dropdown-toggle " >                
                <img src="img/SapLogo.png" width="30" height="26" />Contabilizar  
            </button>
            <button id="btImprimeFacturasSel" type="button" class="btn btn-default dropdown-toggle " >                
                <span class="fas fa-print"></span> Imprimir  
            </button>
            <%--<div class="btn-group">
                <button type="button" class="btn btn-default dropdown-toggle "
                    data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    <i class="fa fa-file-excel-o fa-1x "></i>Contabilizar  <span class="caret"></span>                
                </button>
                <ul class="dropdown-menu">  
                    <li><a href="#" id="btContabSel_" >Solo Selecionadas          </a></li>                
                    <li><a href="#" id="btContabTodas">Todas            </a></li>
                </ul>
            </div>--%>

            <div id="lbnReg" class="btn btn-default disabled "> </div>                                                         
            <div class="form-group"> 
                <div id="pager1" >    </div> 
            </div>    
            <select id="selLinPag" class="form-control" style="width:60px;padding:2px;">            
                <option value="28" selected="selected">28</option>
                <option value="10">10</option>
                <option value="100">100</option>            
                <option value="500">500</option>            
                <option value="100000">Sin Pag.</option>            
            </select>
            <div class="btn-group">
                <div id="lbTotal" class="btn btn-default disabled "></div>      
                <div id="lbCostes" class="btn btn-default disabled "></div>                            
            </div>
        </div>          
        <div class="modal-body" style="padding:1px;">
            <div class="grid1" >                                                
                <div class="panel panel-primary" style="height:850px; overflow-y: scroll;">                                                                            
                    <table  id="gvListaFacturas" class="cabecera-fija" > </table>        
                </div>
          </div>     
        </div>                       
    </div>                 
  
     <div id="frmClientes" class="tab-pane fade in "  >                        

         <div  class="modal-header cabecera" >  
            <div class="cabItem" >
				<label><i class="fas fa-users fa-2x"></i> </label> <h5>CLIENTES</h5>                      
			</div>                          
			<div class="cabItem" >  
				<button id="btActClientes" type="button" class="btn " data-toggle="tooltip" data-original-title="Actualiza Lista"><span class="fas fa-sync "></span> </button>
			</div>        
             <div class="cabItem " >                    
				<label id="lbnRegClientes" class=" labelInfo" data-toggle="tooltip" data-placement="bottom" data-original-title="Total Clientes SELECCIONADOS"></label>                                                   
			</div>        
             <div class="cabItem" >  <%-- style="width:480px;"> style="flex:4;"--%>
				<div id="pagerClientes"></div>  
			</div>
             <div class="cabItem" >  
				<select id="selLinPagClientes" class="form-control" style="width:45px;" data-toggle="tooltip" data-placement="top" data-original-title="Lineas por Página" >
					<option value="28" selected="selected">28</option>
					<option value="10">10</option>
					<option value="100">100</option>            
					<option value="1000">1000</option>      					
			   </select>                
			</div>

            <div  id="frmClientesBtnMaxMin" class="BtnMaxMin cabItemR" >                
			    <i class="far fa-list-alt btBar"  ></i>    
			    <i class="far fa-window-maximize btBar btBar"  ></i> 
			    <i class="fas fa-columns btBar" ></i>
			    <i class="far fa-window-restore btBar" ></i>                  
		    </div>                           
         </div>

        <%-- <div class="modal-header form-inline" >         
            <i class="far fas fa-users fa-2x"></i>   <label>Clientes  </label> 
            <a id="btActClientes_" class="form-control bs-iconos ">Act. <i class="fas fa-sync fa-1x"></i></a>

            <div id="lbnRegClientes_" class="btn btn-default disabled ">         </div>
            <div class="form-group " >      <div id="pagerClientes_" > </div>             </div>
            <select id="selLinPagClientes_" class="form-control" style="width:60px;padding:2px;"data-toggle="tooltip" data-placement="right" data-original-title="Lineas por Página">
                <option value="28" selected="selected">28</option>
                <option value="10">10</option>
                <option value="100">100</option>                
                <option value="1000">1000</option>                             
            </select>              
        </div>    --%>
        
        <div id="container" class="flexP"  >
             <div id="menuVIcon" >                                     
                <div id="frmEdClientebtNuevo"><i class="fas fa-plus-circle fa-1x"></i> <label>  Nuevo  </label> </div>                
                <div id="frmEdClientebtInformeExcel"> <i class="far fa-file-excel fa-1x"></i>   <label> Informe</label></div>
            </div>                                
             <div  id="lisClientes" class="panel panel-primary" >
                <table id="tbClientes" class="cabecera-fija" ></table>
             </div>    
                        
            <div id="frmEdCliente" class="panel panel-default" >
                <div class="panel-heading form-inline"   >    
                    <i class="far fa-address-book fa-2x "></i>                
                    <label>Codigo.</label> <span id="frmEdClienteId"  class="form-control noMod"  style="width:40px;"></span>
                    <label>Anulado </label> <input  id="frmEdClienteckAnulado" type="checkbox" />
                    <label>Del Grupo </label> <input  id="frmEdClienteckdelGrupo" type="checkbox" />     
                    <label>Cadena</label>   <select id="frmEdClienteselCadena" class="form-control selCadena" style="width:130px;">   </select>                       
                    <span id="ocultofrmCliente" class="btn navbar-right" style="margin-right:5px;" >		                                                				                 
                        <button id="btGuardarfrmCliente" type="button" class="btn" data-toggle="tooltip" data-placement="bottom" data-original-title="Guarda Información" > 
                            <span class="far fa-save"></span> 
                        </button>                                          					                                    
                        <button id="btCancelafrmCliente" type="button" class="btn" data-toggle="tooltip" data-placement="bottom" data-original-title="Cancela Edición"> 
                            <span class="far fa-window-close"></span>
                        </button>
                    </span>
                </div>  
                <div class="panel-body" >
                    <div class="form-group form-inline">         
                        <label>Id.</label>   <select id="frmEdClienteTipoDoc" class="form-control" style="width:60px;">  
                            <option value="0" selected="selected">NO Asig</option>
                            <option value="1">CIF/NIF</option>                            
                            <option value="2">NIE</option>            
                            <option value="3">OTRO</option>                                        
                         </select>                    
                        <%--<label>NIF</label>  --%>
                            <input id="frmEdClienteNIF" type="text" class="form-control " style="width:85px;" />                                    
                        <label>C.SAP</label> <input id="frmEdClienteidSap" type="text" class="form-control " style="width:70px;" />                                                   
                        <label >C.Ext.</label>  <input id="frmEdClienteCodExterno" type="text" class="form-control " style="width:65px;" />
                        <label >C.Graf.</label> <input id="frmEdClienteCodGraf" type="text" class="form-control " style="width:70px;" />
                        <label >C.Conta.</label><input id="frmEdClienteCodContable" type="text" class="form-control " style="width:70px;" />
                    </div>	       
                                
                    <div class="form-group form-inline">                    
                        <label >Razon Social</label>  <input  id="frmEdClienteRazonSocial" type="text" class="form-control " style="width:500px;" />                    
                    </div>	       
                    <div class="form-group form-inline">                    
                        <label >N. Comercial</label>  <input  id="frmEdClienteNombreComercial" type="text" class="form-control " style="width:500px;" />                    
                    </div>	       
                    <div class="form-group form-inline">                    
                        <label>Telefono.</label>  <input id="frmEdClienteTelef01" type="text" class="form-control " style="width:95px;" />                                    
                        <label>Correo</label>  <input id="frmEdClienteEmail" type="text" class="form-control " style="width:300px; " />                        
                        <label>Fax.</label>  <input id="frmEdClienteFax01" type="text" class="form-control " style="width:95px;" />                                    
                    </div>	       
                    <div class="form-group form-inline"> 
                        <label>Contacto</label>  <input  id="frmEdClienteContacto" type="text" class="form-control" style="width:200px;" />
                        <label>F.Alta</label>  <input id="frmEdClienteFAlta" type="text" class="form-control fecha" style="width:75px;" />
                        <label>F.Baja</label>  <input  id="frmEdClienteFBaja" type="text" class="form-control fecha" style="width:75px;" />       
                        <label>Pais</label>   <select id="frmEdClienteSelPais" class="form-control" style="width:100px;">   </select>                    
                    </div>	                                                       
                    <div id="tabCliente" class="panel with-nav-tabs panel-default">                     
                        <div class="panel-heading">
                            <ul class="nav nav-tabs">
                                <li class="active"> <a href="#tabClienGeneral" data-toggle="tab">General</a></li>
                                <li><a href="#tabClienFpago" data-toggle="tab">F.Pago/Observ.</a></li>         
                                <li><a href="#tabClienCarrito" data-toggle="tab">Carrito</a></li>                                
                                                       
                            </ul>
                        </div>
                        <div class="tab-content">
                            <div class="tab-pane fade in active " id="tabClienGeneral">     
                                <div class="form-group form-inline">                                                                                           
                                    <label style="width:80px;">Domicilio</label>  <input  id="frmEdClienteDomicilio" type="text" class="form-control" style="width:500px;" />
                                </div>	       
                                <div class="form-group form-inline">                                                                                           
                                    <label style="width:80px;">Poblacion</label>  <input  id="frmEdClientePoblacion" type="text" class="form-control" style="width:200px;" />
                                    <label>Provincia</label>  <input  id="frmEdClienteProvincia" type="text" class="form-control" style="width:180px;" />
                                    <label>CP</label>  <input  id="frmEdClienteCodpostal" type="text" class="form-control" style="width:60px;" />
                                </div>
                                <br />
                                <div  class="panel panel-default" >    
                                    <div class="panel-heading form-inline" style="background-color:beige; width:auto;height:35px;">                                           
                                        <i class="far fa-envelope"></i><label>Informacion de envio </label>                                        
                                    </div>                    
                                    <div class="panel panel-default" >
                                         <div class="form-group form-inline">                                                                                           
                                            <label style="width:80px;">Domicilio</label> <input  id="frmEdClienteDomicilioEnvio" type="text" class="form-control" style="width:500px;" />
                                        </div>	                                                                                             	       
                                          <div class="form-group form-inline">                                        
                                            <label style="width:80px;">Poblacion</label>  <input  id="frmEdClientePoblacionEnvio" type="text" class="form-control" style="width:200px;" />
                                            <label>Provincia</label>  <input  id="frmEdClienteProvinciaEnvio" type="text" class="form-control" style="width:180px;" />
                                            <label>CP</label>  <input  id="frmEdClienteCodpostalEnvio" type="text" class="form-control" style="width:60px;" />
                                        </div>	                                             
                                    </div>
                                </div>  
                                <br />                                
                            </div>

                            <div class="tab-pane fade in " id="tabClienFpago">    
                                 <div class="panel-heading form-inline" style="background-color:beige; width:auto;height:35px;">                                           
                                    <i class="fas fa-money-bill-alt"></i><label> Forma Pago</label> 
                                </div>           
                                <br />
                                <div class="form-group form-inline"   >                                                                                   
                                    <label>Limite Credito</label>  <input  id="frmEdClienteLimiteCredito" type="text" class="form-control" style="width:70px;" />         
                                    <label>Dias Credito</label>  <input  id="frmEdClienteDiasCredito" type="text" class="form-control" style="width:60px;" />         
                                    <label>Cta. Bancaria</label>  <input  id="frmEdClienteCuentaBancaria" type="text" class="form-control" style="width:220px;" />         
                                 </div>
                                <div class="form-group form-inline"   >                                                                                                                       
                                     <label>Forma de Pago </label>  <select id="frmEdClienteSelFormaPago" class="form-control" style="width:350px;">   </select>                                                                                          
                                 </div>               
                                <br />  
                                <div class="panel-heading form-inline" style="background-color:beige; width:auto;height:35px;">                                           
                                    <i class="far fa-comment-alt"></i><label> Observaciones</label> 
                                </div>                   
                                <div class="form-group">
                                     <textarea id="frmEdClienteObservaciones" class="form-control" rows="6" > </textarea>
                                </div>                                                        
                            </div>       

                            <div class="tab-pane fade in " id="tabClienCarrito">   
                                <div class="panel-heading form-inline" style="background-color:beige; width:auto;height:35px;">                                           
                                    <i class="fas fa-shopping-cart"></i><label> Información del carrito</label> 
                                </div>                         
                                 <br />                                                                                           
                                <div class="form-group form-inline"   >                                                                                                                                                       
                                    <label>Pedido Minimo CON Compromiso</label><input id="frmEdClientePedMinimoConCompromiso" type="text" class="form-control" style="width:60px;" />       
                                    <label>Pedido Minimo SIN Compromiso</label><input id="frmEdClientePedMinimoSinCompromiso" type="text" class="form-control" style="width:60px;" />                                                          
                                 </div>
                                <div class="form-group form-inline"   >                                                                                   
                                    <label>Requiere Autorizacion </label> <input  id="frmEdClienteckReqAutoriza" type="checkbox" />     
                                     <label>Tipo Cliente/Hotel</label>   <select id="frmEdClienteselTipoCliente" class="form-control" style="width:130px;">   </select>   
                                    <label>Tipo Iva</label>   <select id="frmEdClienteselTipoIva" class="form-control" style="width:130px;">   </select>                                     
                                 </div>
                                <div class="form-group form-inline"   >                                                                                   
                                    <label>Contraseña</label>  <input  id="frmEdClienteContrasena" type="text" class="form-control" style="width:120px;" />         
                                     <label>Jefe Economato</label>  <input  id="frmEdClienteJefeEconomato" type="text" class="form-control" style="width:300px;" />                                    
                                 </div>
                                      
                                <div class="form-group form-inline"   >                                                                                                                                                       
                                    <label>Tipo Precio</label> <select id="frmEdClienteselTipoPrecio" class="form-control" style="width:130px;">   </select>   
                                    <label>Marca</label>  <select id="frmEdClienteselMarca" class="form-control" style="width:130px;">   </select>   
                                 </div>
                                               
                                <div class="panel-heading form-inline" style="background-color:aliceblue; height:50px;">                                                                                  
                                    <i class="fas fa-user"></i><label>Usuarios </label>
                                    <a id="btNuevoServicioFrmLocaliza" class="form-control bs-iconos ">Nuevo Usuario <i class="fas fa-plus-circle "></i></a>                        
                                </div>                    
                                <div class="panel panel-primary" style="overflow:auto; overflow-y:scroll; height:190px;">                                    
                                    <table id="tbUsuariosCliente" class="cabecera-fija" ></table>                                    
                                </div>  
                             </div>              
                                              
                            
                                                     
                        </div>                        
                    </div>                    
                 </div>                                                                                                                                               
            </div>                                          
        </div>                        
    </div> 


   <div id="CapaOrdenCol" class="capa" >
        <div class="modal-header" >						
            <label id="lbOrdenCol"> Filtro.. </label>   <a class="close far fa-window-close fa-2x" onclick="javascript:$(this).parent().parent().hide();"></a>
        </div>
        <div class="modal-body">              
           <div class="form-inline">
                <a class="form-control" id="btOrAsc"> <i class="fas fa-sort-amount-up"></i>Ascente</a>
                <a class="form-control" id="btOrDesc"> <i class="fas fa-sort-amount-down"></i>Descen.</a>
            </div>
            <br />        
            <div id="dvFiltrosCapa">
                <div class="form-group"> <input  id="idClienteFiltroCapa" class="form-control" style="width:180px;" placeholder="Codigo. "/> </div>                                                            
                <div class="form-group"> <input  id="idSapFiltroCapa" class="form-control" style="width:180px;" placeholder="Cod. SAP"/> </div>                            
                <div class="form-group"> <input  id="CodExternoFiltroCapa" class="form-control" style="width:180px;" placeholder="Cod. Externo"/> </div>                            
                <div class="form-group"> <input  id="NIFFiltroCapa" class="form-control" style="width:180px;" placeholder="N.I.F."/> </div>
                <div class="form-group"> <input  id="RazonSocialFiltroCapa" class="form-control" style="width:180px;" placeholder="Razon Social"/> </div>
                <div class="form-group"> <input  id="NombreComercialFiltroCapa" class="form-control" style="width:180px;" placeholder="Nombre Comercial"/> </div>                
                <div class="form-group"> <select id="selCadenaFiltroCapa" class="form-control selCadena"  >  </select>  </div>                
                <div class="form-group"> <input  id="PoblacionFiltroCapa" class="form-control" style="width:180px;" placeholder="Poblacion"/> </div>         
                <div class="form-group" id="rbFiltroCapaAnulado">                    
                    <b>ANULADOS</b>
                    <div class="radio"><label><input id="rbTodosFiltroCapa" type="radio" name="rbAnul" value="3" checked="checked"/>Todos </label></div>
                    <div class="radio"><label><input id="rbAnuladoTodosFiltroCapa" type="radio" name="rbAnul" value="1" /> Anulados </label></div>
                    <div class="radio"><label><input id="rbNoAnuladoTodosFiltroCapa" type="radio" name="rbAnul" value="0" />NO Anulados</label></div>                                                
                </div>                
                <div class="form-group" id="rbFiltroCapaDelGrupo">                    
                    <b>Del Grupo</b>
                    <div class="radio"><label><input id="rbTodosGrFiltroCapa" type="radio" name="rbGr" value="3" checked="checked"/>Todos </label></div>
                    <div class="radio"><label><input id="rbAnuladoGrFiltroCapa" type="radio" name="rbGr" value="1" /> Del Grupo </label></div>
                    <div class="radio"><label><input id="rbNoAnuladoGrFiltroCapa" type="radio" name="rbGr" value="0" />NO Grupo</label></div>                                                
                </div>                
             </div>                  
            <div class="conDinamico"> CONTENIDO DINAMICO    </div>                    
            <div  id="dvAceptarFiltro" class="form-inline" style="margin:5px;">                               
                <a class="form-control" id="btAceptarFiltro"> <i class="fas fa-filter"></i>Filtrar</a>
                <a class="form-control" id="btCancelarFiltro"> <i class="fas fa-times"></i>Cancel Filtro</a>                           
            </div>        
        </div>	               
    </div> 
    

    <div id="frmAlbaranes" class="tab-pane fade  in "  >    
        <div class="modal-header form-inline" >         CAB ALBARANESS     </div>
        <div class="modal-body">
            <h1>ALBARANES   </h1>    
         </div>              
    </div> 
    
</div>    


        


<div class="modal fade" id="frmFactura" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
    <div class="modal-dialog" style="width:1100px;">
        <div class="modal-content">
            <div class="modal-header" style="background-color:aliceblue">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">×</span><span class="sr-only"></span></button>
                <div class="form-inline">    
                                                                    
                    <i class="fa fa-truck fa-2x"></i>                        
                    <label>Factura</label> <span id="txFactura" class="form-control"  ></span>                    
                    <label>Ejercicio</label> <input type="text" class="form-control" id="txEjercicio"  style="width:60px;"  />     
                    <label>Fecha Factura</label> <input type="text" class="form-control fecha" id="txFechaFactura"  />
                    <label>Abierta</label> <input  id="ckAbierta" type="checkbox" disabled="disabled"/>                                               
                    <label>Fecha Contab</label> <input type="text" class="form-control fecha" id="txFContabilizada"  style="width:90px;"/>                                                             
              
                    <div id="ocultofrmPeticion" class="form-control navbar-right bs-iconos ">
                        <span >
                            <a id="btGeneraSap" href="#" data-toggle="tooltip" title="Genera fichero contable SAP " ">                                
                                <img src="img/SapLogo.png" width="30" height="26" />Contabilizar</a>

                             <a id="btImprimeFactura" href="#" data-toggle="tooltip" title="Imprime Factura en Pfd"  ">                                 
                                <span class="fa fa-print"></span>Imprimir
                              </a>                           
                        </span>                     
                    </div>

                </div>                                                                                         
            </div>

            <div class="modal-body" >                                               
                <div class="form-group">
                                        
                    <div class="row"  >
                        <div class="col-md-6 form-inline"   >
                            <label>Cliente</label>                    
                            <input type="text" class="form-control" id="txIdCliente" disabled="disabled" style="width:80px;"  />                           
                            <input type="text" class="form-control" id="txCliente" placeholder="Nombre Cliente" style="width:350px;"/>                     
                            
                            <label>Coste</label>                    
                            <input type="text" class="form-control" id="txCoste" disabled="disabled" style="width:80px;"  />                                                        
                         </div>                            
                         <div class="col-md-4 " >
                           <label>Referencia</label>                                                                
                           <textarea id="txReferencia" class="form-control" rows="3" style="width:450px;" > </textarea>                          
                         </div>                                                                  
                    </div>                                     
                </div>
                                                    
               <div class="panel panel-default">
                    <div class="panel-heading">
                        <h5 class="panel-title form-inline">                            
                            <i class="fa fa-truck fa-1x"></i>
                            <a data-toggle="collapse" data-parent="#accordion" href="#collapse1">Detalles Factura </a>                            
                        </h5>
                    </div>
                    <div id="collapse1" class="panel-collapse collapse in">
                        <div class="grid1" style="padding-right: 1px; max-height:350px; overflow-y: auto;">
                            <table class="table  table-hover table-striped table-condensed display compact" id="gvDetallesFactura" >
                            </table>                                                                      
                        </div>
                    </div>                   
                </div>                                         
            </div>

        </div>
    </div>
</div><!-- fin Dialogo frmFactura  ****** -->


<div id="capaFiltroFacturas" class="capa" style="width:260px;">
    <div class="modal-header" style="height:50px;">
        <a class="close fa-2x" onclick="javascript:$('#capaFiltroFacturas').hide();">×</a>        
        <a href="#" id="btOkFiltroServicios" class="btn danger" ><span class="fa fa-search"></span>Aplicar filtro</a>
    </div>
    <div class="modal-body">                                 
        <select id="selEjercicio" class="form-control" >

            <option value="2023">Ejer. 2023</option>
            <option value="2022">Ejer. 2022</option>
            <option value="2021">Ejer. 2021</option>
            <option value="2020">Ejer. 2020</option>
            <option value="2019">Ejer. 2019</option>
            <option value="2018">Ejer. 2018</option>
            <option value="2017">Ejer. 2017</option>
            <option value="2016">Ejer. 2016</option>
            <option value="2015">Ejer. 2015</option>
            <option value="2014">Ejer. 2014</option>
            <option value="2013">Ejer. 2013</option>
            <option value="2012">Ejer. 2012</option>
            <option value="2011">Ejer. 2011</option>
            <option value="2010">Ejer. 2010</option>
            <option value="2009">Ejer. 2009</option>
            <option value="2008">Ejer. 2008</option>                
        </select>
            
        <%--<label>Cadena:</label>--%>
        <select id="selCadenaFiltro" class="form-control selCadena" >
                <option value="0" selected="selected">*** TODAS LAS CADENAS ***</option>        
        </select>
        <br />
        <div class="row" >
            <div class="col-md-5"   >
                <div class="radio">
                    <label><input id="rbSelF0" type="radio" name="rbSelF"  value="0" />Todas </label>
                </div>
                <div class="radio">
                    <label><input id="rbSelF1" type="radio" name="rbSelF"  value="1" /> 2 días </label>
                </div>
                <div class="radio">
                    <label><input id="rbSelF2" type="radio" name="rbSelF"  checked="checked" value="2" />Una semana</label>
                </div>                
                <div class="radio">
                    <label><input id="rbSelF4" type="radio" name="rbSelF"   value="4" />Año Actual</label>
                </div>        
            </div>            
            <div class="col-md-3 ">
                <label class="control-label" for="tbF2">Desde </label>
                <div class="input-group ">
                    <div class="input-group-addon">
                        <div class="glyphicon glyphicon-calendar" aria-hidden="true"></div>
                    </div>
                    <input id="fFiltroDesde" type="text" class="fecha" maxlength="10" />
                </div>

                <label class="control-label" for="tbF2">Hasta </label>
                <div class="input-group">
                    <div class="input-group-addon">
                        <div class="glyphicon glyphicon-calendar" aria-hidden="true"></div>
                    </div>
                    <input id="fFiltroHasta" class="fecha" maxlength="10" />
                </div>                

            </div>          
         </div> <%--FIN row--%>

        <div class=" form-group " > 
            <label>Búsquedas (Anula Filtros)</label>           
            <input id="txSelFactura"  class="form-control" placeholder="Factura" />                        
            <input id="txSelCliente"  class="form-control" placeholder="Cliente" />
            <input type="text" id="txSelIdCliente" class="form-control" disabled="disabled" style="display:none"/>                          
            <input id="txSelPedido"  class="form-control" placeholder="Pedido" />
            <input id="txSelAlbaran"  class="form-control" placeholder="Albaran" />
            <input id="txSelidSap"  class="form-control" placeholder="Codigo SAP" />
            <input id="txSelHRuta"  class="form-control" placeholder="Hoja Ruta" />
             
         </div>
        <div class=" form-group " > 
                <label>Cargo/Abono </label>
                <div class="radio"><label><input type="radio" name="rbCA" value="0" />Todos </label></div>
                <div class="radio"><label><input type="radio" name="rbCA" checked="checked"value="1" />Cargos</label>      </div>
                <div class="radio"><label><input type="radio" name="rbCA" value="2" />Abonos</label>
             </div>
             <div class=" form-group " > 
                <label>Contabilizados </label>
                <div class="radio"><label><input type="radio" name="rbFac" checked="checked" value="0" />Todos </label></div>
                <div class="radio"><label><input type="radio" name="rbFac" value="1" />NO Contabilizados</label>      </div>
                <div class="radio"><label><input type="radio" name="rbFac" value="2" />Contabilizados </label>
             </div>
        </div>
     </div>
    </div>
   <%--<div class="modal-footer">       
        <a href="#" onclick="javascript:$('#capaFiltroFacturas').hide();" class="btn danger"><span class="fa fa-close"></span>Cerrar</a>
    </div>--%>
</div> <%--FIN capaFiltroFacturas--%>

  

<div class="modal fade" id="msgAct" role="dialog" >
    <div class="modal-dialog modal-sm" >        
        <div class="modal-header" style="background-color:aliceblue">                 
            <i class="fa fa-spinner fa-spin "></i><span> ACTUALIZANDO </span>                                
        </div>                    
    </div>
</div>



<div class="modal fade" id="msgAler" >
  <div class="modal-dialog modal-sm" >
    <div class="modal-content">
      <div class="modal-header">
        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
        <h4 id="msgAlerTitulo" class="modal-title"><i class="fa fa-exclamation-triangle " aria-hidden="true"></i><span>!! ATENCION !!</span>    </h4>
      </div>
      <div class="modal-body">                          
        <textarea id="txMsg" disabled="disabled" style="width:100%"> </textarea>                                                                     
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>        
      </div>
    </div>
  </div>
</div>
    
  
<div class="msgAct2"></div>

 
<div style="visibility:hidden">        
    <input type="text" id="IdFacturaOculto" runat="server"  />  
    <span id="infOculta" runat="server" ></span>        
</div> 

        

</form>


 

</body>
</html>

